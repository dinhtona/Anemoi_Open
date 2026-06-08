# Phase 21 - Employee Self Service (ESS)

Status: Planned

---

## Objective

Provide an employee-facing portal for self-service HR operations. ESS must reuse existing HR modules while enforcing employee-scoped access control.

---

## Business Requirements

- View personal profile
- View leave balance
- Submit leave request
- View attendance summary
- View payslips
- Submit overtime request
- View contract summary

---

## Domain Model

ESS is not a separate HR domain aggregate.

It is an application/API layer that exposes employee-scoped capabilities from existing modules.

Identity must be resolved from the authenticated user.

---

## Workflow

```text
Authenticated User
    ↓
Resolve EmployeeId
    ↓
Employee-Scoped ESS APIs
```

---

## Business Rules / Architecture Notes

- Follow Clean Architecture and CQRS + MediatR.
- Keep domain rules in the domain/application layer, not in controllers.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.
- Employees cannot pass arbitrary EmployeeId for ESS queries.
- Payslip access must be employee-scoped.

---

## Commands

- SubmitEssLeaveRequestCommand
- SubmitEssOvertimeRequestCommand

---

## Queries

- GetMyProfileQuery
- GetMyLeaveBalanceQuery
- GetMyAttendanceSummaryQuery
- GetMyPayslipsQuery
- GetMyCurrentContractQuery

---

## Permissions

```text
ess.profile.view
ess.leave.view
ess.leave.request
ess.attendance.view
ess.payslip.view
ess.overtime.request
ess.contract.view
```

---

## API Endpoints

```text
GET    /api/ess/profile
GET    /api/ess/leave/balance
POST   /api/ess/leave/requests
GET    /api/ess/attendance/summary
GET    /api/ess/payslips
GET    /api/ess/payslips/{id}
POST   /api/ess/overtime-requests
GET    /api/ess/contracts/current
```

---

## Frontend

- My Profile
- My Leave
- My Attendance
- My Payslips
- My Overtime
- My Contract

---

## Future Enhancements

- Mobile ESS
- Employee document center
- Employee profile update request
- ESS notifications
