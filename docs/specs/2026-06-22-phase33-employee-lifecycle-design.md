# Phase 33: Employee Lifecycle & HR Core Process Automation

**Date:** 2026-06-22
**Status:** Draft (pre-implementation)
**Phase:** 33

## Overview

Build a complete Employee Lifecycle from Recruitment → Onboarding → Active → Transfer → Separation. This phase connects the previously isolated modules (Recruitment, Onboarding, Workflow) into an end-to-end process automation system.

## Architecture Principles

- Clean Architecture, CQRS, Event-Driven
- Domain events for state changes → handlers for side effects
- Workflow Engine (Phase 29) for approval processes
- No business logic in Controllers
- No Guid.NewGuid() — use IdGenerator.NextGuid()
- xmin concurrency for all entities
- Append-only EmployeeHistory (domain event handlers write)
- IWorkflowTargetStatusUpdater handles post-approval business logic
- CorrelationId propagation across all domain events for full audit trail
- Standardized MetadataJson format per event type (one schema per EventType)

## Module 1: Employee Lifecycle State Machine

### EmployeeStatusCode

Replace the binary Active/Inactive with a full lifecycle:

```csharp
Draft = "Draft"
PendingOnboarding = "PendingOnboarding"
Onboarding = "Onboarding"
Active = "Active"
Suspended = "Suspended"
Resigned = "Resigned"
Terminated = "Terminated"
Archived = "Archived"
```

Valid transitions:
- Draft → PendingOnboarding (when hired from recruitment)
- PendingOnboarding → Onboarding (when onboarding starts)
- Onboarding → Active (when onboarding completes)
- Active ↔ Suspended (suspend/resume)
- Active → Resigned (voluntary resignation)
- Active → Terminated (involuntary termination)
- Resigned/Terminated → Archived (final archiving)

### Domain Methods on Employee

- `StartOnboarding()` — PendingOnboarding → Onboarding
- `Activate()` — Onboarding → Active, or Suspended → Active
- `Suspend()` — Active → Suspended
- `Resume()` — Suspended → Active
- `Resign()` — Active → Resigned
- `Terminate()` — Active → Terminated
- `Archive()` — Resigned/Terminated → Archived

Each method validates the current status, checks valid transitions, fires `EmployeeStatusChangedDomainEvent`.

### Onboarding Completion → Activate

When `OnboardingInstanceCompletedDomainEvent` fires (all onboarding tasks done), an event handler calls `employee.Activate()`:

```
OnboardingInstanceCompletedDomainEvent
  → OnboardingCompletedActivateEmployeeHandler
    → employee.Activate()
    → EmployeeStatusChangedDomainEvent
    → EmployeeHistory handler writes "Status: Onboarding → Active"
```

This ensures employee cannot be marked Active until onboarding is fully complete.

### Migration

- Active → Active
- Inactive → Archived
- Backward-compatible: retain old field temporarily, deprecate, phase out

### EmployeeOrganizationHistory (Assignment History for Reporting)

New entity tracking the employee's organizational assignment snapshot over time. Unlike the existing individual history tables (EmployeeDepartmentHistory, EmployeePositionHistory, EmployeeGradeHistory, EmployeeManagerHistory), this captures the composite state at a point in time for reporting and analytics.

**Entity:** `EmployeeOrganizationHistory : Entity<EmployeeOrganizationHistoryId>`
**Schema:** `"Hr"."EmployeeOrganizationHistories"`

**Fields:**
- `EmployeeOrganizationHistoryId Id`
- `EmployeeId EmployeeId`
- `DepartmentId DepartmentId`
- `PositionId PositionId`
- `EmployeeId? ManagerEmployeeId`
- `string GradeCode`
- `DateOnly EffectiveDate`
- `DateOnly? EndDate` (null = current assignment)
- `string ChangeReasonCode` (Initial, Transfer, Promotion, Demotion, Reorganization)

**Index:** `EmployeeId + EffectiveDate DESC` (for current assignment lookup), `DepartmentId + EffectiveDate` (department roster at a point in time)

**Rules:**
- Written by the same status updaters that handle transfer/separation actions
- `EndDate` is set on the previous record when a new assignment starts
- Enables reporting queries like "who was in Department X on date Y" without complex joins
- Not a source of truth for current state — Employee entity fields are the source of truth

### Refactor Employee base class

