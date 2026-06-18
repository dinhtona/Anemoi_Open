# Anemoi Development Guidelines

This document summarizes the development conventions for Anemoi_Open. When in doubt, architecture decisions in [docs/architecture/ARCHITECTURE_DECISIONS.md](docs/architecture/ARCHITECTURE_DECISIONS.md) take precedence.

## 1. Architecture

The project follows Clean Architecture and can be deployed as a modular monolith or independently deployable services.

Each service normally follows this shape:

```text
{Service}.ModelIds
{Service}.Domain
{Service}.Application
{Service}.Infrastructure
{Service}.Api or {Service}.WorkerService
```

Some cross-service contracts live under `Anemoi.Contract`. Local aggregate IDs belong in the owning service's `*.ModelIds` project unless an existing service-specific convention says otherwise.

## 2. Layer Responsibilities

| Layer | Responsibility | Dependency Rule |
| :--- | :--- | :--- |
| ModelIds | Strongly typed ID wrappers only | Depends only on shared domain primitives |
| Domain | Entities, aggregate roots, value objects, domain events, domain rules | Must not depend on Application, Infrastructure, or Api |
| Application | CQRS use cases, handlers, validators, DTOs, Mapperly mappers, abstractions | Must not depend on Infrastructure |
| Infrastructure | EF Core DbContext, mappings, migrations, repositories, external providers | Depends on Application and Domain |
| Api / WorkerService | Host startup, DI, controllers, GraphQL, MassTransit wiring | Depends on Infrastructure |
| Contract | Cross-service DTOs, integration events, shared wire contracts | Must stay implementation-free |

## 3. CQRS and Result Handling

- Every mutation must be a command.
- Every read must be a query.
- Commands and queries should have dedicated handler files.
- Handlers return `OneOf<TResult, ErrorDetailResponse>` for expected success/error outcomes.
- Do not throw exceptions for normal business validation failures.
- Controllers must not contain business logic or query EF Core directly.

## 4. Strongly Typed IDs

- Use typed ID wrappers such as `EmployeeId`, `UserId`, or `PayrollRunId` for local aggregate IDs.
- Create new IDs with `new XxxId(IdGenerator.NextGuid())`.
- Primitive IDs are allowed at wire boundaries only: DTOs, headers, claims, route values, and integration events.
- Convert primitive IDs to strongly typed IDs before using them in domain/application logic.
- Domain factories should accept IDs from the caller; ID generation belongs in the application layer.

## 5. Mapping

- Use Riok.Mapperly for object mapping.
- Do not use AutoMapper.
- Mapper classes belong in `Application/Mappings`.
- Mapper classes must be `partial` and annotated with `[Mapper]`.
- Use manual wrapper methods when mapping needs generated IDs, hashing, localization, or other non-trivial logic.

## 6. Validation and Localization

- Use FluentValidation for command/query validation.
- The MediatR validation pipeline triggers validation automatically.
- User-facing messages must be localized through `IStringLocalizer<SharedResource>` or service-owned resources.
- Business errors must expose stable error codes.
- Do not hard-code user-facing Vietnamese or English text in controllers, handlers, validators, filters, or middleware.

## 7. Messaging

- Use MassTransit for event-driven coordination.
- Publish integration events for cross-service side effects that do not need an immediate result.
- Use MassTransit request-response for narrow internal calls that require an immediate result.
- Do not use HTTP/gRPC for internal service-to-service workflows unless explicitly approved.
- External provider calls belong behind application abstractions and infrastructure implementations.

## 8. Authorization

- Use permission-based authorization.
- Add permission constants before exposing protected endpoints.
- Protect endpoints with `[HasPermission(...)]` or the existing permission mechanism.
- Do not hard-code business role names in feature code.
- `Administrator` is a reserved system role handled by centralized authorization logic.

## 9. Feature Workflow

Implement features in reviewed steps:

1. Domain and data: strongly typed IDs, entities, EF Core configuration, DbContext registration.
2. Application layer: DTOs/responses, Mapperly mappers, commands, queries, handlers, validators.
3. API and communication: controllers/GraphQL endpoints, MassTransit events, worker wiring.

Stop after each step for review when the task follows the project's iterative workflow.

For frontend or full-stack work, add a final browser validation step before reporting completion. Use the available browser automation / DevTools MCP to load the implemented page, check console and network errors, exercise the main user flow, verify permission gating and localization, and confirm React Query refresh after mutations. If browser tooling is unavailable, report that limitation explicitly.

## 10. Do and Do Not

- DO keep the Domain layer free of EF Core and infrastructure dependencies.
- DO use existing BuildingBlocks abstractions before adding new infrastructure.
- DO use `IQueryable` projections or dedicated response projections for read-heavy paths.
- DO centralize business/status/type string literals as constants.
- DO add tests proportional to the risk and blast radius of the change.
- DO run browser validation for frontend-impacting changes.
- DO NOT inject repositories into other repositories.
- DO NOT return EF Core entities directly from APIs.
- DO NOT introduce new libraries without explicit approval.
- DO NOT bypass localization, permission checks, or audit requirements for sensitive HR operations.
- DO NOT claim frontend completion based only on build, lint, TypeScript, or unit tests.
