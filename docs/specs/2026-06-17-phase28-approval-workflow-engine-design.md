# Phase 28 — Approval Workflow Engine Foundation

**Date:** 2026-06-17
**Status:** Approved
**Last Updated:** 2026-06-17

## Purpose

Build a standalone Workflow Engine that can power approval workflows across all HR modules. This phase creates the engine foundation only — no integration with Recruitment, Leave, Overtime, or any business domain.

Future phases will integrate:
- Phase 29 — Recruitment Request Workflow Integration
- Phase 30 — Leave & Overtime Workflow Integration
- Phase 31 — Notification Workflow Automation
- Phase 32 — Delegation & Escalation
- Phase 33 — Organization Approval Matrix

## Architecture Principles

- Clean Architecture (Domain → Application → Infrastructure → API)
- CQRS + MediatR (no business logic in controllers)
- Strongly Typed IDs via `StronglyTypedId<Guid>` records
- `Entity<TId>` base class for aggregate roots (WorkflowDefinition, WorkflowInstance)
- `Entity<TId>` base class for all entities including WorkflowHistory (has identity, audit trail)
- Manual mapper following existing HR mapper pattern (`WorkflowMapper`)
- `xmin` concurrency on all mutable aggregates
- No integration events in this phase (Phase 31 handles notification automation)
- No business logic in controllers

## Domain Model

### WorkflowDefinition (Aggregate Root)

```
Entity<WorkflowDefinitionId>
├── Code: string (unique business code)
├── Name: string
├── Description: string?
├── WorkflowTypeCode: string (WorkflowTypeCode constants)
├── IsActive: bool
├── Steps: List<WorkflowDefinitionStep> (child entities, ordered by Sequence)
├── CreatedAt: DateTime
├── UpdatedAt: DateTime
├── xmin (concurrency token)
```

**Domain methods:**
- `AddStep(sequence, approverType, approverValue, isRequired)` — appends step to Steps list
- `Activate()` — sets IsActive = true (validates at least 1 step exists)
- `Deactivate()` — sets IsActive = false
- `ReplaceSteps(newSteps)` — clears existing steps, adds new ones
- `EnsureActive()` — throws if not active

### WorkflowDefinitionStep (Child Entity of WorkflowDefinition)

```
Entity<WorkflowDefinitionStepId>
├── WorkflowDefinitionId: WorkflowDefinitionId (FK)
├── Sequence: int (1, 2, 3...)
├── ApproverType: string (ApproverType constants)
├── ApproverValue: string? (role name, permission key, or user ID; null for DirectManager)
├── IsRequired: bool
```

### WorkflowInstance (Aggregate Root)

```
Entity<WorkflowInstanceId>
├── WorkflowDefinitionId: WorkflowDefinitionId (FK)
├── EntityType: string (e.g., "RecruitmentRequest", "LeaveRequest")
├── EntityId: string (string-formatted entity ID)
├── CurrentStep: int (current sequence number)
├── Status: string (WorkflowStatusCode)
├── StartedBy: string (UserId, resolved from auth context)
├── StartedAt: DateTime
├── CompletedAt: DateTime?
├── Steps: List<WorkflowInstanceStep> (snapshots of definition steps)
├── Histories: List<WorkflowHistory> (audit trail)
├── xmin (concurrency token)
```

**Domain methods:**
- `Approve(actionBy, comment?)` — approves current step; if last step → set Approved
- `Reject(actionBy, comment?)` — sets status to Rejected
- `Cancel(actionBy)` — sets status to Cancelled
- `ReturnForRevision(actionBy, comment?)` — sets status to Returned, sets CompletedAt, no step reset (terminal state)
- `EnsurePending()` — throws if not in Pending status
- `EnsureStepMatches(currentStep)` — validates current step sequence

### WorkflowInstanceStep (Child Entity of WorkflowInstance)

