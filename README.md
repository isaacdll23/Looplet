# Looplet

Lightweight C# job‐scheduling framework composed of a central Hub, pluggable Workers and sample jobs.

## Projects

- **Looplet.Abstractions**
  Interfaces & DTOs (`IJob`, `ExecuteRequestDto`, etc.).

- **Looplet.Hub**
  ASP.NET Core Web API + background `SchedulerService`:

- **Looplet.Worker**
  ASP.NET Core Web API hosting `IJob` implementations:
  
- **Looplet.SampleJobs**
  Example `IJob` plugins (e.g. `SampleJob2Sec`).

## Requirements

- .NET 8.0 SDK
- MongoDB instance
- Environment variables (Hub):
  - `INFISICAL_LOOPLET_PROJECT_ID`
  - `INFISICAL_UNIVERSAL_AUTH_CLIENT_ID`
