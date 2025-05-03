using Looplet.Hub.Features.Jobs.Repositories;
using MongoDB.Driver;

namespace Looplet.Hub.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["DatabaseConnectionString"];
        var databaseName = configuration["DatabaseName"];

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(configuration), "MongoDB connection string is not configured.");
        }

        if (string.IsNullOrEmpty(databaseName))
        {
            throw new ArgumentNullException(nameof(configuration), "MongoDB database name is not configured.");
        }

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
        services.AddScoped(_ =>
        {
            IMongoClient client = _.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });

        services.AddScoped<IJobInstanceRepository, JobInstanceRepository>();
        services.AddScoped<IJobDefinitionRepository, JobDefinitionRepository>();

        return services;
    }
}
