# Anemoi Development Guidelines

This document summarizes development conventions for Anemoi_Open. Normative rules live in `AGENTS.md`; approved decisions in [docs/architecture/ARCHITECTURE_DECISIONS.md](docs/architecture/ARCHITECTURE_DECISIONS.md) take precedence over this guide.

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
| --- | --- | --- |
| ModelIds | Strongly typed ID wrappers only | Depends only on shared domain primitives |
| Domain | Entities, aggregate roots, value objects, domain events, domain rules | Must not depend on Application, Infrastructure, or Api |
| Application | CQRS use cases, handlers, validators, DTOs, Mapperly mappers, abstractions | Must not depend on Infrastructure |
| Infrastructure | EF Core DbContext, mappings, migrations, repositories, external providers | Depends on Application and Domain |
| Api / WorkerService | Host startup, DI, controllers, GraphQL, MassTransit wiring | Depends on Infrastructure |
| Contract | Cross-service DTOs, integration events, shared wire contracts | Must stay implementation-free |

## 3. CQRS, Results, And Mapping

- Use command/handler/validator for writes and query/handler for reads.
- Handlers return `OneOf<TResult, ErrorDetailResponse>` for expected outcomes.
- Do not throw for normal business validation failures.
- Use Riok.Mapperly. Mapper classes are `partial` and annotated with `[Mapper]` in `Application/Mappings`.
- Do not use AutoMapper.
- Use manual wrapper methods when mapping needs generated IDs, hashing, localization, or other non-trivial logic.

## 4. Strongly Typed IDs

- Use typed ID wrappers such as `EmployeeId`, `UserId`, or `PayrollRunId` for local aggregate IDs.
- Create new IDs in the application layer with `new XxxId(IdGenerator.NextGuid())`.
- Domain factories accept IDs from the caller.
- Primitive IDs are allowed at wire boundaries only: DTOs, headers, claims, route values, and integration events.
- Convert primitive IDs to strongly typed IDs before using them in domain or application logic.

## 5. Validation, Localization, And Messaging

- Use FluentValidation; the MediatR validation pipeline triggers validation automatically.
- Localize user-facing messages through `IStringLocalizer<SharedResource>` or service-owned resources.
- Business errors expose stable error codes.
- Use MassTransit for event-driven coordination and internal request-response.
- Do not use HTTP/gRPC for internal service-to-service workflows unless explicitly approved.
- Put external provider calls behind application abstractions and infrastructure implementations.

## 6. Authorization

- Use permission-based authorization and central permission constants.
- Protect endpoints with `[HasPermission(...)]`.
- Do not hard-code business role names in feature code.
- `Administrator` is a reserved system role handled by centralized authorization logic.
- See `docs/ai/security/permission-model.md` for the permission catalog and Identity mapping.

## 7. Development JWT Key Ownership

- Development JWT signing keys must stay outside the repository.
- `Anemoi.Identity` is the only service allowed to create the development JWT key pair.
- `Anemoi.Centralize` and `Anemoi.Hr` treat JWT keys as read-only and load only the configured public key.
- If the public key is missing in Development, start `Anemoi.Identity` first or ensure the shared `jwt_keys` Docker volume already contains the key pair.
- Do not reintroduce startup logic that lets non-Identity services generate JWT signing keys.

## 8. Feature Workflow

Implement features in reviewed steps:

1. Domain and data: strongly typed IDs, entities, EF Core configuration, DbContext registration.
2. Application layer: DTOs/responses, Mapperly mappers, commands, queries, handlers, validators.
3. API and communication: controllers/GraphQL endpoints, MassTransit events, worker wiring.
4. Frontend or full-stack: complete browser validation before reporting completion.

Stop after each step for review when the task follows the project's iterative workflow.

## 9. Additional Conventions

- Keep the Domain layer free of EF Core and infrastructure dependencies.
- Use existing BuildingBlocks abstractions before adding new infrastructure.
- Use `IQueryable` projections or dedicated response projections for read-heavy paths.
- Centralize business/status/type string literals as constants.
- Add tests proportional to risk and blast radius.
- Do not inject repositories into other repositories.
- Do not return EF Core entities directly from APIs.
- Do not introduce new libraries without explicit approval.
- Do not bypass localization, permission checks, or audit requirements for sensitive HR operations.

## 10. Verification

Use the statuses and evidence rules in `docs/ai/core/verification-standard.md` and `AGENTS.md` section 8. Code review alone is not evidence.

For frontend or full-stack work, browser validation is the source of truth:

```text
Browser -> Network -> API -> Database -> Code
```

Frontend rules live in `cody-web-app/AGENTS.md` and `cody-web-app/frontend-guidelines.md`.

If browser tooling is unavailable, report that limitation explicitly instead of claiming browser validation.
