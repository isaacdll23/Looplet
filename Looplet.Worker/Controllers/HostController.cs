using Looplet.Abstractions.Models.Responses;
using Looplet.Worker.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Looplet.Worker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HostController : ControllerBase
{
    private readonly string _hostname;
    private readonly string? _hostAlias;

    public HostController(IConfiguration configuration)
    {
        _hostname = Environment.MachineName;
        _hostAlias = configuration["WorkerNodeConfiguration:Alias"];
    }

    [HttpGet]
    [Route("modules")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHostModules()
    {
        IReadOnlyList<string> modules = PluginLoader.LoadedModules;

        if (modules == null || !modules.Any())
        {
            return NotFound("No modules found");
        }

        var response = new GetHostModulesResponse
        {
            Hostname = _hostname,
            HostAlias = _hostAlias,
            Modules = modules.ToList()
        };

        return Ok(response);
    }

    [HttpGet]
    [Route("jobs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHostJobs()
    {
        IReadOnlyList<string> jobs = PluginLoader.LoadedJobs;

        if (jobs == null || !jobs.Any())
        {
            return NotFound("No jobs found.");
        }

        var response = new GetHostJobsResponse
        {
            Hostname = _hostname,
            HostAlias = _hostAlias,
            Jobs = jobs.ToList()
        };

        return Ok(response);
    }
}