```
Entity<WorkflowInstanceStepId>
├── WorkflowInstanceId: WorkflowInstanceId (FK)
├── Sequence: int
├── ApproverTypeSnapshot: string (copied from definition step at start time)
├── ApproverValueSnapshot: string? (copied from definition step at start time)
├── ApproverUserId: string? (resolved at StartWorkflow for SpecificUser/DirectManager; null for Role/Permission — lazy via claims)
├── Status: string (WorkflowStepStatusCode)
├── ApprovedAt: DateTime?
├── RejectedAt: DateTime?
├── Comment: string?
```

### WorkflowHistory (Entity)

```
Entity<WorkflowHistoryId>
├── WorkflowInstanceId: WorkflowInstanceId (FK)
├── Action: string (Approve/Reject/Cancel/ReturnForRevision)
├── PerformedBy: string (UserId)
├── Comment: string?
├── PerformedAt: DateTime
```

## Strongly Typed IDs

```csharp
public sealed record WorkflowDefinitionId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record WorkflowDefinitionStepId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record WorkflowInstanceId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record WorkflowInstanceStepId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record WorkflowHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
```

## Status & Type Constants

### WorkflowTypeCode.cs
```csharp
public static class WorkflowTypeCode
{
    public const string Approval = "Approval";
}
```

### WorkflowStatusCode.cs
```csharp
public static class WorkflowStatusCode
{
    public const string Draft = "Draft";
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
    public const string Returned = "Returned";
}
```

### WorkflowStepStatusCode.cs
```csharp
public static class WorkflowStepStatusCode
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Skipped = "Skipped";
    public const string Cancelled = "Cancelled";
}
```

### ApproverType.cs
```csharp
public static class ApproverType
{
    public const string Role = "Role";
    public const string Permission = "Permission";
    public const string DirectManager = "DirectManager";
    public const string SpecificUser = "SpecificUser";
}
```

## CQRS — Commands

All handlers follow this pattern:
1. Fetch entity via `ISqlRepository<T>.GetQueryable()`
2. If not found → return `HrErrorResponses.Create(ErrorCode)`
3. Call domain method
4. Save via `await unitOfWork.SaveChangesAsync()`
5. If save fails → return concurrency error
6. Map to response DTO → return Ok

### CreateWorkflowDefinition
- Input: `Code`, `Name`, `Description`, `WorkflowTypeCode`, `Steps[]` (Sequence, ApproverType, ApproverValue, IsRequired)
- Creates `WorkflowDefinition` + `WorkflowDefinitionStep` children
- Status: Draft (IsActive = false)

### UpdateWorkflowDefinition
- Input: `Id`, `Name`, `Description`, `Steps[]`
- Validates: definition exists, **IsActive must be false** (deactivate first)
- Replaces all steps via `ReplaceSteps()`
- Steps must have sequential sequence numbers without gaps or duplicates

### ActivateWorkflowDefinition
- Input: `Id`
- Validates: at least 1 step defined
- Sets `IsActive = true`

### DeactivateWorkflowDefinition
- Input: `Id`
- Sets `IsActive = false`

### StartWorkflow
- Input: `DefinitionId`, `EntityType`, `EntityId`
- `StartedBy` resolved from `HttpContext.GetUserId()` — NOT from client input
- Snapshot: copies all definition steps into instance steps (ApproverType, ApproverValue)
- Sets status: Pending, CurrentStep: 1
- **Restricted:** requires `hr.workflow.manage` permission. No frontend action in this phase. Marked as dev/admin only until module integrations exist (Phase 29+).

### ApproveWorkflowStep
- Input: `InstanceId`, `Comment?`
- Validates: instance exists, status is Pending
- **Approver validation:** current user must match the step's approver:
  - If `ApproverUserId` is set (SpecificUser, DirectManager) → current user must match
  - If `ApproverTypeSnapshot` is `Role` → current user must have the role
  - If `ApproverTypeSnapshot` is `Permission` → current user must have the permission
- Approves current step; advances to next step or completes

