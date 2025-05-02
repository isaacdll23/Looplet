using MongoDB.Bson;

namespace Looplet.Abstractions.Interfaces;

public interface IJob
{
    Task ExecuteAsync(BsonDocument? parameters, CancellationToken cancellationToken);
}
