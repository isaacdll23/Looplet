using Microsoft.AspNetCore.Mvc;

namespace Looplet.Hub.Infrastructure.Scheduling;

[ApiController]
[Route("api/[controller]")]
public class SchedulerController : ControllerBase
{
    private readonly SchedulerState _schedulerState;

    public SchedulerController(SchedulerState schedulerState)
    {
        _schedulerState = schedulerState;
    }

    [HttpGet("status")]
    public ActionResult<SchedulerState> GetSchedulerStatus()
    {
        return Ok(new { Status = _schedulerState.Enabled ? "Enabled" : "Disabled" });
    }

    [HttpPost("enable")]
    public IActionResult EnableScheduler()
    {
        _schedulerState.Enabled = true;
        return NoContent();
    }

    [HttpPost("disable")]
    public IActionResult DisableScheduler()
    {
        _schedulerState.Enabled = false;
        return NoContent();
    }
}
