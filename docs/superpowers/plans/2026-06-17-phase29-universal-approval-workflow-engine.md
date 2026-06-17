# Phase 29 — Universal Approval Workflow Engine Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build hierarchy-driven universal approval workflow engine powering Leave, Overtime, Payroll, Recruitment modules.

**Architecture:** Extends Phase 28 engine within Anemoi.Hr. Adds `IWorkflowEngine` internal facade, `IWorkflowHierarchyResolver` for org-chain resolution, `WorkflowBuilder` for definition-or-hierarchy flow, and `IWorkflowTargetStatusUpdater` per module. Domain events update business aggregates in same transaction; integration events drive notifications.

**Tech Stack:** .NET 10, EF Core/PostgreSQL, MediatR, OneOf, MassTransit, xUnit

---

## File Structure Map

### Domain (modify existing)
```
Anemoi.Hr.Domain/Workflow/
  WorkflowDefinition.cs        — add TargetEntityType, Version
  WorkflowInstance.cs          — add RequesterEmployeeId, RequesterUserId, domain events, nullable WFDefinitionId
  WorkflowInstanceStep.cs      — unchanged
  WorkflowStatusCode.cs        — unchanged
  WorkflowStepStatusCode.cs    — unchanged
```

### Application (new interfaces + implementations)
```
Anemoi.Hr.Application/Abstractions/
  IWorkflowEngine.cs              — NEW
  IWorkflowHierarchyResolver.cs   — NEW
  IWorkflowBuilder.cs             — NEW
  IWorkflowTargetStatusUpdater.cs — NEW

Anemoi.Hr.Application/Services/
  WorkflowEngine.cs               — NEW
  WorkflowHierarchyResolver.cs    — NEW
  WorkflowBuilder.cs              — NEW

Anemoi.Hr.Application/Configurations/
  WorkflowConstants.cs            — NEW (TargetEntityTypes + DefaultPolicy)

Anemoi.Hr.Application/WorkflowTargetStatusUpdaters/
  LeaveWorkflowStatusUpdater.cs           — NEW
  OvertimeWorkflowStatusUpdater.cs        — NEW
  PayrollWorkflowStatusUpdater.cs         — NEW
  RecruitmentWorkflowStatusUpdater.cs     — NEW
```

### Application (modify existing CQRS)
```
Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/StartWorkflow/
  StartWorkflowHandler.cs          — refactor to delegate to IWorkflowEngine
Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/ApproveWorkflowStep/
  ApproveWorkflowStepHandler.cs    — refactor to delegate to IWorkflowEngine
Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/RejectWorkflowStep/
  RejectWorkflowStepHandler.cs     — refactor to delegate to IWorkflowEngine
Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/CancelWorkflow/
  CancelWorkflowHandler.cs         — refactor to delegate to IWorkflowEngine
Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/ReturnWorkflow/
  ReturnWorkflowHandler.cs         — refactor to delegate to IWorkflowEngine
Anemoi.Hr.Application/Cqrs/Queries/WorkflowQueries/GetPendingApprovals/
  GetPendingApprovalsHandler.cs    — use CanHandle pattern
Anemoi.Hr.Application/Mappings/
  WorkflowMapper.cs                — add new properties
Anemoi.Hr.Application/Responses/
  WorkflowResponses.cs             — add new fields
Anemoi.Hr.Application/Configurations/
  HrBusinessErrorCodes.cs          — add new error codes
  HrPermissions.cs                 — add WorkflowApprove
```

### Infrastructure (modify)
```
Anemoi.Hr.Infrastructure/Configurations/
  WorkflowDefinitionModelMapping.cs — add TargetEntityType, Version columns
  WorkflowInstanceModelMapping.cs   — add RequesterEmployeeId, RequesterUserId columns
Anemoi.Hr.Infrastructure/Installers/
  ServiceInstaller.cs               — register all new services
```

### API (modify controllers)
```
Anemoi.Hr.Api/Controllers/Workflow/
  WorkflowDefinitionsController.cs  — update permissions
  WorkflowInstancesController.cs    — update permissions, add GetCurrentApprovers endpoint
```

### Contract (add integration events)
```
Anemoi.Contract/Anemoi.Contract.Hr/WorkflowIntegrationEvents.cs — NEW
```

### Notification (Phase 29: integration events only — consumers deferred to Phase 30+)
```
Anemoi.Notification/Anemoi.Notification.Application/Consumers/
  WorkflowConsumers.cs — DEFERRED (notification wiring in separate phase)
```

### BuildingBlocks (add permission constant)
```
Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/
  Permissions.cs — add HrWorkflowApprove
```

### Tests
```
Anemoi.Hr/Anemoi.Hr.Test/
  Domain/Workflow/WorkflowDefinitionTests.cs       — extend
  Domain/Workflow/WorkflowInstanceTests.cs         — extend (domain events, Requester)
  Application/Workflow/WorkflowEngineTests.cs      — NEW
  Application/Workflow/WorkflowBuilderTests.cs     — NEW
  Application/Workflow/WorkflowHierarchyResolverTests.cs — NEW
```

---

## Pre-Flight Checks (Read Before Implementing)

### P1: No `IdGenerator` in Domain (TD-001)
Domain entities must NOT generate IDs. If `WorkflowInstance.Approve()` currently uses `new WorkflowHistoryId(IdGenerator.NextGuid())`, verify whether TD-001 resolved this. If the existing codebase already passes IDs from application layer (check how other domain entities handle `HistoryId`), follow that pattern — the caller supplies the ID, the domain method receives it as a parameter.

If the existing `WorkflowInstance.Approve()` already uses `IdGenerator` and the codebase hasn't refactored this, leave it as-is for Phase 29 consistency. File a separate tech debt item.

### P2: No Exception-Based Control Flow in `WorkflowBuilder`
`WorkflowBuilder.BuildAsync()` must NOT throw `InvalidOperationException` when a definition is required but not found. Use a return type instead:
```csharp
public sealed record WorkflowBuildError(string ErrorCode);
// Return: OneOf<IReadOnlyList<WorkflowInstanceStep>, WorkflowBuildError>
```
The `WorkflowEngine.StartAsync()` maps `WorkflowBuildError` to the appropriate `ErrorDetailResponse`. This keeps business flow out of exception handling.

### P3: Verify Payroll Aggregate API Before Implementing `PayrollWorkflowStatusUpdater`
Before writing `PayrollWorkflowStatusUpdater.MarkApprovedAsync()`, check the actual `PayrollRun` aggregate:
- Does it have a public `Approve()` method? What parameters?
- Does it have a public `Reject()` method?
- What status does it expect (e.g., `SubmittedForApproval`)?
If `Approve()` doesn't exist or has different semantics, add `MarkWorkflowApproved()` to the `PayrollRun` domain entity instead.

---

## Implementation Tasks

### Task 1: Add `HrWorkflowApprove` to BuildingBlocks Permissions

**Files:**
- Modify: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/Permissions.cs`

- [ ] **Step 1: Add the permission constant**

Find the permissions file and add after existing HR workflow permissions:
```csharp
public const string HrWorkflowApprove = "hr.workflow.approve";
```

- [ ] **Step 2: Verify build**

```bash
dotnet build Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/ --no-restore
```
Expected: build succeeds

- [ ] **Step 3: Commit**

```bash
git add Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/Permissions.cs
git commit -m "Phase 29 - add HrWorkflowApprove permission constant"
```

---

### Task 2: Add `TargetEntityType` + `Version` to `WorkflowDefinition` domain entity

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowDefinition.cs`

- [ ] **Step 1: Modify WorkflowDefinition entity**

Add properties:
```csharp
public string TargetEntityType { get; private set; }
public int Version { get; private set; }
```

