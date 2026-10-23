# Sensitive Permissions

Sensitive permissions are marked in the central catalog with `IsSensitive = true` and a `RiskLevel`.

## Risk Levels And Assignment Policy

```text
Medium: warning confirmation, reason, audit log.
High: warning confirmation, reason, re-authentication or OTP, notification, audit log.
Critical: warning confirmation, reason, re-authentication or OTP, second approver, notification, audit log, active only after approval.
```

Do not assign sensitive permissions from a normal role-edit screen without the required confirmation flow.

The catalog currently contains `High` and `Critical` entries. Treat any new `IsSensitive` entry as subject to the matching policy even if no workflow has been implemented for it yet.

## Current High-Risk Permissions

29 entries, verified against `Permissions.cs` on 2026-10-23. The catalog is the
source; this list must be updated in the same change that adds a sensitive
permission.

```text
hr.leave.balance.adjust
hr.leave.request.force_approve
hr.leave.request.force_cancel
hr.employee.transfer.create
hr.employee.identity_link
hr.employee.note.manage
hr.promotion.create
hr.position.change
hr.grade.change
hr.contract.view
hr.contract.create
hr.contract.update
hr.contract.terminate
hr.salary.view
hr.salary.change
hr.salary.grade.manage
hr.allowance.type.manage
hr.employee.allowance.change
hr.attendance.lock
hr.overtime.manage
hr.tax.manage
hr.insurance.manage
hr.payroll.calculate
hr.payroll.approve
hr.payroll.export
hr.payroll.reporting.export
hr.probation.manage
hr.separation.create
hr.workflow.approve
```

## Current Critical Permissions

```text
hr.payroll.lock
```

## Rules

- Keep the permission list in sync with `Permissions.cs`; do not maintain a separate module-local list.
- Sensitive actions require an audit log entry.
- Sensitive assignments must not be possible through a normal role edit without the required warning, reason, and approval.
- Critical permissions activate only after the required approval.
