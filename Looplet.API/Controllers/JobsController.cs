using Looplet.Abstractions.Interfaces;
using Looplet.Abstractions.Models;
using Looplet.Abstractions.Repositories;
using Looplet.Abstractions.Static;
using Looplet.API.Infrastructure;
using Looplet.API.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Looplet.API.Controllers;

public class JobsController(IJobDefinitionRepository jobRepository) : ControllerBase
{
    [HttpGet]
    [Route("api/jobs")]
    public async Task<IActionResult> List()
    {
        var jobs = await jobRepository.ListAsync();
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
        var existingJobs = await jobRepository.ListAsync();
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

        var createdJob = await jobRepository.CreateAsync(jobDefinition);


        return CreatedAtAction(nameof(Create), new { id = createdJob.Id.ToString() });
    }

    [HttpGet]
    [Route("api/jobs/types")]
    public IActionResult GetJobTypes()
    {
        // return the list of available job from workers

        return Ok();
    }
}
