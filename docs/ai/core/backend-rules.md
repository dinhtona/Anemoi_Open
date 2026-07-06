# Backend Engineering Rules

## Required Stack

Use the established Anemoi stack:

- Use the same TargetFramework as the existing repository projects. Do not introduce a different .NET version unless explicitly approved.
- EF Core
- Npgsql.EntityFrameworkCore.PostgreSQL
- MassTransit
- MassTransit.RabbitMQ
- MediatR
- OneOf
- Riok.Mapperly
- FluentValidation
- Serilog
- Polly
- ASP.NET Core Localization

Do not introduce random new libraries.

Do not use AutoMapper unless explicitly approved. Use Mapperly.

## Standard Project Layers

Each service follows:

```text
{Service}.ModelIds
{Service}.Domain
{Service}.Application
{Service}.Infrastructure
{Service}.Api
```

## Domain Layer

Contains:

```text
Entities
Aggregate Roots
Value Objects
Domain Events
Strongly typed IDs
Domain services only when necessary
```

Rules:

- Must not depend on Application, Infrastructure, or Api.
- Must not contain EF Core-specific persistence logic.
- Must use strongly typed IDs for local aggregate IDs.

## Application Layer

Contains:

```text
Commands
Queries
Handlers
Validators
Responses / DTOs
Mapperly mappers
Application abstractions
```

Rules:

- Must not depend on Infrastructure.
- Must not return EF Core entities.
- Must use CQRS.
- Must expose stable error codes for business errors.
- Must use localization contracts for user-facing messages.

## Infrastructure Layer

Contains:

```text
DbContext
EntityTypeConfiguration
Migrations
Repository implementations if used
MassTransit persistence / outbox if used
External provider implementations
```

## Api Layer

Contains:

```text
Controllers
GraphQL endpoints if used
Program.cs
DI registration
Swagger
MassTransit host wiring
Rate limiting
Authorization configuration
```

Rules:

- Controllers must only dispatch Commands/Queries.
- Controllers must not contain business logic.
- Controllers must not query DbContext directly.
- Protected endpoints must use permission authorization.

## CQRS Rules

Every write action requires:

```text
XxxCommand
XxxCommandHandler
XxxCommandValidator
```

Every read action requires:

```text
XxxQuery
XxxQueryHandler
```

## Mapperly Rules

Use Mapperly partial classes.

Example:

```csharp
[Mapper]
public partial class EmployeeMapper
{
    public partial EmployeeResponse ToResponse(Employee employee);
}
```

## Strongly Typed ID Rules

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

Primitive IDs are allowed only at serialization boundaries such as DTOs, claims, headers, and integration events if needed.

## Event-Driven Rules

Use MassTransit for cross-service side effects.

Examples:

```text
EmployeeCreatedIntegrationEvent
LeaveRequestSubmittedIntegrationEvent
LeaveRequestApprovedIntegrationEvent
LeaveBalanceChangedIntegrationEvent
SensitivePermissionAssignmentRequestedIntegrationEvent
```

Prefer events for asynchronous side effects.

Use MassTransit request-response only when the current request cannot complete without the external result.
