# Leave Management

Core requirements:

- Employee can submit leave request from app.
- Employee can choose approver such as department manager.
- Leave approval must use shared approval workflow.
- Annual leave accrues 1.25 days at the end of each month.
- Leave balance must be ledger-based.
- Pending leave should reserve balance.

Entities:

```text
LeavePolicy
LeaveBalance
LeaveRequest
LeaveRequestDetail
LeaveTransaction
LeaveAccrualRun
```

LeavePolicy fields:

```text
LeavePolicyId
Code
Name
LeaveTypeCode
MonthlyAccrualDays
AnnualMaxDays
AllowCarryForward
MaxCarryForwardDays
IsActive
```

LeaveBalance fields:

```text
LeaveBalanceId
EmployeeId
Year
OpeningDays
AccruedDays
UsedDays
PendingDays
AdjustedDays
RemainingDays
```

LeaveTransaction types:

```text
Accrual
Used
PendingReserve
PendingRelease
Refund
Adjustment
CarryForward
```

Monthly accrual job:

```text
Find active employees
Check eligibility
Check LeaveAccrualRun does not exist for EmployeeId + YearMonth
Add 1.25 days or policy-defined value
Create LeaveTransaction: Accrual
Update LeaveBalance
Create LeaveAccrualRun
Publish LeaveBalanceChangedIntegrationEvent
```

Rules:

- Do not update RemainingDays without LeaveTransaction.
- Approved leave creates Used transaction.
- Pending leave should reserve balance.
- Rejected/cancelled leave releases pending balance.
- Manual balance adjustment is sensitive and should require approval.