Update factory:
```csharp
private WorkflowDefinition(
    WorkflowDefinitionId id,
    string code,
    string name,
    string? description,
    string workflowTypeCode,
    string targetEntityType,
    int version,
    List<WorkflowDefinitionStep> steps)
{
    Id = id;
    Code = code;
    Name = name;
    Description = description;
    WorkflowTypeCode = workflowTypeCode;
    TargetEntityType = targetEntityType;
    Version = version;
    IsActive = false;
    var now = DateTime.UtcNow;
    CreatedAt = now;
    UpdatedAt = now;
    _steps = steps;
}

public static WorkflowDefinition Create(
    WorkflowDefinitionId id,
    string code,
    string name,
    string? description,
    string workflowTypeCode,
    string targetEntityType,
    int version,
    List<WorkflowDefinitionStep> steps)
{
    return new WorkflowDefinition(id, code, name, description, workflowTypeCode, targetEntityType, version, steps);
}
```

Also update `UpdateDetails` to bump `UpdatedAt` (already does). No changes to Activate/Deactivate logic.

**Note:** `Version` is **informational only** in Phase 29. No versioning logic, no unique constraints on version. Phase 30+ will define version semantics (e.g., draft vs. active version).

- [ ] **Step 2: Verify domain tests still pass**

```bash
dotnet test Anemoi.Hr/Anemoi.Hr.Test --filter "WorkflowDefinition" --no-restore
```
Expected: tests pass (may need to update test factory calls)

- [ ] **Step 3: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowDefinition.cs
git commit -m "Phase 29 - add TargetEntityType and Version to WorkflowDefinition"
```

---

### Task 3: Add `RequesterEmployeeId` + `RequesterUserId` + domain events to `WorkflowInstance`

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowInstance.cs`
- Create: New domain event classes in the same directory

- [ ] **Step 1: Create domain event records**

Create `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowInstanceApprovedDomainEvent.cs`:
```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed record WorkflowInstanceApprovedDomainEvent(
    WorkflowInstanceId WorkflowInstanceId,
    string EntityType,
    string EntityId,
    string PerformedBy) : IDomainEvent;
```

Create `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowInstanceRejectedDomainEvent.cs`:
```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed record WorkflowInstanceRejectedDomainEvent(
    WorkflowInstanceId WorkflowInstanceId,
    string EntityType,
    string EntityId,
    string PerformedBy,
    string? Comment) : IDomainEvent;
```

- [ ] **Step 2: Modify WorkflowInstance entity**

Add properties:
```csharp
public EmployeeId RequesterEmployeeId { get; private set; }
public UserId RequesterUserId { get; private set; }
```

Update constructor and factory:
```csharp
private WorkflowInstance(
    WorkflowInstanceId id,
    WorkflowDefinitionId? workflowDefinitionId,
    string entityType,
    string entityId,
    string startedBy,
    EmployeeId requesterEmployeeId,
    UserId requesterUserId,
    List<WorkflowInstanceStep> steps)
{
    Id = id;
    WorkflowDefinitionId = workflowDefinitionId;
    EntityType = entityType;
    EntityId = entityId;
    CurrentStep = steps.Count > 0 ? steps.Min(s => s.Sequence) : 0;
    Status = WorkflowStatusCode.Pending;
    StartedBy = startedBy;
    RequesterEmployeeId = requesterEmployeeId;
    RequesterUserId = requesterUserId;
    StartedAt = DateTime.UtcNow;
    _steps = steps;
}

public static WorkflowInstance Start(
    WorkflowInstanceId id,
    WorkflowDefinitionId? workflowDefinitionId,
    string entityType,
    string entityId,
    string startedBy,
    EmployeeId requesterEmployeeId,
    UserId requesterUserId,
    List<WorkflowInstanceStep> steps)
{
    return new WorkflowInstance(id, workflowDefinitionId, entityType, entityId, startedBy, requesterEmployeeId, requesterUserId, steps);
}
```

Make `WorkflowDefinitionId` nullable:
```csharp
/// <summary>
/// null = hierarchy-generated workflow (no WorkflowDefinition was used).
/// not null = definition-generated workflow.
/// </summary>
public WorkflowDefinitionId? WorkflowDefinitionId { get; private set; }
```

Add domain event raising in `Approve()` when terminal:
```csharp
public WorkflowHistory Approve(string performedBy, string? comment)
{
    EnsurePending();
    var step = GetCurrentStepEntity();
    step.Approve(comment);
    var history = WorkflowHistory.Create(
        new WorkflowHistoryId(IdGenerator.NextGuid()), Id,
        "Approve", performedBy, comment);
    AddHistory(history);

    var nextSequence = step.Sequence + 1;
    var nextStep = _steps.FirstOrDefault(s => s.Sequence == nextSequence);
    if (nextStep is null || nextStep.Status != WorkflowStepStatusCode.Pending)
    {
        Status = WorkflowStatusCode.Approved;
        CompletedAt = DateTime.UtcNow;
        AddEvent(new WorkflowInstanceApprovedDomainEvent(Id, EntityType, EntityId, performedBy));
    }
    else
    {
        CurrentStep = nextSequence;
    }

    return history;
}
```

Add domain event raising in `Reject()`:
```csharp
public WorkflowHistory Reject(string performedBy, string? comment)
{
    EnsurePending();
    var step = GetCurrentStepEntity();
    step.Reject(comment);
    Status = WorkflowStatusCode.Rejected;
    CompletedAt = DateTime.UtcNow;
    var history = WorkflowHistory.Create(
        new WorkflowHistoryId(IdGenerator.NextGuid()), Id,
        "Reject", performedBy, comment);
    AddHistory(history);
    AddEvent(new WorkflowInstanceRejectedDomainEvent(Id, EntityType, EntityId, performedBy, comment));
    return history;
}
```

Add `using Anemoi.BuildingBlock.Application.Helpers;` for `IdGenerator`.

- [ ] **Step 3: Verify build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Domain/ --no-restore
```
Expected: build succeeds

- [ ] **Step 4: Run existing workflow instance tests**

```bash
dotnet test Anemoi.Hr/Anemoi.Hr.Test --filter "WorkflowInstance" --no-restore
```
Expected: tests pass (may need to update test factory calls)

- [ ] **Step 5: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Domain/Workflow/
git commit -m "Phase 29 - add RequesterEmployeeId, RequesterUserId, domain events to WorkflowInstance"
```

---

### Task 4: Create `WorkflowConstants` and error codes

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Configurations/WorkflowConstants.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Configurations/HrBusinessErrorCodes.cs`

- [ ] **Step 1: Create WorkflowConstants**

```csharp
namespace Anemoi.Hr.Application.Configurations;

public static class WorkflowConstants
{
    public static class TargetEntityTypes
    {
        public const string LeaveRequest = "LeaveRequest";
        public const string OvertimeRequest = "OvertimeRequest";
        public const string PayrollRun = "PayrollRun";
        public const string RecruitmentRequest = "RecruitmentRequest";
    }

    public static class DefaultPolicy
    {
        public const int LeaveRequestSteps = 2;
        public const int OvertimeRequestSteps = 2;
        public const int RecruitmentRequestSteps = 0;
        public const int PayrollRunSteps = 0;

        public static int GetStepCount(string entityType) => entityType switch
        {
            TargetEntityTypes.LeaveRequest => LeaveRequestSteps,
            TargetEntityTypes.OvertimeRequest => OvertimeRequestSteps,
            TargetEntityTypes.RecruitmentRequest => RecruitmentRequestSteps,
            TargetEntityTypes.PayrollRun => PayrollRunSteps,
            _ => throw new InvalidOperationException($"No default workflow policy for '{entityType}'.")
        };

        public static bool RequiresDefinition(string entityType) => GetStepCount(entityType) == 0;
    }
}
```

- [ ] **Step 2: Add error codes**

Add to `HrBusinessErrorCodes.cs`:
```csharp
// Workflow errors
public const string WorkflowDefinitionRequiresDefinition = "HR_WF_DEF_REQUIRES_DEFINITION";
public const string WorkflowHierarchyResolutionFailed = "HR_WF_HIERARCHY_RESOLUTION_FAILED";
public const string WorkflowUniqueActiveConstraint = "HR_WF_UNIQUE_ACTIVE_CONSTRAINT";
```

- [ ] **Step 3: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Application/ --no-restore
```

