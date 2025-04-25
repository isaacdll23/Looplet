using MongoDB.Bson;

namespace BackgroundWorker.DAL.Models;

public class Worker
{
    public ObjectId Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsEnabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 1000;
}
