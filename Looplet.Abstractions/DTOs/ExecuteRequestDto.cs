using System.Text.Json;

namespace Looplet.Abstractions.DTOs;

public class ExecuteRequestDto
{
    public string InstanceId { get; set; } = default!;
    public string JobType { get; set; } = default!;
    public JsonElement? Parameters { get; set; }
}
