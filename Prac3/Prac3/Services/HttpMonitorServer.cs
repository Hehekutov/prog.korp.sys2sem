using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using Prac3.Models;

namespace Prac3.Services;

public class HttpMonitorServer
{
    private readonly AppLogger _logger;
    private readonly object _sync = new();
    private readonly ConcurrentDictionary<Guid, MessageRecord> _messages = new();
    private readonly ConcurrentDictionary<DateTime, int> _minuteBuckets = new();
    private readonly ConcurrentDictionary<DateTime, int> _hourBuckets = new();
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private DateTime? _startedAt;
    private int _totalRequests;
    private int _getRequests;
    private int _postRequests;
    private double _totalProcessingTime;
    private int? _lastStatusCode;

    public HttpMonitorServer(AppLogger logger)
    {
        _logger = logger;
        _logger.LogCreated += entry => LogCreated?.Invoke(entry);
    }

    public event Action<LogEntry>? LogCreated;

    public event Action<MessageRecord>? MessageReceived;

    public event Action<ServerStatistics>? StatisticsUpdated;

    public bool IsRunning => _listener?.IsListening == true;

    public int Port { get; private set; }

    public Task StartAsync(int port)
    {
        if (IsRunning)
        {
            return Task.CompletedTask;
        }

        Port = port;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _listener.Start();
        _cts = new CancellationTokenSource();
        _startedAt = DateTime.Now;

        StatisticsUpdated?.Invoke(GetStatisticsSnapshot());
        _ = Task.Run(() => ListenLoopAsync(_cts.Token));
        return Task.CompletedTask;
    }