### RejectWorkflowStep
- Input: `InstanceId`, `Comment?`
- Validates: instance exists, status is Pending
- **Approver validation:** same as ApproveWorkflowStep
- Sets instance status: Rejected

### CancelWorkflow
- Input: `InstanceId`, `Comment?`
- Cancels all pending steps and instance

### ReturnWorkflow
- Input: `InstanceId`, `Comment?`
- Sets instance status: Returned (terminal). Module owner will create new instance after revision.
- Phase này không auto-restart. Resubmit sẽ do Phase 29+ xử lý.

## CQRS — Queries

### GetWorkflowDefinitions
- Paginated list, filterable by IsActive, WorkflowTypeCode

### GetWorkflowDefinitionById
- Single definition with steps

### GetWorkflowInstances
- Paginated list, filterable by Status, EntityType, WorkflowDefinitionId

### GetWorkflowInstanceById
- Single instance with steps and history

### GetPendingApprovals
- Current user's pending approvals (filtered by ApproverUserId or resolved via DirectManager/Role/Permission)
- Permission: `hr.workflow.execute` (actionable inbox)

## EF Core Configuration

### WorkflowDefinitionModelMapping
- Table: `WorkflowDefinitions`
- Indexes: `Code` (unique), `WorkflowTypeCode`, `IsActive`
- xmin concurrency token
- WorkflowDefinitionStep: unique index on `(WorkflowDefinitionId, Sequence)`

### WorkflowInstanceModelMapping
- Table: `WorkflowInstances`
- Indexes: `EntityType + EntityId`, `Status`, `WorkflowDefinitionId`, `StartedBy`
- xmin concurrency token

### WorkflowInstanceStepModelMapping
- Table: `WorkflowInstanceSteps`
- Indexes: `WorkflowInstanceId`, `Status`
- Strongly-typed ID conversions for `WorkflowInstanceStepId`, `WorkflowInstanceId`

### WorkflowHistoryModelMapping
- Table: `WorkflowHistories`
- Indexes: `WorkflowInstanceId`, `PerformedAt`

## Permissions

### Backend — Permissions.cs
```csharp
public const string HrWorkflowView = "hr.workflow.view";
public const string HrWorkflowManage = "hr.workflow.manage";
public const string HrWorkflowExecute = "hr.workflow.execute";
```

### Backend — HrPermissions.cs
```csharp
public const string WorkflowView = Permissions.HrWorkflowView;
public const string WorkflowManage = Permissions.HrWorkflowManage;
public const string WorkflowExecute = Permissions.HrWorkflowExecute;
```

### Frontend — permissions.ts
```typescript
HR_WORKFLOW_VIEW: "hr.workflow.view",
HR_WORKFLOW_MANAGE: "hr.workflow.manage",
HR_WORKFLOW_EXECUTE: "hr.workflow.execute",
```

## Notification Category

```csharp
// Add to NotificationConstants.Categories
public const string Workflow = "Workflow";

// Update AllowedCategories
AllowedCategories = { System, Workspace, Task, Environment, Leave, Overtime, Payroll, Recruitment, Workflow }
```

No notification business flow in this phase — only category preparation.

## Response DTOs

```csharp
public sealed record WorkflowDefinitionResponse(
    string Id, string Code, string Name, string? Description,
    string WorkflowTypeCode, bool IsActive,
    IReadOnlyCollection<WorkflowDefinitionStepResponse> Steps,
    DateTime CreatedAt, DateTime UpdatedAt);

public sealed record WorkflowDefinitionStepResponse(
    string Id, int Sequence,
    string ApproverType, string? ApproverValue, bool IsRequired);

public sealed record WorkflowInstanceResponse(
    string Id, string WorkflowDefinitionId, string WorkflowDefinitionName,
    string EntityType, string EntityId, int CurrentStep,
    string Status, string StartedBy, DateTime StartedAt, DateTime? CompletedAt,
    IReadOnlyCollection<WorkflowInstanceStepResponse> Steps,
    IReadOnlyCollection<WorkflowHistoryResponse> Histories);

public sealed record WorkflowInstanceStepResponse(
    string Id, int Sequence,
    string ApproverTypeSnapshot, string? ApproverValueSnapshot,
    string? ApproverUserId, string Status,
    DateTime? ApprovedAt, DateTime? RejectedAt, string? Comment);

public sealed record WorkflowHistoryResponse(
    string Id, string WorkflowInstanceId,
    string Action, string PerformedBy, string? Comment, DateTime PerformedAt);
```

