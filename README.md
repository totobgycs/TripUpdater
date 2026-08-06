# TripUpdater Mini-API

A small ASP.NET Core 10 REST API that processes batch trip-updates, recalculates each trip's status, and writes an audit log. The domain is: a `Trip` is operated by an `Operator` on a `Line`; an `UpdateLog` records every change.

## Architecture

Clean Architecture, single module. Dependencies point inward; the Domain has no outward references.

```
src/
  TripUpdater.Domain          # entities, enums, Result, DomainException — pure C#
  TripUpdater.Application     # use cases (commands/queries), validators, Mediator pipeline
  TripUpdater.Infrastructure  # EF Core, AppDbContext, InMemory store, seed data
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
- All domain entities have an **Id**, foreign keys refer to this internal **Id** instead of the external **Id**s like **LineNo**
- I worked with the assumption that a **Trip** once create for a **Line** would not change to another **Line**
- Another assumption is that we have a reference arrival time, and the **Early**, **Late**, **Ontime** status is calculated from the actual arrival time against the reference arrival time.
- Along the implementation I used **KiloCode** with the **GLM 5.2** model
- **EF Core** - I chose it because I have prior experience with it.
- **EF Core InMemory (mock database).** Volatile store that resets on restart; seed data restores demo state each run. Keeps the `IAppDbContext`/`DbSet<T>`/LINQ seam intact so swapping to a real provider is a one-line change. NodaTime `Instant` is stored as the CLR type directly; on PostgreSQL the Npgsql NodaTime plugin maps it natively to `timestamptz`.


The API runs against an in-memory database seeded on each start, so state resets every run — no file to manage. To start clean, just restart the API.

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
```
```json
{
  "updates": [
    {
      "tripId": 1003,
      "departureTime": "2026-07-29T08:31:00+00:00",
      "actualArrivalTime": "2026-07-29T09:10:00+00:00"
    }
  ]
}
```
```json
{
  "updates": [
    {
      "tripId": 1003,
      "actualArrivalTime": "2026-07-29T08:50:00+00:00"
    }
  ]
}
```
```json
{
  "updates": [
    {
      "tripId": 1003,
      "actualArrivalTime": "2026-07-29T08:50:00+00:00"
    },
    {
      "tripId": 1004,
      "departureTime": "2026-07-29T08:30:00+00:00"
    },
    {
      "tripId": 1005,
      "departureTime": "2026-07-29T08:31:00+00:00",
      "actualArrivalTime": "2026-07-29T08:30:00+00:00"
    }
  ]
}
```

See `docs/api.http` for all three endpoints.

## What I would do differently in production
**The most important change I would do in a real project is that I would put the creation of the log record in a trigger. This way all the changes are captured, including manual updates and updates made in other parts of the program. Also there are no extra inserts sent from the program for every log record.**

## What GLM 5.2 would do differently in production
- **Authentication & authorization.** JWT bearer with role/claim-based policies. The endpoints are currently anonymous.
- **Rate limiting & output caching.** `AddRateLimiter` on `POST /updates/trips`, `AddOutputCache` on the read endpoints.
- **Structured logging with Serilog.** Enrichers for `RequestId`, `CorrelationId`, `TraceId`; sinks for console + Seq/AppInsights.
- **OpenTelemetry.** Tracing + metrics around the Mediator pipeline (the source generator has built-in support).
- **Real database.** PostgreSQL with `dotnet ef migrations` and idempotent SQL scripts in CI/CD. The EF Core configurations (unique indexes, FKs) are already in place; `Instant` maps natively to `timestamptz` via the Npgsql NodaTime plugin.
- **Domain events.** `TripStatusChanged` raised inside `Trip.ApplyUpdate`; handlers react in-process and (via outbox) to a broker.
- **Docker image.** Multi-stage Dockerfile (or `dotnet publish /t:PublishContainer`) running as a non-root user.
- **CI/CD.** GitHub Actions: `restore → build → test → format verify → publish container`.
- **Health checks.** `AddHealthChecks().AddDbContextCheck<AppDbContext>()` exposed at `/health/live` and `/health/ready`.
- **More tests.** Property-based tests for the status rule; integration tests using Testcontainers + real Postgres; snapshot tests for the response shape.

