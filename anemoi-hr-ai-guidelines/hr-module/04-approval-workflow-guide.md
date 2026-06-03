# HR Approval Workflow Guide

## Purpose

Approval Workflow must be reusable across HR modules.

It must support:

```text
Leave Request
Salary Change
Department Transfer
Contract Approval
Promotion Approval
Sensitive Permission Assignment
Leave Balance Adjustment
```

## Core model

```text
ApprovalWorkflow
- ApprovalWorkflowId
- Code
- Name
- TargetTypeCode
- IsActive
```

```text
ApprovalStep
- ApprovalStepId
- WorkflowId
- StepNo
- ApproverTypeCode
- ApproverRoleCode nullable
- SpecificApproverEmployeeId nullable
- IsRequired
```

```text
ApprovalRequest
- ApprovalRequestId
- WorkflowCode
- TargetTypeCode
- TargetId
- RequestedByEmployeeId
- CurrentStepNo
- StatusCode
- Reason nullable
- CreatedAt
- CompletedAt nullable
```

```text
ApprovalAction
- ApprovalActionId
- ApprovalRequestId
- StepNo
- ApproverEmployeeId
- ActionCode
- Comment nullable
- ActionAt
```

## Approver types

```text
direct_manager
department_manager
hr_manager
specific_employee
permission_role
requester_selected
```

## Leave request flow

```text
Employee creates leave request
→ employee selects approver or system resolves department manager
→ create ApprovalRequest
→ notify approver
→ approver approves/rejects/requests change
→ update LeaveRequest status
→ if approved, create LeaveTransaction and update LeaveBalance
→ audit log
```

## Sensitive permission flow

```text
Admin selects sensitive permission
→ UI displays risk warning
→ admin enters reason
→ high/critical requires re-authentication
→ critical creates approval request
→ second approver approves
→ permission is assigned
→ notification + audit log
```

## Rules

- Do not hard-code Department Manager approval in Leave handlers.
- Use approval workflow configuration.
- LeaveRequest may store `CurrentApproverEmployeeId` for query performance, but source of truth is ApprovalRequest/ApprovalAction.
- Every approval action must be audited.
- Approval status codes must be stable and not localized.
