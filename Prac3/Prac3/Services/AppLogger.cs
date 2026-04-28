using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Prac3.Models;

namespace Prac3.Services;

public class AppLogger
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public AppLogger()
    {
        LogFilePath = Path.Combine(AppContext.BaseDirectory, "logs.txt");
    }

    public string LogFilePath { get; }

    public event Action<LogEntry>? LogCreated;

    public void Log(LogEntry entry)
    {
        _entries.Enqueue(entry);
        _ = AppendToFileAsync(entry);
        LogCreated?.Invoke(entry);
    }

    public IReadOnlyCollection<LogEntry> GetEntries()
    {
        return _entries.ToArray();
    }

    public void ClearInMemory()
    {
        while (_entries.TryDequeue(out _))
        {
        }
    }

    private async Task AppendToFileAsync(LogEntry entry)
    {
        await _fileLock.WaitAsync();
        try
        {
            var payload = entry.ToLongText() + Environment.NewLine + new string('=', 120) + Environment.NewLine;
            await File.AppendAllTextAsync(LogFilePath, payload, Encoding.UTF8);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
