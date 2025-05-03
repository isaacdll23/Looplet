using Looplet.Abstractions.Models.DTOs;
using Looplet.Hub.Features.Workers.Models;
using Looplet.Hub.Features.Workers.Repositories;

namespace Looplet.Hub.Features.Workers.Services;

public interface IWorkerService
{
    public Task<IEnumerable<Worker>> GetAllWorkersAsync();
    public Task<Worker?> GetWorkerByIdAsync(string id);
    public Task<Worker?> GetWorkerByAliasAsync(string alias);
    public Task<Worker> RegisterWorkerAsync(string alias, string baseUrl);
}

public class WorkerService : IWorkerService
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ILogger<WorkerService> _logger;

    public WorkerService(IWorkerRepository workerRepository, ILogger<WorkerService> logger)
    {
        _workerRepository = workerRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Worker>> GetAllWorkersAsync()
    {
        return await _workerRepository.GetAllAsync();
    }

    public async Task<Worker?> GetWorkerByIdAsync(string id)
    {
        return await _workerRepository.GetByIdAsync(id);
    }

    public async Task<Worker?> GetWorkerByAliasAsync(string alias)
    {
        return await _workerRepository.GetByAliasAsync(alias);
    }

    public async Task<Worker> RegisterWorkerAsync(string alias, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Alias and BaseUrl cannot be null or empty.");
        }

        var existingWorker = await _workerRepository.GetByAliasAsync(alias);
        if (existingWorker != null)
        {
            throw new InvalidOperationException($"A worker with the alias '{alias}' already exists.");
        }

        // Validate the URI is valid
        var uri = new Uri(baseUrl);
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("BaseUrl must be a valid HTTP or HTTPS URL.");
        }

        // Ping health endpoint
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(5);

        var healthCheckUrl = $"{baseUrl.TrimEnd('/')}/health";
        _logger.LogInformation($"Pinging health endpoint at {healthCheckUrl} for worker registration.");

        HttpResponseMessage response = await client.GetAsync(healthCheckUrl);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Worker at {healthCheckUrl} is not reachable. Status code: {response.StatusCode}");
        }

        HealthCheckResponseDto? responseContent = await response.Content.ReadFromJsonAsync<HealthCheckResponseDto>();
        if (responseContent == null || responseContent.Status != "Healthy")
        {
            throw new InvalidOperationException($"Worker at {baseUrl} is not healthy. Status: {responseContent?.Status}");
        }

        var worker = new Worker
        {
            Alias = alias,
            BaseUrl = baseUrl,
            Hostname = responseContent.Hostname,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return await _workerRepository.CreateAsync(worker);
    }
}
