using Looplet.Abstractions.Interfaces;
using Looplet.Worker.Infrastructure;
using Serilog;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IJobFactory, JobFactory>();
builder.Services.AddSingleton<IConfiguration>(configuration);

PluginLoader.LoadPlugins(builder.Services, "Plugins");

var app = builder.Build();

app.MapControllers();

app.Run();