## Mapper

Create `WorkflowMapper.cs` in `Anemoi.Hr.Application/Mappings/`:
- `ToResponse(WorkflowDefinition)` → `WorkflowDefinitionResponse`
- `ToResponses(IEnumerable<WorkflowDefinition>)` → `IReadOnlyCollection<WorkflowDefinitionResponse>`
- `ToResponse(WorkflowInstance)` → `WorkflowInstanceResponse`
- `ToResponses(IEnumerable<WorkflowInstance>)` → `IReadOnlyCollection<WorkflowInstanceResponse>`
- `ToStepResponse(WorkflowInstanceStep)` → `WorkflowInstanceStepResponse`
- `ToHistoryResponse(WorkflowHistory)` → `WorkflowHistoryResponse`

## Business Error Codes

```csharp
// Workflow Definition
WorkflowDefinitionNotFound = "HR_WF_DEF_NOT_FOUND"
WorkflowDefinitionNoSteps = "HR_WF_DEF_NO_STEPS"
WorkflowDefinitionAlreadyActive = "HR_WF_DEF_ALREADY_ACTIVE"
WorkflowDefinitionAlreadyInactive = "HR_WF_DEF_ALREADY_INACTIVE"

// Workflow Instance
WorkflowInstanceNotFound = "HR_WF_INSTANCE_NOT_FOUND"
WorkflowInstanceInvalidStatus = "HR_WF_INSTANCE_INVALID_STATUS"
WorkflowInstanceStepNotFound = "HR_WF_INSTANCE_STEP_NOT_FOUND"
WorkflowInstanceAlreadyCompleted = "HR_WF_INSTANCE_ALREADY_COMPLETED"

// Validation
ValWorkflowDefinitionIdRequired = "VAL_WF_DEF_ID_REQUIRED"
ValWorkflowInstanceIdRequired = "VAL_WF_INSTANCE_ID_REQUIRED"
ValWorkflowCodeRequired = "VAL_WF_CODE_REQUIRED"
ValWorkflowNameRequired = "VAL_WF_NAME_REQUIRED"
ValWorkflowStepSequenceInvalid = "VAL_WF_STEP_SEQUENCE_INVALID"
ValWorkflowApproverTypeRequired = "VAL_WF_APPROVER_TYPE_REQUIRED"
```

## API Endpoints

### WorkflowDefinitionsController (`api/hr/workflows/WorkflowDefinitions/`)
| Method | Action | Auth | Notes |
|--------|--------|------|-------|
| GET | `GetWorkflowDefinitions` | `hr.workflow.view` | Paginated |
| GET | `GetWorkflowDefinitionById/{id}` | `hr.workflow.view` | |
| POST | `CreateWorkflowDefinition` | `hr.workflow.manage` | |
| PUT | `UpdateWorkflowDefinition` | `hr.workflow.manage` | |
| POST | `ActivateWorkflowDefinition` | `hr.workflow.manage` | |
| POST | `DeactivateWorkflowDefinition` | `hr.workflow.manage` | |