**Verified:** `Employee : ValueObject` at `Anemoi.Hr.Domain/Employees/Employee.cs:8`. No `_domainEvents`, no `AddEvent()`/`ClearEvents()`.

Change to `Entity<EmployeeId>` to support domain events:
- Remove `GetEqualityComponents()` — `Entity<TId>` handles equality via `Id`
- Inherit `_domainEvents` list, `AddEvent()`, `ClearEvents()` from `Entity<TId>`
- Domain methods (Activate, Suspend, Resign, etc.) must call `this.AddEvent(new EmployeeStatusChangedDomainEvent(...))`

### New Domain Events

- `EmployeeCreatedDomainEvent`
- `EmployeeStatusChangedDomainEvent`

### New Integration Events

- `EmployeeCreatedIntegrationEvent` — queue: `employee-created`
- `EmployeeStatusChangedIntegrationEvent` — queue: `employee-status-changed`

## Module 2: Recruitment Integration

### Flow

When `CandidateApplicationStageCode = Hired` + `HiringDecision = Hire`:

1. `CandidateHiredDomainEvent` fires from CandidateApplication stage transition
2. Consumer: `CreateEmployeeFromCandidateConsumer`
3. Creates:
   - Employee record (EmployeeStatusCode = PendingOnboarding)
   - Employee number (auto-generated)
   - OnboardingInstance (Draft status)
4. Links Candidate.EmployeeId to the new Employee
5. Publishes `EmployeeCreatedIntegrationEvent`

### Initial EmployeeOrganizationHistory

When Employee is created (either from recruitment or manually), an event handler for `EmployeeCreatedDomainEvent` must create the initial `EmployeeOrganizationHistory` record with:
- `DepartmentId` = employee's `PrimaryDepartmentId`
- `PositionId` = employee's `PrimaryPositionId`
- `ManagerEmployeeId` = employee's `DirectManagerEmployeeId`
- `GradeCode` = employee's `GradeCode`
- `EffectiveDate` = employee's `JoinDate`
- `ChangeReasonCode` = `Initial`
- `EndDate` = null (current assignment)

This ensures every employee always has at least one organization history record from day one, enabling reliable reporting queries.

### Changes to Existing Code

Modify `ConvertCandidateToEmployeeHandler`:
- Instead of setting Active directly, set PendingOnboarding
- Auto-create OnboardingInstance after employee creation
- Publish integration event (currently exists but unused)

## Module 3: Probation Management

### Entity: ProbationRecord

- Base: `Entity<ProbationRecordId>` (domain events)
- Namespace: `Anemoi.Hr.Domain.Probation`
- Schema: `"Hr"."ProbationRecords"`

**Fields:**
- `ProbationRecordId Id`
- `EmployeeId EmployeeId`
- `DateOnly StartDate`
- `DateOnly EndDate`
- `ProbationStatusCode StatusCode` (Pending, Passed, Failed, Extended)
- `string Result` (nullable — details on pass/fail)
- `string Comment` (nullable)
- `EmployeeId? ReviewerEmployeeId`
- `Xmin` concurrency

**Methods:**
- `Start(employeeId, startDate, endDate)` — static factory
- `Pass(result, comment, reviewerId)` — fires `ProbationStatusChangedDomainEvent`
- `Fail(comment, reviewerId)` — fires `ProbationStatusChangedDomainEvent`
- `Extend(newEndDate)` — fires `ProbationStatusChangedDomainEvent`

### Commands

- `StartProbationCommand` → `StartProbationHandler`
- `PassProbationCommand` → `PassProbationHandler`
- `FailProbationCommand` → `FailProbationHandler`
- `ExtendProbationCommand` → `ExtendProbationHandler`

### Queries

- `GetProbationsQuery` (filter: status, employeeId, date range)
- `GetProbationByIdQuery`

### Domain Events

- `ProbationStatusChangedDomainEvent`

### Integration Events (conditional)

- `ProbationPassedIntegrationEvent` — only if notification/payroll consumer exists

### Flow

- Pass/Fail/Extended only update `ProbationRecord.StatusCode`. They do NOT change `Employee.EmploymentStatusCode`.
- Employee status changes are owned by the lifecycle state machine (onboarding completion → Active, separation → Resigned/Terminated), not by probation outcomes.
- Fail: separation process can be triggered manually via Separation module.

## Module 4: Employee Transfer

