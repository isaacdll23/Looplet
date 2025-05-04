using Looplet.Abstractions.DTOs;
using Looplet.Hub.Features.Jobs.Repositories;
using Looplet.Hub.Features.Workers.Models;
using Looplet.Hub.Features.Workers.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Looplet.Hub.Features.Plugins.Controllers;
public class PluginsController : ControllerBase
{
    private readonly IWorkerRepository _workerRepository;

    public PluginsController(IWorkerRepository workerRepository)
    {
        _workerRepository = workerRepository;
    }

    [HttpGet]
    [Route("api/plugins")]
    public async Task<ActionResult<List<PluginDto>>> ListPlugins()
    {
        List<PluginDto> allPlugins = [];

        List<Worker> workers = await _workerRepository.GetAllAsync();

        foreach (var worker in workers)
        {
            try
            {
                // Assuming each worker node has an endpoint to list modules
                using var httpClient = new HttpClient { BaseAddress = new Uri(worker.BaseUrl) };
                HttpResponseMessage response = await httpClient.GetAsync("plugins");

                if (response.IsSuccessStatusCode)
                {
                    List<PluginDto>? plugins = await response.Content.ReadFromJsonAsync<List<PluginDto>>();
                    if (plugins != null)
                    {
                        allPlugins.AddRange(plugins);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching plugins from {worker.Alias} [{worker.BaseUrl}]: {ex.Message}");
            }
        }

        return Ok(allPlugins);
    }

    [HttpPost]
    [Route("api/plugins/upload")]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    public async Task<IActionResult> UploadPlugin([FromForm] string pluginName, [FromForm] bool singleFileUpload, [FromForm] IFormFile pluginFile)
    {
        if (singleFileUpload)
        {
            // Handle single file upload
            foreach (var worker in await _workerRepository.GetAllAsync())
            {
                using var httpClient = new HttpClient { BaseAddress = new Uri(worker.BaseUrl) };
                using var formContent = new MultipartFormDataContent();
                formContent.Add(new StringContent(pluginName), "pluginName");
                formContent.Add(new StreamContent(pluginFile.OpenReadStream()), "pluginFile", pluginFile.FileName);

                HttpResponseMessage response = await httpClient.PostAsync("plugins/upload", formContent);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int) response.StatusCode, $"Failed to upload plugin to {worker.Alias}. Error: {response.ReasonPhrase}");
                }
            }

            // TODO: Register the plugin in the database

            return Ok("Plugin uploaded successfully.");
        }

        // TODO: Handle multiple file upload (bundles of plugins and dependencies)
        throw new NotImplementedException();
    }
}
