# Plan: TripUpdater Mini-API

**Architecture:** Clean Architecture (single module)  
**Affected layers:** Domain, Application, Infrastructure, Api  
**Estimated steps:** 8  
**Time estimate:** 2–3 hours  

## Context

Build a small ASP.NET Core REST API that processes trip updates and updates the plan. The assignment focuses on data processing + domain logic with light persistence. Requirements:

- **3 endpoints:** Process trip updates (POST), filter trips (GET), read update logs (GET)
- **Domain model:** Trip, Line, UpdateLog, Operator (stub)
- **Status rules:** ±2 min margin for ONTIME; null actual time = CANCELLED; arrival < departure = INVALID
- **Persistence:** EF Core with SQLite, seeded with sample trips
- **Tests:** 2–3 unit tests (status/validation), 1 integration test
- **Documentation:** Swagger, README with architecture and choices

## Key Design Decisions

1. **Single `Status` enum** (Ontime, Early, Late, Cancelled, Invalid) — shared by Trip and UpdateLog
2. **UpdateLog.TripId** (renamed from TripNo) — foreign key to Trip
3. **Trip.OriginalArrivalTime** — used as baseline for status calculation
4. **Business rules encapsulated in `Trip.CalculateStatus(actualArrivalTime)`** method
5. **Mediator (source-generated, MIT)** — CQRS pattern with FluentValidation pipeline
6. **IAppDbContext abstraction** — no repository pattern (per .kilo/CLAUDE.md rules)
7. **Result pattern** — explicit error handling, no exceptions for expected failures
8. **SQLite (file-based)** — realistic persistence, lightweight, no external dependencies
9. **Minimal APIs + IEndpointGroup auto-discovery** — Program.cs never changes when adding endpoints

## Business Rule Summary

**Status Calculation Logic:**
```
IF actualArrivalTime IS NULL → CANCELLED
ELSE IF actualArrivalTime < DepartureTime → INVALID
ELSE:
  diff = actualArrivalTime - OriginalArrivalTime
  IF |diff| <= 2 minutes → ONTIME
  ELSE IF diff < -2 minutes → EARLY
  ELSE → LATE
```

## Steps

