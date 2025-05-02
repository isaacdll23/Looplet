using Looplet.Abstractions.Extensions;
using Looplet.API.Infrastructure.Scheduling;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

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
