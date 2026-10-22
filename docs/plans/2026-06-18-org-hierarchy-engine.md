# Organizational Hierarchy Engine Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement Organizational Hierarchy Engine enabling dynamic approver resolution from company structure for the Workflow Engine.

**Architecture:** Evolutionary design extending existing Employee.DirectManagerEmployeeId and Department.ManagerEmployeeId. Domain records for resolution context/result. Application-layer IApprovalResolver with DefaultApprovalResolver handling DirectManager, DepartmentManager, and HrManager (via WorkflowRole). Snapshot-when-step-active strategy. No resolver dependency in domain entities. Business error on approver resolution failure (HR_WORKFLOW_APPROVER_NOT_FOUND).

**Tech Stack:** .NET 10, CQRS, EF Core, OneOf, Next.js, shadcn/ui, React Query

---

### File Structure Map

**Domain (Create/Modify):**
- `Anemoi.Hr.Domain/Workflow/ApproverType.cs` — add DepartmentManager, HrManager
- `Anemoi.Hr.Domain/Workflow/WorkflowRole.cs` — CREATE: HrManager constant
- `Anemoi.Hr.Domain/Workflow/ApprovalRoutingContext.cs` — CREATE: context record
- `Anemoi.Hr.Domain/Workflow/ResolvedApprover.cs` — CREATE: resolution result record
- `Anemoi.Hr.Domain/Organization/OrganizationNode.cs` — CREATE: tree node record
- `Anemoi.Hr.Domain/Organization/ReportingRelationship.cs` — CREATE: chain record
- `Anemoi.Hr.Domain/Organization/HierarchyLevel.cs` — CREATE: level constants
- `Anemoi.Hr.Domain/Workflow/WorkflowInstance.cs` — MODIFY: simplify IsCurrentStepApprover
- `Anemoi.Hr.Domain/Workflow/WorkflowInstanceStep.cs` — MODIFY: add ApproverEmployeeId
- `Anemoi.Hr.Domain/Workflow/WorkflowDefinitionStep.cs` — check if missing fields

**Application (Create/Modify):**
- `Anemoi.Hr.Application/Abstractions/IApprovalResolver.cs` — CREATE
- `Anemoi.Hr.Application/Abstractions/IWorkflowRoleResolver.cs` — CREATE
- `Anemoi.Hr.Application/Abstractions/IWorkflowEngine.cs` — MODIFY: add ResolveApproversAsync
- `Anemoi.Hr.Application/Abstractions/IWorkflowHierarchyResolver.cs` — MODIFY: delegate design
- `Anemoi.Hr.Application/Services/DefaultApprovalResolver.cs` — CREATE
- `Anemoi.Hr.Application/Services/DefaultWorkflowRoleResolver.cs` — CREATE
- `Anemoi.Hr.Application/Services/WorkflowEngine.cs` — MODIFY: step activation, dynamic resolution
- `Anemoi.Hr.Application/Services/WorkflowBuilder.cs` — MODIFY: store types not userIds
- `Anemoi.Hr.Application/Services/WorkflowHierarchyResolver.cs` — MODIFY: delegate to DefaultApprovalResolver
- `Anemoi.Hr.Application/Configurations/HrBusinessErrorCodes.cs` — MODIFY: add error code
- `Anemoi.Hr.Application/Configurations/HrPermissions.cs` — MODIFY: add org permissions
- `Anemoi.Hr.Application/Configurations/WorkflowConstants.cs` — MODIFY: update steps
- `Anemoi.Hr.Application/Cqrs/Commands/LeaveRequestCommands/SubmitLeaveRequest/` — MODIFY: remove ApproverEmployeeId, add workflow start
- `Anemoi.Hr.Application/Cqrs/Commands/OvertimeRequestCommands/CreateOvertimeRequest/` — MODIFY: add workflow start
- `Anemoi.Hr.Application/Cqrs/Commands/PayrollCommands/SubmitPayrollRunForApproval/` — MODIFY: add workflow start
- `Anemoi.Hr.Application/Cqrs/Commands/RecruitmentCommands/SubmitRecruitmentRequest/` — MODIFY: add workflow start
- `Anemoi.Hr.Application/WorkflowTargetStatusUpdaters/` — check/update existing
- `Anemoi.Hr.Application/Abstractions/IWorkflowTargetStatusUpdater.cs` — check interface

**API (Create):**
- `Anemoi.Hr.Api/Controllers/Organization/OrganizationHierarchyController.cs` — CREATE

