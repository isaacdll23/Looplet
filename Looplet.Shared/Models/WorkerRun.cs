using MongoDB.Bson;

namespace Looplet.Shared.Models;

public enum WorkerRunStatus
{
    Starting,
    Running,
    Completed,
    Failed
}
public class WorkerRun
{
    public ObjectId Id { get; set; }
    public ObjectId WorkerId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public WorkerRunStatus Status { get; set; } = WorkerRunStatus.Starting;
    public string? ErrorMessage { get; set; } // Optional, used if Status is Failed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}