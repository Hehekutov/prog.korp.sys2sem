namespace Prac3.Models;

public class ServerStatistics
{
    public int Port { get; init; }

    public int TotalRequests { get; init; }

    public int GetRequests { get; init; }

    public int PostRequests { get; init; }

    public double AverageProcessingTimeMs { get; init; }

    public int StoredMessages { get; init; }

    public int? LastStatusCode { get; init; }
}