**Infrastructure (Modify):**
- `Anemoi.Hr.Infrastructure/Configurations/WorkflowInstanceModelMapping.cs` — MODIFY: add ApproverEmployeeId mapping
- `Anemoi.Hr.Infrastructure/Configurations/WorkflowInstanceStepModelMapping.cs` — MODIFY: add ApproverEmployeeId mapping
- Migration — CREATE: indexes only

**Frontend (Create):**
- `cody-web-app/src/services/hr/organizationService.ts` — CREATE
- `cody-web-app/src/types/hr/organization.ts` — CREATE
- `cody-web-app/src/app/[locale]/(dashboard)/hr/organization/page.tsx` — CREATE
- Translation files — MODIFY

**Tests (Create):**
- `Anemoi.Hr.Test/Application/Workflow/DefaultApprovalResolverTests.cs` — CREATE
- `Anemoi.Hr.Test/Application/Workflow/WorkflowRoutingTests.cs` — CREATE
- `Anemoi.Hr.Test/Application/Workflow/WorkflowIntegrationTests.cs` — CREATE

---

### Task 1: Domain Records and Constants

**Files:**
- Create: `Anemoi.Hr.Domain/Workflow/ApprovalRoutingContext.cs`
- Create: `Anemoi.Hr.Domain/Workflow/ResolvedApprover.cs`
- Create: `Anemoi.Hr.Domain/Workflow/WorkflowRole.cs`
- Create: `Anemoi.Hr.Domain/Organization/OrganizationNode.cs`
- Create: `Anemoi.Hr.Domain/Organization/ReportingRelationship.cs`
- Create: `Anemoi.Hr.Domain/Organization/HierarchyLevel.cs`
- Modify: `Anemoi.Hr.Domain/Workflow/ApproverType.cs`

- [ ] **Step 1: Create ApprovalRoutingContext**

```csharp
// Anemoi.Hr.Domain/Workflow/ApprovalRoutingContext.cs
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed record ApprovalRoutingContext(
    EmployeeId RequesterEmployeeId,
    DepartmentId? DepartmentId,
    PositionId? PositionId,
    string EntityType,
    string? EntityId);
```

- [ ] **Step 2: Create ResolvedApprover**

```csharp
// Anemoi.Hr.Domain/Workflow/ResolvedApprover.cs
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed record ResolvedApprover(
    UserId UserId,
    EmployeeId EmployeeId,
    string FullName,
    string Email,
    string ResolutionSource);
```

- [ ] **Step 3: Create WorkflowRole**

```csharp
// Anemoi.Hr.Domain/Workflow/WorkflowRole.cs
namespace Anemoi.Hr.Domain.Workflow;

public static class WorkflowRole
{
    public const string HrManager = "HrManager";
}
```

- [ ] **Step 4: Create OrganizationNode**

```csharp
// Anemoi.Hr.Domain/Organization/OrganizationNode.cs
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Organization;

public sealed record OrganizationNode(
    EmployeeId Id,
    string FullName,
    string EmployeeCode,
    EmployeeId? ManagerId,
    string? ManagerName,
    DepartmentId DepartmentId,
    string DepartmentName,
    string PositionName,
    string GradeCode,
    List<OrganizationNode> DirectReports);
```

- [ ] **Step 5: Create ReportingRelationship**

```csharp
// Anemoi.Hr.Domain/Organization/ReportingRelationship.cs
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Organization;

public sealed record ReportingRelationship(
    EmployeeId EmployeeId,
    string EmployeeName,
    EmployeeId? ManagerEmployeeId,
    string? ManagerName,
    string Level);
```

- [ ] **Step 6: Create HierarchyLevel**

```csharp
// Anemoi.Hr.Domain/Organization/HierarchyLevel.cs
namespace Anemoi.Hr.Domain.Organization;

public static class HierarchyLevel
{
    public const string Employee = "Employee";
    public const string DirectManager = "DirectManager";
    public const string DepartmentManager = "DepartmentManager";
    public const string HrManager = "HrManager";
}
```

- [ ] **Step 7: Extend ApproverType**

```csharp
// Anemoi.Hr.Domain/Workflow/ApproverType.cs
namespace Anemoi.Hr.Domain.Workflow;

public static class ApproverType
{
    public const string Role = "Role";
    public const string Permission = "Permission";
    public const string DirectManager = "DirectManager";
    public const string DepartmentManager = "DepartmentManager";
    public const string HrManager = "HrManager";
    public const string SpecificUser = "SpecificUser";
}
```

