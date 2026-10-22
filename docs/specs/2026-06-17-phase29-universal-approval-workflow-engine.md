# Phase 29 — Universal Approval Workflow Engine (Hierarchy-Driven + Override Support)

**Date:** 2026-06-17
**Status:** Approved
**Last Updated:** 2026-06-30
**Based on:** Phase 28 — Approval Workflow Engine Foundation

> **Architecture Reassessment (2026-06-30):** The original spec allowed
> hierarchy-driven workflow building (no WorkflowDefinition needed) for
> LeaveRequest and OvertimeRequest. This has been hardened: ALL required
> business workflow types are now definition-bound. Hierarchy fallback
> (`BuildFromHierarchyAsync`) is preview-only/test-only and must never
> be used for production workflow routing. See ADR-029 and the current
> `WorkflowConstants.RequiresDefinition` implementation.
> This document is preserved for historical reference only. The current
> implementation in `WorkflowConstants.cs` and `WorkflowBuilder.cs`
> takes precedence over the spec text below.

## Purpose

Build a universal Approval Workflow Engine that powers all HR business modules (Leave, Overtime, Payroll, Recruitment). Phase 28 built the engine foundation (WorkflowDefinition, WorkflowInstance, multi-step sequential approval). Phase 29 adds:

- Hierarchy-driven workflow building (no WorkflowDefinition needed)
- Optional WorkflowDefinition override per entity type
- Business module integration (Leave, Overtime, Payroll, Recruitment)
- `IWorkflowEngine` internal facade
- `IWorkflowHierarchyResolver` + `IWorkflowBuilder`
- `IWorkflowTargetStatusUpdater` — module-owned status transitions
- Integration events for notifications
- Frontend pages for workflow management + my approvals

## Architecture Principles

- **All code stays in Anemoi.Hr** — designed extractable but not extracted yet
- Clean Architecture (Domain → Application → Infrastructure → API)
- CQRS + MediatR (no business logic in controllers)
- Strongly Typed IDs via `StronglyTypedId<Guid>` records
- `Entity<TId>` base class for all aggregates
- `xmin` concurrency on all mutable aggregates
- Manual mapper (`WorkflowMapper`) following existing pattern
- **Domain events** for in-process business aggregate updates (same transaction)
- **Integration events** for external side effects only (notifications, audit)
- Workflow Engine does NOT know business module aggregates

## Key Design Decisions

### Decision 1: Keep Workflow Engine in Anemoi.Hr
- All consumers are HR bounded contexts
- Engine needs Employee/Department/Position data (HR-owned)
- "Design it extractable, but do not extract it yet"

### Decision 2: TargetEntityType on WorkflowDefinition
- Enables lookup: "is there an active definition for LeaveRequest?"
- Partial unique index: one active definition per entity type
- Version field for future variant support

### Decision 3: IWorkflowEngine as Internal Facade
- Controllers → MediatR Handlers → IWorkflowEngine
- Business module handlers → IWorkflowEngine directly (no MediatR commands)
- Engine must not call MediatR commands (avoids MediatR-in-MediatR)

### Decision 4: Domain Events for Business Aggregate Updates
- `WorkflowInstance` raises domain events on terminal status
- `IWorkflowTargetStatusUpdater` per entity type updates the aggregate
- Same transaction, same `SaveChangesAsync`
- Integration events published after commit for external side effects

### Decision 5: Hierarchy Resolver vs. WorkflowBuilder
- `IWorkflowHierarchyResolver` returns **full** org chain (Employee → CEO)
- `WorkflowBuilder` applies `DefaultWorkflowPolicy` to trim chain
- Resolver = org structure, Builder = policy, Definition = override

## Domain Model Changes from Phase 28

### WorkflowDefinition — New Properties

```csharp
// Add to existing WorkflowDefinition
public string TargetEntityType { get; private set; }  // "LeaveRequest", "PayrollRun", etc.
public int Version { get; private set; }               // optimistic versioning
```

Updated factory:
```csharp
public static WorkflowDefinition Create(
    WorkflowDefinitionId id, string code, string name, string? description,
    string workflowTypeCode, string targetEntityType, int version,
    List<WorkflowDefinitionStep> steps);
```

