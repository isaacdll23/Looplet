using System.Text.Json;
using Cronos;
using Looplet.Abstractions.Models;
using Looplet.Abstractions.Models.Requests;
using Looplet.Abstractions.Models.Responses;
using Looplet.Abstractions.Repositories;
using MongoDB.Bson;

namespace Looplet.API.Infrastructure.Scheduling;

public class JobSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobSchedulerService> _logger;
    private readonly IHttpClientFactory _httpFactory;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(1);
    private readonly int _maxParallelJobs = 4;
    private readonly Uri _workerBaseUri;
    private readonly Uri _callbackBaseUri;

    public JobSchedulerService(
      IServiceProvider serviceProvider,
      ILogger<JobSchedulerService> logger,
      IHttpClientFactory httpFactory,
      IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _httpFactory = httpFactory;


        // Read configuration values
        if (config["SchedulerConfiguration:WorkerBaseUri"] == null)
            throw new ArgumentNullException("WorkerBaseUri is not configured.");
        if (config["SchedulerConfiguration:CallbackBaseUri"] == null)
            throw new ArgumentNullException("CallbackBaseUri is not configured.");

        _workerBaseUri = new Uri(config["SchedulerConfiguration:WorkerBaseUri"]!);
        _callbackBaseUri = new Uri(config["SchedulerConfiguration:CallbackBaseUri"]!);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var sem = new SemaphoreSlim(_maxParallelJobs);
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("Polling for due jobs at {Now}", DateTime.UtcNow);
            using var scope = _serviceProvider.CreateScope();
            var jobDefinitionRepository = scope.ServiceProvider.GetRequiredService<IJobDefinitionRepository>();
            var jobInstanceRepository = scope.ServiceProvider.GetRequiredService<IJobInstanceRepository>();
            var allJobs = await jobDefinitionRepository.ListAsync();
            var toRun = allJobs
                                .Where(j => j.Enabled && j.NextRunAt <= DateTime.UtcNow)
                                .ToList();

            foreach (var jobDefinition in toRun)
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
            // Validate job definition is available on worker
            // make get request to worker
            var client = _httpFactory.CreateClient();
            var resp = await client.GetAsync($"{_workerBaseUri}/host/jobs", cancellationToken);
            if (resp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Worker is not available. Status code: {StatusCode}", resp.StatusCode);
                return;
            }
            var workerResponse = await resp.Content.ReadFromJsonAsync<GetHostJobsResponse>(cancellationToken: cancellationToken);

            if (workerResponse == null)
            {
                _logger.LogError("Failed to get worker modules.");
                return;
            }

            var jobType = jobDefinition.JobType;
            if (!workerResponse.Jobs.Any(m => m.Equals(jobType, StringComparison.OrdinalIgnoreCase)))
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
            var request = new ExecuteRequest
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
                            $"{_workerBaseUri}/execute", request, cancellationToken);

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

            // Mark instance as “Dispatched”
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
