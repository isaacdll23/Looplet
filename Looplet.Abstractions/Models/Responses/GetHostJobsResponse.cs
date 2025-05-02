namespace Looplet.Abstractions.Models.Responses;

public class GetHostJobsResponse
{
    public string Hostname { get; set; } = string.Empty;
    public string? HostAlias { get; set; }
    public List<string> Jobs { get; set; } = [];
}
