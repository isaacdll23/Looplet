# Looplet

Lightweight C# job‐scheduling framework composed of a central Hub, pluggable Workers and sample jobs.

## Projects

- **Looplet.Abstractions**
  Interfaces & DTOs (`IJob`, `ExecuteRequestDto`, etc.).
- **Looplet.Hub**
  ASP.NET Core Web API + background `SchedulerService`:
  • Manage job definitions & instances (MongoDB)
  • Dispatch due jobs to Workers
  • Endpoints:
    - `GET  /api/jobs`
    - `POST /api/jobs`
    - `GET  /api/jobs/types`
- **Looplet.Worker**
  ASP.NET Core Web API hosting `IJob` implementations:
  • `/plugins/jobs` to list supported job types
  • `/execute` to run jobs
- **Looplet.SampleJobs**
  Example `IJob` plugins (e.g. `SampleJob2Sec`).

## Requirements

- .NET 8.0 SDK
- MongoDB instance
- Environment variables (Hub):
  - `INFISICAL_LOOPLET_PROJECT_ID`
  - `INFISICAL_UNIVERSAL_AUTH_CLIENT_ID`
  …etc.