New constraint: one active `WorkflowDefinition` per `TargetEntityType`. Enforced **at the database level only** via partial unique index — no handler validation (DB is source of truth; catch PostgreSQL unique violation).

### WorkflowInstance — New Properties

```csharp
// Add to existing WorkflowInstance
public EmployeeId RequesterEmployeeId { get; private set; }
public UserId RequesterUserId { get; private set; }
```

These are set at creation time and persisted. Enables:
- Pending approvals queries without joining business aggregates
- Notification to requester without querying business module
- SLA tracking (who submitted?)
- Full audit trail in workflow database

Updated factory:
```csharp
public static WorkflowInstance Start(
    WorkflowInstanceId id,
    WorkflowDefinitionId? workflowDefinitionId,
    string entityType,
    string entityId,
    string startedBy,
    EmployeeId requesterEmployeeId,
    UserId requesterUserId,
    List<WorkflowInstanceStep> steps);
```

### New Value Object (Application Layer — future-ready)

```csharp
public sealed record WorkflowEntityReference(
    string EntityType,
    Guid EntityId);
```

Not mandatory for Phase 29, but recommended for integration events and engine API to replace the loose `(string EntityType, Guid EntityId)` pairs. Consider for Phase 30+.

### New Value Objects (Application Layer)

```csharp
public sealed record ResolvedApproverStep(
    int StepOrder,
    string ApproverType,      // ApproverType constants
    string? ApproverValue,     // role name, permission key, or null
    UserId? ApproverUserId);   // resolved user ID for SpecificUser/DirectManager
```

### WorkflowStatusCode — Add

```csharp
public const string Returned = "Returned";  // already exists from Phase 28
```

### AppoverType — Already exists

```
DirectManager
SpecificUser
Role
Permission
```

## Service/Interface Design

### IWorkflowEngine (Application/Abstractions/)

```csharp
public interface IWorkflowEngine
{
    Task<OneOf<WorkflowInstanceId, ErrorDetailResponse>> StartAsync(
        string entityType,
        Guid entityId,
        EmployeeId requesterEmployeeId,
        UserId requesterUserId,
        UserId startedBy,
        CancellationToken ct);

    Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> ApproveAsync(
        WorkflowInstanceId workflowInstanceId,
        UserId performedBy,
        string? comment,
        CancellationToken ct);

    Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> RejectAsync(
        WorkflowInstanceId workflowInstanceId,
        UserId performedBy,
        string? comment,
        CancellationToken ct);

    Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> CancelAsync(
        WorkflowInstanceId workflowInstanceId,
        UserId performedBy,
        CancellationToken ct);

    Task<IReadOnlyList<UserId>> GetCurrentApproversAsync(
        WorkflowInstanceId workflowInstanceId,
        CancellationToken ct);
}
```

### IWorkflowHierarchyResolver (Application/Abstractions/)

```csharp
public interface IWorkflowHierarchyResolver
{
    Task<IReadOnlyList<ResolvedApproverStep>> ResolveHierarchyAsync(
        EmployeeId requesterEmployeeId,
        CancellationToken ct);
}
```

Implementation (`WorkflowHierarchyResolver` in `Application/Services/`):

1. Load `Employee` by `requesterEmployeeId` (includes `PrimaryDepartment`)
2. Step 1: Direct Manager via `Employee.DirectManagerEmployeeId` → resolve `Employee.UserId`
3. Step 2: Department Manager via `Department.ManagerEmployeeId` → resolve `UserId`
4. Step 3+: Walk `Department.ParentDepartment` chain, collect `ManagerEmployeeId` at each level
5. Stop when no more parent departments or no manager found
6. **Deduplicate** via `HashSet<UserId>` — skip if the same user appears as both DirectManager and DepartmentManager
7. Return all steps as `ApproverType.SpecificUser` with resolved `UserId`

### IWorkflowBuilder (Application/Abstractions/)

```csharp
public interface IWorkflowBuilder
{
    Task<IReadOnlyList<WorkflowInstanceStep>> BuildAsync(
        string entityType,
        EmployeeId requesterEmployeeId,
        string startedBy,
        CancellationToken ct);
}
```

Implementation (`WorkflowBuilder` in `Application/Services/`):

