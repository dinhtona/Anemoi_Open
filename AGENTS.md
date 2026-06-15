# AGENTS.md - Anemoi_Open Project Memory

## Project Overview
**Anemoi_Open** is an enterprise HR management system built as a .NET 10 modular monolith / microservices platform using Clean Architecture, CQRS, Event-Driven Architecture (EDA), and Saga Orchestration patterns.

### Tech Stack
- **Framework**: .NET 10 (ASP.NET Core)
- **ORM**: EF Core + PostgreSQL (Npgsql)
- **Message Broker**: RabbitMQ via MassTransit
- **Object Mapping**: Riok.Mapperly (source generators, compile-time)
- **CQRS/Mediator**: MediatR with OneOf<> for success/error handling
- **Validation**: FluentValidation
- **GraphQL**: HotChocolate
- **Logging**: Serilog
- **Resilience**: Polly
- **Auth**: JWT-based identity management

## Architecture

### Microservices
| Service | Purpose |
|---------|---------|
| `Anemoi.Centralize` | Gateway API, aggregates services, exposes REST/GraphQL to external clients |
| `Anemoi.Identity` | Authentication, authorization, user management, role groups, permissions |
| `Anemoi.MasterData` | Static/reference data (provinces, districts, etc.) shared across services |
| `Anemoi.Workspace` | Multi-tenancy - workspace/organization management, member invitations |
| `Anemoi.Hr` | Core HR domain: employee lifecycle, attendance, payroll, compensation, benefits |
| `Anemoi.Employee` | Self-service portal for employees to manage their own data |
| `Anemoi.Orchestration` | Saga orchestrator for distributed transactions (uses MongoDB) |
| `Anemoi.Notification` | Notification and email dispatching |
| `Anemoi.Secure` | Security, encryption service |
| `Anemoi.BuildingBlocks` | Shared kernel: base classes, abstractions, CQRS pipeline, helpers |
| `Anemoi.Contract` | Shared DTOs, strongly-typed IDs, integration events |
| `Anemoi.Storage` | File storage service (S3-compatible) |
| `Anemoi.Grpc` | gRPC service layer |

### Per-Service Directory Structure (Clean Architecture)
```
{Service}/
  {Service}.Domain/          # Entities, Value Objects, Domain Events, Strongly-typed IDs
  {Service}.Application/     # CQRS commands/queries, handlers, mappers, validators
    Cqrs/Commands/           # Command + CommandHandler per use case
    Cqrs/Queries/            # Query + QueryHandler per read operation
    Mappings/                # Mapperly partial classes
  {Service}.Infrastructure/  # EF Core DbContext, migrations, repository implementations
  {Service}.Api/             # Entry point (Program.cs), DI, controllers
```

## Business Domain Areas

### Identity & Access Management
- User registration/login/logout with JWT authentication
- Role groups composed of granular permissions (capability-based auth)
- Permission-per-feature-action model, NOT role-based in feature code
- `[HasPermission(Permissions.X)]` attribute for protected endpoints

### HR - Employee Lifecycle
- Employee onboarding/offboarding, status transitions
- Organization & department structure management
- Position/career management

### HR - Time & Attendance
- Clock-in/clock-out tracking with shifts and work schedules
- Leave request and approval workflows
- Work log management

### HR - Payroll & Compensation
- Salary component calculations (base salary, allowances)
- Payslip generation
- Payroll reporting and export (latest: Phase 15 MVP)
- Bonus, deduction, overtime calculations
- Approval workflows for payroll processing

### Workspace & Multi-Tenancy
- Organization management (create, assign admins)
- Member invitations with email flow
- Multi-workspace membership with role assignments

### Master Data & Administration
- Geographic hierarchy: provinces → districts → wards
- System seed data generation with idempotent seeding
- Environment configuration management

## Coding Conventions - MUST Follow

