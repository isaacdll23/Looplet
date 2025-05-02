namespace Looplet.Abstractions.Models.Responses;

public class GetHostModulesResponse
{
    public string Hostname { get; set; } = string.Empty;
    public string? HostAlias { get; set; }
    public List<string> Modules { get; set; } = [];
}
