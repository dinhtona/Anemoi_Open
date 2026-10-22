# Phase 30 — Organizational Hierarchy Engine Design

## Overview

Implement Organizational Hierarchy Engine that enables the Workflow Engine to dynamically
resolve approvers from company structure. This completes the Workflow Engine as a shared
platform for all HR modules.

## Design Principles

- Use existing `Employee.DirectManagerEmployeeId`, `Department.ManagerEmployeeId`,
  `Department.ParentDepartmentId` as the source of truth — no new aggregates
- Resolve approvers **dynamically at execution time**, not at workflow instance creation
- Keep `ApproverType` + `ApproverValue` as routing instructions (not concrete user IDs)
- Centralize all resolution logic in `IApprovalResolver` / `DefaultApprovalResolver`
- No silent fallback — return stable business error if approver cannot be resolved

## Domain Layer

### Extend `ApproverType` (in `Anemoi.Hr.Domain.Workflow.ApproverType`)

Add constants:
- `DepartmentManager` — resolved from `Department.ManagerEmployeeId`
- `DivisionManager` — resolved by walking `Department.ParentDepartmentId` to top, returning highest manager
- `HrManager` — resolved through role/permission assignment

Existing constants remain: `Role`, `Permission`, `DirectManager`, `SpecificUser`

### New Value Objects / Records

```csharp
// Context passed to resolver
public sealed record ApprovalRoutingContext(
    EmployeeId RequesterEmployeeId,
    DepartmentId? DepartmentId,
    PositionId? PositionId,
    string EntityType,
    string? EntityId);

// Organization node for tree/chain views
public sealed record OrganizationNode(
    EmployeeId Id,
    string FullName,
    string EmployeeCode,
    EmployeeId? ManagerId,
    DepartmentId DepartmentId,
    string DepartmentName,
    List<OrganizationNode> DirectReports);

// Hierarchy level
public static class HierarchyLevel
{
    public const string DirectManager = "DirectManager";
    public const string DepartmentManager = "DepartmentManager";
    public const string DivisionManager = "DivisionManager";
    public const string HrManager = "HrManager";
    public const string Ceo = "Ceo";
}

// Reporting relationship
public sealed record ReportingRelationship(
    EmployeeId EmployeeId,
    string EmployeeName,
    EmployeeId? ManagerId,
    string? ManagerName,
    string Level);

// Workflow route preview
public sealed record WorkflowApproverCandidate(
    int StepOrder,
    string ApproverType,
    string ApproverValue,
    EmployeeId? ResolvedEmployeeId,
    string? ResolvedName,
    string? ResolvedEmail);
```

### Resolution Rules

| ApproverType | Resolution |
|---|---|
| `DirectManager` | `Employee.DirectManagerEmployeeId` → Employee → UserId |
| `DepartmentManager` | `Department.ManagerEmployeeId` → Employee → UserId |
| `DivisionManager` | Walk `Department.ParentDepartmentId` to root, collect `ManagerEmployeeId` at each level, return highest (last non-null) |
| `HrManager` | Resolve through centralized role/permission check (employee assigned HR Manager role) |
| `Role` | All active employees with matching role |
| `SpecificUser` | Direct lookup by configured user ID |

## Application Layer

### Interfaces

```csharp
public interface IApprovalResolver
{
    Task<IReadOnlyList<UserId>> ResolveApproversAsync(
        string approverType, string? approverValue,
        ApprovalRoutingContext context, CancellationToken ct);
}

// Extend IWorkflowEngine
Task<IReadOnlyList<WorkflowApproverCandidate>> ResolveApproversAsync(
    WorkflowInstanceId workflowInstanceId, CancellationToken ct);
```

### DefaultApprovalResolver

- Delegates to specific resolution strategies based on `ApproverType`
- Uses `ISqlRepository<Employee>`, `ISqlRepository<Department>`
- For `HrManager`: uses centralized role/permission service
- Returns empty list if cannot resolve

### WorkflowEngine Changes

- `BuildFromDefinition()`: Update to store `ApproverType` + `ApproverValue` snapshot on `WorkflowInstanceStep` but NOT resolve concrete user IDs
- `IsCurrentStepApprover()`: Update to use `IApprovalResolver` to check if the performing user matches the current step's required approver
- `GetCurrentApproversAsync()`: Update to resolve dynamically via `IApprovalResolver`
- New `ResolveApproversAsync()`: Previews all steps with resolved approvers

### WorkflowBuilder Changes

- `BuildFromDefinition()`: Store `ApproverType` + `ApproverValue` as-is on instance steps
- `BuildFromHierarchyAsync()`: Resolve hierarchy via `IWorkflowHierarchyResolver` (which delegates to `DefaultApprovalResolver`)

## Module Integration

