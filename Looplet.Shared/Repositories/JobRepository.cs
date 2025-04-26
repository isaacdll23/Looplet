using Looplet.Shared.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Looplet.Shared.Repositories;


public interface IJobRepository
{
    Task<JobDefinition> CreateAsync(JobDefinition jobDefinition);
    Task<JobDefinition> GetAsync(string id);
    Task<List<JobDefinition>> ListAsync();
    Task<JobDefinition> UpdateAsync(JobDefinition jobDefinition);
    Task DeleteAsync(string id);
}
public class JobRepository(IMongoDatabase database) : IJobRepository
{
    private readonly IMongoCollection<JobDefinition> _collection = database.GetCollection<JobDefinition>("Jobs");

    public async Task<JobDefinition> CreateAsync(JobDefinition jobDefinition)
    {
        await _collection.InsertOneAsync(jobDefinition, null);
        return jobDefinition;
    }

    public async Task<JobDefinition> GetAsync(string id)
    {
        var filter = Builders<JobDefinition>.Filter.Eq(j => j.Id, ObjectId.Parse(id));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<JobDefinition>> ListAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<JobDefinition> UpdateAsync(JobDefinition jobDefinition)
    {
        var filter = Builders<JobDefinition>.Filter.Eq(j => j.Id, jobDefinition.Id);
        await _collection.ReplaceOneAsync(filter, jobDefinition, new ReplaceOptions { IsUpsert = false });
        return jobDefinition;
    }

    public async Task DeleteAsync(string id)
    {
        var filter = Builders<JobDefinition>.Filter.Eq(j => j.Id, ObjectId.Parse(id));
        await _collection.DeleteOneAsync(filter);
    }
}
