# Sensitive Permissions

Risk levels:

```text
Low
Medium
High
Critical
```

## Assignment Policy

Medium: warning confirmation, reason, audit log.

High: warning confirmation, reason, password re-authentication or OTP, notification, audit log.

Critical: warning confirmation, reason, password re-authentication or OTP, second approver, notification, audit log, active only after approval.

## Sensitive HR Permissions

```text
hr.employee.personal_info.view
hr.employee.personal_info.update
hr.employee.bank_info.view
hr.employee.bank_info.update
hr.employee.identity_document.view
hr.employee.identity_document.update
hr.salary.view_own
hr.salary.view_all
hr.salary.update
hr.salary.adjust
hr.salary.history.view
hr.salary.change.approve
hr.contract.view_all
hr.contract.create
hr.contract.update
hr.contract.terminate
hr.contract.file.view
hr.contract.file.download
hr.leave.balance.adjust
hr.leave.request.force_approve
hr.leave.request.force_cancel
hr.department.transfer
hr.position.change
hr.grade.change
hr.performance.review.view
hr.performance.review.update
hr.promotion.approve
identity.role.update
identity.permission.assign
identity.permission.assign_sensitive
identity.user.deactivate
```

Do not allow sensitive permission assignment from a normal role-edit screen without explicit warning and confirmation.
