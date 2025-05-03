using System.Text.Json;
using MongoDB.Bson;

namespace Looplet.Abstractions.Interfaces;

public interface IJob
{
    Task ExecuteAsync(JsonElement? parameters, CancellationToken cancellationToken);
}
