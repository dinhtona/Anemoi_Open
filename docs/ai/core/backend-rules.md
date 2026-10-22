# Backend Engineering Rules

Normative rules are in `AGENTS.md` section 3. This file explains the expected project shape and patterns.

## Required Stack

Use the established Anemoi stack:

- The same TargetFramework as the existing repository projects (.NET 10).
- EF Core with `Npgsql.EntityFrameworkCore.PostgreSQL`.
- MassTransit with RabbitMQ.
- MediatR and OneOf.
- Riok.Mapperly.
- FluentValidation.
- Serilog.
- Polly.
- ASP.NET Core localization.

Do not introduce a different framework or library without explicit approval.

## Standard Project Layers

```text
{Service}.ModelIds
{Service}.Domain
{Service}.Application
{Service}.Infrastructure
{Service}.Api or {Service}.WorkerService
```

## Domain Layer

Contains entities, aggregate roots, value objects, domain events, strongly typed IDs, and domain services only when necessary.

Keep the domain free of EF Core, Application, Infrastructure, and Api dependencies. Domain factories accept IDs from the caller; ID generation belongs in the application layer.

## Application Layer

Contains commands, queries, handlers, validators, responses/DTOs, Mapperly mappers, and application abstractions.

Do not reference Infrastructure. Do not return EF Core entities. Use stable error codes for business errors and localization contracts for user-facing messages.

## Infrastructure Layer

Contains the DbContext, entity type configurations, migrations, repository implementations if used, MassTransit persistence/outbox, and external provider implementations.

## Api Layer

Contains controllers, GraphQL endpoints if used, `Program.cs`, DI registration, Swagger, MassTransit host wiring, rate limiting, and authorization configuration.

Controllers only dispatch commands/queries. They do not contain business logic or direct DbContext queries.

## CQRS Pattern

Every write action has:

```text
XxxCommand
XxxCommandHandler
XxxCommandValidator
```

Every read action has:

```text
XxxQuery
XxxQueryHandler
```

Return `OneOf<TResult, ErrorDetailResponse>` for expected business outcomes.

## Mapperly Pattern

Use Mapperly partial classes annotated with `[Mapper]`:

```csharp
[Mapper]
public partial class EmployeeMapper
{
    public partial EmployeeResponse ToResponse(Employee employee);
}
```

Do not use AutoMapper.

## Strongly Typed IDs

Use strongly typed IDs for local aggregate IDs:

```text
EmployeeId
DepartmentId
PositionId
LeaveRequestId
LeavePolicyId
LeaveBalanceId
LeaveTransactionId
```

Create new IDs in the application layer:

```csharp
new EmployeeId(IdGenerator.NextGuid())
```

Primitive IDs are allowed only at serialization boundaries such as DTOs, claims, headers, route values, and integration events.

## Event-Driven Patterns

Use past-tense integration event names:

```text
EmployeeCreatedIntegrationEvent
LeaveRequestSubmittedIntegrationEvent
LeaveRequestApprovedIntegrationEvent
LeaveBalanceChangedIntegrationEvent
SensitivePermissionAssignmentRequestedIntegrationEvent
```

Publish async side effects through MassTransit. Use request-response only when the current request cannot complete without the external result.

## Read-Heavy Queries

Use `IQueryable` projections or dedicated response projections. Do not load full entities when a projection is enough. Do not inject repositories into other repositories.
