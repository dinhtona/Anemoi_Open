# Prompt - Phase 2 Leave Backend - Step 2 Application CQRS

```text
Read all AI guideline files first.

Task:
Implement Phase 2 Leave Management backend Step 2 only: Application Layer - CQRS & Mappings.

Prerequisite:
Leave Domain & Data must already exist.

Scope:
Leave Policy:
- CreateLeavePolicyCommand
- UpdateLeavePolicyCommand
- GetLeavePoliciesQuery

Leave Balance:
- GetMyLeaveBalanceQuery
- GetEmployeeLeaveBalanceQuery
- AdjustLeaveBalanceCommand
- GetLeaveTransactionsQuery

Leave Request:
- CreateLeaveRequestCommand
- SubmitLeaveRequestCommand
- CancelLeaveRequestCommand
- ApproveLeaveRequestCommand
- RejectLeaveRequestCommand
- GetMyLeaveRequestsQuery
- GetTeamLeaveRequestsQuery
- GetLeaveRequestByIdQuery

Accrual:
- AccrueMonthlyLeaveCommand
- GetLeaveAccrualRunsQuery

Requirements:
- Use Mapperly for mapping.
- Use FluentValidation.
- Use MediatR + OneOf.
- Use stable error codes.
- Localize user-facing messages.
- Check leave balance and pending days before submission.
- On approval, create LeaveTransaction and update LeaveBalance.
- Do not hard-code Department Manager. Use selected approver or approval workflow integration seam.
- Do not publish integration events directly unless project convention allows handler-level publishing.

Do not implement API controllers.
Do not implement frontend.

Stop after Step 2 and provide a summary of changed files.
```