- [ ] **Step 8: Build domain project**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj`
Expected: Build succeeds

---

### Task 2: WorkflowInstanceStep and WorkflowInstance Changes

**Files:**
- Modify: `Anemoi.Hr.Domain/Workflow/WorkflowInstanceStep.cs` — add `ApproverEmployeeId`
- Modify: `Anemoi.Hr.Domain/Workflow/WorkflowInstance.cs` — simplify `IsCurrentStepApprover`

- [ ] **Step 1: Read WorkflowInstanceStep.cs**

- [ ] **Step 2: Add ApproverEmployeeId to WorkflowInstanceStep**

Add property `ApproverEmployeeId` (nullable EmployeeId) and update the Create method if it exists.

- [ ] **Step 3: Read WorkflowInstance.cs**

- [ ] **Step 4: Simplify IsCurrentStepApprover**

Change from complex Func-based signature to simple `EmployeeId` comparison:
```csharp
public bool IsCurrentStepApprover(EmployeeId employeeId)
{
    var step = Steps.FirstOrDefault(s => s.Sequence == CurrentStep);
    return step?.ApproverEmployeeId == employeeId;
}
```

- [ ] **Step 5: Build domain project**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj`
Expected: Build succeeds (existing callers may show errors — that's expected, we'll fix later)

---

### Task 3: Application Abstractions

**Files:**
- Create: `Anemoi.Hr.Application/Abstractions/IApprovalResolver.cs`
- Create: `Anemoi.Hr.Application/Abstractions/IWorkflowRoleResolver.cs`
- Modify: `Anemoi.Hr.Application/Abstractions/IWorkflowEngine.cs`
- Modify: `Anemoi.Hr.Application/Abstractions/IWorkflowHierarchyResolver.cs`

- [ ] **Step 1: Create IApprovalResolver**

```csharp
// Anemoi.Hr.Application/Abstractions/IApprovalResolver.cs
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using OneOf;

namespace Anemoi.Hr.Application.Abstractions;

public interface IApprovalResolver
{
    Task<OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>> ResolveApproversAsync(
        string approverType, string? approverValue,
        ApprovalRoutingContext context, CancellationToken ct);
}
```

- [ ] **Step 2: Create IWorkflowRoleResolver**

```csharp
// Anemoi.Hr.Application/Abstractions/IWorkflowRoleResolver.cs
using Anemoi.Hr.Domain.Workflow;
using OneOf;
using Anemoi.BuildingBlock.Application.Responses;

namespace Anemoi.Hr.Application.Abstractions;

public interface IWorkflowRoleResolver
{
    Task<OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>> ResolveAsync(
        string workflowRole, CancellationToken ct);
}
```

- [ ] **Step 3: Update IWorkflowEngine — add ResolveApproversAsync**

```csharp
// Anemoi.Hr.Application/Abstractions/IWorkflowEngine.cs
// Add:
Task<IReadOnlyList<WorkflowApproverCandidate>> ResolveApproversAsync(
    WorkflowInstanceId workflowInstanceId, CancellationToken ct);
```

Note: `WorkflowApproverCandidate` will be defined in Task 6 (API preview model). For now, define it as an Application model:

```csharp
// Anemoi.Hr.Application/Models/WorkflowApproverCandidate.cs
namespace Anemoi.Hr.Application.Models;

public sealed record WorkflowApproverCandidate(
    int StepOrder,
    string ApproverType,
    string ApproverValue,
    EmployeeId? ResolvedEmployeeId,
    string? ResolvedName,
    string? ResolvedEmail);
```

- [ ] **Step 4: Update IWorkflowHierarchyResolver**

Keep the interface compatible. `WorkflowHierarchyResolver` will delegate to `IApprovalResolver`.

---

### Task 4: Business Error Codes and Permissions

**Files:**
- Modify: `Anemoi.Hr.Application/Configurations/HrBusinessErrorCodes.cs`
- Modify: `Anemoi.Hr.Application/Configurations/HrPermissions.cs`
- Modify (if needed): `Anemoi.Hr.Application/Configurations/WorkflowConstants.cs`

- [ ] **Step 1: Read and extend HrBusinessErrorCodes**

Add: `HR_WORKFLOW_APPROVER_NOT_FOUND`

- [ ] **Step 2: Read and extend HrPermissions**

Add:
- `hr.organization.view`
- `hr.organization.manage`
- `hr.workflow.override`