- [ ] **Step 4: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Application/Configurations/WorkflowConstants.cs
git add Anemoi.Hr/Anemoi.Hr.Application/Configurations/HrBusinessErrorCodes.cs
git commit -m "Phase 29 - add WorkflowConstants and error codes"
```

---

### Task 5: Create service interfaces (`IWorkflowEngine`, `IWorkflowHierarchyResolver`, `IWorkflowBuilder`, `IWorkflowTargetStatusUpdater`)

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Abstractions/IWorkflowEngine.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Abstractions/IWorkflowHierarchyResolver.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Abstractions/IWorkflowBuilder.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Abstractions/IWorkflowTargetStatusUpdater.cs`

- [ ] **Step 1: IWorkflowEngine**

```csharp
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Abstractions;

/// <summary>
/// Internal application facade for workflow operations.
/// Does NOT call SaveChangesAsync — the caller owns the transaction.
/// Returns domain entities, not response DTOs.
/// </summary>
public interface IWorkflowEngine
{
    Task<OneOf<WorkflowInstance, ErrorDetailResponse>> StartAsync(
        string entityType,
        Guid entityId,
        EmployeeId requesterEmployeeId,
        UserId requesterUserId,
        UserId startedBy,
        CancellationToken ct);

    Task<OneOf<WorkflowInstance, ErrorDetailResponse>> ApproveAsync(
        WorkflowInstanceId workflowInstanceId,
        UserId performedBy,
        string? comment,
        CancellationToken ct);

    Task<OneOf<WorkflowInstance, ErrorDetailResponse>> RejectAsync(
        WorkflowInstanceId workflowInstanceId,
        UserId performedBy,
        string? comment,
        CancellationToken ct);

    Task<OneOf<WorkflowInstance, ErrorDetailResponse>> CancelAsync(
        WorkflowInstanceId workflowInstanceId,
        UserId performedBy,
        CancellationToken ct);

    Task<IReadOnlyList<UserId>> GetCurrentApproversAsync(
        WorkflowInstanceId workflowInstanceId,
        CancellationToken ct);
}
```

- [ ] **Step 2: IWorkflowHierarchyResolver**

```csharp
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Abstractions;

public sealed record ResolvedApproverStep(
    int StepOrder,
    string ApproverType,
    string? ApproverValue,
    UserId? ApproverUserId);

public interface IWorkflowHierarchyResolver
{
    Task<IReadOnlyList<ResolvedApproverStep>> ResolveHierarchyAsync(
        EmployeeId requesterEmployeeId,
        CancellationToken ct);
}
```

- [ ] **Step 3: IWorkflowBuilder**

```csharp
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Abstractions;

public interface IWorkflowBuilder
{
    Task<IReadOnlyList<WorkflowInstanceStep>> BuildAsync(
        string entityType,
        EmployeeId requesterEmployeeId,
        string startedBy,
        CancellationToken ct);
}
```

- [ ] **Step 4: IWorkflowTargetStatusUpdater**

```csharp
namespace Anemoi.Hr.Application.Abstractions;

public interface IWorkflowTargetStatusUpdater
{
    bool CanHandle(string entityType);

    Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct);

    Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct);
}
```

- [ ] **Step 5: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Application/ --no-restore
```

- [ ] **Step 6: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Application/Abstractions/
git commit -m "Phase 29 - add workflow service interfaces"
```

---

### Task 6: Implement `WorkflowHierarchyResolver`

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Services/WorkflowHierarchyResolver.cs`

- [ ] **Step 1: Write implementation**

**IMPORTANT:** Use batch-loading to avoid N+1 queries. Load all departments in the hierarchy chain in one query, then batch-resolve all manager employees.

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Services;

public sealed class WorkflowHierarchyResolver(
    ISqlRepository<Domain.Employees.Employee> employeeRepository,
    ISqlRepository<Domain.Departments.Department> departmentRepository)
    : IWorkflowHierarchyResolver
{
    public async Task<IReadOnlyList<ResolvedApproverStep>> ResolveHierarchyAsync(
        EmployeeId requesterEmployeeId, CancellationToken ct)
    {
        var employee = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == requesterEmployeeId, ct);

        if (employee is null)
            return [];

        var seen = new HashSet<UserId>();
        var steps = new List<ResolvedApproverStep>();
        var stepOrder = 0;

        // Step 1: Direct Manager
        if (employee.DirectManagerEmployeeId is not null)
        {
            var manager = await employeeRepository.GetQueryable()
                .FirstOrDefaultAsync(e => e.Id == employee.DirectManagerEmployeeId, ct);
            if (manager?.UserId is not null && seen.Add(manager.UserId))
                steps.Add(new ResolvedApproverStep(++stepOrder,
                    Domain.Workflow.ApproverType.SpecificUser, null, manager.UserId));
        }

        // Step 2+: Collect all department IDs in hierarchy
        var departmentIds = new List<DepartmentId>();
        var currentDeptId = employee.PrimaryDepartmentId;
        while (currentDeptId is not null)
        {
            departmentIds.Add(currentDeptId);
            var parent = await departmentRepository.GetQueryable()
                .FirstOrDefaultAsync(d => d.Id == currentDeptId, ct);
            currentDeptId = parent?.ParentDepartmentId;
        }

        // Batch-load all departments
        var departments = await departmentRepository.GetQueryable()
            .Where(d => departmentIds.Contains(d.Id))
            .ToListAsync(ct);

        var deptMap = departments.ToDictionary(d => d.Id);

        // Collect all manager employee IDs
        var managerEmployeeIds = departments
            .Select(d => d.ManagerEmployeeId)
            .Where(id => id is not null)
            .Cast<EmployeeId>()
            .Distinct()
            .ToList();

        // Batch-load all manager employees
        var managers = managerEmployeeIds.Count > 0
            ? await employeeRepository.GetQueryable()
                .Where(e => managerEmployeeIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, ct)
            : [];

        // Build steps from department chain (preserving order)
        foreach (var deptId in departmentIds)
        {
            if (!deptMap.TryGetValue(deptId, out var dept) || dept.ManagerEmployeeId is null)
                continue;

            if (!managers.TryGetValue(dept.ManagerEmployeeId, out var mgr) || mgr.UserId is null)
                continue;

            if (seen.Add(mgr.UserId))
                steps.Add(new ResolvedApproverStep(++stepOrder,
                    Domain.Workflow.ApproverType.SpecificUser, null, mgr.UserId));
        }

        return steps;
    }
}
```

Batch-loading reduces query count from O(N) to O(1) regardless of department hierarchy depth.

- [ ] **Step 2: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Application/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Application/Services/WorkflowHierarchyResolver.cs
git commit -m "Phase 29 - implement WorkflowHierarchyResolver"
```

---

### Task 7: Implement `WorkflowBuilder`

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Services/WorkflowBuilder.cs`

- [ ] **Step 1: Write implementation**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Services;