1. Query active `WorkflowDefinition` by `TargetEntityType == entityType`
2. If found → map definition steps to instance steps. Resolve `ApproverType.DirectManager` → actual `UserId` via `IWorkflowHierarchyResolver`
3. If not found → check `RequiresDefinition(entityType)` — if true, return error `HR_WF_DEF_REQUIRES_DEFINITION`
4. Otherwise (non-required type only) → call `BuildFromHierarchyAsync()` as preview-only fallback

**Note:** `BuildFromHierarchyAsync` is preview-only/test-only. ALL production
business workflow types are definition-bound. This path is preserved only
for development/testing scenarios with no active WorkflowDefinition.

### IWorkflowTargetStatusUpdater (Application/Abstractions/)

```csharp
public interface IWorkflowTargetStatusUpdater
{
    bool CanHandle(string entityType);

    Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct);

    Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct);
}
```

Implementations (in each module's area):

| Updater | CanHandle | Behavior |
|---------|-----------|----------|
| `LeaveWorkflowStatusUpdater` | `entityType == "LeaveRequest"` | Sets `LeaveRequest.Status = Approved/Rejected` only. Leave balance deduction is handled by Leave module via `LeaveRequestApprovedDomainEvent` |
| `OvertimeWorkflowStatusUpdater` | `entityType == "OvertimeRequest"` | Sets `OvertimeRequest.Status = Approved/Rejected` |
| `PayrollWorkflowStatusUpdater` | `entityType == "PayrollRun"` | Sets `PayrollRun.Status = Approved/Rejected` |
| `RecruitmentWorkflowStatusUpdater` | `entityType == "RecruitmentRequest"` | Sets `RecruitmentRequest.Status = Approved/Rejected` |

Registered via DI as `IEnumerable<IWorkflowTargetStatusUpdater>`. Domain event handler resolves by `CanHandle()`:

```csharp
public sealed class WorkflowInstanceApprovedHandler(
    IEnumerable<IWorkflowTargetStatusUpdater> updaters)
    : INotificationHandler<WorkflowInstanceApprovedDomainEvent>
{
    public async Task Handle(WorkflowInstanceApprovedDomainEvent evt, CancellationToken ct)
    {
        var updater = updaters.FirstOrDefault(u => u.CanHandle(evt.EntityType));
        if (updater is null) return;
        await updater.MarkApprovedAsync(evt.EntityId, evt.PerformedBy, ct);
    }
}
```

Benefits of `CanHandle()`:
- Supports aliases: `"LeaveRequest"` and `"EssLeaveRequest"` can share one updater
- Easier to refactor without changing string comparisons in multiple places
- Future versioning: `CanHandle()` can check version or feature flags

### WorkflowConstants (Application/Configurations/) — Current Implementation

```csharp
public static class WorkflowConstants
{
    public static class TargetEntityTypes
    {
        public const string LeaveRequest = "LeaveRequest";
        public const string OvertimeRequest = "OvertimeRequest";
        public const string PayrollRun = "PayrollRun";
        public const string RecruitmentRequest = "RecruitmentRequest";
        public const string EmployeeTransfer = "EmployeeTransfer";
        public const string EmployeeSeparation = "EmployeeSeparation";
        public const string ProbationRecord = "ProbationRecord";
    }

    // All 7 types require an active WorkflowDefinition.
    // RequiresDefinition(entityType) == IsRequiredEntityType(entityType),
    // NOT GetStepCount(entityType) == 0 (as the original spec stated).
    // This is a deliberate hardening — hierarchy fallback is not allowed
    // for production business workflows.
    public static class DefaultPolicy
    {
        public const int LeaveRequestSteps = 2;
        public const int OvertimeRequestSteps = 2;
        public const int PayrollRunSteps = 2;
        public const int RecruitmentRequestSteps = 2;
        public const int EmployeeTransferSteps = 2;
        public const int EmployeeSeparationSteps = 2;
        public const int ProbationRecordSteps = 2;

        public static int GetStepCount(string entityType) => entityType switch { ... };
        public static bool RequiresDefinition(string entityType) => IsRequiredEntityType(entityType);
    }
}
```

## CQRS & API Layer

### Refactored CQRS Handlers

Phase 28 handlers are refactored to delegate to `IWorkflowEngine`:

| Handler | Phase 28 | Phase 29 |
|---------|----------|----------|
| `StartWorkflowHandler` | Built instance from definition directly | Calls `workflowEngine.StartAsync()` |
| `ApproveWorkflowStepHandler` | Direct domain logic | Calls `workflowEngine.ApproveAsync()` |
| `RejectWorkflowStepHandler` | Direct domain logic | Calls `workflowEngine.RejectAsync()` |
| `CancelWorkflowHandler` | Direct domain logic | Calls `workflowEngine.CancelAsync()` |

### Updated Permission Attributes

| Controller Action | Old Permission | New Permission |
|------------------|---------------|----------------|
| ApproveWorkflowStep | `WorkflowExecute` | `WorkflowApprove` |
| RejectWorkflowStep | `WorkflowExecute` | `WorkflowApprove` |
| CancelWorkflow | `WorkflowExecute` | `WorkflowApprove` |
| ReturnWorkflow | `WorkflowExecute` | `WorkflowApprove` |
| GetPendingApprovals | `WorkflowExecute` | `WorkflowApprove` |

Keep `WorkflowExecute` as alias for backward compat.

### New Queries

- `GetWorkflowInstanceHistoryQuery` — returns full `WorkflowHistory` audit trail for a given instance

## Business Module Integration

### WorkflowInstance.Domain Events

`WorkflowInstance.Approve()` raises `WorkflowInstanceApprovedDomainEvent(WorkflowInstanceId, EntityType, EntityId, PerformedBy)` when status becomes `Approved`.

`WorkflowInstance.Reject()` raises `WorkflowInstanceRejectedDomainEvent(WorkflowInstanceId, EntityType, EntityId, PerformedBy, Comment)` when status becomes `Rejected`.

### Domain Event Handler

Already shown above (see `CanHandle()` section). Same pattern for `WorkflowInstanceRejectedHandler`.

### Module Integration Flow

**Submitting a Leave Request:**
```
SubmitLeaveRequestHandler:
  1. Create LeaveRequest aggregate
  2. Call workflowEngine.StartAsync("LeaveRequest", leaveRequestId, employeeId, userId)
  3. SaveChangesAsync
     → WorkflowInstance saved
     → WorkflowStartedIntegrationEvent queued
```

**Approving a Leave Request:**
```
ApproveWorkflowStepCommand → ApproveWorkflowStepHandler:
  1. Call workflowEngine.ApproveAsync(instanceId, userId, comment)
     → WorkflowInstance.Approve() → raises WorkflowInstanceApprovedDomainEvent
     → LeaveWorkflowStatusUpdater.MarkApprovedAsync() → LeaveRequest.Status = Approved
     → SaveChangesAsync (both WorkflowInstance + LeaveRequest in same transaction)
     → WorkflowApprovedIntegrationEvent queued
```

**Rejection** follows the same pattern.

### Legacy Command Removal

The following **direct** approve/reject commands on business aggregates are deprecated in Phase 29:

- `ApproveLeaveRequestCommand` — `[Obsolete("Use ApproveWorkflowStepCommand instead")]`
- `RejectLeaveRequestCommand` — `[Obsolete("Use RejectWorkflowStepCommand instead")]`
- `ApproveOvertimeRequestCommand` — `[Obsolete("Use ApproveWorkflowStepCommand instead")]`
- `RejectOvertimeRequestCommand` — `[Obsolete("Use RejectWorkflowStepCommand instead")]`
- `ForceApproveLeaveRequestCommand` — kept for emergency bypass (bypasses workflow)

The UI should route all approval actions through `/api/hr/workflows/workflowinstances/approveworkflowstep` instead of the old per-module endpoints.

## Integration Events

Add to `Anemoi.Contract.Hr`:

```csharp
public sealed record WorkflowStartedIntegrationEvent(
    Guid WorkflowInstanceId, string EntityType, string EntityId,
    string StartedBy, DateTime StartedAt);

public sealed record WorkflowApprovedIntegrationEvent(
    Guid WorkflowInstanceId, string EntityType, string EntityId,
    string PerformedBy, string? Comment, DateTime ApprovedAt,
    bool IsCompleted);  // true if this was the final step

public sealed record WorkflowRejectedIntegrationEvent(
    Guid WorkflowInstanceId, string EntityType, string EntityId,
    string PerformedBy, string? Comment, DateTime RejectedAt,
    bool IsCompleted);  // rejection is always terminal
```

The `WorkflowInstance` EF mapping must also configure `RequesterEmployeeId` (→ `uuid`) and `RequesterUserId` (→ `VARCHAR(128)`).

Published **after** transaction commit via `IEventBus` (MassTransit). Not used for business aggregate updates — only for:
- Notification dispatch
- External audit
- Cross-service integration

## Notification Integration

Wired from existing `Anemoi.Notification` consumers:

| Integration Event | Notification Flow |
|------------------|-------------------|
| `WorkflowStartedIntegrationEvent` | Notify first-step approver: "New approval request: {EntityType}" |
| `WorkflowApprovedIntegrationEvent` (IsCompleted == false) | Notify next-step approver: "Previous step approved" |
| `WorkflowApprovedIntegrationEvent` (IsCompleted == true) | Notify requester: "Request approved" |
| `WorkflowRejectedIntegrationEvent` | Notify requester: "Request rejected" |

Consumers use existing `INotificationRecipientResolver` + `CreateNotificationCommand`.

## Permissions

### New Permission Constants

```csharp
// BuildingBlocks/Application/Authorization/Permissions.cs
public const string HrWorkflowApprove = "hr.workflow.approve";

// HrPermissions.cs
public const string WorkflowApprove = Permissions.HrWorkflowApprove;
```

### Updated Permission Definitions

| Permission | Existing | Phase 29 |
|-----------|----------|----------|
| `hr.workflow.view` | `WorkflowView` | Kept |
| `hr.workflow.manage` | `WorkflowManage` | Kept (CRUD definitions) |
| `hr.workflow.approve` | `WorkflowExecute` (aliased) | `WorkflowApprove` (can approve/reject steps) |

Add `WorkflowApprove` to `SensitivePermissions` (RiskLevel: High).

## Frontend

### Menu Structure
```
HR
 └─ Workflow Management
     ├─ Workflow Definitions  → /hr/workflows/definitions
     └─ My Approvals          → /hr/workflows/approvals/my
```

### Pages

**Workflow Definitions** — `/hr/workflows/definitions`
- Table: Code, Name, TargetEntityType, IsActive, Steps count
- Actions: Create, Edit, Activate, Deactivate
- Modal for step configuration

**Workflow Definition Detail** — `/hr/workflows/definitions/[id]`
- Edit name/description
- Add/remove/reorder steps
- Activate/Deactivate

**My Approvals** — `/hr/workflows/approvals/my`
- Table: EntityType, EntityId, Requester, Current Step, Submitted At
- Actions: Approve (with comment), Reject (with reason)
- Real-time update on action

### i18n

Add keys for:
- `workflow.title` = "Approval Workflow"
- `workflow.definitions` = "Workflow Definitions"
- `workflow.myApprovals` = "My Approvals"
- `workflow.approve` = "Approve"
- `workflow.reject` = "Reject"
- `workflow.pending` = "Pending Approval"
- `workflow.status.approved` = "Approved"
- `workflow.status.rejected` = "Rejected"
- `workflow.status.pending` = "Pending"

## EF Core Migration

### Phase 29 Migration

```sql
-- Add columns to WorkflowInstances (Requester tracking)
ALTER TABLE "WorkflowInstances"
  ADD COLUMN "RequesterEmployeeId" uuid;

ALTER TABLE "WorkflowInstances"
  ADD COLUMN "RequesterUserId" VARCHAR(128);

UPDATE "WorkflowInstances" SET "RequesterEmployeeId" = '00000000-0000-0000-0000-000000000000';
UPDATE "WorkflowInstances" SET "RequesterUserId" = '';
ALTER TABLE "WorkflowInstances"
  ALTER COLUMN "RequesterEmployeeId" SET NOT NULL;
ALTER TABLE "WorkflowInstances"
  ALTER COLUMN "RequesterUserId" SET NOT NULL;

-- Add columns to WorkflowDefinitions
ALTER TABLE "WorkflowDefinitions"
  ADD COLUMN "TargetEntityType" VARCHAR(100) NOT NULL DEFAULT '';

ALTER TABLE "WorkflowDefinitions"
  ADD COLUMN "Version" INTEGER NOT NULL DEFAULT 1;

-- Backfill existing records
UPDATE "WorkflowDefinitions"
SET "TargetEntityType" = CASE "Code"
    WHEN 'LEAVE_REQUEST' THEN 'LeaveRequest'
    WHEN 'OVERTIME_REQUEST' THEN 'OvertimeRequest'
    WHEN 'PAYROLL_RUN' THEN 'PayrollRun'
    WHEN 'RECRUITMENT_REQUEST' THEN 'RecruitmentRequest'
    ELSE "Code"
  END,
  "Version" = 1;

-- Set inactive any records that can't be mapped
UPDATE "WorkflowDefinitions"
SET "IsActive" = false
WHERE "TargetEntityType" = '';

-- Partial unique index: one active definition per entity type
CREATE UNIQUE INDEX "IX_WorkflowDefinitions_TargetEntityType_Active"
  ON "WorkflowDefinitions" ("TargetEntityType")
  WHERE "IsActive" = true;
```

## Testing Strategy

### Domain Tests

| Test | Scope |
|------|-------|
| `WorkflowDefinition` with `TargetEntityType` | Validate factory, activate, deactivate |
| `WorkflowInstance` domain events | Verify `Approve()` raises domain event on terminal status |
| `WorkflowInstance` non-terminal approve | Verify no domain event raised |
| Hierarchy resolver | Mock `Employee`/`Department` chain, verify steps |

### Application Tests

| Test | Scope |
|------|-------|
| `WorkflowBuilder` — definition found | Verify steps from definition |
| `WorkflowBuilder` — no definition | Verify hierarchy + policy |
| `WorkflowBuilder` — PayrollRun | Verify throws (definition required) |
| `WorkflowEngine.StartAsync` | Integration of builder + instance creation |
| `WorkflowEngine.ApproveAsync` | Approve → domain event → status updater |
| `DefaultWorkflowPolicy` | Verify step counts per entity type |

### Integration Tests

| Test | Scope |
|------|-------|
| Approve workflow → LeaveRequest updated | Full flow via `ApproveWorkflowStepHandler` |
| Reject workflow → OvertimeRequest updated | Full flow via `RejectWorkflowStepHandler` |
| Pending approvals query | Filter by current user's pending steps |
| Partial unique index | Attempt to activate second definition for same entity type |

### Test Files

- `Anemoi.Hr.Test/Domain/Workflow/WorkflowDefinitionTests.cs` — extend with TargetEntityType tests
- `Anemoi.Hr.Test/Domain/Workflow/WorkflowInstanceTests.cs` — extend with domain event tests
- `Anemoi.Hr.Test/Application/Workflow/WorkflowEngineTests.cs` — new
- `Anemoi.Hr.Test/Application/Workflow/WorkflowBuilderTests.cs` — new
- `Anemoi.Hr.Test/Application/Workflow/WorkflowHierarchyResolverTests.cs` — new

## Deliverables

1. Domain model changes (TargetEntityType, Version, domain events)
2. `IWorkflowEngine` implementation
3. `IWorkflowHierarchyResolver` implementation
4. `WorkflowBuilder` implementation
5. `IWorkflowTargetStatusUpdater` + per-module implementations
6. `DefaultWorkflowPolicy` constants
7. Refactored CQRS handlers (delegate to engine)
8. Updated permissions (WorkflowApprove)
9. Integration events (WorkflowStarted, WorkflowApproved, WorkflowRejected)
10. Notification consumers wiring
11. Frontend pages (workflow definitions, my approvals)
12. EF Core migration with backfill + partial unique index
13. Unit tests (domain + application)
14. Integration tests
15. Architecture review report
16. Security review report
17. Final completion report

## Future-Proofing

Phase 29 is designed so that Phase 30+ can add:

- **Delegation** — replace approver in `IWorkflowHierarchyResolver` or via override
- **Escalation** — timer service checks `WorkflowInstanceStep.Pending` duration, escalates to next level
- **Auto-approval** — `IWorkflowEngine.ApproveAsync()` called by scheduler for low-risk items
- **SLA tracking** — add `Deadline` to `WorkflowInstanceStep`, monitor via background service
- **Parallel approval** — allow multiple steps at same sequence with "all must approve" or "any can approve"
- **Approval matrix** — lookup approver by combination of amount + department + region

All these require changes to the Workflow Engine only — no changes to Leave, Overtime, Payroll, or Recruitment modules.
