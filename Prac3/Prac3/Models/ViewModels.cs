namespace Prac3.Models;

public record StatisticRow(string Name, string Value);

public class ChartPointViewModel
{
    public string Label { get; init; } = string.Empty;

    public int Count { get; init; }

    public double Height { get; init; }
}

public class MessageRecordViewModel
{
    public MessageRecordViewModel(MessageRecord record)
    {
        Id = record.Id;
        Message = record.Message;
        CreatedAtDisplay = record.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss");
    }

    public Guid Id { get; }

    public string Message { get; }

    public string CreatedAtDisplay { get; }
}

public class LogEntryViewModel
{
    public LogEntryViewModel(LogEntry entry)
    {
        TimestampDisplay = entry.Timestamp.ToString("dd.MM.yyyy HH:mm:ss");
        Direction = entry.Direction;
        Method = entry.Method;
        StatusCode = entry.StatusCode.ToString();
        Url = entry.Url;
    }

    public string TimestampDisplay { get; }

    public string Direction { get; }

    public string Method { get; }

    public string StatusCode { get; }

    public string Url { get; }
}