public sealed class WorkflowBuilder(
    ISqlRepository<WorkflowDefinition> definitionRepository,
    IWorkflowHierarchyResolver hierarchyResolver)
    : IWorkflowBuilder
{
    public async Task<IReadOnlyList<WorkflowInstanceStep>> BuildAsync(
        string entityType, EmployeeId requesterEmployeeId, string startedBy, CancellationToken ct)
    {
        var definition = await definitionRepository.GetQueryable()
            .Include(d => d.Steps)
            .Where(d => d.TargetEntityType == entityType && d.IsActive)
            .FirstOrDefaultAsync(ct);

        // Sort steps in memory — EF Core does not support OrderBy in Include
        var sortedSteps = definition?.Steps.OrderBy(s => s.Sequence).ToList();

        if (definition is not null)
            return BuildFromDefinition(sortedSteps!, requesterEmployeeId);

        if (WorkflowConstants.DefaultPolicy.RequiresDefinition(entityType))
            throw new InvalidOperationException(
                $"No active WorkflowDefinition found for '{entityType}' and hierarchy fallback is disabled.");

        return await BuildFromHierarchyAsync(entityType, requesterEmployeeId, ct);
    }

    private static IReadOnlyList<WorkflowInstanceStep> BuildFromDefinition(
        List<WorkflowDefinitionStep> steps, EmployeeId requesterEmployeeId)
    {
        return steps.Select(s =>
        {
            var stepId = new WorkflowInstanceStepId(IdGenerator.NextGuid());
            string? approverUserId = s.ApproverType switch
            {
                ApproverType.SpecificUser => s.ApproverValue,
                ApproverType.DirectManager => null, // resolved at start time via hierarchy
                _ => null
            };
            return WorkflowInstanceStep.Create(stepId, default, s.Sequence,
                s.ApproverType, s.ApproverValue, approverUserId);
        }).ToList();
    }

    private async Task<IReadOnlyList<WorkflowInstanceStep>> BuildFromHierarchyAsync(
        string entityType, EmployeeId requesterEmployeeId, CancellationToken ct)
    {
        var hierarchy = await hierarchyResolver.ResolveHierarchyAsync(requesterEmployeeId, ct);
        var maxSteps = WorkflowConstants.DefaultPolicy.GetStepCount(entityType);
        var selected = hierarchy.Take(maxSteps).ToList();

        return selected.Select(s =>
        {
            var stepId = new WorkflowInstanceStepId(IdGenerator.NextGuid());
            return WorkflowInstanceStep.Create(stepId, default, s.StepOrder,
                ApproverType.SpecificUser, null,
                s.ApproverUserId?.Value.ToString());
        }).ToList();
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Application/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Application/Services/WorkflowBuilder.cs
git commit -m "Phase 29 - implement WorkflowBuilder"
```

---

### Task 8: Implement `WorkflowEngine`

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Services/WorkflowEngine.cs`

- [ ] **Step 1: Write implementation**

**IMPORTANT ARCHITECTURE RULE:** `WorkflowEngine` must NOT call `SaveChangesAsync`. The caller (CQRS handler or business handler) owns the transaction. Engine only creates/mutates entities and returns them. This matches the existing Anemoi pattern where the handler calls `unitOfWork.SaveChangesAsync()`.

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Services;

public sealed class WorkflowEngine(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowHistory> historyRepository,
    IWorkflowBuilder workflowBuilder)
    : IWorkflowEngine
{
    public async Task<OneOf<WorkflowInstance, ErrorDetailResponse>> StartAsync(
        string entityType, Guid entityId,
        EmployeeId requesterEmployeeId, UserId requesterUserId,
        UserId startedBy, CancellationToken ct)
    {
        IReadOnlyList<WorkflowInstanceStep> steps;
        try
        {
            steps = await workflowBuilder.BuildAsync(entityType, requesterEmployeeId, startedBy.ToString(), ct);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionRequiresDefinition);
        }

        var instanceId = new WorkflowInstanceId(IdGenerator.NextGuid());

        var instance = WorkflowInstance.Start(
            instanceId, null, entityType, entityId.ToString(),
            startedBy.Value.ToString(),
            requesterEmployeeId, requesterUserId,
            steps.ToList());

        var createResult = await instanceRepository.CreateOneAsync(instance, ct);
        if (createResult.TryPickT1(out _, out _))
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return instance;
    }

    public async Task<OneOf<WorkflowInstance, ErrorDetailResponse>> ApproveAsync(
        WorkflowInstanceId workflowInstanceId, UserId performedBy, string? comment, CancellationToken ct)
    {
        var instance = await LoadInstanceAsync(workflowInstanceId, ct);
        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        try
        {
            if (!IsCurrentStepApprover(instance, performedBy))
                return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotApprover);

            var history = instance.Approve(performedBy.Value.ToString(), comment);
            await historyRepository.CreateOneAsync(history, ct);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceInvalidStatus);
        }

        return instance;
    }

    public async Task<OneOf<WorkflowInstance, ErrorDetailResponse>> RejectAsync(
        WorkflowInstanceId workflowInstanceId, UserId performedBy, string? comment, CancellationToken ct)
    {
        var instance = await LoadInstanceAsync(workflowInstanceId, ct);
        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        try
        {
            if (!IsCurrentStepApprover(instance, performedBy))
                return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotApprover);

            var history = instance.Reject(performedBy.Value.ToString(), comment);
            await historyRepository.CreateOneAsync(history, ct);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceInvalidStatus);
        }

        return instance;
    }

    public async Task<OneOf<WorkflowInstance, ErrorDetailResponse>> CancelAsync(
        WorkflowInstanceId workflowInstanceId, UserId performedBy, CancellationToken ct)
    {
        var instance = await LoadInstanceAsync(workflowInstanceId, ct);
        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        try
        {
            var history = instance.Cancel(performedBy.Value.ToString());
            await historyRepository.CreateOneAsync(history, ct);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceInvalidStatus);
        }

        return instance;
    }

    public async Task<IReadOnlyList<UserId>> GetCurrentApproversAsync(
        WorkflowInstanceId workflowInstanceId, CancellationToken ct)
    {
        var instance = await LoadInstanceAsync(workflowInstanceId, ct);
        if (instance is null) return [];

        var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
        if (step is null) return [];

        return step.ApproverTypeSnapshot switch
        {
            ApproverType.SpecificUser when step.ApproverUserId is not null
                => [new UserId(Guid.Parse(step.ApproverUserId))],
            ApproverType.DirectManager when step.ApproverUserId is not null
                => [new UserId(Guid.Parse(step.ApproverUserId))],
            _ => []
        };
    }

    private async Task<WorkflowInstance?> LoadInstanceAsync(WorkflowInstanceId id, CancellationToken ct)
    {
        return await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    private static bool IsCurrentStepApprover(WorkflowInstance instance, UserId userId)
    {
        return instance.IsCurrentStepApprover(
            userId.Value.ToString(),
            _ => false, // role check — Phase 30+: integrate IUserRolePermissionService
            _ => false  // permission check — Phase 30+: integrate
        );
    }
}
```

Note: `IsCurrentStepApprover` passes `false` for role/permission checks. Acceptable for Phase 29 where hierarchy resolver always produces `SpecificUser` steps. Phase 30+ integrates `IUserRolePermissionService`.

- [ ] **Step 2: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Application/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Application/Services/WorkflowEngine.cs
git commit -m "Phase 29 - implement WorkflowEngine"
```

---

### Task 9: Create `IWorkflowTargetStatusUpdater` implementations

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/WorkflowTargetStatusUpdaters/LeaveWorkflowStatusUpdater.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/WorkflowTargetStatusUpdaters/OvertimeWorkflowStatusUpdater.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/WorkflowTargetStatusUpdaters/PayrollWorkflowStatusUpdater.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/WorkflowTargetStatusUpdaters/RecruitmentWorkflowStatusUpdater.cs`

- [ ] **Step 1: LeaveWorkflowStatusUpdater**

**IMPORTANT:** Must call domain methods, not set properties directly. If `LeaveRequest` lacks a `MarkWorkflowApproved()` method, add one to the domain entity.

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class LeaveWorkflowStatusUpdater(
    ISqlRepository<LeaveRequest> leaveRepository)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.LeaveRequest;

    public async Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new LeaveRequestId(Guid.Parse(entityId));
        var leave = await leaveRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (leave is null) return;
        // Must use domain method — do NOT set StatusCode directly
        leave.MarkWorkflowApproved(performedBy);
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new LeaveRequestId(Guid.Parse(entityId));
        var leave = await leaveRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (leave is null) return;
        leave.MarkWorkflowRejected(performedBy, reason);
    }
}
```

Add domain methods to `LeaveRequest` if not present:
```csharp
// In LeaveRequest.cs
public void MarkWorkflowApproved(string approverId)
{
    StatusCode = LeaveRequestStatusCode.Approved;
    ApproverEmployeeId = new EmployeeId(Guid.Parse(approverId));
}

public void MarkWorkflowRejected(string approverId, string? reason)
{
    StatusCode = LeaveRequestStatusCode.Rejected;
    ApproverEmployeeId = new EmployeeId(Guid.Parse(approverId));
    // Add rejection reason if LeaveRequest has a reason field
}
```

