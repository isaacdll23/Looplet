using Looplet.Abstractions.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Looplet.Abstractions.Repositories;

public interface IJobInstanceRepository
{
    Task<JobInstance> CreateJobInstaceAsync(JobInstance jobInstance);

    Task<JobInstance> GetAsync(string id);
    Task<List<JobInstance>> ListAsync();
    Task<JobInstance> UpdateAsync(JobInstance jobInstance);
    Task DeleteAsync(string id);
}

public class JobInstanceRepository(IMongoDatabase database) : IJobInstanceRepository
{
    private readonly IMongoCollection<JobInstance> _collection = database.GetCollection<JobInstance>("JobInstances");

    public async Task<JobInstance> CreateJobInstaceAsync(JobInstance jobInstance)
    {
        await _collection.InsertOneAsync(jobInstance, null);
        return jobInstance;
    }

    public async Task<JobInstance> GetAsync(string id)
    {
        var filter = Builders<JobInstance>.Filter.Eq(j => j.Id, ObjectId.Parse(id));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<JobInstance>> ListAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<JobInstance> UpdateAsync(JobInstance jobInstance)
    {
        var filter = Builders<JobInstance>.Filter.Eq(j => j.Id, jobInstance.Id);
        await _collection.ReplaceOneAsync(filter, jobInstance, new ReplaceOptions { IsUpsert = false });
        return jobInstance;
    }

    public async Task DeleteAsync(string id)
    {
        var filter = Builders<JobInstance>.Filter.Eq(j => j.Id, ObjectId.Parse(id));
        await _collection.DeleteOneAsync(filter);
    }

}
