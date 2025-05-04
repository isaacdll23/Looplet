using System.Text.Json;
using Cronos;
using Looplet.Abstractions.DTOs;
using Looplet.Hub.Features.Jobs.Models;
using Looplet.Hub.Features.Jobs.Repositories;
using Looplet.Hub.Features.Workers.Models;
using Looplet.Hub.Features.Workers.Repositories;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;

namespace Looplet.Hub.Features.Scheduler.Services;

public class SchedulerService(
  IServiceScopeFactory _serviceScopeFactory,
  ILogger<SchedulerService> _logger,
  IHttpClientFactory _httpFactory,
  SchedulerState _schedulerState) : BackgroundService
{
    private readonly int _maxParallelJobs = 4;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _disabledPollInterval = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var sem = new SemaphoreSlim(_maxParallelJobs);
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!_schedulerState.Enabled)
            {
                _logger.LogInformation("Job scheduler is disabled. Standing By.");
                await Task.Delay(_disabledPollInterval, cancellationToken);
                continue;
            }

            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            IWorkerRepository workerRepository = scope.ServiceProvider.GetRequiredService<IWorkerRepository>();

            if (!_cache.TryGetValue("workers", out List<Worker>? workers))
            {
                workers = await workerRepository.GetAllAsync();
                if (workers.Count == 0)
                {
                    _logger.LogInformation("No workers found. Standing By.");
                    await Task.Delay(_disabledPollInterval, cancellationToken);
                    continue;
                }
                _cache.Set("workers", workers, _cacheExpiration);
            }

            _logger.LogInformation("Polling for due jobs.");
            IJobDefinitionRepository jobDefinitionRepository = scope.ServiceProvider.GetRequiredService<IJobDefinitionRepository>();
            IJobInstanceRepository jobInstanceRepository = scope.ServiceProvider.GetRequiredService<IJobInstanceRepository>();
            List<JobDefinition> allJobs = await jobDefinitionRepository.ListAsync();
            var toRun = allJobs
                                .Where(j => j.Enabled && j.NextRunAt <= DateTime.UtcNow)
                                .ToList();

            if (toRun.Count == 0)
            {
                _logger.LogInformation("No jobs to run.");
                await Task.Delay(_pollInterval, cancellationToken);
                continue;
            }

            _logger.LogInformation("Found {Count} jobs to run. Dispatching jobs.", toRun.Count);
            foreach (JobDefinition? jobDefinition in toRun)
            {
                await sem.WaitAsync(cancellationToken);
                _ = DispatchJobAsync(jobDefinition, sem, jobDefinitionRepository, jobInstanceRepository, cancellationToken)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            _logger.LogError(t.Exception, "Dispatch failed for {Job}", jobDefinition.Name);
                    }, TaskScheduler.Default);
            }

            await Task.Delay(_pollInterval, cancellationToken);
        }
    }

    private async Task DispatchJobAsync(
      JobDefinition jobDefinition,
      SemaphoreSlim semaphore,
      IJobDefinitionRepository jobDefinitionRepository,
      IJobInstanceRepository jobInstanceRepository,
      CancellationToken cancellationToken)
    {
        try
        {
            var worker = _cache.Get<List<Worker>>("workers")?.FirstOrDefault();
            if (worker == null)
            {
                _logger.LogError("Worker not found.");
                return;
            }

            HttpClient client = _httpFactory.CreateClient();
            HttpResponseMessage resp = await client.GetAsync($"{worker.BaseUrl}/plugins/jobs", cancellationToken);
            if (resp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Worker is not available. Status code: {StatusCode}", resp.StatusCode);
                return;
            }
            List<PluginJobDto>? workerResponse = await resp.Content.ReadFromJsonAsync<List<PluginJobDto>>(cancellationToken: cancellationToken);

            if (workerResponse == null)
            {
                _logger.LogError("Failed to get worker modules.");
                return;
            }

            var jobType = jobDefinition.JobType;
            if (!workerResponse.Any(m => m.Name.Equals(jobType, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogError("Job type {JobType} is not available on worker.", jobType);
                return;
            }

            // Create the JobInstance in Pending state
            var jobInstance = new JobInstance
            {
                JobDefinitionId = jobDefinition.Id,
                ScheduledAt = jobDefinition.NextRunAt!.Value,
                Status = JobStatus.Pending,
                Attempt = 1
            };
            await jobInstanceRepository.CreateJobInstaceAsync(jobInstance);

            // Compute next run for cron or disable
            if (!string.IsNullOrEmpty(jobDefinition.CronSchedule))
            {
                var expr = CronExpression.Parse(jobDefinition.CronSchedule,
                                                 CronFormat.IncludeSeconds);
                jobDefinition.NextRunAt = expr.GetNextOccurrence(DateTime.UtcNow)?
                                    .ToUniversalTime();
            }
            else
            {
                jobDefinition.Enabled = false;
            }
            await jobDefinitionRepository.UpdateAsync(jobDefinition);

            // Build ExecuteRequest DTO
            var request = new ExecuteRequestDto
            {
                InstanceId = jobInstance.Id.ToString(),
                JobType = jobDefinition.JobType,
                Parameters = jobDefinition.Parameters != null
                    ? JsonDocument.Parse(jobDefinition.Parameters.ToJson()).RootElement
                    : (JsonElement?) null
            };

            // POST to worker
            client = _httpFactory.CreateClient();
            resp = await client.PostAsJsonAsync(
                            $"{worker.BaseUrl}/execute", request, cancellationToken);

            if (resp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Failed to dispatch job {Job} to worker. Status code: {StatusCode}",
                    jobDefinition.Name, resp.StatusCode);

                // Update Job Instance status to Failed
                jobInstance.Status = JobStatus.Failed;
                jobInstance.ErrorMessage = $"Failed to dispatch job {jobDefinition.Name}";
                jobInstance.Attempt++;
                await jobInstanceRepository.UpdateAsync(jobInstance);
                return;
            }

            _logger.LogInformation("Job {Job} dispatched to worker.", jobDefinition.Name);

            // Mark instance as "Dispatched"
            jobInstance.Status = JobStatus.Running;
            await jobInstanceRepository.UpdateAsync(jobInstance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch job {Job}", jobDefinition.Name);
            // mark failure immediately
            var failInst = new JobInstance
            {
                JobDefinitionId = jobDefinition.Id,
                ScheduledAt = DateTime.UtcNow,
                Status = JobStatus.Failed,
                Attempt = 1,
                ErrorMessage = ex.Message
            };
            await jobInstanceRepository.CreateJobInstaceAsync(failInst);

            jobDefinition.Enabled = false;
            await jobDefinitionRepository.UpdateAsync(jobDefinition);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