- [ ] **Step 2: OvertimeWorkflowStatusUpdater**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Overtime;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class OvertimeWorkflowStatusUpdater(
    ISqlRepository<OvertimeRequest> overtimeRepository)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.OvertimeRequest;

    public async Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new OvertimeRequestId(Guid.Parse(entityId));
        var overtime = await overtimeRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (overtime is null) return;
        overtime.Approve(performedBy);
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new OvertimeRequestId(Guid.Parse(entityId));
        var overtime = await overtimeRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (overtime is null) return;
        overtime.Reject(performedBy, reason);
    }
}
```

- [ ] **Step 3: PayrollWorkflowStatusUpdater**

**Note:** Verify `PayrollRun` has `Approve()` and `Reject()` methods. Current aggregate status flow: `Calculated` → `SubmittedForApproval` → `Approved` → `Finalized`. Workflow moves `SubmittedForApproval` → `Approved`. `Finalize()` is NOT part of workflow — handled separately.

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class PayrollWorkflowStatusUpdater(
    ISqlRepository<PayrollRun> payrollRepository)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.PayrollRun;

    public async Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new PayrollRunId(Guid.Parse(entityId));
        var payroll = await payrollRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (payroll is null) return;
        payroll.Approve();
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new PayrollRunId(Guid.Parse(entityId));
        var payroll = await payrollRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (payroll is null) return;
        payroll.Reject();
    }
}
```

- [ ] **Step 4: RecruitmentWorkflowStatusUpdater**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class RecruitmentWorkflowStatusUpdater(
    ISqlRepository<RecruitmentRequest> recruitmentRepository)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.RecruitmentRequest;

    public async Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new RecruitmentRequestId(Guid.Parse(entityId));
        var request = await recruitmentRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (request is null) return;
        request.Approve(performedBy, DateTime.UtcNow);
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new RecruitmentRequestId(Guid.Parse(entityId));
        var request = await recruitmentRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (request is null) return;
        request.Reject(performedBy, DateTime.UtcNow, reason);
    }
}
```

- [ ] **Step 5: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Application/ --no-restore
```

- [ ] **Step 6: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Application/WorkflowTargetStatusUpdaters/
git commit -m "Phase 29 - add IWorkflowTargetStatusUpdater implementations"
```

---

### Task 10: Refactor CQRS handlers to delegate to `IWorkflowEngine`

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/StartWorkflow/StartWorkflowHandler.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/ApproveWorkflowStep/ApproveWorkflowStepHandler.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/RejectWorkflowStep/RejectWorkflowStepHandler.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/CancelWorkflow/CancelWorkflowHandler.cs`

**IMPORTANT:** Since `IWorkflowEngine` now returns `WorkflowInstance` (not `WorkflowInstanceResponse`), handlers must use `WorkflowMapper` to convert. The handler owns the transaction — it calls `unitOfWork.SaveChangesAsync()` after the engine returns. This matches the existing Anemoi pattern.

- [ ] **Step 1: Refactor StartWorkflowHandler**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.StartWorkflow;

public sealed class StartWorkflowHandler(
    IWorkflowEngine workflowEngine,
    WorkflowMapper mapper,
    IUnitOfWork unitOfWork)
    : ICommandHandler<StartWorkflowCommand, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        var result = await workflowEngine.StartAsync(
            request.EntityType,
            Guid.Parse(request.EntityId),
            new EmployeeId(Guid.Parse(request.RequesterEmployeeId)),
            new UserId(Guid.Parse(request.RequesterUserId)),
            new UserId(Guid.Parse(request.StartedBy)),
            cancellationToken);

        return await result.Match<Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>>(async instance =>
        {
            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (saveResult.TryPickT1(out _, out _))
                return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
            return mapper.ToResponse(instance);
        }, error => Task.FromResult<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>(error));
    }
}
```

Note: The `StartWorkflowCommand` DTO may need updating to include `RequesterEmployeeId` and `RequesterUserId`. Check the existing command:
```csharp
public sealed record StartWorkflowCommand(
    string DefinitionId, string EntityType, string EntityId,
    string RequesterEmployeeId, string RequesterUserId,
    string StartedBy) : ICommand<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>;
```

- [ ] **Step 2: Refactor ApproveWorkflowStepHandler**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ApproveWorkflowStep;

public sealed class ApproveWorkflowStepHandler(
    IWorkflowEngine workflowEngine,
    WorkflowMapper mapper,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ApproveWorkflowStepCommand, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        ApproveWorkflowStepCommand request, CancellationToken cancellationToken)
    {
        var result = await workflowEngine.ApproveAsync(
            new WorkflowInstanceId(Guid.Parse(request.InstanceId)),
            new UserId(Guid.Parse(request.PerformedBy)),
            request.Comment,
            cancellationToken);

        return await result.Match<Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>>(async instance =>
        {
            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (saveResult.TryPickT1(out _, out _))
                return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
            return mapper.ToResponse(instance);
        }, error => Task.FromResult<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>(error));
    }
}
```

- [ ] **Step 3: Refactor RejectWorkflowStepHandler**

Same pattern as ApproveWorkflowStepHandler, calling `workflowEngine.RejectAsync(...)`.

- [ ] **Step 4: Refactor CancelWorkflowHandler**

Same pattern, calling `workflowEngine.CancelAsync(...)`.

- [ ] **Step 5: Update StartWorkflowCommand DTO**

Check `StartWorkflowCommand.cs` and add `RequesterEmployeeId` and `RequesterUserId` if not present:

- [ ] **Step 6: Build and run existing workflow tests**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Application/ --no-restore
dotnet test Anemoi.Hr/Anemoi.Hr.Test --filter "Workflow" --no-restore
```
Expected: build succeeds (existing tests may need updates for new constructor signatures)

- [ ] **Step 7: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/
git commit -m "Phase 29 - refactor CQRS handlers to delegate to IWorkflowEngine"
```

---

### Task 11: Update `WorkflowMapper` and `WorkflowResponses` with new properties

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Mappings/WorkflowMapper.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Responses/WorkflowResponses.cs`

- [ ] **Step 1: Add new fields to response DTOs**

In `WorkflowResponses.cs`, add to `WorkflowInstanceResponse`:
```csharp
public sealed record WorkflowInstanceResponse(
    string Id, string? WorkflowDefinitionId, string WorkflowDefinitionName,
    string EntityType, string EntityId, int CurrentStep,
    string Status, string StartedBy, string RequesterEmployeeId, string RequesterUserId,
    DateTime StartedAt, DateTime? CompletedAt,
    IReadOnlyCollection<WorkflowInstanceStepResponse> Steps,
    IReadOnlyCollection<WorkflowHistoryResponse> Histories);
```

- [ ] **Step 2: Update mapper**

In `WorkflowMapper.cs`, update `ToResponse(WorkflowInstance, ...)`:
```csharp
public WorkflowInstanceResponse ToResponse(WorkflowInstance instance, string definitionName = null)
{
    if (instance is null) return null;
    return new WorkflowInstanceResponse(
        Id: instance.Id.Value.ToString(),
        WorkflowDefinitionId: instance.WorkflowDefinitionId?.Value.ToString(),
        WorkflowDefinitionName: definitionName,
        EntityType: instance.EntityType,
        EntityId: instance.EntityId,
        CurrentStep: instance.CurrentStep,
        Status: instance.Status,
        StartedBy: instance.StartedBy,
        RequesterEmployeeId: instance.RequesterEmployeeId.Value.ToString(),
        RequesterUserId: instance.RequesterUserId.Value.ToString(),
        StartedAt: instance.StartedAt,
        CompletedAt: instance.CompletedAt,
        Steps: instance.Steps.Select(ToStepResponse).ToList(),
        Histories: instance.Histories.Select(ToHistoryResponse).ToList());
}
```

Also update `WorkflowDefinitionResponse`:
```csharp
public sealed record WorkflowDefinitionResponse(
    string Id, string Code, string Name, string? Description,
    string WorkflowTypeCode, string TargetEntityType, int Version, bool IsActive,
    IReadOnlyCollection<WorkflowDefinitionStepResponse> Steps,
    DateTime CreatedAt, DateTime UpdatedAt);
