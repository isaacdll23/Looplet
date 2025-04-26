using Looplet.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Looplet.Jobs;

public class SampleConsoleJob(ILogger<SampleConsoleJob> logger) : IJob
{
    public Task ExecuteAsync(BsonDocument? parameters, CancellationToken cancellationToken)
    {
        Console.WriteLine("Hello from ConsoleWriteJob!");
        Console.WriteLine($"Current Time: {DateTime.UtcNow}");

        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                Console.WriteLine($"{parameter.Name}: {parameter.Value}");
                logger.LogInformation($"{parameter.Name}: {parameter.Value}");
            }
        }
        return Task.CompletedTask;
    }
}
