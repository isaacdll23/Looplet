using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Looplet.Abstractions.Models;

public enum JobStatus { Pending, Running, Succeeded, Failed }

public class JobInstance
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId JobDefinitionId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public JobStatus Status { get; set; }
    public int Attempt { get; set; }
    public string? ErrorMessage { get; set; }
}