```

And the definition mapper:
```csharp
public WorkflowDefinitionResponse ToResponse(WorkflowDefinition def)
{
    if (def is null) return null;
    return new WorkflowDefinitionResponse(
        Id: def.Id.Value.ToString(),
        Code: def.Code,
        Name: def.Name,
        Description: def.Description,
        WorkflowTypeCode: def.WorkflowTypeCode,
        TargetEntityType: def.TargetEntityType,
        Version: def.Version,
        IsActive: def.IsActive,
        Steps: def.Steps.Select(ToStepResponse).ToList(),
        CreatedAt: def.CreatedAt,
        UpdatedAt: def.UpdatedAt);
}
```

- [ ] **Step 3: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Application/ --no-restore
```

- [ ] **Step 4: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Application/Mappings/WorkflowMapper.cs
git add Anemoi.Hr/Anemoi.Hr.Application/Responses/WorkflowResponses.cs
git commit -m "Phase 29 - update mapper and responses with new properties"
```

---

### Task 12: Update `GetMyPendingApprovalsQuery` handler to use `CanHandle` pattern

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/WorkflowQueries/GetPendingApprovals/GetPendingApprovalsHandler.cs`

- [ ] **Step 1: Update handler to use `CanHandle` for permission checks**

The existing handler checks `UserId` against `ApproverUserId` directly. Replace the `SpecificUser` / `DirectManager` check with the new pattern. The key change is that `ApproverType.SpecificUser` and `ApproverType.DirectManager` checks should match `step.ApproverUserId == request.UserId`:

```csharp
// The existing logic is already correct for SpecificUser/DirectManager
// Update the using statements to match new project structure
// No major change needed — just ensure RequesterEmployeeId and RequesterUserId
// are included in the query projection
```

Keep existing logic. The only update needed is ensuring the response includes `RequesterEmployeeId` and `RequesterUserId`.

- [ ] **Step 2: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Application/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git commit -m "Phase 29 - update GetPendingApprovalsHandler for new response fields"
```

---

### Task 13: Update EF Core model mappings

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Configurations/WorkflowDefinitionModelMapping.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Configurations/WorkflowInstanceModelMapping.cs`

- [ ] **Step 1: Update WorkflowDefinitionModelMapping**

Add TargetEntityType and Version:
```csharp
builder.Property(x => x.TargetEntityType).HasMaxLength(100).IsRequired();
builder.Property(x => x.Version).IsRequired();

// Add index for lookup
builder.HasIndex(x => new { x.TargetEntityType, x.IsActive });
```

- [ ] **Step 2: Update WorkflowInstanceModelMapping**

Add RequesterEmployeeId and RequesterUserId (nullable — existing records lack this data):
```csharp
builder.Property(x => x.RequesterEmployeeId)
    .HasConversion(x => x.Value, id => new EmployeeId(id))
    .HasColumnType("uuid")
    .IsRequired(false);  // nullable for existing records

builder.Property(x => x.RequesterUserId)
    .HasConversion(x => x.Value, id => new UserId(id))
    .HasMaxLength(128)
    .IsRequired(false);  // nullable for existing records

// Nullable WorkflowDefinitionId
builder.Property(x => x.WorkflowDefinitionId)
    .HasConversion(x => x.Value, id => new WorkflowDefinitionId(id))
    .HasColumnType("uuid")
    .IsRequired(false); // changed from IsRequired()
```

- [ ] **Step 3: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Infrastructure/ --no-restore
```

- [ ] **Step 4: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Infrastructure/Configurations/
git commit -m "Phase 29 - update EF Core model mappings for new properties"
```

---

### Task 14: Update `ServiceInstaller` to register new services

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Installers/ServiceInstaller.cs`

- [ ] **Step 1: Add registrations**

```csharp
services.AddScoped<IWorkflowEngine, WorkflowEngine>();
services.AddScoped<IWorkflowHierarchyResolver, WorkflowHierarchyResolver>();
services.AddScoped<IWorkflowBuilder, WorkflowBuilder>();
services.AddScoped<IWorkflowTargetStatusUpdater, LeaveWorkflowStatusUpdater>();
services.AddScoped<IWorkflowTargetStatusUpdater, OvertimeWorkflowStatusUpdater>();
services.AddScoped<IWorkflowTargetStatusUpdater, PayrollWorkflowStatusUpdater>();
services.AddScoped<IWorkflowTargetStatusUpdater, RecruitmentWorkflowStatusUpdater>();
```

Also register the domain event handlers if using MediatR `INotificationHandler`:
```csharp
services.AddScoped<INotificationHandler<WorkflowInstanceApprovedDomainEvent>, WorkflowInstanceApprovedHandler>();
services.AddScoped<INotificationHandler<WorkflowInstanceRejectedDomainEvent>, WorkflowInstanceRejectedHandler>();
```

- [ ] **Step 2: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Infrastructure/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Infrastructure/Installers/ServiceInstaller.cs
git commit -m "Phase 29 - register workflow services in DI"
```

---

### Task 15: Create domain event handlers for workflow status updates

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Events/WorkflowInstanceApprovedHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Events/WorkflowInstanceRejectedHandler.cs`

- [ ] **Step 1: WorkflowInstanceApprovedHandler**

```csharp
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Domain.Workflow;
using MediatR;

namespace Anemoi.Hr.Application.Cqrs.Events;

public sealed class WorkflowInstanceApprovedHandler(
    IEnumerable<IWorkflowTargetStatusUpdater> updaters)
    : INotificationHandler<WorkflowInstanceApprovedDomainEvent>
{
    public async Task Handle(WorkflowInstanceApprovedDomainEvent evt, CancellationToken cancellationToken)
    {
        var updater = updaters.FirstOrDefault(u => u.CanHandle(evt.EntityType));
        if (updater is null) return;
        await updater.MarkApprovedAsync(evt.EntityId, evt.PerformedBy, cancellationToken);
    }
}
```

- [ ] **Step 2: WorkflowInstanceRejectedHandler**

```csharp
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Domain.Workflow;
using MediatR;

namespace Anemoi.Hr.Application.Cqrs.Events;

