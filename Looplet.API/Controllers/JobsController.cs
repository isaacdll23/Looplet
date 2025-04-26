using Looplet.API.Models;
using Looplet.Shared.Models;
using Looplet.Shared.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Looplet.API.Controllers;

public class JobsController : Controller
{
    private readonly IJobRepository _jobRepository;
    public JobsController(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    [HttpGet]
    [Route("api/jobs")]
    public async Task<IActionResult> List()
    {
        var jobs = await _jobRepository.ListAsync();
        return Ok(jobs);
    }

    [HttpPost]
    [Route("api/jobs")]
    public async Task<IActionResult> Create([FromBody] CreateJobRequest jobRequest)
    {
        if (jobRequest == null)
        {
            return BadRequest("Job definition cannot be null.");
        }

        // Validate that a job with the same name does not already exist
        var existingJobs = await _jobRepository.ListAsync();
        if (existingJobs.Any(j => j.Name.Equals(jobRequest.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Conflict($"A job with the name '{jobRequest.Name}' already exists.");
        }

        var jobDefinition = new JobDefinition
        {
            Name = jobRequest.Name,
            JobType = jobRequest.JobType,
            Parameters = jobRequest.Parameters is null
                   ? null
                   : new BsonDocument(
                       jobRequest.Parameters
                          .Select(kv => new BsonElement(
                             kv.Key,
                             kv.Value.ToBson()))),
            CronSchedule = jobRequest.CronSchedule,
            NextRunAt = jobRequest.NextRunAt ?? DateTime.UtcNow,
            Enabled = jobRequest.Enabled ?? true,
            MaxRetries = jobRequest.MaxRetries ?? 3,
            RetryBackoff = TimeSpan.FromSeconds(jobRequest.RetryBackoffSecs ?? 30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdJob = await _jobRepository.CreateAsync(jobDefinition);


        return CreatedAtAction(nameof(Create), new { id = createdJob.Id.ToString() });
    }
}
