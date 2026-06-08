# Phase 16 - Overtime Management

Status: Planned

---

## Objective

Implement overtime management as a first-class HR module. Overtime must support request submission, approval readiness, calculation readiness, and future payroll integration.

---

## Business Requirements

- Create overtime requests
- Submit overtime requests
- Approve or reject overtime
- Track overtime history per employee
- Prepare approved overtime as payroll input

---

## Domain Model

### OvertimeRequest
Represents an employee overtime request.

Suggested fields:

- Id
- EmployeeId
- WorkDate
- StartTime
- EndTime
- TotalHours
- Reason
- Status
- SubmittedAt
- ApprovedAt
- ApprovedBy
- RejectedAt
- RejectedBy
- RejectionReason

### OvertimeRule
Defines how overtime should be classified and calculated.

Examples:

- Normal day overtime
- Weekend overtime
- Holiday overtime
- Night overtime

---

## Workflow

```text
Draft
    ↓
Submitted
    ↓
Approved / Rejected
    ↓
Payroll Consumed
```

Approved overtime will become payroll input. Payroll must snapshot overtime values into PayrollItem.

---

## Business Rules / Architecture Notes

- Follow Clean Architecture and CQRS + MediatR.
- Keep domain rules in the domain/application layer, not in controllers.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.
- Reports must not recalculate overtime from OvertimeRequest after payroll finalization.

---

## Commands

- CreateOvertimeRequestCommand
- SubmitOvertimeRequestCommand
- ApproveOvertimeRequestCommand
- RejectOvertimeRequestCommand
- CancelOvertimeRequestCommand

---

## Queries

- GetOvertimeRequestsQuery
- GetEmployeeOvertimeHistoryQuery
- GetApprovedOvertimeForPayrollPeriodQuery

---

## Permissions

```text
hr.overtime.view
hr.overtime.request
hr.overtime.approve
hr.overtime.manage
```

---

## API Endpoints

```text
GET    /api/hr/overtime-requests
GET    /api/hr/overtime-requests/{id}
POST   /api/hr/overtime-requests
POST   /api/hr/overtime-requests/{id}/submit
POST   /api/hr/overtime-requests/{id}/approve
POST   /api/hr/overtime-requests/{id}/reject
POST   /api/hr/overtime-requests/{id}/cancel
```

---

## Frontend

- Overtime Request List
- Create Overtime Request
- Overtime Approval Queue
- Employee Overtime History

---

## Future Enhancements

- Multi-level approval
- Overtime budget control
- Overtime limit warnings
- Shift and holiday aware calculation
