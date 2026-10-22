# Approval Workflow

Approval routing is owned by the HR Workflow Engine. Every required business workflow type is definition-bound; see ADR-029 in `docs/architecture/ARCHITECTURE_DECISIONS.md`.

## Domain Model

```text
WorkflowDefinition
- Id, Code, Name, Description
- WorkflowTypeCode, TargetEntityType
- Version, IsActive
- Steps

WorkflowDefinitionStep
- Sequence
- ApproverType, ApproverValue
- IsRequired

WorkflowInstance
- WorkflowDefinitionId nullable
- EntityType, EntityId
- CurrentStep, Status
- StartedBy, RequesterEmployeeId, RequesterUserId
- StartedAt, CompletedAt
- Steps, Histories

WorkflowInstanceStep
- Sequence
- ApproverTypeSnapshot, ApproverValueSnapshot
- ApproverUserId, ApproverEmployeeId
- Status, ApprovedAt, RejectedAt, Comment

WorkflowHistory
- Action, PerformedBy, Comment, PerformedAt
```

## Status Codes

Workflow instance: `Draft`, `Pending`, `Approved`, `Rejected`, `Cancelled`, `Returned`.

Workflow step: `Pending`, `Approved`, `Rejected`, `Skipped`, `Cancelled`.

## Approver Types

```text
Role
Permission
DirectManager
DepartmentManager
HrManager
SpecificUser
```

Workflow roles control approval routing only. They never grant system permissions or JWT claims.

## Definition-Bound Rules

- Required entity types: `LeaveRequest`, `OvertimeRequest`, `PayrollRun`, `RecruitmentRequest`, `EmployeeTransfer`, `EmployeeSeparation`, `ProbationRecord`.
- Every required type must have an active `WorkflowDefinition` and a seed definition in `HrDevSeedData.cs`.
- `RequiresDefinition(entityType)` returns `IsRequiredEntityType(entityType)`.
- `BuildFromHierarchyAsync` is preview/test only and must never route production workflows.
- Any new workflow-enabled type must be added to `RequiredEntityTypes` and receive a seed definition.
- Changes to `RequiresDefinition` or `IsRequiredEntityType` require architecture review.

## Scopes And Inbox

- ADR-027: Employee (ESS), Approval (manager/approvals), and HR/Admin scopes are separate.
- ADR-028: the approval inbox is centralized under `/manager/approvals`.
- Approvers only see requests they are responsible for approving.
- Approval actions go through workflow commands, not direct status updates on the target entity.
