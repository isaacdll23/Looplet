using System.Text.Json;
using Looplet.Abstractions.Interfaces;

namespace Looplet.SampleJobs;

public class SampleJob2Sec : IJob
{
    public async Task ExecuteAsync(JsonElement? parameters, CancellationToken cancellationToken)
    {
        // Simulates a 2-second job
        Console.WriteLine("SampleJob2Sec started.");
        await Task.Delay(2000, cancellationToken);
        Console.WriteLine("SampleJob2Sec finished.");
    }
}
