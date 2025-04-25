using Looplet.Shared.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Looplet.DAL.Repositories;

public interface IWorkerRunRepository
{
    Task<IEnumerable<WorkerRun>> GetAllWorkerRunsAsync();
    Task<IEnumerable<WorkerRun>> GetWorkerRunsByWorkerIdAsync(ObjectId workerId);
    Task AddWorkerRunAsync(WorkerRun workerRun);
    Task UpdateWorkerRunAsync(WorkerRun workerRun);
    Task DeleteWorkerRunAsync(ObjectId id);
}

public class WorkerRunRepository : IWorkerRunRepository
{
    private readonly IMongoCollection<WorkerRun> _workerRuns;

    public WorkerRunRepository(IMongoDatabase database)
    {
        _workerRuns = database.GetCollection<WorkerRun>("WorkerRuns");
    }

    public async Task<IEnumerable<WorkerRun>> GetAllWorkerRunsAsync()
    {
        return await _workerRuns.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<WorkerRun>> GetWorkerRunsByWorkerIdAsync(ObjectId workerId)
    {
        return await _workerRuns.Find(run => run.WorkerId == workerId).ToListAsync();
    }

    public async Task AddWorkerRunAsync(WorkerRun workerRun)
    {
        await _workerRuns.InsertOneAsync(workerRun);
    }

    public async Task UpdateWorkerRunAsync(WorkerRun workerRun)
    {
        await _workerRuns.ReplaceOneAsync(r => r.Id == workerRun.Id, workerRun);
    }

    public async Task DeleteWorkerRunAsync(ObjectId id)
    {
        await _workerRuns.DeleteOneAsync(run => run.Id == id);
    }
}