### Step 1: Solution Setup — Root level — Foundation
- Create `TripUpdater.slnx` (modern solution format)
- Add `Directory.Build.props` (C# 14, nullable, implicit usings, analyzers)
- Add `Directory.Packages.props` (central package management)
- Add `.gitignore` (Visual Studio standard)
- Add `.editorconfig` (code style rules)
- Create folder structure: `src/`, `tests/`, `docs/`

### Step 2: Domain Layer — `src/TripUpdater.Domain/` — Pure business logic, zero dependencies
- **Entities:**
  - `Trip` (TripId, LineNo, DepartureTime, OriginalArrivalTime, ArrivalTime, Status) — with `CalculateStatus()` method
  - `Line` (LineId, OperatorNo, LinePlanningNumber) — simple entity
  - `UpdateLog` (UpdateLogId, TripId, UpdateTimestamp, Status) — FK to Trip
  - `Operator` — stub entity (minimal: OperatorNo, Name)
- **Enums:**
  - `Status` (Ontime, Early, Late, Cancelled, Invalid) — single enum for both Trip and UpdateLog
- **Common:**
  - `Entity` base class with `Id`
  - `Result` and `Result<T>` for error handling
- **Exceptions:**
  - `DomainException` for business rule violations

### Step 3: Application Layer — `src/TripUpdater.Application/` — Use cases and orchestration
- **Common:**
  - `IAppDbContext` interface (DbSets + SaveChangesAsync)
  - `ValidationBehavior` (Mediator pipeline for FluentValidation)
- **Use Cases (Commands):**
  - `ProcessTripUpdates` — batch process updates
    - Command: `ProcessTripUpdatesCommand(List<TripUpdateDto>)`
    - Handler: For each update → find trip → call `trip.CalculateStatus(actualTime)` → update trip → create UpdateLog → return summary
    - Validator: `ProcessTripUpdatesValidator` (check TripId exists, times are valid)
- **Use Cases (Queries):**
  - `GetTrips` — filter by lineId, from, to
    - Query: `GetTripsQuery(string? LineId, DateTimeOffset? From, DateTimeOffset? To)`
    - Handler: Filter trips, return `List<TripDto>`
  - `GetUpdateLogs` — filter by from, to, status
    - Query: `GetUpdateLogsQuery(DateTimeOffset? From, DateTimeOffset? To, Status? Status)`
    - Handler: Filter logs, return `List<UpdateLogDto>`
- **DTOs:**
  - `TripDto`, `UpdateLogDto`, `TripUpdateDto`, `UpdateSummaryDto`

### Step 4: Infrastructure Layer — `src/TripUpdater.Infrastructure/` — EF Core + persistence
- **Persistence:**
  - `AppDbContext` (implements `IAppDbContext`)
  - Entity configurations: `TripConfiguration`, `LineConfiguration`, `UpdateLogConfiguration`
  - SQLite database (file-based)
- **Seed Data:**
  - `SeedData` class: Create 5–10 trips across 2–3 lines with different `OriginalArrivalTime` values
  - Some trips with updates already applied
- **DI Registration:**
  - `DependencyInjection.cs` — `AddInfrastructure()` extension method
  - Register `AppDbContext` and map to `IAppDbContext`

### Step 5: Api Layer — `src/TripUpdater.Api/` — Thin endpoints, Swagger
- **Endpoints (IEndpointGroup auto-discovery):**
  - `TripEndpoints.cs` — GET /trips?lineId=&from=&to=
  - `UpdateEndpoints.cs` — POST /updates/trips (batch processing)
  - `UpdateLogEndpoints.cs` — GET /updatelogs?from=&to=&status=
- **Program.cs:**
  - Register Mediator (source-generated)
  - Register FluentValidation
  - Register Infrastructure
  - Add Swagger/OpenAPI (Swashbuckle)
  - Call `app.MapEndpoints()` for auto-discovery
  - Add global exception handler
  - Seed database in development
- **Middleware:**
  - Global exception handler (catches unhandled exceptions, returns ProblemDetails)

### Step 6: Tests — `tests/TripUpdater.Tests/` — Unit + Integration
- **Unit Tests (2–3):**
  - `TripStatusCalculationTests` — **Test all business rules:**
    - Null actual time → Cancelled
    - Arrival < Departure → Invalid
    - Within ±2 min → Ontime
    - More than 2 min early → Early
    - More than 2 min late → Late
  - `ProcessTripUpdatesValidatorTests` — Test validation rules (missing TripId, invalid times)
  - `ProcessTripUpdatesHandlerTests` — Test happy path with mocked IAppDbContext
- **Integration Tests (1):**
  - `TripEndpointsIntegrationTests` — WebApplicationFactory + Testcontainers
    - Test GET /trips with filters returns correct data
    - Test POST /updates/trips processes batch and writes logs
- **Fixtures:**
  - `ApiFixture` — WebApplicationFactory setup with test database

### Step 7: Documentation — Root + docs/ — README and architecture
- **README.md:**
  - Architecture overview (bullets: Clean Architecture layers)
  - Key choices (Mediator, FluentValidation, EF Core SQLite, Result pattern, single Status enum)
  - Trade-offs (SQLite vs PostgreSQL, single module vs multi-module)
  - Run instructions (`dotnet run`, `dotnet test`)
  - API endpoints with examples
  - Future improvements (what to do differently in production)
- **Optional:** Postman/REST Client script (`api.http` file)

### Step 8: Final Polish — Verify and commit — Quality gates
- Run `dotnet build` — ensure no errors/warnings
- Run `dotnet test` — all tests pass
- Run `dotnet format --verify-no-changes` — code style check
- Create clear, atomic commits:
  - `chore: add solution structure and build configuration`
  - `feat: add domain entities with status calculation logic`
  - `feat: add application use cases with validation`
  - `feat: add infrastructure persistence layer`
  - `feat: add api endpoints with swagger`
  - `test: add unit and integration tests`
  - `docs: add README with architecture and instructions`

## File Structure

```
TripUpdater/
├── src/
│   ├── TripUpdater.Domain/
│   │   ├── Entities/
│   │   │   ├── Trip.cs              # Has OriginalArrivalTime + CalculateStatus()
│   │   │   ├── Line.cs
│   │   │   ├── UpdateLog.cs         # TripId (not TripNo)
│   │   │   └── Operator.cs
│   │   ├── Enums/
│   │   │   └── Status.cs            # Single enum: Ontime, Early, Late, Cancelled, Invalid
│   │   ├── Common/
│   │   │   ├── Entity.cs
│   │   │   └── Result.cs
│   │   └── Exceptions/
│   │       └── DomainException.cs
│   ├── TripUpdater.Application/
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   └── IAppDbContext.cs
│   │   │   └── Behaviors/
│   │   │       └── ValidationBehavior.cs
│   │   ├── Trips/
│   │   │   └── Queries/
│   │   │       └── GetTrips/
│   │   │           ├── GetTripsQuery.cs
│   │   │           ├── GetTripsHandler.cs
│   │   │           └── TripDto.cs
│   │   ├── Updates/
│   │   │   └── Commands/
│   │   │       └── ProcessTripUpdates/
│   │   │           ├── ProcessTripUpdatesCommand.cs
│   │   │           ├── ProcessTripUpdatesHandler.cs
│   │   │           ├── ProcessTripUpdatesValidator.cs
│   │   │           ├── TripUpdateDto.cs
│   │   │           └── UpdateSummaryDto.cs
│   │   └── UpdateLogs/
│   │       └── Queries/
│   │           └── GetUpdateLogs/
│   │               ├── GetUpdateLogsQuery.cs
│   │               ├── GetUpdateLogsHandler.cs
│   │               └── UpdateLogDto.cs
│   ├── TripUpdater.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── TripConfiguration.cs
│   │   │   │   ├── LineConfiguration.cs
│   │   │   │   └── UpdateLogConfiguration.cs
│   │   │   └── SeedData.cs
│   │   └── DependencyInjection.cs
│   └── TripUpdater.Api/
│       ├── Endpoints/
│       │   ├── TripEndpoints.cs
│       │   ├── UpdateEndpoints.cs
│       │   └── UpdateLogEndpoints.cs
│       ├── Program.cs
│       └── appsettings.json
├── tests/
│   └── TripUpdater.Tests/
│       ├── Unit/
│       │   ├── Domain/
│       │   │   └── TripStatusCalculationTests.cs
│       │   └── Application/
│       │       ├── ProcessTripUpdatesValidatorTests.cs
│       │       └── ProcessTripUpdatesHandlerTests.cs
│       ├── Integration/
│       │   └── TripEndpointsIntegrationTests.cs
│       └── Fixtures/
│           └── ApiFixture.cs
├── docs/
│   └── api.http
├── .gitignore
├── .editorconfig
├── Directory.Build.props
├── Directory.Packages.props
├── README.md
└── TripUpdater.slnx
```

## Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| **Time constraint (2–3 hours)** | Keep scope minimal — no auth, no complex queries, no pagination |
| **Over-engineering** | Single module, no repository pattern, no DDD tactical patterns |
| **Status calculation edge cases** | Comprehensive unit tests covering all branches |
| **Test setup overhead** | Use Testcontainers SQLite, share fixture across tests |
| **Swagger setup** | Use Swashbuckle (built-in), add XML comments |

## Success Criteria

- ✅ All 3 endpoints work correctly
- ✅ Status calculation follows business rules exactly
- ✅ Database persists data with seed data
- ✅ 2–3 unit tests + 1 integration test all pass
- ✅ Swagger UI shows all endpoints with examples
- ✅ Code follows Clean Architecture principles (dependencies point inward)
- ✅ Result pattern used for expected failures
- ✅ Validation via FluentValidation in Mediator pipeline
- ✅ Clear, atomic git commits
- ✅ README documents architecture, choices, and run instructions
