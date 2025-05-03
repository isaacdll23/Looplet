using Looplet.Abstractions.Models.DTOs;
using Looplet.Worker.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Looplet.Worker.Controllers;

[ApiController]
public class PluginsController : ControllerBase
{
    private readonly string _hostname = Environment.MachineName;

    [HttpGet]
    [Route("api/plugins/jobs")]
    public ActionResult<List<PluginJobDto>> ListJobs()
    {
        IReadOnlyList<string> jobs = PluginLoader.LoadedJobs;

        var response = jobs.Select(job => new PluginJobDto
        {
            Name = job,
            Hostname = _hostname
        }).ToList();

        return Ok(response);
    }

    [HttpGet]
    [Route("api/plugins/modules")]
    public ActionResult<List<PluginModuleDto>> ListModules()
    {
        IReadOnlyList<string> modules = PluginLoader.LoadedModules;

        var response = modules.Select(module => new PluginModuleDto
        {
            Name = module,
            Hostname = _hostname
        }).ToList();

        return Ok(response);
    }
}