### Leave Request
- `SubmitLeaveRequestHandler`: Remove `ApproverEmployeeId` parameter. Call `IWorkflowEngine.StartAsync()` after creation.
- `LeaveWorkflowStatusUpdater`: Already exists — keep as-is for `MarkApprovedAsync`/`MarkRejectedAsync`

### Overtime Request
- `CreateOvertimeRequestHandler`: After creation, call `IWorkflowEngine.StartAsync()`
- `OvertimeWorkflowStatusUpdater`: Already exists — keep as-is

### Recruitment Request
- `SubmitRecruitmentRequestHandler`: After submit, call `IWorkflowEngine.StartAsync()`
- `RecruitmentWorkflowStatusUpdater`: Already exists — keep as-is

### Payroll Run
- `SubmitPayrollRunForApprovalHandler`: After submit, call `IWorkflowEngine.StartAsync()`
- `PayrollWorkflowStatusUpdater`: Already exists — keep as-is

## API Endpoints

Controller: `OrganizationHierarchyController` — route `api/hr/organization/[action]`

| Method | Action | Permission |
|---|---|---|
| GET | `GetOrganizationTree` | `hr.organization.view` |
| GET | `GetReportingChain/{employeeId}` | `hr.organization.view` |
| PUT | `UpdateManager` | `hr.organization.manage` |
| GET | `GetApproversPreview` | `hr.workflow.override` |
| GET | `GetWorkflowRoutePreview` | `hr.workflow.override` |

## Permissions (in `HrPermissions`)

Add:
- `hr.organization.view`
- `hr.organization.manage`
- `hr.workflow.override`

## Database

No new tables required — hierarchy data lives in existing `Employees` and `Departments` tables.

Add indexes (migration):
- `IX_Employees_DirectManagerEmployeeId` on `Employees(DirectManagerEmployeeId)`
- `IX_Departments_ManagerEmployeeId` on `Departments(ManagerEmployeeId)`
- `IX_Departments_ParentDepartmentId` on `Departments(ParentDepartmentId)`

## Frontend

Route: `/hr/organization`

Components:
- `OrganizationPage` — main page with permission gating
- `OrganizationTree` — hierarchical tree view (shadcn/ui Tree or recursive list)
- `ManagerAssignmentDialog` — dialog for assigning manager to employee
- `ReportingChainViewer` — shows chain from employee → ... → CEO
- `ApprovalPreview` — shows what approvers would be resolved for a given entity type
- `WorkflowRoutePreview` — preview of full workflow routing for an entity

Services:
- `organizationService.ts` — API calls for all 5 endpoints

Types:
- `organization.ts` — OrganizationNode, ReportingRelationship, WorkflowApproverCandidate

Localization (EN/VI):
- Add `organization` namespace to translation files

## Tests

### Unit Tests (in `Anemoi.Hr.Test`)

1. **DefaultApprovalResolverTests**
   - DirectManager resolves correctly
   - DepartmentManager resolves correctly
   - DivisionManager resolves correctly
   - HrManager resolves correctly
   - Missing employee returns empty
   - Missing department returns empty

2. **WorkflowHierarchyResolverTests** (update existing)
   - Delegates to DefaultApprovalResolver
   - Deduplication still works

3. **WorkflowRoutingTests**
   - Leave request resolves DirectManager + DepartmentManager
   - Overtime request resolves DirectManager
   - Recruitment request resolves DepartmentManager + HRManager
   - Payroll run resolves HRManager + Finance Director + CEO

4. **Integration Tests**
   - Leave submit triggers workflow auto-start
   - Overtime submit triggers workflow auto-start
   - Recruitment submit triggers workflow auto-start
   - Payroll submit triggers workflow auto-start
   - Approval updates entity status
   - Rejection updates entity status

### Test Requirements
- All tests pass
- No existing tests broken
- Concurrency tests for manager reassignment (if applicable)

## Implementation Order

1. Extend `ApproverType` constants
2. Create domain value objects/records
3. Add new permissions constants
4. Implement `IApprovalResolver` + `DefaultApprovalResolver`
5. Refactor `WorkflowHierarchyResolver` to delegate
6. Update `WorkflowEngine` (dynamic resolution, `ResolveApproversAsync`)
7. Update `WorkflowBuilder` (store types, not resolved IDs)
8. Update `WorkflowInstance.IsCurrentStepApprover` to use resolver
9. Integrate Leave, Overtime, Recruitment, Payroll modules
10. Add database migration (indexes)
11. Create `OrganizationHierarchyController`
12. Build frontend page and components
13. Write tests
14. Build verification

## Risks & Mitigations

| Risk | Mitigation |
|---|---|
| Performance: dynamic resolution per check | Cache employee+department data; batch queries |
| Circular manager hierarchy | Validation on manager assignment |
| In-flight workflows broken by hierarchy change | Dynamic resolution means they auto-adapt |
| HrManager role not yet implemented | Use permission-based resolution as fallback |
