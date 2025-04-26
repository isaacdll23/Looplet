using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Looplet.Shared.Models;

public class JobDefinition
{
    [BsonId] public ObjectId Id { get; set; }
    public string Name { get; set; } = default!;
    public string JobType { get; set; } = default!;
    public BsonDocument? Parameters { get; set; }
    public string? CronSchedule { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTime? NextRunAt { get; set; }
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RetryBackoff { get; set; } = TimeSpan.FromSeconds(30);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
