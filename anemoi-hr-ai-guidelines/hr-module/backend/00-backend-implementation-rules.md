# HR Backend Implementation Rules

## Mandatory rules

Before coding, read:

```text
shared/00-backend-architecture-guide.md
hr-module/00-hr-platform-overview.md
hr-module/01-domain-design-guide.md
hr-module/02-masterdata-category-guide.md
hr-module/03-permission-sensitive-guide.md
hr-module/04-approval-workflow-guide.md
```

## Layering

Follow Clean Architecture strictly:

```text
Domain:
  Entities, Value Objects, Domain Events, Strongly Typed IDs

Application:
  CQRS Commands/Queries, Handlers, DTOs, Mapperly mappings, Validators

Infrastructure:
  EF Core DbContext, EntityTypeConfiguration, Repositories, migrations, external integrations

Api/WorkerService:
  Controllers/GraphQL, DI, MassTransit, hosted jobs
```

## CQRS

Every feature must have explicit commands and queries.

Examples:

```text
CreateLeaveRequestCommand
ApproveLeaveRequestCommand
GetMyLeaveRequestsQuery
GetLeaveBalanceQuery
```

## Validation

Use FluentValidation. Do not put validation rules directly in controllers.

## Mapping

Use Riok.Mapperly. Do not introduce AutoMapper.

## Authorization

Every protected endpoint must use permission constants.

Do not hard-code permission strings in controllers.

## Localization

User-facing messages must be localized via backend localization resources.

Return stable error codes.

## Events

Use MassTransit for cross-service communication.

Recommended HR events:

```text
EmployeeCreated
EmployeeUpdated
DepartmentChanged
LeaveRequestSubmitted
LeaveRequestApproved
LeaveRequestRejected
LeaveBalanceChanged
MonthlyLeaveAccrued
SalaryChangeRequested
SalaryChanged
ContractExpiredSoon
SensitivePermissionAssignmentRequested
SensitivePermissionAssigned
```

## Background jobs

Monthly leave accrual must be idempotent.

Use a unique key for EmployeeId + YearMonth.

Never add 1.25 days twice for the same employee and month.