### Entity: EmployeeTransfer

- Base: `Entity<EmployeeTransferId>` (domain events)
- Namespace: `Anemoi.Hr.Domain.Transfers`
- Schema: `"Hr"."EmployeeTransfers"`

**Fields:**
- `EmployeeTransferId Id`
- `EmployeeId EmployeeId`
- `DepartmentId FromDepartmentId`, `DepartmentId ToDepartmentId`
- `PositionId FromPositionId`, `PositionId ToPositionId`
- `EmployeeId? FromManagerId`, `EmployeeId? ToManagerId`
- `string FromGradeCode`, `string ToGradeCode`
- `DateOnly EffectiveDate`
- `string Reason`
- `TransferStatusCode StatusCode` (Draft, PendingApproval, Approved, Rejected, Cancelled)
- `WorkflowInstanceId? WorkflowInstanceId`

**Methods:**
- `Submit()` — Draft → PendingApproval
- `Approve()` — PendingApproval → Approved, fires `TransferApprovedDomainEvent`
- `Reject(reason)` — PendingApproval → Rejected, fires `TransferRejectedDomainEvent`
- `Cancel()` — any → Cancelled

### Workflow

- `WorkflowConstants.TargetEntityTypes.EmployeeTransfer`
- Steps: DirectManager → HR Manager (seed data)
- On approve: `EmployeeTransferWorkflowStatusUpdater` loads Employee, updates department/position/manager/grade, writes `EmployeeOrganizationHistory` snapshot, fires domain events
- On reject: workflow rejected, transfer status → Rejected
- Approve/reject only via existing workflow endpoints, not custom endpoints

### Commands

- `SubmitTransferCommand` — creates EmployeeTransfer + starts workflow

### Queries

- `GetTransfersQuery` (filter: status, employeeId, dateFrom, dateTo, pagination, sort: CreatedAt DESC)
- `GetTransferByIdQuery`

### Domain Events

- `TransferSubmittedDomainEvent`
- `TransferApprovedDomainEvent`
- `TransferRejectedDomainEvent`

### Integration Events

- `TransferApprovedIntegrationEvent` — queue: `transfer-approved`

## Module 5: Employee Separation

### Entity: EmployeeSeparation

- Base: `Entity<EmployeeSeparationId>` (domain events)
- Namespace: `Anemoi.Hr.Domain.Separations`
- Schema: `"Hr"."EmployeeSeparations"`

**Fields:**
- `EmployeeSeparationId Id`
- `EmployeeId EmployeeId`
- `SeparationTypeCode SeparationType` (Resignation, Termination, Retirement)
- `string Reason`
- `DateOnly LastWorkingDate`
- `SeparationStatusCode StatusCode` (Draft, PendingApproval, Approved, Rejected, Cancelled)
- `WorkflowInstanceId? WorkflowInstanceId`

**Methods:**
- `Submit()` — Draft → PendingApproval
- `Approve()` — PendingApproval → Approved, fires `SeparationApprovedDomainEvent`
- `Reject(reason)` — PendingApproval → Rejected, fires `SeparationRejectedDomainEvent`
- `Cancel()` — any → Cancelled

### Workflow

- `WorkflowConstants.TargetEntityTypes.EmployeeSeparation`
- Steps: DirectManager → HR Manager (seed data)
- On approve: `EmployeeSeparationWorkflowStatusUpdater` loads Employee, calls `Resign()`/`Terminate()` per type, handles side effects
- On reject: workflow rejected, separation status → Rejected, Employee unchanged

### Side Effects (in application handlers, not entity)

On separation approved:
1. Employee status → Resigned/Terminated (via domain method)
2. Close active onboarding instances (only InProgress/Draft — not Completed)
3. Cancel pending leave/overtime requests (only Pending — not Approved)
4. Archive employee subscriptions (future)
5. Write EmployeeHistory via domain event handlers

**Critical rule: Separation handlers must NOT cancel or modify already-approved/completed data.**
- Approved leave/overtime: untouched
- Completed onboarding: untouched
- Approved contracts: untouched
- Paid payroll runs: untouched
- Only pending/in-progress items are affected.
- This aligns with the domain principle: approved business transactions are immutable.

### Commands

- `SubmitSeparationCommand` — creates EmployeeSeparation + starts workflow

### Queries

