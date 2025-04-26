using Cronos;
using Looplet.Shared.Interfaces;
using Looplet.Shared.Models;
using Looplet.Shared.Repositories;

namespace Looplet.Controller.Infrastructure;

public class JobSchedulerService(IServiceProvider serviceProvider, ILogger<JobSchedulerService> logger) : BackgroundService
{
    private readonly TimeSpan _pollInterval = TimeSpan.FromMilliseconds(1000);
    private readonly int _maxParrallelJobs = 4;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var semaphore = new SemaphoreSlim(_maxParrallelJobs);

        while (!cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("JobSchedulerService is running at: {time}", DateTimeOffset.Now);
            using (var scope = serviceProvider.CreateScope())
            {
                var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                var now = DateTime.UtcNow;
                var allJobs = await jobRepository.ListAsync();
                var jobsToRun = allJobs
                    .Where(job => job.Enabled && job.NextRunAt <= now)
                    .ToList();

                logger.LogInformation("Found {JobCount} jobs to run", jobsToRun.Count);

                foreach (var job in jobsToRun)
                {
                    await semaphore.WaitAsync(cancellationToken);
                    logger.LogInformation("Running job {JobName} at {RunTime}", job.Name, now);
                    _ = RunJobAsync(job, semaphore, cancellationToken)
                        .ContinueWith(task =>
                        {
                            if (task.Exception != null)
                                logger.LogError(task.Exception, "Error running job {JobName}", job.Name);
                        });
                }
            }
            await Task.Delay(_pollInterval, cancellationToken);
        }
    }

    private async Task RunJobAsync(JobDefinition jobDefinition, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var jobInstanceRepository = scope.ServiceProvider.GetRequiredService<IJobInstanceRepository>();
            var factory = scope.ServiceProvider.GetRequiredService<IJobFactory>();

            var instance = new JobInstance
            {
                JobDefinitionId = jobDefinition.Id,
                ScheduledAt = jobDefinition.NextRunAt!.Value,
                Status = JobStatus.Running,
                Attempt = 1,
                StartedAt = DateTime.UtcNow,
            };
            await jobInstanceRepository.CreateJobInstaceAsync(instance);

            if (jobDefinition.CronSchedule != null)
            {
                var cronSchedule = CronExpression.Parse(jobDefinition.CronSchedule, CronFormat.IncludeSeconds);
                var nextRun = cronSchedule.GetNextOccurrence(DateTime.UtcNow, true);
                jobDefinition.NextRunAt = nextRun;
            }
            else
            {
                jobDefinition.Enabled = false;
            }

            var job = factory.Create(jobDefinition.JobType);
            await job.ExecuteAsync(jobDefinition.Parameters, cancellationToken);

            instance.Status = JobStatus.Succeeded;
            instance.FinishedAt = DateTime.UtcNow;
            await jobInstanceRepository.UpdateAsync(instance);
            await jobRepository.UpdateAsync(jobDefinition);
        }
        catch (Exception ex)
        {
            using var scope = serviceProvider.CreateScope();
            var jobInstanceRepository = scope.ServiceProvider.GetRequiredService<IJobInstanceRepository>();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var instance = new JobInstance
            {
                JobDefinitionId = jobDefinition.Id,
                ScheduledAt = DateTime.UtcNow,
                Status = JobStatus.Failed,
                Attempt = 1,
                ErrorMessage = ex.Message,
            };
            await jobInstanceRepository.CreateJobInstaceAsync(instance);
            jobDefinition.Enabled = false;
            await jobRepository.UpdateAsync(jobDefinition);

            logger.LogError(ex, "Job {JobName} failed: {ErrorMessage}", jobDefinition.Name, ex.Message);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
