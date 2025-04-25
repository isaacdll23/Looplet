using Looplet.DAL.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Looplet.DAL.Repositories;

public interface IWorkerRepository
{
    Task<IEnumerable<Worker>> GetAllWorkersAsync();
    Task<Worker> GetWorkerByIdAsync(ObjectId id);
    Task AddWorkerAsync(Worker worker);
    Task UpdateWorkerAsync(Worker worker);
    Task DeleteWorkerAsync(ObjectId id);
}

public class WorkerRepository : IWorkerRepository
{
    private readonly IMongoCollection<Worker> _workers;

    public WorkerRepository(IMongoDatabase database)
    {
        _workers = database.GetCollection<Worker>("Workers");
    }

    public async Task<IEnumerable<Worker>> GetAllWorkersAsync()
    {
        return await _workers.Find(_ => true).ToListAsync();
    }

    public async Task<Worker> GetWorkerByIdAsync(ObjectId id)
    {
        return await _workers.Find(worker => worker.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddWorkerAsync(Worker worker)
    {
        await _workers.InsertOneAsync(worker);
    }
    public async Task UpdateWorkerAsync(Worker worker)
    {
        await _workers.ReplaceOneAsync(w => w.Id == worker.Id, worker);
    }

    public async Task DeleteWorkerAsync(ObjectId id)
    {
        await _workers.DeleteOneAsync(worker => worker.Id == id);
    }
}
