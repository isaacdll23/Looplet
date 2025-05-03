using System.ComponentModel.DataAnnotations;

namespace Looplet.Abstractions.Models.DTOs;

public class PluginModuleDto
{
    public string Name { get; set; } = default!;
    public string Hostname { get; set; } = default!;
}
