using Looplet.Abstractions.Models.DTOs;
using Looplet.Hub.Features.Workers.Models;
using Looplet.Hub.Features.Workers.Services;
using Microsoft.AspNetCore.Mvc;

namespace Looplet.Hub.Features.Modules.Controllers;

[ApiController]
public class WorkersController : ControllerBase
{
    private readonly IWorkerService _workerService;
    public WorkersController(IWorkerService workerService)
    {
        _workerService = workerService;
    }

    [HttpGet]
    [Route("api/workers")]
    public async Task<IActionResult> GetWorkers()
    {
        IEnumerable<Worker> workers = await _workerService.GetAllWorkersAsync();

        var workerDtos = workers.Select(w => new WorkerDto
        {
            Id = w.Id.ToString(),
            Alias = w.Alias,
            BaseUrl = w.BaseUrl,
            Hostname = w.Hostname,
            IsActive = w.IsActive,
            CreatedAt = w.CreatedAt
        }).ToList();

        return Ok(workers);
    }

    [HttpGet("{id}")]
    [Route("api/workers/{id}")]
    public async Task<IActionResult> GetWorkerById(string id)
    {
        Worker? worker = await _workerService.GetWorkerByIdAsync(id);
        if (worker == null)
        {
            return NotFound($"Worker with ID {id} not found.");
        }

        var workerDto = new WorkerDto
        {
            Id = worker.Id.ToString(),
            Alias = worker.Alias,
            BaseUrl = worker.BaseUrl,
            Hostname = worker.Hostname,
            IsActive = worker.IsActive,
            CreatedAt = worker.CreatedAt
        };
        return Ok(workerDto);
    }

    [HttpPost]
    [Route("api/workers/register")]
    public async Task<IActionResult> RegisterWorker([FromBody] RegisterWorkerDto request)
    {
        try
        {
            Worker worker = await _workerService.RegisterWorkerAsync(request.Alias, request.BaseUrl);
            var createdWorkerDto = new WorkerDto
            {
                Id = worker.Id.ToString(),
                Alias = worker.Alias,
                BaseUrl = worker.BaseUrl,
                Hostname = worker.Hostname,
                IsActive = worker.IsActive,
                CreatedAt = worker.CreatedAt
            };
            return Created(nameof(RegisterWorker), createdWorkerDto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
