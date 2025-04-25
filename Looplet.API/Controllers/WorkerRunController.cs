using Looplet.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

public class WorkerRunController : Controller
{

    private readonly IWorkerRunRepository _workerRunRepository;
    public WorkerRunController(IWorkerRunRepository workerRunRepository)
    {
        _workerRunRepository = workerRunRepository;
    }

    #region GET
    [HttpGet]
    [Route("api/workerruns")]
    public async Task<IActionResult> GetWorkerRuns()
    {
        var workerRuns = await _workerRunRepository.GetAllWorkerRunsAsync();
        var simplifiedWorkerRuns = workerRuns.Select(wr => new
        {
            Id = wr.Id.ToString(),
            WorkerId = wr.WorkerId.ToString(),
            StartTime = wr.StartTime,
            EndTime = wr.EndTime,
            Status = wr.Status.ToString(),
            ErrorMessage = wr.ErrorMessage,
            CreatedAt = wr.CreatedAt,
            UpdatedAt = wr.UpdatedAt
        });
        return Ok(simplifiedWorkerRuns);
    }

    [HttpGet]
    [Route("api/workerruns/{workerId}")]
    public async Task<IActionResult> GetWorkerRunsByWorkerId(string workerId)
    {
        if (!ObjectId.TryParse(workerId, out var objectId))
        {
            return BadRequest("Invalid ID format.");
        }

        var workerRuns = await _workerRunRepository.GetWorkerRunsByWorkerIdAsync(objectId);
        if (workerRuns == null)
        {
            return NotFound();
        }

        var simplifiedWorkerRuns = workerRuns.Select(wr => new
        {
            Id = wr.Id.ToString(),
            WorkerId = wr.WorkerId.ToString(),
            StartTime = wr.StartTime,
            EndTime = wr.EndTime,
            Status = wr.Status.ToString(),
            ErrorMessage = wr.ErrorMessage,
            CreatedAt = wr.CreatedAt,
            UpdatedAt = wr.UpdatedAt
        });
        return Ok(simplifiedWorkerRuns);
    }
    #endregion
}