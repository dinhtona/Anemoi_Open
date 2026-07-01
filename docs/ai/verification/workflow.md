# Workflow Verification

Use this for LeaveRequest, OvertimeRequest, PayrollRun, RecruitmentRequest, EmployeeTransfer, EmployeeSeparation, ProbationRecord, or any new workflow-enabled type.

## Required Evidence

- Active WorkflowDefinition exists.
- Submit/start creates WorkflowInstance.
- Pending approval appears for the correct approver.
- Pending approval does not appear for unrelated users.
- Approve/reject goes through workflow engine commands/endpoints.
- Target entity status updates correctly.
- Workflow history is created.
- Notification behavior is verified or explicitly marked PARTIAL.

## Standard Report

```text
Workflow Verification:
- TargetEntityType:
- Active definition: PASS/FAIL
- Submit/start: PASS/FAIL
- Correct approver visibility: PASS/FAIL
- Unrelated user visibility: PASS/FAIL/N/A
- Approve/reject command path: PASS/FAIL
- Target status update: PASS/FAIL
- History: PASS/FAIL
- Notification: PASS/FAIL/PARTIAL/N/A
- Evidence:
```

Legacy entity-specific approve/reject endpoints are not valid evidence unless the task is explicitly about deprecating or redirecting legacy paths.