- `GetSeparationsQuery` (filter: status, type, employeeId, date range, pagination, sort: CreatedAt DESC)
- `GetSeparationByIdQuery`

### Domain Events

- `SeparationSubmittedDomainEvent`
- `SeparationApprovedDomainEvent`
- `SeparationRejectedDomainEvent`

### Integration Events

- `SeparationApprovedIntegrationEvent` — queue: `separation-approved`

## Module 6: Employee History Timeline

### Entity: EmployeeHistory

- Base: `Entity<EmployeeHistoryId>` (append-only read model)
- Namespace: `Anemoi.Hr.Domain.Employees`
- Schema: `"Hr"."EmployeeHistories"`

**Fields:**
- `EmployeeHistoryId Id`
- `EmployeeId EmployeeId`
- `string EntityType` — aggregate type that produced this event (Employee, ProbationRecord, EmployeeTransfer, EmployeeSeparation, Contract, Compensation)
- `string EntityId` — string representation of the source aggregate's strongly typed ID (e.g., "ProbationRecordId:...")
- `string EventType` — standardized code (Created, StatusChanged, ProbationPassed, TransferApproved, SeparationApproved, ContractChanged, SalaryChanged)
- `string Title` — localized display title
- `string Description` — localized detail
- `string MetadataJson` — standardized JSON payload per EventType (see schema table below)
- `DateTime OccurredAt`
- `Guid? ActorUserId`
- `EmployeeId? ActorEmployeeId`
- `string CorrelationId`

**Index:** `EmployeeId + OccurredAt DESC`

**Rules:**
- Append-only, never updated or deleted
- Written only by domain event handlers implementing `INotificationHandler<T>`
- Not used for current state calculation

### Event Handlers (Timeline Writers)

| Domain Event | EntityType | EventType | Title |
|---|---|---|---|
| `EmployeeCreatedDomainEvent` | Employee | `Created` | "Employee Created" |
| `EmployeeStatusChangedDomainEvent` | Employee | `StatusChanged` | "Status: {Old} → {New}" |
| `ProbationStatusChangedDomainEvent` | ProbationRecord | `Probation{Status}` | "Probation {Passed/Failed/Extended}" |
| `TransferApprovedDomainEvent` | EmployeeTransfer | `TransferApproved` | "Transferred: {From} → {To}" |
| `SeparationApprovedDomainEvent` | EmployeeSeparation | `SeparationApproved` | "Separation: {Type}" |
| `ContractChangedDomainEvent` | Contract | `ContractChanged` | "Contract: {OldType} → {NewType}" (future) |
| `SalaryChangedDomainEvent` | Compensation | `SalaryChanged` | "Salary: {Old} → {New}" (future) |

### MetadataJson Schema (Standardized Per EventType)

| EventType | MetadataJson Schema |
|---|---|
| `Created` | `{}` |
| `StatusChanged` | `{"fromStatus": "..", "toStatus": ".."}` |
| `ProbationPassed` | `{"result": "..", "reviewerId": ".."}` |
| `ProbationFailed` | `{"comment": "..", "reviewerId": ".."}` |
| `ProbationExtended` | `{"newEndDate": "..", "previousEndDate": ".."}` |
| `TransferApproved` | `{"fromDepartment": "..", "toDepartment": "..", "fromPosition": "..", "toPosition": "..", "fromManager": "..", "toManager": "..", "fromGrade": "..", "toGrade": ".."}` |
| `SeparationApproved` | `{"type": "..", "lastWorkingDate": "..", "reason": ".."}` |
| `ContractChanged` | `{"fromType": "..", "toType": "..", "fromDate": "..", "toDate": ".."}` (future) |
| `SalaryChanged` | `{"fromAmount": .., "toAmount": .., "fromGrade": "..", "toGrade": ".."}` (future) |

### CorrelationId Propagation

All domain events in this phase accept an optional `CorrelationId`:
- `TransferApprovedDomainEvent`, `SeparationApprovedDomainEvent`, `ProbationStatusChangedDomainEvent`
- The `CorrelationId` originates from the HTTP request or integration event and flows through every handler
- Enables full audit trail: "this transfer approval → this employee status change → this timeline entry"

### Query

- `GetEmployeeTimelineQuery(EmployeeId, page, pageSize, eventType?, dateFrom?, dateTo?, entityType?)`

## Module 7: Default Workflow Templates (Seed Data)

