namespace Prac3.Models;

public class LogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.Now;

    public string Direction { get; init; } = "Входящий";

    public string Method { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public string Headers { get; init; } = string.Empty;

    public string RequestBody { get; init; } = string.Empty;

    public string ResponseBody { get; init; } = string.Empty;

    public int StatusCode { get; init; }

    public double DurationMilliseconds { get; init; }

    public string ToLongText()
    {
        return
            $"[{Timestamp:dd.MM.yyyy HH:mm:ss}] {Direction} {Method} {Url}{Environment.NewLine}" +
            $"Статус: {StatusCode}, обработка: {DurationMilliseconds:F2} мс{Environment.NewLine}" +
            $"Заголовки:{Environment.NewLine}{Headers}{Environment.NewLine}" +
            $"Тело запроса:{Environment.NewLine}{(string.IsNullOrWhiteSpace(RequestBody) ? "<пусто>" : RequestBody)}{Environment.NewLine}" +
            $"Тело ответа:{Environment.NewLine}{(string.IsNullOrWhiteSpace(ResponseBody) ? "<пусто>" : ResponseBody)}";
    }
}
