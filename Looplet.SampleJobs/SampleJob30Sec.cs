using System.Text.Json;
using Looplet.Abstractions.Interfaces;

namespace Looplet.SampleJobs;

public class SampleJob30Sec : IJob
{
    public async Task ExecuteAsync(JsonElement? parameters, CancellationToken cancellationToken)
    {
        // Simulates a 30-second job
        Console.WriteLine("SampleJob30Sec started.");
        await Task.Delay(30000, cancellationToken);
        Console.WriteLine("SampleJob30Sec finished.");
    }
}
