# Anemoi Open - Platform Architecture & Guidelines

Normative rules are in `AGENTS.md`. This guide explains the architecture and the intended service shape.

## 1. System Overview

`Anemoi_Open` uses a microservices architecture with Docker Compose for local orchestration. Services communicate asynchronously through RabbitMQ and follow Clean Architecture with CQRS. Shared building blocks live in `Anemoi.BuildingBlocks`.

Key services:

- `anemoi_centralize`: gateway API and aggregation service.
- `anemoi_identity`: authentication, authorization, users, and role groups.
- `anemoi_masterdata`: shared reference data.
- `anemoi_workspace`: workspace management.
- `anemoi_orchestrator`: saga orchestration for distributed transactions.
- `anemoi_notification`: notification and email delivery.
- `anemoi_secure`: security and encryption.
- `anemoi_hr`: HR bounded context.

Infrastructure:

- RabbitMQ for asynchronous messaging.
- PostgreSQL for all services, including MassTransit EF Core saga persistence.

## 2. Core Stack

Use the established stack:

- .NET 10 / ASP.NET Core 10.
- Clean Architecture + CQRS + event-driven design.
- EF Core with `Npgsql.EntityFrameworkCore.PostgreSQL`.
- MassTransit with RabbitMQ and EF Core persistence.
- Riok.Mapperly (not AutoMapper).
- FluentValidation.
- MediatR with OneOf.
- HotChocolate for GraphQL where used.
- Serilog.
- Polly.
- Strongly typed IDs instead of raw `Guid` or `int` identifiers.

Do not substitute or add frameworks without explicit approval.

## 3. Service Structure

Each service follows:

```text
{Service}.ModelIds
{Service}.Domain
{Service}.Application
{Service}.Infrastructure
{Service}.Api or {Service}.WorkerService
```

- `Domain` is independent and contains entities, value objects, domain events, and typed IDs.
- `Application` contains CQRS use cases, DTOs, Mapperly mappers, validators, and abstractions. It does not reference Infrastructure.
- `Infrastructure` contains EF Core persistence, migrations, repository implementations, and external providers.
- `Api` / `WorkerService` configures DI, Serilog, rate limiting, MassTransit, controllers or GraphQL, and references Infrastructure.
- `Contract` contains cross-service DTOs and integration events and stays implementation-free.

## 4. Localization, Authorization, And Conventions

These rules are defined in one place:

- Localization: `docs/ai/core/localization.md`.
- Permissions and authorization: `AGENTS.md` section 5 and `docs/ai/security/permission-model.md`.
- Backend patterns: `docs/ai/core/backend-rules.md`.
- Verification: `docs/ai/core/verification-standard.md`.
- Workflow: `docs/ai/workflows/approval-workflow.md` and ADR-027/028/029.

## 5. Iterative Execution

When implementing a feature, stop for review after each step:

1. Domain and data: typed IDs, entities, EF Core configuration, DbContext registration.
2. Application layer: DTOs/responses, Mapperly mappers, commands, queries, handlers, validators.
3. API and communication: controllers/GraphQL endpoints, MassTransit events, worker wiring.
4. Frontend or full-stack: browser validation before reporting completion.

Do not proceed to the next step until the current step is reviewed and approved.