public sealed class WorkflowInstanceRejectedHandler(
    IEnumerable<IWorkflowTargetStatusUpdater> updaters)
    : INotificationHandler<WorkflowInstanceRejectedDomainEvent>
{
    public async Task Handle(WorkflowInstanceRejectedDomainEvent evt, CancellationToken cancellationToken)
    {
        var updater = updaters.FirstOrDefault(u => u.CanHandle(evt.EntityType));
        if (updater is null) return;
        await updater.MarkRejectedAsync(evt.EntityId, evt.PerformedBy, evt.Comment, cancellationToken);
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Application/ --no-restore
```

- [ ] **Step 4: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Events/
git commit -m "Phase 29 - add domain event handlers for workflow status updates"
```

---

### Task 16: Update HrPermissions with WorkflowApprove

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Configurations/HrPermissions.cs`

- [ ] **Step 1: Add WorkflowApprove constant and update lists**

```csharp
public const string WorkflowApprove = Permissions.HrWorkflowApprove;
```

Add to `All` list:
```csharp
WorkflowApprove,
```

Add to `SensitivePermissions`:
```csharp
[WorkflowApprove] = new(WorkflowApprove, "High"),
```

- [ ] **Step 2: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Application/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Application/Configurations/HrPermissions.cs
git commit -m "Phase 29 - add WorkflowApprove permission"
```

---

### Task 17: Update controllers with new permissions

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Api/Controllers/Workflow/WorkflowDefinitionsController.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Api/Controllers/Workflow/WorkflowInstancesController.cs`

- [ ] **Step 1: Update WorkflowDefinitionsController**

Replace `HasPermission(HrPermissions.WorkflowExecute)` with `HasPermission(HrPermissions.WorkflowApprove)` in all approve/reject/cancel/return actions.

- [ ] **Step 2: Update WorkflowInstancesController**

Same permission changes. Also add a `GetCurrentApprovers` endpoint:
```csharp
[HttpGet("{id}/approvers")]
[HasPermission(HrPermissions.WorkflowView)]
[ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetCurrentApprovers(
    [FromRoute] string id, CancellationToken cancellationToken)
{
    var approvers = await workflowEngine.GetCurrentApproversAsync(
        new WorkflowInstanceId(Guid.Parse(id)), cancellationToken);
    return Ok(approvers.Select(a => a.Value.ToString()));
}
```

- [ ] **Step 3: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Api/ --no-restore
```

- [ ] **Step 4: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Api/Controllers/Workflow/
git commit -m "Phase 29 - update controllers with new permissions and GetCurrentApprovers"
```

---

### Task 18: Add `[Obsolete]` to legacy direct approve/reject commands

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/LeaveRequestCommands/ApproveLeaveRequest/ApproveLeaveRequestCommand.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/LeaveRequestCommands/RejectLeaveRequest/RejectLeaveRequestCommand.cs`
- Find and modify equivalent Overtime commands

- [ ] **Step 1: Mark commands as [Obsolete]**

```csharp
[Obsolete("Use ApproveWorkflowStepCommand instead")]
public sealed record ApproveLeaveRequestCommand(...)
```

Same for `RejectLeaveRequestCommand` and Overtime equivalents.

Do NOT mark `ForceApproveLeaveRequestCommand` — it's kept for emergency bypass.

- [ ] **Step 2: Build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Application/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/
git commit -m "Phase 29 - mark legacy approve/reject commands as [Obsolete]"
```

---

### Task 19: Create integration events in Contract

**Files:**
- Create: `Anemoi.Contract/Anemoi.Contract.Hr/WorkflowIntegrationEvents.cs`

- [ ] **Step 1: Write integration events**

```csharp
using MassTransit;

namespace Anemoi.Contract.Hr;

[EntityName("workflow-started")]
public sealed record WorkflowStartedIntegrationEvent(
    Guid WorkflowInstanceId,
    string EntityType,
    string EntityId,
    string StartedBy,
    string RequesterUserId,
    string RequesterEmployeeId,
    DateTime StartedAt);

[EntityName("workflow-approved")]
public sealed record WorkflowApprovedIntegrationEvent(
    Guid WorkflowInstanceId,
    string EntityType,
    string EntityId,
    string PerformedBy,
    string? Comment,
    DateTime ApprovedAt,
    bool IsCompleted);

[EntityName("workflow-rejected")]
public sealed record WorkflowRejectedIntegrationEvent(
    Guid WorkflowInstanceId,
    string EntityType,
    string EntityId,
    string PerformedBy,
    string? Comment,
    DateTime RejectedAt,
    bool IsCompleted);
```

- [ ] **Step 2: Build**

```bash
dotnet build Anemoi.Contract/Anemoi.Contract.Hr/ --no-restore
```

- [ ] **Step 3: Commit**

```bash
git add Anemoi.Contract/Anemoi.Contract.Hr/WorkflowIntegrationEvents.cs
git commit -m "Phase 29 - add workflow integration events"
```

---

### Task 20: Publish integration events from domain events (notification consumers deferred)

**Files:**
- No new consumer files in Phase 29 — notification wiring is deferred to a dedicated phase

**IMPORTANT:** Phase 29 does NOT implement Notification consumers. The fake/placeholder consumers in the original plan would produce dead code. Instead, Phase 29:
1. Publishes `WorkflowStartedIntegrationEvent`, `WorkflowApprovedIntegrationEvent`, `WorkflowRejectedIntegrationEvent` via MediatR domain event pipeline (which automatically dispatches to MassTransit after SaveChanges via existing infrastructure)
2. Notification consumers (WorkflowStartedConsumer, WorkflowApprovedConsumer, WorkflowRejectedConsumer) will be implemented in the Phase 30 notification integration phase

- [ ] **Step 1: Verify integration events are published**

The existing domain event → integration event pipeline in Anemoi should automatically publish integration events when domain events are raised and `SaveChangesAsync` completes. Verify this by checking if the existing MediatR pipeline or EF Core interceptor publishes integration events (look at existing patterns like `RecruitmentRequestSubmittedDomainEvent` → integration event).

If there's a custom `IEventBus` or `IntegrationEventPublisher` pattern, wire:
```csharp
// In WorkflowInstanceApprovedHandler or a separate integration event handler:
public class WorkflowApprovedIntegrationEventPublisher(
    IEventBus eventBus) 
    : INotificationHandler<WorkflowInstanceApprovedDomainEvent>
{
    public async Task Handle(WorkflowInstanceApprovedDomainEvent evt, CancellationToken ct)
    {
        await eventBus.PublishAsync(new WorkflowApprovedIntegrationEvent(
            evt.WorkflowInstanceId.Value,
            evt.EntityType,
            evt.EntityId,
            evt.PerformedBy,
            null, // comment — capture from domain event if available
            DateTime.UtcNow,
            true  // IsCompleted — always true for terminal approve
        ), ct);
    }
}
```

Follow the exact integration event publishing pattern used by existing modules (check `RecruitmentRequestSubmittedIntegrationEvent` for reference).

- [ ] **Step 2: Commit**

```bash
git commit -m "Phase 29 - wire workflow integration event publishing via existing domain event pipeline"
```

---

### Task 21: Update domain tests

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Test/Domain/Workflow/WorkflowDefinitionTests.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Test/Domain/Workflow/WorkflowInstanceTests.cs`

- [ ] **Step 1: Update WorkflowDefinitionTests**

Update factory calls to include `TargetEntityType` and `Version`:
```csharp
// Before:
var def = WorkflowDefinition.Create(id, "LEAVE", "Leave Approval", null, "Approval", steps);
// After:
var def = WorkflowDefinition.Create(id, "LEAVE", "Leave Approval", null, "Approval",
    "LeaveRequest", 1, steps);
```

Add tests for `TargetEntityType`:
```csharp
[Fact]
public void Create_Should_Set_TargetEntityType()
{
    var id = new WorkflowDefinitionId(Guid.NewGuid());
    var steps = CreateSteps(id);
    var def = WorkflowDefinition.Create(id, "LEAVE", "Leave", null, "Approval",
        "LeaveRequest", 1, steps);
    Assert.Equal("LeaveRequest", def.TargetEntityType);
}

[Fact]
public void Activate_Should_Throw_When_TargetEntityType_Empty()
{
    // handled by DB constraint, not domain — no throw expected
}
```

- [ ] **Step 2: Update WorkflowInstanceTests**

Update factory calls to include `RequesterEmployeeId` and `RequesterUserId`:
```csharp
// Before:
var instance = WorkflowInstance.Start(id, defId, "LeaveRequest", "entity-1", "user-1", steps);
// After:
var instance = WorkflowInstance.Start(id, defId, "LeaveRequest", "entity-1", "user-1",
    new EmployeeId(Guid.NewGuid()), new UserId(Guid.NewGuid()), steps);
```

Add tests for domain events:
```csharp
[Fact]
public void Approve_LastStep_Should_Raise_ApprovedDomainEvent()
{
    var instance = CreatePendingInstance(steps: [CreateStep(1)]);
    instance.Approve("approver-1", null);

    var domainEvents = instance.GetDomainEvents();
    Assert.Contains(domainEvents, e => e is WorkflowInstanceApprovedDomainEvent);
}

[Fact]
public void Approve_NonTerminalStep_Should_Not_Raise_DomainEvent()
{
    var instance = CreatePendingInstance(steps: [CreateStep(1), CreateStep(2)]);
    instance.Approve("approver-1", null);

    var domainEvents = instance.GetDomainEvents();
    Assert.DoesNotContain(domainEvents, e => e is WorkflowInstanceApprovedDomainEvent);
}

[Fact]
public void Reject_Should_Raise_RejectedDomainEvent()
{
    var instance = CreatePendingInstance(steps: [CreateStep(1)]);
    instance.Reject("approver-1", "Not needed");

    var domainEvents = instance.GetDomainEvents();
    Assert.Contains(domainEvents, e => e is WorkflowInstanceRejectedDomainEvent);
}
```

Note: `GetDomainEvents()` may need to be exposed via a method on `Entity<TId>` or via reflection. The base class has `_domainEvents` — add a method if needed:
```csharp
// In Entity.cs or test helper
public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => DomainEvents;
```

- [ ] **Step 3: Run tests**

```bash
dotnet test Anemoi.Hr/Anemoi.Hr.Test --filter "Workflow" --no-restore
```
Expected: all tests pass

- [ ] **Step 4: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Test/Domain/Workflow/
git commit -m "Phase 29 - update domain tests for new properties and domain events"
```

---

### Task 22: Create application layer tests

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Test/Application/Workflow/WorkflowEngineTests.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Test/Application/Workflow/WorkflowBuilderTests.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Test/Application/Workflow/WorkflowHierarchyResolverTests.cs`

- [ ] **Step 1: WorkflowBuilderTests**

```csharp
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Services;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Moq;
using Xunit;

namespace Anemoi.Hr.Test.Application.Workflow;

public sealed class WorkflowBuilderTests
{
    // Test: definition found → returns steps from definition
    // Test: no definition for LeaveRequest → returns hierarchy steps
    // Test: no definition for PayrollRun → throws
}
```

Implement using Moq to mock `ISqlRepository<WorkflowDefinition>` and `IWorkflowHierarchyResolver`.

- [ ] **Step 2: WorkflowHierarchyResolverTests**

```csharp
[Fact]
public async Task ResolveHierarchyAsync_Should_Deduplicate_Same_Manager()
{
    // Arrange: Employee with DirectManager = A, Department.Manager = A
    // Act: call resolver
    // Assert: A appears only once
}
```

- [ ] **Step 3: WorkflowEngineTests**

```csharp
[Fact]
public async Task StartAsync_Should_Create_Instance_And_Return_Id()
{
    // Mock IWorkflowBuilder to return steps
    // Mock ISqlRepository<WorkflowInstance> to accept instance
    // Assert: returns WorkflowInstanceId
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test Anemoi.Hr/Anemoi.Hr.Test --filter "Workflow" --no-restore
```

- [ ] **Step 5: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Test/Application/Workflow/
git commit -m "Phase 29 - add application layer tests for workflow engine"
```

---

### Task 23: Create EF Core migration

**Files:**
- Run migration commands to generate migration artifacts

**IMPORTANT:** `RequesterEmployeeId` and `RequesterUserId` must be **nullable** in the migration. Never create fake GUIDs. Existing records will have NULL values; new records will be populated by the engine. The EF model mapping should use `.IsRequired(false)`.

- [ ] **Step 1: Update EF model mapping for nullable requester fields**

In `WorkflowInstanceModelMapping.cs`:
```csharp
builder.Property(x => x.RequesterEmployeeId)
    .HasConversion(x => x.Value, id => new EmployeeId(id))
    .HasColumnType("uuid")
    .IsRequired(false);  // nullable — existing records lack this data

builder.Property(x => x.RequesterUserId)
    .HasConversion(x => x.Value, id => new UserId(id))
    .HasMaxLength(128)
    .IsRequired(false);  // nullable — existing records lack this data
```

- [ ] **Step 2: Add migration**

```bash
dotnet ef migrations add Phase29_AddTargetEntityTypeAndRequester \
  --project Anemoi.Hr/Anemoi.Hr.Infrastructure/ \
  --startup-project Anemoi.Hr/Anemoi.Hr.Api/
```

- [ ] **Step 3: Verify migration SQL**

Check the generated migration file includes:
- `ALTER TABLE "WorkflowDefinitions" ADD COLUMN "TargetEntityType" VARCHAR(100) NOT NULL DEFAULT '';`
- `ALTER TABLE "WorkflowDefinitions" ADD COLUMN "Version" INTEGER NOT NULL DEFAULT 1;`
- Backfill existing definitions (see spec migration section)
- `ALTER TABLE "WorkflowInstances" ADD COLUMN "RequesterEmployeeId" uuid NULL;` (**nullable** — no fake GUIDs)
- `ALTER TABLE "WorkflowInstances" ADD COLUMN "RequesterUserId" varchar(128) NULL;` (**nullable**)
- `CREATE UNIQUE INDEX "IX_WorkflowDefinitions_TargetEntityType_Active"` (partial index: `WHERE "IsActive" = true`)

If the partial unique index is not auto-generated, add manually in `Up()`:
```csharp
migrationBuilder.Sql("""
    CREATE UNIQUE INDEX "IX_WorkflowDefinitions_TargetEntityType_Active"
    ON "WorkflowDefinitions" ("TargetEntityType")
    WHERE "IsActive" = true;
""");
```

Do NOT add UPDATE statements for WorkflowInstances — existing data keeps NULL. Do not create fake GUIDs.

- [ ] **Step 3: Commit**

```bash
git add Anemoi.Hr/Anemoi.Hr.Infrastructure/Migrations/
git commit -m "Phase 29 - add EF Core migration for TargetEntityType and Requester"
```

---

### Task 24: Verify full build and run all tests

- [ ] **Step 1: Full solution build**

```bash
dotnet build Anemoi.sln --no-restore
```
Expected: zero build errors

- [ ] **Step 2: Run all HR tests**

```bash
dotnet test Anemoi.Hr/Anemoi.Hr.Test --no-restore
```

- [ ] **Step 3: Run all notification tests**

```bash
dotnet test Anemoi.Notification/ --no-restore
```

- [ ] **Step 4: Final commit**

```bash
git add -A
git commit -m "Phase 29 — Universal Approval Workflow Engine completion"
```

---

## Spec Coverage Checklist

- [x] Domain: `TargetEntityType` + `Version` on `WorkflowDefinition` (Tasks 2)
- [x] Domain: `RequesterEmployeeId` + `RequesterUserId` on `WorkflowInstance` (Task 3)
- [x] Domain: Domain events on terminal approve/reject (Task 3)
- [x] Service: `IWorkflowEngine` interface (Task 5)
- [x] Service: `IWorkflowHierarchyResolver` interface + implementation (Tasks 5-6)
- [x] Service: `IWorkflowBuilder` interface + implementation (Tasks 5, 7)
- [x] Service: `IWorkflowTargetStatusUpdater` interface + 4 implementations (Tasks 5, 9)
- [x] Service: `WorkflowEngine` implementation (Task 8)
- [x] Service: `CanHandle()` pattern (Task 5)
- [x] Service: `GetCurrentApproversAsync()` (Task 5, 8)
- [x] Config: `WorkflowConstants.TargetEntityTypes` + `DefaultPolicy` (Task 4)
- [x] Config: `Recruitment` = definition required (Task 4)
- [x] CQRS: Refactored handlers delegate to `IWorkflowEngine` (Task 10)
- [x] CQRS: Domain event handlers for status updates (Task 15)
- [x] API: Permission update (`WorkflowExecute` → `WorkflowApprove`) (Task 17)
- [x] API: `GetCurrentApprovers` endpoint (Task 17)
- [x] API: `[Obsolete]` on legacy commands (Task 18)
- [x] EF: Model mappings for new properties (Task 13)
- [x] EF: Migration with backfill + partial unique index (Task 23)
- [x] DI: All services registered (Task 14)
- [x] Contract: Integration events with `IsCompleted` (Task 19)
- [ ] Notification: Consumers for workflow events — **deferred to Phase 30** (Task 20)
- [x] Tests: Domain tests updated (Task 21)
- [x] Tests: Application layer tests (Task 22)
- [x] Hierarchy: Deduplication via `HashSet<UserId>` (Task 6)
- [x] Leave: Status updater only sets `Status`, not balance (Task 9)
- [x] DB: Partial unique index = source of truth (Task 23)
