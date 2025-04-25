using Looplet.DAL.Repositories;
using MongoDB.Bson;

namespace BackgroundWorkerController.Workers;

public class HelloWorldWorker : BackgroundService
{
    private readonly ILogger<HelloWorldWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _workerId;

    public HelloWorldWorker(ILogger<HelloWorldWorker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _workerId = configuration["RegisteredServices:HelloWorldService:WorkerId"] ?? throw new ArgumentNullException(nameof(configuration), "WorkerId is not configured in appsettings.json");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Worker [WorkerID: {workerId}]", _workerId);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            {
                var workerRepository = scope.ServiceProvider.GetRequiredService<IWorkerRepository>();

                var worker = await workerRepository.GetWorkerByIdAsync(ObjectId.Parse(_workerId));
                if (worker == null)
                {
                    _logger.LogWarning("Worker not found [WorkerID: {workerId}]", _workerId);
                    break;
                }
                if (worker.IsEnabled)
                {
                    _logger.LogInformation("Worker is enabled. Executing. [WorkerID: {workerId}. Worker Name: {workerName}]", _workerId, worker.Name);
                    await ExecuteTaskAsync();
                }
                else
                {
                    _logger.LogInformation("Worker is disabled [WorkerID: {workerId}. Worker Name: {workerName}]", _workerId, worker.Name);
                }

                await Task.Delay(worker.IntervalSeconds * 1000, stoppingToken);
            }
        }
    }

    private Task ExecuteTaskAsync()
    {
        _logger.LogInformation("HELLO WORLD");
        return Task.CompletedTask;
    }
}
