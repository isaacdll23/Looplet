using Looplet.Abstractions.DTOs;
using Looplet.Abstractions.Interfaces;
using Looplet.Worker.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Looplet.Worker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExecutionController : ControllerBase
{
    private readonly IJobFactory _jobFactory;
    private readonly ILogger<ExecutionController> _logger;

    public ExecutionController(IJobFactory jobFactory, ILogger<ExecutionController> logger)
    {
        _jobFactory = jobFactory;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Execute(
        [FromBody] ExecuteRequestDto req,
        CancellationToken cancellationToken)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.JobType))
            return BadRequest("JobType is required.");

        _logger.LogInformation(
            "Executing job {JobType} with parameters {Parameters}",
            req.JobType, req.Parameters);

        try
        {
            IJob job = _jobFactory.Create(req.JobType);
            await job.ExecuteAsync(req.Parameters, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobType} failed", req.JobType);
            return Problem(
                title: "Job execution error",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