    public void Stop()
    {
        try
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
        }
        catch
        {
        }
        finally
        {
            _listener = null;
            _cts = null;
            Port = 0;
            _startedAt = null;
            StatisticsUpdated?.Invoke(GetStatisticsSnapshot());
        }
    }

    public TimeSpan GetUptime()
    {
        return _startedAt.HasValue ? DateTime.Now - _startedAt.Value : TimeSpan.Zero;
    }

    public ServerStatistics GetStatisticsSnapshot()
    {
        lock (_sync)
        {
            return new ServerStatistics
            {
                Port = Port,
                TotalRequests = _totalRequests,
                GetRequests = _getRequests,
                PostRequests = _postRequests,
                AverageProcessingTimeMs = _totalRequests == 0 ? 0 : _totalProcessingTime / _totalRequests,
                StoredMessages = _messages.Count,
                LastStatusCode = _lastStatusCode
            };
        }
    }

    public IReadOnlyCollection<LoadBucket> GetMinuteLoad()
    {
        return GetBuckets(_minuteBuckets, "HH:mm");
    }

    public IReadOnlyCollection<LoadBucket> GetHourlyLoad()
    {
        return GetBuckets(_hourBuckets, "dd.MM HH:00");
    }

    private async Task ListenLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _listener is not null)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => ProcessContextAsync(context), token);
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
    }

    private async Task ProcessContextAsync(HttpListenerContext context)
    {
        var startedAt = DateTime.Now;
        var request = context.Request;
        var statusCode = 200;
        var requestBody = string.Empty;
        var responseBody = string.Empty;

        try
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8);
            requestBody = await reader.ReadToEndAsync();

            if (request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                responseBody = await HandleGetAsync(request);
                statusCode = 200;
            }
            else if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                var result = await HandlePostAsync(requestBody);
                responseBody = result.Body;
                statusCode = result.StatusCode;
            }
            else
            {
                statusCode = 405;
                responseBody = JsonSerializer.Serialize(new { error = "Метод не поддерживается" }, JsonOptions());
            }
        }
        catch (JsonException ex)
        {
            statusCode = 400;
            responseBody = JsonSerializer.Serialize(new { error = $"Некорректный JSON: {ex.Message}" }, JsonOptions());
        }
        catch (Exception ex)
        {
            statusCode = 500;
            responseBody = JsonSerializer.Serialize(new { error = ex.Message }, JsonOptions());
        }

        var duration = (DateTime.Now - startedAt).TotalMilliseconds;
        await WriteResponseAsync(context.Response, statusCode, responseBody);
        RegisterRequest(request.HttpMethod, statusCode, duration);

        _logger.Log(new LogEntry
        {
            Direction = "Входящий",
            Method = request.HttpMethod,
            Url = request.Url?.ToString() ?? string.Empty,
            Headers = request.Headers.ToString() ?? string.Empty,
            RequestBody = requestBody,
            ResponseBody = responseBody,
            StatusCode = statusCode,
            DurationMilliseconds = duration
        });
    }

    private Task<string> HandleGetAsync(HttpListenerRequest request)
    {
        var snapshot = GetStatisticsSnapshot();
        var messages = _messages.Values
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new { item.Id, item.Message, item.CreatedAt });

        var payload = new
        {
            server = new
            {
                port = snapshot.Port,
                totalRequests = snapshot.TotalRequests,
                getRequests = snapshot.GetRequests,
                postRequests = snapshot.PostRequests,
                averageProcessingTimeMs = Math.Round(snapshot.AverageProcessingTimeMs, 2),
                uptime = GetUptime().ToString(@"hh\:mm\:ss")
            },
            path = request.Url?.AbsolutePath ?? "/",
            messages
        };

        return Task.FromResult(JsonSerializer.Serialize(payload, JsonOptions()));
    }

    private Task<(int StatusCode, string Body)> HandlePostAsync(string requestBody)
    {
        var dto = JsonSerializer.Deserialize<MessagePayload>(requestBody, JsonOptions());
        if (dto is null || string.IsNullOrWhiteSpace(dto.Message))
        {
            return Task.FromResult((400, JsonSerializer.Serialize(new { error = "Поле message обязательно" }, JsonOptions())));
        }

        var record = new MessageRecord
        {
            Id = Guid.NewGuid(),
            Message = dto.Message.Trim(),
            CreatedAt = DateTime.Now
        };

        _messages[record.Id] = record;
        MessageReceived?.Invoke(record);

        var payload = JsonSerializer.Serialize(new
        {
            id = record.Id,
            savedAt = record.CreatedAt,
            message = record.Message
        }, JsonOptions());

        return Task.FromResult((201, payload));
    }

    private async Task WriteResponseAsync(HttpListenerResponse response, int statusCode, string body)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/json; charset=utf-8";
        var bytes = Encoding.UTF8.GetBytes(body);
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes);
        response.OutputStream.Close();
    }

    private void RegisterRequest(string method, int statusCode, double durationMs)
    {
        lock (_sync)
        {
            _totalRequests++;
            _totalProcessingTime += durationMs;
            _lastStatusCode = statusCode;

            if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                _getRequests++;
            }
            else if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                _postRequests++;
            }

            var now = DateTime.Now;
            var minuteKey = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            var hourKey = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            _minuteBuckets.AddOrUpdate(minuteKey, 1, (_, current) => current + 1);
            _hourBuckets.AddOrUpdate(hourKey, 1, (_, current) => current + 1);
            TrimBuckets(now);
        }

        StatisticsUpdated?.Invoke(GetStatisticsSnapshot());
    }

    private IReadOnlyCollection<LoadBucket> GetBuckets(ConcurrentDictionary<DateTime, int> source, string format)
    {
        return source
            .OrderBy(pair => pair.Key)
            .Select(pair => new LoadBucket
            {
                Label = pair.Key.ToString(format),
                Count = pair.Value
            })
            .ToList();
    }

    private void TrimBuckets(DateTime now)
    {
        foreach (var minute in _minuteBuckets.Keys.Where(key => key < now.AddMinutes(-59)).ToList())
        {
            _minuteBuckets.TryRemove(minute, out _);
        }

        foreach (var hour in _hourBuckets.Keys.Where(key => key < now.AddHours(-23)).ToList())
        {
            _hourBuckets.TryRemove(hour, out _);
        }
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    private class MessagePayload
    {
        public string? Message { get; set; }
    }
}

