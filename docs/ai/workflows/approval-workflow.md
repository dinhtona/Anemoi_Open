# Approval Workflow

Approval workflow must be reusable across HR modules.

Supported use cases:

```text
Leave request approval
Salary change approval
Department transfer approval
Promotion approval
Contract approval
Sensitive permission assignment approval
Leave balance adjustment approval
```

## Core Tables

```text
ApprovalWorkflows
- Id
- Code
- Name
- TargetType
- IsActive

ApprovalSteps
- Id
- WorkflowId
- StepNo
- ApproverType
- ApproverRoleCode nullable
- SpecificEmployeeId nullable
- RequiredPermissionCode nullable
- IsRequired

ApprovalRequests
- Id
- WorkflowId
- TargetType
- TargetId
- RequestedByEmployeeId
- Status
- CurrentStepNo
- CreatedAt
- CompletedAt nullable

ApprovalActions
- Id
- ApprovalRequestId
- StepNo
- ApproverEmployeeId
- Action
- Comment
- CreatedAt
```

Approver types:

```text
DirectManager
DepartmentManager
HrManager
SpecificEmployee
PermissionBasedApprover
RoleGroup
```

Statuses:

```text
Draft
Pending
Approved
Rejected
Cancelled
Returned
Expired
```
