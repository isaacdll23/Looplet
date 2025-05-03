using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Looplet.API.Models;

public class Worker
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string Alias { get; set; } = default!;
    public string BaseUrl { get; set; } = default!;
    public string Hostname { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
