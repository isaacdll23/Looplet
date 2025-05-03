using Looplet.Abstractions.Interfaces;
using Looplet.Abstractions.Models.Requests;
using Looplet.Abstractions.Static;
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
        [FromBody] ExecuteRequest req,
        CancellationToken cancellationToken)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.JobType))
            return BadRequest("JobType is required.");


        BsonDocument? parameters = JsonToBsonConverter.ConvertJsonElementToBsonDocument(req.Parameters);

        _logger.LogInformation(
            "Executing job {JobType} with parameters {Parameters}",
            req.JobType, parameters);

        try
        {
            IJob job = _jobFactory.Create(req.JobType);
            await job.ExecuteAsync(parameters, cancellationToken);
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
