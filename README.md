# TripUpdater Mini-API

A small ASP.NET Core 10 REST API that processes batch trip-updates, recalculates each trip's status, and writes an audit log. The domain is transit: a `Trip` is operated by an `Operator` on a `Line`; an `UpdateLog` records every change.

## Architecture

Clean Architecture, single module. Dependencies point inward; the Domain has no outward references.

```
src/
  TripUpdater.Domain          # entities, enums, Result, DomainException — pure C#
  TripUpdater.Application     # use cases (commands/queries), validators, Mediator pipeline
  TripUpdater.Infrastructure  # EF Core, AppDbContext, SQLite, seed data
  TripUpdater.Api             # minimal API endpoints, Swagger, DI composition root
tests/
  TripUpdater.Tests           # xUnit v3 + WebApplicationFactory
```

Dependency direction:
- `Application → Domain`
- `Infrastructure → Application, Domain`
- `Api → Application, Infrastructure`
- `Tests → Api, Application, Domain`

## Key Choices

- **Mediator (source-generated, MIT).** The Mediator source generator produces the `IMediator` implementation and DI registrations at compile time. Handlers are plain `IRequestHandler<TRequest, TResponse>` classes.
- **FluentValidation pipeline.** A single `ValidationBehavior<TRequest, TResponse>` runs every registered validator before the handler. Failures are converted to `Result.Failure(errors)` and returned to the caller without throwing.
- **Result pattern.** Expected failures (validation, "trip not found", etc.) are returned as `Result.Failure(...)`, never thrown. The global exception handler is the last line of defense for unexpected exceptions.
- **EF Core + SQLite (file-based).** `DateTimeOffset` values are stored as `BIGINT` ticks via a value converter so SQLite can `ORDER BY` and index them. The DB file is created and seeded on first start.
- **`IAppDbContext` over repository.** No wrapper around `DbSet<T>` — handlers query the DbContext directly.
- **`IEndpointGroup` auto-discovery.** Every endpoint group implements `IEndpointGroup` and is discovered by `app.MapEndpoints()`. `Program.cs` does not change when new endpoints are added.
- **Single `Status` enum.** Shared by `Trip` and `UpdateLog` (`Ontime`, `Early`, `Late`, `Cancelled`, `Invalid`).
- **`TimeProvider` for timestamps.** `ProcessTripUpdatesHandler` resolves `TimeProvider` to stamp `UpdateLog.UpdateTimestamp` — no `DateTime.Now`.

## Business Rules

Status is computed from `actualArrivalTime` against `DepartureTime` and `OriginalArrivalTime`:

```
IF actualArrivalTime IS NULL        → CANCELLED
ELSE IF actualArrivalTime < DepartureTime → INVALID
ELSE
  diff (minutes) = actualArrivalTime - OriginalArrivalTime
  |diff| <= 2                       → ONTIME
  diff < -2                         → EARLY
  diff > 2                          → LATE
```

The rule lives in `Trip.CalculateStatus(DateTimeOffset?)`.

## Run

```bash
# Restore + build
dotnet restore
dotnet build

# Run the API (binds to http://localhost:5156 by default; see launchSettings.json)
dotnet run --project src/TripUpdater.Api

# Open Swagger UI
http://localhost:5156/swagger

# Run tests
dotnet test
```

The SQLite database file (`tripupdater.db`) is created next to the running API. To start clean, delete it and re-run.

## API

| Method | Path               | Description                                         |
| ------ | ------------------ | --------------------------------------------------- |
| GET    | `/trips`           | List trips, filtered by `lineNo`, `from`, `to`      |
| POST   | `/updates/trips`   | Process a batch of trip updates, returns summary    |
| GET    | `/updatelogs`      | List update logs, filtered by `from`, `to`, `status`|

### Example: process a batch

```http
POST /updates/trips
Content-Type: application/json

{
  "updates": [
    { "tripId": 1001, "actualArrivalTime": "2026-07-29T08:31:00+00:00" },
    { "tripId": 1002, "actualArrivalTime": "2026-07-29T08:40:00+00:00" },
    { "tripId": 1003, "actualArrivalTime": null }
  ]
}
```

```json
{
  "value": {
    "totalUpdates": 3,
    "ontime": 1,
    "early": 1,
    "late": 0,
    "cancelled": 1,
    "invalid": 0,
    "processedTripIds": [1001, 1002, 1003]
  },
  "isSuccess": true,
  "isFailure": false,
  "errors": []
}
```

See `docs/api.http` for all three endpoints.

## Trade-offs

- **SQLite over PostgreSQL.** Keeps the demo zero-dependency. The schema is portable; the value converter on `DateTimeOffset` is the only provider-specific code.
- **Single module.** No bounded contexts; everything lives in one `Application` assembly. Splitting into per-module projects (e.g. `Trips`, `Updates`) would help if the domain grew, but is premature now.
- **In-process integration tests use a temp SQLite file.** Not `UseInMemoryDatabase` — we use a real SQLite file in `%TEMP%` to match production SQL semantics.
- **`EnsureCreated` instead of migrations.** Fine for the demo; a real system would use `dotnet ef migrations`.

## What I would do differently in production

- **Authentication & authorization.** JWT bearer with role/claim-based policies. The endpoints are currently anonymous.
- **Rate limiting & output caching.** `AddRateLimiter` on `POST /updates/trips`, `AddOutputCache` on the read endpoints.
- **Structured logging with Serilog.** Enrichers for `RequestId`, `CorrelationId`, `TraceId`; sinks for console + Seq/AppInsights.
- **OpenTelemetry.** Tracing + metrics around the Mediator pipeline (the source generator has built-in support).
- **Real database.** PostgreSQL with `dotnet ef migrations` and idempotent SQL scripts in CI/CD.
- **Domain events.** `TripStatusChanged` raised inside `Trip.ApplyUpdate`; handlers react in-process and (via outbox) to a broker.
- **Docker image.** Multi-stage Dockerfile (or `dotnet publish /t:PublishContainer`) running as a non-root user.
- **CI/CD.** GitHub Actions: `restore → build → test → format verify → publish container`.
- **Health checks.** `AddHealthChecks().AddDbContextCheck<AppDbContext>()` exposed at `/health/live` and `/health/ready`.
- **More tests.** Property-based tests for the status rule; integration tests using Testcontainers + real Postgres; snapshot tests for the response shape.

## Layout

```
TripUpdater.slnx
Directory.Build.props          # target framework, nullable, analyzers
Directory.Packages.props       # central package management
.editorconfig
.gitignore
README.md
src/
  TripUpdater.Domain/          # entities, enums, Result, exceptions
  TripUpdater.Application/     # queries/commands, validators, IAppDbContext
  TripUpdater.Infrastructure/  # AppDbContext, configurations, SeedData
  TripUpdater.Api/             # Program.cs, Endpoints/, appsettings.json
tests/
  TripUpdater.Tests/           # unit + integration
docs/
  plan.md
  api.http
```
