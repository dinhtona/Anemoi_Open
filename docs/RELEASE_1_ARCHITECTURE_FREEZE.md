# Release 1 Architecture Freeze

This document defines the frozen architecture for Anemoi HR Platform Release 1. No future feature or refactor may violate these rules without an approved Architecture Decision Record (ADR).

## 1. Clean Architecture Boundaries

All services follow Clean Architecture with strictly separated layers:

```
API / WorkerService      → Controllers, endpoints, startup
Application              → CQRS handlers, DTOs, validators, mappers
Domain                   → Aggregates, entities, value objects, domain events
Infrastructure           → EF Core persistence, MassTransit, external clients
BuildingBlocks           → Shared kernel, base classes, authorization, helpers
```

Rules:
- Controllers must not query DbContext or contain business logic
- Domain objects must not depend on infrastructure
- Application handlers return `OneOf<TResult, ErrorDetailResponse>`
- Use Mapperly (not AutoMapper)
- Use FluentValidation for all request validation
- Use strongly typed IDs for local aggregate identifiers

## 2. CQRS Rules

| Direction | Pattern | Purpose |
|-----------|---------|---------|
| Commands  | `XxxCommand` + `XxxCommandHandler` | Create, Update, Submit, Approve, Reject, Cancel |
| Queries   | `XxxQuery` + `XxxQueryHandler`   | Lists, Details, Dashboards, Reports |

Rules:
- Handlers must not mix read/write concerns
- Commands must produce domain events when state changes
- Queries may use raw SQL or projections for performance

## 3. Strongly Typed IDs

All aggregate IDs use strongly typed ID wrappers:
```csharp
public readonly record struct EmployeeId(Guid Value) { ... }
```

Rules:
- New IDs created via `IdGenerator.NextGuid()`
- No `Guid.NewGuid()` in production code
- Strongly typed IDs are ignored in EF model (`modelBuilder.Ignore<>`)

## 4. Three-Scope Architecture (ADR-027)

Every HR module must separate three perspectives:

| Scope    | Route Prefix | Purpose |
|----------|-------------|---------|
| Employee | `/ess/*`    | Current employee's own data |
| Manager  | `/manager/*` | Data under manager's approval scope |
| HR/Admin | `/hr/*`     | Organization-wide data |

Rules:
- ESS endpoints must scope queries to the authenticated employee
- Manager approval endpoints must only return items the current user is responsible to approve
- HR pages must not default to the current user or manager scope
- Permission conventions: `hr.ess.*`, `*.approve`, `hr.*.view`/`hr.*.manage`

## 5. Workflow Engine (ADR-028, ADR-029)

The Workflow Engine is the single source of truth for approvals.

### Required Entity Types (Definition-Bound)

All of the following must have active `WorkflowDefinition` records:

| Entity Type           | Status     |
|-----------------------|------------|
| LeaveRequest          | Required   |
| OvertimeRequest       | Required   |
| PayrollRun            | Required   |
| RecruitmentRequest    | Required   |
| EmployeeTransfer      | Required   |
| EmployeeSeparation    | Required   |
| ProbationRecord       | Required   |

Rules:
- Hierarchy fallback (`BuildFromHierarchyAsync`) is NOT allowed for production
- All required types must have seed definitions
- New types require: (1) add to RequiredEntityTypes, (2) add seed definition, (3) architecture review

### Workflow Steps

Default workflow: 2-step approval (Direct Manager → HR Manager)

### Approval Flow

1. Submit target entity → WorkflowInstance created from active definition
2. Current step resolves to designated approver
3. Non-approver cannot approve/reject
4. Approve/Reject through `ApproveWorkflowStepCommand` / `RejectWorkflowStepCommand`
5. Final approval updates target entity status via target status updaters
6. Rejection updates target entity according to business rules
7. History is append-only and auditable

## 6. Permission Model

All permissions are defined in:
```
Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/Permissions.cs
```

Three concepts must remain separate:
1. **Employee Position** — never grants permissions or JWT claims
2. **System Role / Role Group** — the only source of system permissions
3. **Workflow Role** — approval routing only; never grants system access

Rules:
- Frontend authorization uses `user.permissions`, never `user.roles`
- Module-local permission constants delegate to central `Permissions.cs`
- Adding a permission requires: constant + definition + EN/VI localization keys + delegate constant
- `Permissions.All` is the authoritative catalog for admin role group seeding

## 7. Notification Model

- Notifications follow the pattern: Entity + Event → Queue → Notification Service → Persist + WebSocket
- SignalR hub: `/hubs/notification`
- Real-time delivery via SignalR WebSocket
- Persistence in PostgreSQL + MongoDB
- Notification types: workflow approval, assignment, status changes, system alerts

## 8. Employee Lifecycle Model

| Entity               | Table             | Schema |
|----------------------|-------------------|--------|
| Employee              | Employees         | public |
| EmployeeTransfer      | EmployeeTransfers | Hr     |
| EmployeeSeparation    | EmployeeSeparations| Hr     |
| ProbationRecord       | ProbationRecords  | Hr     |
| EmployeeHistory       | EmployeeHistories | Hr     |
| OrganizationHistory   | EmployeeOrganizationHistories | Hr |

Lifecycle events are recorded in `EmployeeHistories` as append-only audit entries.

## 9. Domain Events vs Integration Events

| Type              | Purpose | Transport |
|-------------------|---------|-----------|
| Domain Events     | Within-bounded-context notification | In-process |
| Integration Events| Cross-service communication | MassTransit + RabbitMQ |

## 10. Frontend Standards

- Framework: Next.js (in `cody-web-app/`)
- Authorization: `user.permissions` only
- Permission rendering: API-localized group/description, no client-side translation
- Missing translations must cause test failure
- Three-Scope routing: `/ess/*`, `/manager/*`, `/hr/*`, `/en/notifications`, `/en/roles`
- API calls: through services layer
- State management: React Query + hooks
- Localization: `messages/en.json` + `messages/vi.json`

## 11. Production Hardening Rules

As of Sprint 1:

| Rule                                      | Status     |
|-------------------------------------------|------------|
| No hardcoded credentials in appsettings   | Enforced   |
| HttpLoggingFields gated to Development    | Enforced   |
| No docker.sock mount in application containers | Enforced |
| No `user: root` on application containers | Enforced   |
| Health checks on API services             | Enforced   |
| Backup/restore documentation              | Present    |
| Observability baseline                    | Present    |

## 12. Amendment Process

Any change to this document requires:
1. A written Architecture Decision Record (ADR) in `docs/architecture/`
2. Approval via architecture review
3. Update to this document
4. Verification that no frozen rule is broken without ADR

Violations without an ADR are release-blocking defects.
