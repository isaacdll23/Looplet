using Looplet.Abstractions.DTOs;
using Looplet.Hub.Configuration;
using Looplet.Hub.Features.Jobs.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Looplet.Hub.Features.Modules.Controllers;
public class ModuleController : ControllerBase
{
    private readonly List<WorkerConfig> _workerConfigs;

    public ModuleController(IJobDefinitionRepository jobRepository, IConfiguration configuration)
    {
        _workerConfigs = configuration.GetSection("Workers").Get<List<WorkerConfig>>()!;
    }

    [HttpGet]
    [Route("api/modules")]
    public async Task<ActionResult<List<PluginModuleDto>>> ListModules()
    {
        List<PluginModuleDto>? allModules = new List<PluginModuleDto>();

        foreach (var worker in _workerConfigs)
        {
            try
            {
                // Assuming each worker node has an endpoint to list modules
                using var httpClient = new HttpClient { BaseAddress = new Uri(worker.BaseUrl) };
                HttpResponseMessage response = await httpClient.GetAsync("api/plugins/modules");

                if (response.IsSuccessStatusCode)
                {
                    List<PluginModuleDto>? modules = await response.Content.ReadFromJsonAsync<List<PluginModuleDto>>();
                    if (modules != null)
                    {
                        allModules.AddRange(modules);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching modules from {worker.Alias} [{worker.BaseUrl}]: {ex.Message}");
            }
        }

        return Ok(allModules);
    }

    [HttpPost]
    [Route("api/modules/{name}/verify")]
    public async Task<IActionResult> VerifyModule(string name, [FromBody] string alias)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(alias))
        {
            return BadRequest("Module name and alias are required.");
        }

        // Confirm the module exists in on the worker
        var worker = _workerConfigs.FirstOrDefault(w => w.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase));
        if (worker == null)
        {
            return NotFound($"Worker with alias '{alias}' not found.");
        }

        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(worker.BaseUrl) };
            HttpResponseMessage response = await httpClient.GetAsync($"api/plugins/modules/{name}");

            if (response.IsSuccessStatusCode)
            {
                return Ok($"Module '{name}' is available on worker '{alias}'.");
            }
            else
            {
                return NotFound($"Module '{name}' not found on worker '{alias}'.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error verifying module: {ex.Message}");
        }
    }

}
