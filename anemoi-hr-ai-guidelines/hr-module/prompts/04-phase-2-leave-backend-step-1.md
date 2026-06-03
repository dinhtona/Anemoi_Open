# Prompt - Phase 2 Leave Backend - Step 1 Domain & Data

```text
Read all AI guideline files first.

Task:
Implement Phase 2 Leave Management backend Step 1 only: Domain & Data.

Scope:
- LeavePolicy
- LeaveBalance
- LeaveTransaction
- LeaveRequest
- LeaveApproval
- LeaveAccrualRun
- Strongly typed IDs
- EF Core configurations
- DbContext registration

Rules:
- Leave balance must use transaction ledger design.
- Do not only store RemainingDays without transactions.
- LeavePolicy must support MonthlyAccrualDays, default 1.25.
- LeaveAccrualRun must prevent duplicate monthly accrual with unique EmployeeId + YearMonth.
- LeaveRequest must support selected/current approver.
- LeaveApproval must support multi-step approval.
- Status and type values must be stable codes, not localized text.

Do not implement CQRS handlers.
Do not implement API controllers.
Do not implement background job yet unless the existing project convention requires Worker domain registration.
Do not implement frontend.

Stop after Step 1 and provide a summary of changed files.
```
