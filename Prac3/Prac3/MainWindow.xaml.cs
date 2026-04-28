using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Prac3.Models;
using Prac3.Services;

namespace Prac3;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly AppLogger _logger;
    private readonly HttpMonitorServer _server;
    private readonly DispatcherTimer _uptimeTimer;
    private string _serverPort = "8080";
    private string _serverStatusText = "Сервер остановлен";
    private string _clientUrl = "https://jsonplaceholder.typicode.com/posts";
    private string _selectedClientMethod = "GET";
    private string _clientRequestBody = "{\n  \"message\": \"Привет от клиента\"\n}";
    private string _clientResponseText = "Здесь будет отображаться ответ сервера.";
    private string _logText = string.Empty;
    private string _uptimeText = "Время работы: 00:00:00";
    private string _selectedLogMethodFilter = "Все";
    private string _selectedLogStatusFilter = "Все";
    private string _selectedChartMode = "По минутам";

    public MainWindow()
    {
        InitializeComponent();

        _logger = new AppLogger();
        _server = new HttpMonitorServer(_logger);
        _server.LogCreated += OnLogCreated;
        _server.MessageReceived += OnMessageReceived;
        _server.StatisticsUpdated += OnStatisticsUpdated;

        HttpMethods = new ObservableCollection<string> { "GET", "POST" };
        LogMethodFilters = new ObservableCollection<string> { "Все", "GET", "POST" };
        LogStatusFilters = new ObservableCollection<string> { "Все", "200", "201", "400", "404", "405", "500" };
        ChartModes = new ObservableCollection<string> { "По минутам", "По часам" };
        StatisticRows = new ObservableCollection<StatisticRow>();
        ChartPoints = new ObservableCollection<ChartPointViewModel>();
        ReceivedMessages = new ObservableCollection<MessageRecordViewModel>();
        FilteredLogs = new ObservableCollection<LogEntryViewModel>();

        DataContext = this;
        LogFilePathText = $"Лог-файл: {_logger.LogFilePath}";
        RefreshStatistics(_server.GetStatisticsSnapshot());
        RefreshChart();

        _uptimeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _uptimeTimer.Tick += (_, _) => UptimeText = $"Время работы: {_server.GetUptime():hh\\:mm\\:ss}";
        _uptimeTimer.Start();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> HttpMethods { get; }

    public ObservableCollection<string> LogMethodFilters { get; }

    public ObservableCollection<string> LogStatusFilters { get; }

    public ObservableCollection<string> ChartModes { get; }

    public ObservableCollection<StatisticRow> StatisticRows { get; }

    public ObservableCollection<ChartPointViewModel> ChartPoints { get; }

    public ObservableCollection<MessageRecordViewModel> ReceivedMessages { get; }

    public ObservableCollection<LogEntryViewModel> FilteredLogs { get; }

    public string ServerPort
    {
        get => _serverPort;
        set => SetProperty(ref _serverPort, value);
    }

    public string ServerStatusText
    {
        get => _serverStatusText;
        set
        {
            if (SetProperty(ref _serverStatusText, value))
            {
                OnPropertyChanged(nameof(ServerButtonText));
            }
        }
    }

    public string ServerButtonText => _server.IsRunning ? "Остановить сервер" : "Запустить сервер";

    public string ClientUrl
    {
        get => _clientUrl;
        set => SetProperty(ref _clientUrl, value);
    }

    public string SelectedClientMethod
    {
        get => _selectedClientMethod;
        set => SetProperty(ref _selectedClientMethod, value);
    }

    public string ClientRequestBody
    {
        get => _clientRequestBody;
        set => SetProperty(ref _clientRequestBody, value);
    }

    public string ClientResponseText
    {
        get => _clientResponseText;
        set => SetProperty(ref _clientResponseText, value);
    }

    public string LogText
    {
        get => _logText;
        set => SetProperty(ref _logText, value);
    }

    public string UptimeText
    {
        get => _uptimeText;
        set => SetProperty(ref _uptimeText, value);
    }

    public string LogFilePathText { get; private set; } = string.Empty;

    public string SelectedLogMethodFilter
    {
        get => _selectedLogMethodFilter;
        set => SetProperty(ref _selectedLogMethodFilter, value);
    }

    public string SelectedLogStatusFilter
    {
        get => _selectedLogStatusFilter;
        set => SetProperty(ref _selectedLogStatusFilter, value);
    }

    public string SelectedChartMode
    {
        get => _selectedChartMode;
        set
        {
            if (SetProperty(ref _selectedChartMode, value))
            {
                RefreshChart();
            }
        }
    }

    private async void ToggleServerButton_Click(object sender, RoutedEventArgs e)
    {
        if (_server.IsRunning)
        {
            _server.Stop();
            ServerStatusText = "Сервер остановлен";
            UptimeText = "Время работы: 00:00:00";
            return;
        }

        if (!int.TryParse(ServerPort, out var port) || port is < 1 or > 65535)
        {
            MessageBox.Show("Укажите корректный порт в диапазоне 1-65535.", "Некорректный порт",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await _server.StartAsync(port);
            ServerStatusText = $"Сервер работает на http://localhost:{port}/";
            UptimeText = $"Время работы: {_server.GetUptime():hh\\:mm\\:ss}";
            ClientUrl = $"http://localhost:{port}/";
            OnPropertyChanged(nameof(ServerButtonText));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Не удалось запустить сервер", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void SendRequestButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ClientUrl))
        {
            MessageBox.Show("Укажите URL запроса.", "URL не задан", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ClientResponseText = $"Отправка {SelectedClientMethod}-запроса на {ClientUrl}...";

        try
        {
            using var request = new HttpRequestMessage(new HttpMethod(SelectedClientMethod), ClientUrl);
            if (SelectedClientMethod == "POST")
            {
                request.Content = new StringContent(ClientRequestBody, Encoding.UTF8, "application/json");
            }

            var startedAt = DateTime.Now;
            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            ClientResponseText = $"Статус: {(int)response.StatusCode} {response.StatusCode}{Environment.NewLine}" +
                                 $"Время: {DateTime.Now:dd.MM.yyyy HH:mm:ss}{Environment.NewLine}" +
                                 $"Длительность: {(DateTime.Now - startedAt).TotalMilliseconds:F0} мс{Environment.NewLine}{Environment.NewLine}" +
                                 body;

            _logger.Log(new LogEntry
            {
                Direction = "Исходящий",
                Method = SelectedClientMethod,
                Url = ClientUrl,
                Headers = request.Headers.ToString() ?? string.Empty,
                RequestBody = SelectedClientMethod == "POST" ? ClientRequestBody : string.Empty,
                ResponseBody = body,
                StatusCode = (int)response.StatusCode,
                DurationMilliseconds = (DateTime.Now - startedAt).TotalMilliseconds
            });
        }
        catch (TaskCanceledException)
        {
            ClientResponseText = "Ошибка отправки запроса:\nИстекло время ожидания ответа (10 секунд).\n\nЕсли отправляешь на локальный сервер, сначала нажми 'Запустить сервер'. Если на внешний API, проверь интернет и доступность URL.";
            _logger.Log(new LogEntry
            {
                Direction = "Исходящий",
                Method = SelectedClientMethod,
                Url = ClientUrl,
                RequestBody = SelectedClientMethod == "POST" ? ClientRequestBody : string.Empty,
                ResponseBody = "Таймаут ожидания ответа",
                StatusCode = 408
            });
        }
        catch (Exception ex)
        {
            ClientResponseText = $"Ошибка отправки запроса:{Environment.NewLine}{ex.Message}";
            _logger.Log(new LogEntry
            {
                Direction = "Исходящий",
                Method = SelectedClientMethod,
                Url = ClientUrl,
                RequestBody = SelectedClientMethod == "POST" ? ClientRequestBody : string.Empty,
                ResponseBody = ex.Message,
                StatusCode = 500
            });
        }
    }

    private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshFilteredLogs();
    }

    private void ClearLogsButton_Click(object sender, RoutedEventArgs e)
    {
        _logger.ClearInMemory();
        LogText = string.Empty;
        FilteredLogs.Clear();
    }

    private void OnLogCreated(LogEntry entry)
    {
        Dispatcher.Invoke(() =>
        {
            var builder = new StringBuilder(LogText);
            if (builder.Length > 0)
            {
                builder.AppendLine(new string('-', 100));
            }

            builder.AppendLine(entry.ToLongText());
            LogText = builder.ToString();
            RefreshFilteredLogs();
        });
    }

    private void OnMessageReceived(MessageRecord message)
    {
        Dispatcher.Invoke(() =>
        {
            ReceivedMessages.Insert(0, new MessageRecordViewModel(message));
        });
    }

    private void OnStatisticsUpdated(ServerStatistics statistics)
    {
        Dispatcher.Invoke(() =>
        {
            RefreshStatistics(statistics);
            RefreshChart();
            ServerStatusText = _server.IsRunning
                ? $"Сервер работает на http://localhost:{statistics.Port}/"
                : "Сервер остановлен";
            OnPropertyChanged(nameof(ServerButtonText));
        });
    }

    private void RefreshStatistics(ServerStatistics statistics)
    {
        StatisticRows.Clear();
        StatisticRows.Add(new StatisticRow("Порт", statistics.Port == 0 ? "Не запущен" : statistics.Port.ToString()));
        StatisticRows.Add(new StatisticRow("Всего запросов", statistics.TotalRequests.ToString()));
        StatisticRows.Add(new StatisticRow("GET-запросов", statistics.GetRequests.ToString()));
        StatisticRows.Add(new StatisticRow("POST-запросов", statistics.PostRequests.ToString()));
        StatisticRows.Add(new StatisticRow("Среднее время обработки", $"{statistics.AverageProcessingTimeMs:F2} мс"));
        StatisticRows.Add(new StatisticRow("Сообщений сохранено", statistics.StoredMessages.ToString()));
        StatisticRows.Add(new StatisticRow("Последний статус", statistics.LastStatusCode?.ToString() ?? "Нет данных"));
        StatisticRows.Add(new StatisticRow("Время работы", _server.GetUptime().ToString(@"hh\:mm\:ss")));
    }

    private void RefreshChart()
    {
        var buckets = SelectedChartMode == "По часам"
            ? _server.GetHourlyLoad()
            : _server.GetMinuteLoad();

        var maxCount = Math.Max(1, buckets.Count == 0 ? 1 : buckets.Max(item => item.Count));
        ChartPoints.Clear();

        foreach (var bucket in buckets)
        {
            ChartPoints.Add(new ChartPointViewModel
            {
                Label = bucket.Label,
                Count = bucket.Count,
                Height = 20 + (bucket.Count / (double)maxCount * 140)
            });
        }
    }

    private void RefreshFilteredLogs()
    {
        var entries = _logger.GetEntries()
            .Where(entry => SelectedLogMethodFilter == "Все" || entry.Method.Equals(SelectedLogMethodFilter, StringComparison.OrdinalIgnoreCase))
            .Where(entry => SelectedLogStatusFilter == "Все" || entry.StatusCode.ToString() == SelectedLogStatusFilter)
            .OrderByDescending(entry => entry.Timestamp)
            .Select(entry => new LogEntryViewModel(entry))
            .ToList();

        FilteredLogs.Clear();
        foreach (var entry in entries)
        {
            FilteredLogs.Add(entry);
        }
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected override void OnClosed(EventArgs e)
    {
        _server.Stop();
        _httpClient.Dispose();
        base.OnClosed(e);
    }
}
