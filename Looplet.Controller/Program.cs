using Looplet.DAL.Repositories;
using BackgroundWorkerController.Workers;
using Serilog;
using Looplet.Shared.Extensions;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .Build();

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting worker host");
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMongoServices(configuration);
            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IWorkerRepository, WorkerRepository>();
            services.AddScoped<IWorkerRunRepository, WorkerRunRepository>();
            services.AddHostedService<HelloWorldWorker>();
        })
        .Build();
    
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
