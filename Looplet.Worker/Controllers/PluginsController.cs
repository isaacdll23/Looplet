using Looplet.Abstractions.DTOs;
using Looplet.Worker.Infrastructure.Features.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace Looplet.Worker.Controllers;

[ApiController]
public class PluginsController(ILogger<PluginsController> _logger) : ControllerBase
{
    private readonly string _hostname = Environment.MachineName;
    private const string _pluginsDirectory = "Plugins";

    [HttpGet]
    [Route("api/plugins")]
    public ActionResult<List<PluginModuleDto>> ListPlugins()
    {
        var plugins = PluginLoader.GetAvailablePlugins();

        var response = plugins.Select(plugin => new PluginModuleDto
        {
            Name = plugin,
            Hostname = _hostname
        }).ToList();

        return Ok(response);
    }

    [HttpGet]
    [Route("api/plugins/{pluginName}/jobs")]
    public ActionResult<List<PluginJobDto>> ListJobs(string pluginName)
    {
        var jobs = PluginLoader.GetAvailableJobs(pluginName);

        if (jobs.Count == 0)
        {
            _logger.LogInformation("Plugin not found: {PluginName}", pluginName);
            return NotFound($"Plugin '{pluginName}' not found.");
        }

        var response = jobs.Select(job => new PluginJobDto
        {
            Name = job,
            Hostname = _hostname
        }).ToList();

        return Ok(response);
    }

    [HttpPost]
    [Route("api/plugins/upload")]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    public async Task<IActionResult> UploadPlugin([FromForm] string pluginName, [FromForm] IFormFile pluginFile)
    {
        if (pluginFile == null || pluginFile.Length == 0)
        {
            _logger.LogInformation("Bad Request: No file uploaded.");
            return BadRequest("No file uploaded.");
        }

        var fileExtension = Path.GetExtension(pluginFile.FileName).ToLowerInvariant();
        if (fileExtension != ".dll")
        {
            _logger.LogInformation("Bad Request: Invalid File Type Uploaded");
            return BadRequest("Only .dll files are allowed.");
        }


        var pluginsPath = Path.Combine(AppContext.BaseDirectory, _pluginsDirectory);
        if (!Directory.Exists(pluginsPath))
        {
            _logger.LogInformation("Plugins directory not found at runtime. Creating directory at {PluginsPath}", pluginsPath);
            Directory.CreateDirectory(pluginsPath);
        }


        // Create a subdirectory for the plugin
        var pluginDir = Path.Combine(pluginsPath, pluginName);
        if (!Path.Exists(pluginDir))
        {
            _logger.LogInformation("Plugin directory not found: {PluginDir}. Creating directory.", pluginDir);
            Directory.CreateDirectory(pluginDir);
        }

        var filePath = Path.Combine(pluginDir, pluginFile.FileName);
        if (System.IO.File.Exists(filePath))
        {
            _logger.LogInformation("File already exists: {FilePath}", filePath);
            return Conflict("File already exists.");
        }

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await pluginFile.CopyToAsync(stream);
        }
        _logger.LogInformation("File uploaded successfully: {FilePath}", filePath);

        return Ok("File uploaded successfully.");
    }
}