### WorkflowInstancesController (`api/hr/workflows/WorkflowInstances/`)
| Method | Action | Auth | Notes |
|--------|--------|------|-------|
| POST | `StartWorkflow` | `hr.workflow.manage` | Dev/admin only — no frontend action this phase |
| POST | `ApproveWorkflowStep` | `hr.workflow.execute` | |
| POST | `RejectWorkflowStep` | `hr.workflow.execute` | |
| POST | `CancelWorkflow` | `hr.workflow.execute` | |
| POST | `ReturnWorkflow` | `hr.workflow.execute` | |
| GET | `GetWorkflowInstances` | `hr.workflow.view` | Paginated |
| GET | `GetWorkflowInstanceById/{id}` | `hr.workflow.view` | |
| GET | `GetPendingApprovals` | `hr.workflow.execute` | Current user's pending |

## Files to Create

### Domain — `Anemoi.Hr.Domain/Workflow/`
- `WorkflowDefinition.cs` (Entity<WorkflowDefinitionId>, aggregate root)
- `WorkflowDefinitionStep.cs` (Entity<WorkflowDefinitionStepId>, child)
- `WorkflowInstance.cs` (Entity<WorkflowInstanceId>, aggregate root)
- `WorkflowInstanceStep.cs` (Entity<WorkflowInstanceStepId>, child)
- `WorkflowHistory.cs` (Entity<WorkflowHistoryId>)
- `WorkflowTypeCode.cs` (constants)
- `WorkflowStatusCode.cs` (constants)
- `WorkflowStepStatusCode.cs` (constants)
- `ApproverType.cs` (constants)

### ModelIds — `Anemoi.Hr.ModelIds/ModelIds/`
- `WorkflowDefinitionId.cs`
- `WorkflowDefinitionStepId.cs`
- `WorkflowInstanceId.cs`
- `WorkflowInstanceStepId.cs`
- `WorkflowHistoryId.cs`

### Application — Commands
- `.../CreateWorkflowDefinition/CreateWorkflowDefinitionCommand.cs`
- `.../CreateWorkflowDefinition/CreateWorkflowDefinitionHandler.cs`
- `.../CreateWorkflowDefinition/CreateWorkflowDefinitionValidator.cs`
- `.../UpdateWorkflowDefinition/UpdateWorkflowDefinitionCommand.cs`
- `.../UpdateWorkflowDefinition/UpdateWorkflowDefinitionHandler.cs`
- `.../UpdateWorkflowDefinition/UpdateWorkflowDefinitionValidator.cs`
- `.../ActivateWorkflowDefinition/ActivateWorkflowDefinitionCommand.cs`
- `.../ActivateWorkflowDefinition/ActivateWorkflowDefinitionHandler.cs`
- `.../ActivateWorkflowDefinition/ActivateWorkflowDefinitionValidator.cs`
- `.../DeactivateWorkflowDefinition/DeactivateWorkflowDefinitionCommand.cs`
- `.../DeactivateWorkflowDefinition/DeactivateWorkflowDefinitionHandler.cs`
- `.../DeactivateWorkflowDefinition/DeactivateWorkflowDefinitionValidator.cs`
- `.../StartWorkflow/StartWorkflowCommand.cs`
- `.../StartWorkflow/StartWorkflowHandler.cs`
- `.../StartWorkflow/StartWorkflowValidator.cs`
- `.../ApproveWorkflowStep/ApproveWorkflowStepCommand.cs`
- `.../ApproveWorkflowStep/ApproveWorkflowStepHandler.cs`
- `.../ApproveWorkflowStep/ApproveWorkflowStepValidator.cs`
- `.../RejectWorkflowStep/RejectWorkflowStepCommand.cs`
- `.../RejectWorkflowStep/RejectWorkflowStepHandler.cs`
- `.../RejectWorkflowStep/RejectWorkflowStepValidator.cs`
- `.../CancelWorkflow/CancelWorkflowCommand.cs`
- `.../CancelWorkflow/CancelWorkflowHandler.cs`
- `.../CancelWorkflow/CancelWorkflowValidator.cs`
- `.../ReturnWorkflow/ReturnWorkflowCommand.cs`
- `.../ReturnWorkflow/ReturnWorkflowHandler.cs`
- `.../ReturnWorkflow/ReturnWorkflowValidator.cs`

