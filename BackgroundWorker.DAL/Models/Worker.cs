using MongoDB.Bson;

namespace BackgroundWorker.DAL.Models;

public class Worker
{
    public ObjectId Id { get; set; }
    public string Name { get; set; } = "";  
}
