using System.ComponentModel.DataAnnotations;

namespace Looplet.Abstractions.DTOs;
public class WorkerDto
{
    public string Id { get; set; } = default!;
    public string Alias { get; set; } = default!;
    public string BaseUrl { get; set; } = default!;
    public string Hostname { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
