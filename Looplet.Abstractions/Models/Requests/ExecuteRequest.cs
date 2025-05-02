using System.Text.Json;

namespace Looplet.Abstractions.Models.Requests;

public class ExecuteRequest
{
    public string InstanceId { get; set; } = default!;
    public string JobType { get; set; } = default!;
    public JsonElement? Parameters { get; set; }
}
