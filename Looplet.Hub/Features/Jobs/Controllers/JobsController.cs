using Looplet.Abstractions.DTOs;
using Looplet.Hub.Features.Jobs.Models;
using Looplet.Hub.Features.Jobs.Repositories;
using Looplet.Hub.Infrastructure.Static;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Looplet.Hub.Features.Jobs.Controllers;

[ApiController]
public class JobsController(IJobDefinitionRepository jobRepository) : ControllerBase
{
    [HttpGet]
    [Route("api/jobs")]
    public async Task<IActionResult> List()
    {
        List<JobDefinition> jobs = await jobRepository.ListAsync();
        return Ok(jobs);
    }

    [HttpPost]
    [Route("api/jobs")]
    public async Task<IActionResult> Create([FromBody] CreateJobDto jobRequest)
    {
        // TODO: Review this to comply with new understanding of a lifecycle of a plugin/job.
        if (jobRequest == null)
        {
            return BadRequest("Job definition cannot be null.");
        }

        // Validate that a job with the same name does not already exist
        List<JobDefinition> existingJobs = await jobRepository.ListAsync();
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
                             JsonToBsonConverter.ConvertJsonElementToBsonValue(kv.Value)))),
            CronSchedule = jobRequest.CronSchedule,
            NextRunAt = jobRequest.NextRunAt ?? DateTime.UtcNow,
            Enabled = jobRequest.Enabled ?? true,
            MaxRetries = jobRequest.MaxRetries ?? 3,
            RetryBackoff = TimeSpan.FromSeconds(jobRequest.RetryBackoffSecs ?? 30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        JobDefinition createdJob = await jobRepository.CreateAsync(jobDefinition);

        return Created(nameof(Create), createdJob.Id.ToString());
    }
}
