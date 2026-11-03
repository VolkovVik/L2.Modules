# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview
ASPU (L2.Modules) - Modular Monolith Web API with MQTT/NATS/SignalR adapters

## Tech Stack
- .NET 10 (`net10.0`), ASP.NET Core Minimal APIs with Asp.Versioning (`api/v{version}`)
- Source-generated Mediator package (martinothamar) for CQRS
- FluentValidation for command/query validation (via `ValidationBehavior` pipeline)
- `AppResult` / `AppResult<T>` pattern for error handling
- MQTT (Mosquitto), NATS JetStream, SignalR for messaging
- YARP reverse proxy gateway with rate limiting (`Aspu.Gateway`)
- Serilog for structured logging (`serilog.json`, Seq sink)
- OpenAPI + Scalar for API documentation (Development only)
- Docker / docker compose for containerization
- Roslyn incremental source generators (`generators/`) for registrations and smart-enums

## Commands
```bash
dotnet build Aspu.slnx
dotnet test --solution Aspu.slnx
dotnet test --project source/Modules/Orders/tests/Aspu.Modules.Orders.UnitTests
# single class / test (TUnit tree filter: /assembly/namespace/class/test)
dotnet test --project source/Modules/Orders/tests/Aspu.Modules.Orders.UnitTests --treenode-filter "/*/*/GradeShould/*"
dotnet run --project source/API/Aspu.Api
docker compose up -d mosquitto nats
dotnet run -c Release --project benchmarks/Aspu.Benchmarks
```

## Build Rules
- `TreatWarningsAsErrors`, `AnalysisMode=All`, `EnforceCodeStyleInBuild` on every project (`Directory.Build.props`) — new code must be warning-free
- Central package management in `Directory.Packages.props` — no `Version=` in csproj
- Commit messages per `docs/commit.md`: `<type>. <Message>` (`feat.`, `fix.`, `refactor.`, `chore.`, ...)

## Architecture
- Modular Monolith: one host `source/API/Aspu.Api`, modules isolated by bounded context
- Clean Architecture: domain is isolated from infrastructure
- Each module: Domain, Application, Infrastructure, Presentation, IntegrationEvents, PublicApi projects + tests/…UnitTests and tests/…IntegrationTests
- `source/Modules/Orders` is the reference module — copy its structure for new modules
- Dependency direction:
  - Domain → Common.Domain
  - IntegrationEvents → Common.Application
  - Application → Common.Application, Domain, IntegrationEvents
  - Infrastructure → Common.Infrastructure, Application
  - Presentation → Common.Presentation, Application, Infrastructure, PublicApi
  - Domain never references outer layers

## Project Structure
- source/Modules/[Name]/Aspu.Modules.[Name].Domain/ - Aggregates, value objects, `[DomainEnum]` smart-enums, domain events
- source/Modules/[Name]/Aspu.Modules.[Name].Application/ - Use cases (commands/queries, validators, handlers)
- source/Modules/[Name]/Aspu.Modules.[Name].Infrastructure/ - External integrations and adapters
- source/Modules/[Name]/Aspu.Modules.[Name].Presentation/ - HTTP endpoints, MQTT/NATS handlers, module configuration
- source/Modules/[Name]/Aspu.Modules.[Name].PublicApi/ - Cross-module contracts
- source/Modules/[Name]/Aspu.Modules.[Name].IntegrationEvents/ - Integration events
- source/Common/Aspu.Common.Domain/ - `Aggregate`, `DomainEvent`, `Error`/`ErrorType`, `AppResult<T>`
- source/Common/Aspu.Common.Application/ - `IAppCommand`, `IAppQuery`, `IAppNotification` + handlers, ports (`IDateTimeProvider`, SignalR channel)
- source/Common/Aspu.Common.Presentation/ - `IHttpEndpoint`, `IMqttHandler`, `INatsHandler`, inbound processor, `ApiResults`
- source/API/Aspu.Api/ - Program.cs, DI composition, subscribers, exception handlers, options
- generators/ - SourceGenerators.{Domain,Application,Presentation}

