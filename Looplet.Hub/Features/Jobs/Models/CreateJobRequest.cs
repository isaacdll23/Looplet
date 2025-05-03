using System.Text.Json;

namespace Looplet.Hub.Features.Jobs.Models;

public class CreateJobRequest
{
    public string Name { get; set; } = default!;
    public string JobType { get; set; } = default!;
    public Dictionary<string, JsonElement>? Parameters { get; set; }
    public string? CronSchedule { get; set; }
    public DateTime? NextRunAt { get; set; }
    public bool? Enabled { get; set; }
    public int? MaxRetries { get; set; }
    public int? RetryBackoffSecs { get; set; }
}