- [ ] **Step 3: Update WorkflowConstants**

Update `RecruitmentRequestSteps` from 0 to 2 and `PayrollRunSteps` from 0 to 3.

---

### Task 5: DefaultWorkflowRoleResolver

**Files:**
- Create: `Anemoi.Hr.Application/Services/DefaultWorkflowRoleResolver.cs`

This service maps WorkflowRole constants to actual employees. For Phase 30, we support HrManager by looking up employees with a specific role assignment.

A minimal approach: look up employees who have `hr.organization.manage` permission or are in a specific role mapping. Since the full role infrastructure is in the Identity module, we'll provide a simpler approach: check against a `WorkflowRoleAssignment` entity.

Actually, for Phase 30, let's keep it simple. We'll look up employees via a configurable mapping stored in a new simple entity `WorkflowRoleAssignment` with columns: `Role` (varchar), `EmployeeId`.

- [ ] **Step 1: Create WorkflowRoleAssignment domain entity**

```csharp
// Anemoi.Hr.Domain/Workflow/WorkflowRoleAssignment.cs
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowRoleAssignment : ValueObject
{
    public WorkflowRoleAssignmentId Id { get; set; }
    public string Role { get; set; }
    public EmployeeId EmployeeId { get; set; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
```

- [ ] **Step 2: Create WorkflowRoleAssignmentId**

