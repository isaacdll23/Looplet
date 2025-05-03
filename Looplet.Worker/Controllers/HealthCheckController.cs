using Microsoft.AspNetCore.Mvc;

namespace Looplet.Worker.Controllers;

public class HealthCheckController : ControllerBase
{
    [HttpGet]
    [Route("api/health")]
    public IActionResult CheckHealth()
    {
        return Ok(new
        {
            status = "Healthy",
            hostname = Environment.MachineName,
            timestamp = DateTime.UtcNow
        });
    }
}
