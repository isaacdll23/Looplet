using Looplet.API.Extensions;
using Looplet.API.Infrastructure.Scheduling;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Supressing warning since using the WebApplicationBuilder.Configuration does not trigger
// the Load method of the ConfigurationProvider early enough.
// It needs to run before the MondoDB service is initialized.
#pragma warning disable ASP0013 // Suggest switching from using Configure methods to WebApplicationBuilder.Configuration
builder.Host.ConfigureAppConfiguration((hostingContext, configurationBuilder) =>
{
    configurationBuilder.AddInfisical(options =>
    {
        options.ProjectId = Environment.GetEnvironmentVariable("INFISICAL_LOOPLET_PROJECT_ID") ?? throw new ArgumentException("Unable to retrive environment variable: INFISICAL_LOOPLET_PROJECT_ID");
        options.ClientId = Environment.GetEnvironmentVariable("INFISICAL_UNIVERSAL_AUTH_CLIENT_ID") ?? throw new ArgumentException("Unable to retrive environment variable: INFISICAL_UNIVERSAL_AUTH_CLIENT_ID");
        options.ClientSecret = Environment.GetEnvironmentVariable("INFISICAL_UNIVERSAL_AUTH_CLIENT_SECRET") ?? throw new ArgumentException("Unable to retrive environment variable: INFISICAL_UNIVERSAL_AUTH_CLIENT_SECRET");
        options.SiteUrl = Environment.GetEnvironmentVariable("INFISICAL_SITE_URL") ?? throw new ArgumentException("Unable to retrive environment variable: INFISICAL_SITE_URL");

        string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        options.Environment = !string.IsNullOrEmpty(environment) && environment == "Development" ? "dev" : "prod";
    });
});
#pragma warning restore ASP0013 // Suggest switching from using Configure methods to WebApplicationBuilder.Configuration

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Configuration
    .AddEnvironmentVariables();

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Looplet API",
        Version = "v1",
        Description = "Looplet API for managing jobs and job instances."
    });
});

builder.Services.AddHttpClient();
builder.Services.AddMongoServices(builder.Configuration);
builder.Services.AddHostedService<JobSchedulerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
