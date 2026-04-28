namespace Prac3.Models;

public class MessageRecord
{
    public Guid Id { get; init; }

    public string Message { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; } = DateTime.Now;
}
