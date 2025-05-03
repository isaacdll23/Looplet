using Looplet.API.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Looplet.API.Repositories;

public interface IWorkerRepository
{
    Task<Worker?> GetByAliasAsync(string alias);
    Task<Worker?> GetByIdAsync(string id);
    Task<Worker> CreateAsync(Worker worker);
    Task<IEnumerable<Worker>> GetAllAsync();
    Task UpdateAsync(Worker worker);
    Task DeleteAsync(string id);
}

public class WorkerRepository : IWorkerRepository
{
    private readonly IMongoCollection<Worker> _workers;

    public WorkerRepository(IMongoDatabase database)
    {
        _workers = database.GetCollection<Worker>("Workers");
    }

    public async Task<Worker?> GetByAliasAsync(string alias)
    {
        return await _workers.Find(w => w.Alias == alias).FirstOrDefaultAsync();
    }

    public async Task<Worker?> GetByIdAsync(string id)
    {
        return await _workers.Find(w => w.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
    }

    public async Task<Worker> CreateAsync(Worker worker)
    {
        await _workers.InsertOneAsync(worker);
        return worker;
    }

    public async Task<IEnumerable<Worker>> GetAllAsync()
    {
        return await _workers.Find(FilterDefinition<Worker>.Empty).ToListAsync();
    }

    public async Task UpdateAsync(Worker worker)
    {
        await _workers.ReplaceOneAsync(w => w.Id == worker.Id, worker);
    }

    public async Task DeleteAsync(string id)
    {
        await _workers.DeleteOneAsync(w => w.Id == ObjectId.Parse(id));
    }
}
