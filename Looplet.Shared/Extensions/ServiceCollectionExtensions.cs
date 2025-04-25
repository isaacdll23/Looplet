using Looplet.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Looplet.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"];
        var databaseName = configuration["MongoDB:DatabaseName"];

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
            var client = _.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });

        return services;
    }
}