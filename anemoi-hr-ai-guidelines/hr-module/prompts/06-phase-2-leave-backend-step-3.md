# Prompt - Phase 2 Leave Backend - Step 3 API, Events, Worker

```text
Read all AI guideline files first.

Task:
Implement Phase 2 Leave Management backend Step 3 only: API, Events, and Worker wiring.

Prerequisite:
Leave Step 1 and Step 2 must already exist.

Scope:
API endpoints:
- Leave policies CRUD/query endpoints
- My leave balance endpoint
- Employee leave balance endpoint
- Leave transactions endpoint
- Create/submit/cancel leave request
- Approve/reject leave request
- My leave requests
- Team/department leave requests

Background job:
- Monthly leave accrual job
- Accrues 1.25 days from policy at the end of each month
- Idempotent using LeaveAccrualRun unique EmployeeId + YearMonth

Events:
- LeaveRequestSubmitted
- LeaveRequestApproved
- LeaveRequestRejected
- LeaveBalanceChanged
- MonthlyLeaveAccrued

Permissions:
- Add permission constants.
- Update Identity seed.
- Use permission attributes on endpoints.
- Include sensitive permissions for leave balance adjustment and force operations.

Do not implement frontend.

Stop after Step 3 and provide a summary of changed files.
```
