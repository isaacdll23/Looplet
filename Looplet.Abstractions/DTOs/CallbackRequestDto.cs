namespace Looplet.Abstractions.DTOs;

public class CallbackRequestDto
{
    public string JobInstanceId { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? ErrorMessage { get; set; }
    public DateTime FinishedAt { get; set; } = DateTime.UtcNow;
}