Seed data adds default workflow definitions. These are **default templates, not mandatory workflows**.

If no active `WorkflowDefinition` exists for an entity type, the Workflow Engine already falls back to hierarchy-based resolution (`WorkflowConstants.DefaultPolicy` in `WorkflowBuilder`). The same behavior applies for EmployeeTransfer and EmployeeSeparation — the seed is a convenience, not a requirement.

### EmployeeTransferApproval (Default)

| Step | Sequence | ApproverType | ApproverValue |
|---|---|---|---|
| 1 | 1 | DirectManager | — |
| 2 | 2 | HrManager | — |

### EmployeeSeparationApproval (Default)

| Step | Sequence | ApproverType | ApproverValue |
|---|---|---|---|
| 1 | 1 | DirectManager | — |
| 2 | 2 | HrManager | — |

### IWorkflowTargetStatusUpdater Implementations

- `EmployeeTransferWorkflowStatusUpdater` — on approved: load EmployeeTransfer, call `Approve()`, load Employee, update snapshot (dept/pos/manager/grade), `SaveChanges` → domain events → timeline
- `EmployeeSeparationWorkflowStatusUpdater` — on approved: load EmployeeSeparation, call `Approve()`, load Employee, call `Resign()`/`Terminate()`, close `EmployeeOrganizationHistory` (set `EndDate = separation.LastWorkingDate`, not workflow approval date), `SaveChanges` → domain events → timeline + side effects

## Module 8: Notifications

### New Notification Constants

```csharp
// In NotificationWorkflowConstants
TargetServices.HrLifecycle = "HrLifecycle"
ActionCodes: ViewTransfer, ViewSeparation, ViewProbation
```

### Notification Consumers

For each integration event with a notification need:
- `EmployeeCreatedConsumer` — notify manager: "New employee joins your team"
- `TransferApprovedConsumer` — notify employee + both managers
- `SeparationApprovedConsumer` — notify HR + manager
- `ProbationPassedConsumer` — notify employee (if implemented)

Pattern follows existing consumers: integration event → `CreateNotificationCommand` via MediatR.

## Module 9: Frontend

### Routes

| Route | Page | Description |
|---|---|---|
| `/hr/dashboard` | `DashboardPage` | 4 widget cards: Probations Expiring, Pending Transfers, Pending Separations, New Employees |
| `/hr/employees/probations` | `ProbationListPage` | CRUD + pass/fail/extend actions |
| `/hr/employees/transfers` | `TransferListPage` | Submit transfer dialog + status tracking |
| `/hr/employees/separations` | `SeparationListPage` | Submit separation dialog + status tracking |
| `/hr/employees/[id]/timeline` | `EmployeeTimelinePage` | Filterable timeline |
| `/ess/profile/history` | `ProfileHistoryPage` | Self-service timeline |

