using Looplet.Hub.Infrastructure.Scheduling;
using Microsoft.AspNetCore.Mvc;

namespace Looplet.Hub.Controllers;

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
        return Ok(_schedulerState.Enabled);
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
        _schedulerState.Enabled = true;
        return NoContent();
    }
}
