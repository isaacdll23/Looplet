namespace Looplet.Abstractions.Models.DTOs;

public class HealthCheckResponseDto
{
    public string Status { get; set; } = "Healthy";
    public string Hostname { get; set; } = Environment.MachineName;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