### CQRS Rules
- Every mutation gets its own `XxxCommand.cs` + `XxxCommandHandler.cs`
- Every read gets its own `XxxQuery.cs` + `XxxQueryHandler.cs`
- Handlers return `OneOf<TResult, ErrorDetailResponse>` - never throw for business errors

### Strongly-Typed IDs
- Always use typed ID wrappers (e.g., `UserId`, `EmployeeId`) instead of raw Guid/int
- Initialize with `new XxxId(IdGenerator.NextGuid())`
- Primitive types only at wire boundaries (DTOs, claims, headers)

### Mapping
- Mapperly only, partial classes with `[Mapper]` attribute in `Application/Mappings/`
- Use `partial` methods for compile-time generated mappings

### Validation
- FluentValidation per command/query
- Auto-triggered via MediatR pipeline

### Events & Messaging
- Async side effects across services: publish integration events via MassTransit
- Sync responses needed: use MassTransit request-response (NOT HTTP/gRPC internally)
- External integrations (OAuth, S3): encapsulate behind Application abstractions

### Localization
- Languages: `vi-VN` (default), `en-US`
- User-facing messages via `IStringLocalizer<SharedResource>`, never hardcoded
- Stable error codes, not localized labels, in API responses

### Business String Literals (Backend)
- Never use inline business/status/type string literals in `Anemoi.Hr.*` production code
- Domain status values: use constant classes in the domain namespace (e.g., `LeaveRequestStatusCode`, `ContractStatusCode`, `PayrollPeriodStatusCode`, `AttendancePeriodStatusCode`, `TaxRuleSetStatusCode`, `OvertimeStatusCode`, `EmployeeShiftAssignmentStatusCode`, `EmploymentStatusCode`)
- Domain type keys: use constant classes (e.g., `LeaveBalanceTransactionType`, `TaxDeductionTypeCode`, `TaxDeductionInputKey`, `HrSourceModuleCode`, `ChangeTypeCode`, `LeaveTypeCode`, `CalendarExceptionTypeConstants`)
- Application-level constants (display fallbacks, UI strings, API response values): use `PayrollConstants.cs`, `CalendarExceptionTypeConstants.cs` in `Application/Configurations/`
- Allowed inline strings: log message templates, route templates, JSON property names for external contracts, constants definition values, test data in test projects
- CI should reject any `.cs` file in `Anemoi.Hr.Application/` containing `== "` or `!= "` or `= "` with business string values (not error codes or validation messages, which have their own constants)

### Authorization
- Permission constants declared in `BuildingBlocks/Application/Authorization/Permissions.cs`
- Protect with `[HasPermission]`, never hardcode role names in feature code
- `Administrator` is a reserved system role that bypasses permission checks

## Iterative Development Workflow (3 Steps)

When implementing any feature, execute in this order with user review between each step:

1. **Step 1 - Domain & Data**: Strongly-typed IDs → Entities → EF Core config → DbContext registration
2. **Step 2 - Application Layer**: Request/Response DTOs → Mapperly mapper → Commands/Queries/Handlers/Validators
3. **Step 3 - API & Communication**: Controllers/GraphQL endpoints → MassTransit events

## Key Files to Reference First
- `ArchitectureGuide.md` - Full architecture guide, layer rules, coding conventions
- `DEVELOPMENT.md` - Development guidelines, do's and don'ts
- `Anemoi.BuildingBlocks/` - Shared kernel (base classes, abstractions, pipelines)

## Environment Setup
```bash
# Prerequisites: .NET 10 SDK, Docker Desktop, RabbitMQ
cp .env.example .env  # Fill in passwords
docker-compose up -d --build
```

### Build & Run Commands
- Solution file: `Anemoi.sln`
- Use `dotnet build Anemoi.sln` for full build
- Services run Dockerized via docker-compose

## Git
- Current active branch state includes Phase 15 (Payroll Reporting/Export MVP)
- Commit convention: "Phase N - [description]"
- Feature implementation references: `FeatureImplementationTemplate.md`
