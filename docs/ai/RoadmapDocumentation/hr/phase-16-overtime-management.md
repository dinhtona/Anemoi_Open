# Phase 16 - Overtime Management

Status: Implemented

---

## Objective

Implement overtime management as a first-class HR module. Overtime must support request submission, approval readiness, calculation readiness, and future payroll integration.

---

## Business Requirements

- Create overtime requests
- Validate overtime duration, overlap, and request date rules
- Submit overtime requests
- Approve or reject overtime
- Cancel pending overtime requests
- Support employee, manager, and HR request lists
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
- End time must be greater than start time.
- Maximum duration is 12 hours per request unless a later policy explicitly overrides it.
- Approved overtime requests must not overlap for the same employee and work date.
- Pending requests may transition to approved, rejected, or cancelled.
- Approved requests cannot be modified or cancelled.
- Rejected requests cannot be approved.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.
- Reports must not recalculate overtime from OvertimeRequest after payroll finalization.
- Payroll integration must consume approved overtime through a snapshot/provider abstraction and must not directly mutate payroll from the overtime module.

---

## Access Model

- Employee: create and cancel own pending overtime requests.
- Manager: review, approve, and reject team overtime requests.
- HR: view all overtime requests and perform privileged approval/rejection actions when explicitly permitted.

---

## Persistence Notes

Primary table:

```text
hr_overtime_requests
```

Important persisted fields:

- Overtime request ID
- Employee ID
- Work date
- Start time
- End time
- Reason
- Status
- ApprovedBy / ApprovedAt
- RejectedBy / RejectedAt
- CreatedAt / UpdatedAt
- Concurrency token

Recommended indexes:

- Employee + work date
- Status
- Work date
- Created timestamp

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
- My Requests tab
- Team Requests tab
- All Requests tab for HR-authorized users

---

## Testing Notes

Expected coverage:

- Domain validation for duration, overlap, and status transitions
- Command handler validation for create, approve, reject, and cancel
- Permission checks for employee, manager, and HR flows
- Query filtering for own/team/all request views
- Concurrency handling on state-changing actions

---

## Operational Notes

- Use `AsNoTracking()` for read-only list/detail queries.
- Keep overtime data auditable because it can affect payroll and compliance.
- Protect transport with HTTPS in deployed environments.
- Retain overtime data according to HR/payroll retention policy.

---

## Future Enhancements

- Overtime payroll integration
- Overtime multipliers and rates
- Attendance integration
- Overtime reporting
- Multi-level approval
- Overtime budget control
- Overtime limit warnings
- Shift and holiday aware calculation
- Bulk overtime approval