## File Naming
- UseCases/[Area]/Commands/[Name]/[Name]Command.cs — record, validator and handler in one file
- Adapters/Http/[Area]/[Area]Requests.cs — `IHttpEndpoint` implementation
- Adapters/Mqtt/Mqtt[Name]Handler.cs, Adapters/Nats/Nats[Name]Handler.cs
- Namespaces: `Aspu.Modules.[Name].[Layer]...`, e.g. `Aspu.Modules.Orders.Application.UseCases.Codes.Commands.AddCode`

## Code Conventions
- Positional records for commands/queries/notifications: `sealed record [Name]Command(...) : IAppCommand<T>`
- File-scoped namespaces
- Primary constructors for dependency injection
- Sealed classes for implementations
- Validators are `internal sealed`; prefer helpers from `Aspu.Common.Application.Extensions` (e.g. `StringMustBeNotWhiteSpace()`)
- Logging via `[LoggerMessage]` source-generated methods
- Domain factories return `AppResult<T>` (e.g. `Code.Create(...)`); errors convert implicitly to failure

## Patterns We Use
- `AppResult<T>` for all handler return types (`ValueTask<AppResult<T>>`)
- HTTP mapping: `response.Match(Results.Ok, ApiResults.Problem)`
- `IHttpEndpoint` for endpoint registration (route group = `/[tags lowercased]`)
- `IMqttHandler` / `INatsHandler` with constant `Topic` for inbound messages
- `[DomainEnum]` on `sealed partial class` with public static get-only properties for smart-enums
- FluentValidation validators per use case, executed by `ValidationBehavior`
- Inbound processor: subscriber → `InboundProcessorChannel` → `InboundProcessorHostedService` (DI scope per message, bounded parallelism)
- SignalR: `ISignalrNotificationChannel` → `SignalrMessageWorker` → hub at `SignalR:HubPath`

## Patterns We Do NOT Use
- MediatR (use Mediator source generator)
- Manual registration of endpoints, validators, MQTT/NATS handlers (source generators do it)
- `TryAddEnumerable` for inbound handlers (registry won't see them)
- Non-constant or overlapping topics across handlers/modules
- References from Domain to Application/Infrastructure/Presentation

## Source Generators
- Attached automatically by project name (`Aspu.Modules.*.Domain|Application|Presentation`); hosts opt in via `EnableDomainGenerator` / `EnableApplicationGenerator` / `EnablePresentationGenerator`
- Only see source of the project they run in
- Generated namespace: `[Assembly-without-layer].SourceGenerators.[Layer]`
- Domain → smart-enums for `[DomainEnum]`
- Application → `services.AddValidators()`, SignalR notification registration
- Presentation → `HttpEndpointsRegistration.MapHttpEndpoints(...)`, `AddMqttHandlers()`, `AddNatsHandlers()`
- Handler diagnostics: `ASPU001` non-constant `Topic`, `ASPU002` not sealed / not visible, `ASPU003` empty `Topic`, `ASPU004` generic, `ASPU005` duplicate `Topic` in module

## Testing
- TUnit on Microsoft.Testing.Platform (`global.json`)
- Shouldly + TUnit assertions, NSubstitute for mocking
- Testcontainers for integration tests (NATS JetStream, PostgreSQL) — Docker required
- Test class naming: `[Subject]Should`, methods like `ReturnValuesOnGetList`
- Arrange / Act / Assert comments
- BenchmarkDotNet in `benchmarks/`, NBomber load tests in `nbomber/`

## DI Registration
- Each module: `Add[Name]Module(services, configuration)` → `AddApplication()`, `AddPresentation()`, `AddInfrastructure(configuration)`
- Each module: `Map[Name]Endpoints(app, routeGroupBuilder)` → generated `HttpEndpointsRegistration.MapHttpEndpoints`
- Wire new module into host: `Aspu.Api/Extensions/ModuleExtensions.cs` (DI) and `EndpointExtensions.cs` (HTTP)
- MQTT/NATS subscribers enabled via `Mqtt:IsEnabled` / `Nats:IsEnabled` in appsettings
- Optional per-module config files `modules.[name].json` loaded by `AddModuleConfiguration`
