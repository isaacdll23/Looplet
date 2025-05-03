using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace Looplet.Abstractions.DTOs;

public class JobDefitionDto
{
    [Required]
    public string Id { get; set; } = default!;
    [Required]
    public string Alias { get; set; } = default!;
    public JsonObject? Parameters { get; set; } = null;
    public string? CronSchedule { get; set; } = null;
    public bool Enabled { get; set; } = true;
}