### Application — Queries
- `.../GetWorkflowDefinitions/GetWorkflowDefinitionsQuery.cs`
- `.../GetWorkflowDefinitions/GetWorkflowDefinitionsHandler.cs`
- `.../GetWorkflowDefinitionById/GetWorkflowDefinitionByIdQuery.cs`
- `.../GetWorkflowDefinitionById/GetWorkflowDefinitionByIdHandler.cs`
- `.../GetWorkflowInstances/GetWorkflowInstancesQuery.cs`
- `.../GetWorkflowInstances/GetWorkflowInstancesHandler.cs`
- `.../GetWorkflowInstanceById/GetWorkflowInstanceByIdQuery.cs`
- `.../GetWorkflowInstanceById/GetWorkflowInstanceByIdHandler.cs`
- `.../GetPendingApprovals/GetPendingApprovalsQuery.cs`
- `.../GetPendingApprovals/GetPendingApprovalsHandler.cs`

### Application — Other
- `Mappings/WorkflowMapper.cs`
- `Responses/WorkflowResponses.cs`

### Infrastructure — Configurations
- `WorkflowDefinitionModelMapping.cs`
- `WorkflowInstanceModelMapping.cs`

### Infrastructure — Update existing
- `Persistence/HrDbContext.cs` — add DbSets
- `Installers/ServiceInstaller.cs` — register mapper

### API — Controllers
- `Controllers/Workflow/WorkflowDefinitionsController.cs`
- `Controllers/Workflow/WorkflowInstancesController.cs`

### BuildingBlocks — Update
- `Permissions.cs` — add 3 workflow permissions + Definitions

### Contract.Notification — Update
- `NotificationConstants.cs` — add `Workflow` category

### Frontend — Pages
- `app/[locale]/(dashboard)/hr/workflows/page.tsx` (tabbed: Definitions, Instances, Pending Approvals)

### Frontend — Updates
- `constants/permissions.ts` — add workflow permissions
- `constants/api-endpoints.ts` — add workflow endpoints
- `services/hr/workflowService.ts` — add service class
- `types/hr/workflow.ts` — add TypeScript types

## Files to Modify

- `Anemoi.BuildingBlocks/.../Permissions.cs` — add 3 new constants + Definitions
- `Anemoi.Contract.Notification/.../NotificationConstants.cs` — add `Workflow` category
- `Anemoi.Hr/.../HrPermissions.cs` — add 3 new references
- `Anemoi.Hr/.../HrBusinessErrorCodes.cs` — add error codes
- `Anemoi.Hr/.../HrDbContext.cs` — add 5 new DbSets
- `Anemoi.Hr/.../ServiceInstaller.cs` — register mapper
- Sidebar navigation — add HR Workflows menu item

## Test Plan

### Domain Tests
- WorkflowDefinition: create, add step, activate (with/without steps), deactivate
- WorkflowInstance: start, approve (step by step through all steps), reject, cancel, return for revision
- Status transition validation for all 6 statuses (WorkflowStatusCode)
- Step status transitions (WorkflowStepStatusCode)
- Invalid transitions return errors

### Handler Tests
- Each command handler: success path + all error paths (not found, invalid status, validation)
- StartWorkflow: verify step snapshots match definition
- ApproveWorkflowStep: verify step advancement, completion detection, history creation
- RejectWorkflowStep: verify instance rejection, step rejection
- ReturnWorkflow: verify current step reset

### Build
- `dotnet build Anemoi.sln` — 0 errors

### Test Command
- `dotnet test`

## Out of Scope (This Phase)

- Recruitment integration (Phase 29)
- Leave integration (Phase 30+)
- Overtime integration (Phase 30+)
- Notification business flow (Phase 31)
- Escalation (Phase 32+)
- Delegation (Phase 32+)
- Parallel approval (Phase 33+)
- BPMN
