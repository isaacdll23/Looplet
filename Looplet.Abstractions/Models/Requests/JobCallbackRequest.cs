namespace Looplet.Abstractions.Models.Requests;

public class JobCallbackRequest
{
    public string JobInstanceId { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? ErrorMessage { get; set; }
    public DateTime FinishedAt { get; set; } = DateTime.UtcNow;
}