```csharp
// Anemoi.Hr.ModelIds/ModelIds/WorkflowRoleAssignmentId.cs
public sealed record WorkflowRoleAssignmentId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 3: Create DefaultWorkflowRoleResolver**

```csharp
// Anemoi.Hr.Application/Services/DefaultWorkflowRoleResolver.cs
// Uses ISqlRepository<WorkflowRoleAssignment> + ISqlRepository<Employee>
// to resolve WorkflowRole to ResolvedApprover[]
```

- [ ] **Step 4: Register in DI**

Add to `ServiceCollectionExtensions` or `ModuleInstaller`.

---

### Task 6: DefaultApprovalResolver

**Files:**
- Create: `Anemoi.Hr.Application/Services/DefaultApprovalResolver.cs`

- [ ] **Step 1: Implement DefaultApprovalResolver**

Switch on `approverType`:
- `ApproverType.DirectManager` → lookup Employee by requesterEmployeeId, return DirectManagerEmployeeId
- `ApproverType.DepartmentManager` → lookup Department by context.DepartmentId, return Department.ManagerEmployeeId
- `ApproverType.HrManager` → delegate to IWorkflowRoleResolver.ResolveAsync(WorkflowRole.HrManager)
- `ApproverType.SpecificUser` → lookup by approverValue
- `ApproverType.Role` → return error (not implemented in Phase 30, or delegate to role resolver)
- default → return `HR_WORKFLOW_APPROVER_NOT_FOUND`

- [ ] **Step 2: Build**

---

### Task 7: WorkflowEngine Refactoring

**Files:**
- Modify: `Anemoi.Hr.Application/Services/WorkflowEngine.cs`
- Modify: `Anemoi.Hr.Application/Services/WorkflowBuilder.cs`
- Modify: `Anemoi.Hr.Application/Services/WorkflowHierarchyResolver.cs`

- [ ] **Step 1: Refactor WorkflowHierarchyResolver**

Change to delegate to `DefaultApprovalResolver` instead of duplicating resolution logic. Still use the existing pattern of walking the hierarchy, but call `IApprovalResolver` internally.

- [ ] **Step 2: Refactor WorkflowBuilder**

`BuildFromDefinition`: Store `ApproverType` + `ApproverValue` as-is. Set `ApproverEmployeeId = null`.
`BuildFromHierarchyAsync`: Resolve hierarchy, store types but do NOT set ApproverEmployeeId (will be snapshotted when step activates).

- [ ] **Step 3: Refactor WorkflowEngine.StartAsync**

After building and creating the instance, call `ActivateCurrentStepAsync` to snapshot the first step's approver.

- [ ] **Step 4: Implement ActivateCurrentStepAsync**

Private method that:
1. Gets current step
2. If step already has ApproverEmployeeId, skip
3. Build ApprovalRoutingContext from instance data
4. Call IApprovalResolver.ResolveApproversAsync
5. On success, set step.ApproverEmployeeId and step.ApproverUserId from first resolved approver
6. On failure, throw or handle (store error state)

- [ ] **Step 5: Refactor WorkflowEngine.ApproveAsync**

After successful approval, call `ActivateCurrentStepAsync` for the next step.

- [ ] **Step 6: Refactor WorkflowEngine.ApproveAsync/RejectAsync**

Change the approver check:
- Resolve current step's approver (if not yet resolved)
- Compare `performedBy` (UserId) against resolved approver's UserId
- Use `IApprovalResolver` for dynamic check

- [ ] **Step 7: Implement ResolveApproversAsync**

New method that iterates all steps and resolves approvers for preview.

- [ ] **Step 8: Update GetCurrentApproversAsync**

Use resolver for dynamic resolution instead of stored snapshot.

---

### Task 8: Module Integration

**Files:**
- Modify: Submit handlers for Leave, Overtime, Recruitment, Payroll
- Modify: LeaveRequest domain model (remove ApproverEmployeeId from creation)

- [ ] **Step 1: Update SubmitLeaveRequestCommand and Handler**

Remove `ApproverEmployeeId` parameter. After creating the leave entity, call `IWorkflowEngine.StartAsync()`.

- [ ] **Step 2: Update CreateOvertimeRequestHandler**

After creating the overtime entity, call `IWorkflowEngine.StartAsync()`.

- [ ] **Step 3: Update SubmitRecruitmentRequestHandler**

After submitting, call `IWorkflowEngine.StartAsync()`.

- [ ] **Step 4: Update SubmitPayrollRunForApprovalHandler**

After submitting for approval, call `IWorkflowEngine.StartAsync()`.

- [ ] **Step 5: Check Target Status Updaters**

Ensure LeaveWorkflowStatusUpdater, OvertimeWorkflowStatusUpdater, RecruitmentWorkflowStatusUpdater, PayrollWorkflowStatusUpdater all work correctly with the new flow.

---

### Task 9: Database Migration

**Files:**
- Create: Migration for indexes and WorkflowRoleAssignment table
- Modify: `Anemoi.Hr.Infrastructure/Persistence/HrDbContext.cs`
- Modify: `Anemoi.Hr.Infrastructure/Configurations/WorkflowInstanceStepModelMapping.cs`

- [ ] **Step 1: Add WorkflowRoleAssignment DbSet to HrDbContext**

- [ ] **Step 2: Create EF mapping for WorkflowRoleAssignment**

- [ ] **Step 3: Create migration**

Add indexes on Employees(DirectManagerEmployeeId), Departments(ManagerEmployeeId), Departments(ParentDepartmentId).
Add WorkflowRoleAssignment table.

- [ ] **Step 4: Update WorkflowInstanceStep mapping**

Add ApproverEmployeeId column mapping.

---

### Task 10: OrganizationHierarchyController

**Files:**
- Create: `Anemoi.Hr.Api/Controllers/Organization/OrganizationHierarchyController.cs`
- Create: Application queries/commands as needed

- [ ] **Step 1: Create controller with endpoints**

GET `api/hr/organization/get-organization-tree` — returns `List<OrganizationNode>`
GET `api/hr/organization/get-reporting-chain/{employeeId}` — returns `List<ReportingRelationship>`
PUT `api/hr/organization/update-manager` — updates Employee.DirectManagerEmployeeId
GET `api/hr/organization/get-approvers-preview` — returns preview of resolved approvers
GET `api/hr/organization/get-workflow-route-preview` — preview with workflow definition steps

---

### Task 11: Frontend

**Files:**
- Create: `cody-web-app/src/services/hr/organizationService.ts`
- Create: `cody-web-app/src/types/hr/organization.ts`
- Create: `cody-web-app/src/app/[locale]/(dashboard)/hr/organization/page.tsx`
- Modify: Translation files (en, vi)

- [ ] **Step 1: Create types**

- [ ] **Step 2: Create service**

- [ ] **Step 3: Create page with components**

Organization tree, manager assignment dialog, reporting chain viewer, approval preview.

---

### Task 12: Tests

**Files:**
- Create: Test files as specified

- [ ] **Step 1: DefaultApprovalResolver tests**

- [ ] **Step 2: Workflow routing tests**

- [ ] **Step 3: Integration tests**

---

### Task 13: Build Verification

- [ ] **Step 1: Build entire solution**

Run: `dotnet build Anemoi.sln`
Expected: Build succeeds

- [ ] **Step 2: Run all tests**

Run: `dotnet test Anemoi.Hr/Anemoi.Hr.Test/`
Expected: All tests pass, no existing tests broken