### API Endpoints

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/hr/dashboard/lifecycle-summary` | Dashboard widget data |
| GET/POST | `/api/hr/probations` | List/Create probation |
| POST | `/api/hr/probations/{id}/pass` | Pass probation |
| POST | `/api/hr/probations/{id}/fail` | Fail probation |
| POST | `/api/hr/probations/{id}/extend` | Extend probation |
| GET | `/api/hr/probations/{id}` | Single probation |
| GET/POST | `/api/hr/transfers` | List/Submit transfer |
| GET | `/api/hr/transfers/{id}` | Single transfer |
| GET/POST | `/api/hr/separations` | List/Submit separation |
| GET | `/api/hr/separations/{id}` | Single separation |
| GET | `/api/hr/employees/{id}/timeline` | Employee timeline |
| GET | `/api/hr/employees/me/timeline` | Self timeline |

### Dashboard Widgets

| Widget | Data Source | Click Action |
|---|---|---|
| Probations Expiring | `probationsExpiring{7/14/30}Days` | → `/hr/employees/probations?status=Pending&window=N` |
| Pending Transfers | `pendingTransfers` | → `/hr/employees/transfers?status=PendingApproval` |
| Pending Separations | `pendingSeparations` | → `/hr/employees/separations?status=PendingApproval` |
| New Employees | `newEmployees{7/30}Days` | → `/hr/employees?createdWithin=N` |

### Transfer/Separation List Columns

- Employee (name + code)
- EffectiveDate / LastWorkingDate
- Status (business status)
- WorkflowStatus
- CurrentApprover (if available)
- Actions

### Transfer/Separation Submit Dialog

Prefill current employee data:
- Current department, position, manager, grade
- User picks target values

## Module 10: Backend API Details

### Dashboard Lifecycle Summary API

```
GET /api/hr/dashboard/lifecycle-summary?asOfDate=2026-06-22
```

Response:
```json
{
  "probationsExpiring7Days": 3,
  "probationsExpiring14Days": 8,
  "probationsExpiring30Days": 15,
  "pendingTransfers": 2,
  "pendingSeparations": 1,
  "newEmployees7Days": 5,
  "newEmployees30Days": 12
}
```

Logic:
- Probations Expiring: `StatusCode = Pending AND EndDate BETWEEN Today AND Today+N`
- Pending Transfers: `StatusCode = PendingApproval`
- Pending Separations: `StatusCode = PendingApproval`
- New Employees: `CreatedAt >= Today-N`

### New Permissions

```csharp
ProbationView, ProbationManage,
TransferCreate, TransferView,
SeparationCreate, SeparationView,
EmployeeTimelineView,
DashboardView
```

TransferApprove/SeparationApprove permissions align with workflow approval permissions (WorkflowExecute/WorkflowApprove).

## Verification Plan

### Backend Verification

```bash
dotnet build Anemoi.sln
dotnet test
```

### Database Migration Verification

- Check `Active` → `Active`, `Inactive` → `Archived`

### Browser Verification (DevTools MCP)

1. Open `/hr/dashboard` — verify 4 widgets load
2. Click each dashboard card — verify correct filtered list
3. Submit transfer — verify workflow instance created
4. Approve transfer via workflow pending approvals
5. Verify transfer page status changed to Approved
6. Verify employee department/position updated
7. Verify employee timeline shows transfer event
8. Submit separation — verify workflow instance created
9. Approve separation
10. Verify employee status changed (Resigned/Terminated)
11. Verify employee timeline shows separation event
12. Verify notifications received

### Test Requirements

- Submit transfer creates WorkflowInstance
- Approve workflow applies transfer to Employee
- Reject workflow keeps Employee unchanged
- Submit separation creates WorkflowInstance
- Approve workflow changes Employee lifecycle status
- Reject workflow keeps Employee active
- Migration: Active→Active, Inactive→Archived

## Technical Debt Deferred

- ESS profile/history page (can reuse timeline component with employee context)
- Inline dashboard widgets on individual pages (deferred beyond `/hr/dashboard`)
- Probation approval workflow (not needed without explicit business requirement)
- Full event-sourced Employee aggregate (overkill; timeline is append-only audit)
- Remove old `IsActive` field (deprecate first, remove in cleanup phase)

## Deliverables

1. All backend code (Domain, Application, Infrastructure, API)
2. Frontend pages (dashboard, probations, transfers, separations, timeline)
3. Migration scripts (status codes, seed workflows)
4. Integration events + consumers
5. Notification integration
6. Test coverage for workflow integration
7. `PHASE_33_COMPLETION_REPORT.md`

## Pre-Implementation Verification Checklist

These verifications must be performed BEFORE any coding begins:

1. **Employee base class** — read `Anemoi.Hr.Domain/Employees/Employee.cs` to confirm `: ValueObject` (already verified: line 8, confirmed)
2. **WorkflowDefinition seed strategy** — review existing seed patterns to confirm deterministic ID approach
3. **Existing history tables** — review `EmployeeDepartmentHistory`, `EmployeePositionHistory`, `EmployeeGradeHistory`, `EmployeeManagerHistory` structures before creating `EmployeeOrganizationHistory` (to avoid overlap/confusion)
4. **Onboarding completion event** — verify `OnboardingInstanceCompletedDomainEvent` exists and understand its shape before writing the handler that calls `employee.Activate()`

## Risk Registry

| Risk | Impact | Mitigation |
|---|---|---|
| Employee refactor from ValueObject to Entity breaks existing code | High | Check all usages first; add tests; keep old field temporarily |
| Workflow engine not designed for EmployeeTransfer/Separation | Medium | Pattern already exists (Leave, Overtime, Payroll updaters); follow same interface |
| Timeline duplication (handler + status updater both write) | Medium | Design rule: only domain event handlers write timeline; status updaters only call domain methods |
| Large migration for existing Active/Inactive data | Low | Simple mapping; backward-compatible |
