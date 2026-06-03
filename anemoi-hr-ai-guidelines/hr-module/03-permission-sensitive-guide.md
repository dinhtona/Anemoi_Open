# HR Permission, DataScope, and Sensitive Permission Guide

## Principle

Reuse the existing role/permission system. Do not create a separate HR authorization system.

Extend existing permissions with:

```text
DataScope
SensitivePermission
RiskLevel
Assignment confirmation
Assignment approval workflow
Audit log
```

## Permission model

```text
Permission
- Code
- Name
- ModuleCode
- Description
- IsSensitive
- RiskLevelCode
```

```text
RolePermission
- RoleId
- PermissionCode
- ScopeTypeCode
- ScopeDepartmentId nullable
```

## Scope types

```text
own
team
department
sub_departments
company
specific_department
specific_employee
```

## Permission naming convention

Use capability-based permission codes:

```text
hr.{module}.{resource}.{action}
```

Examples:

```text
hr.employee.profile.view
hr.employee.profile.create
hr.employee.profile.update
hr.leave.request.create
hr.leave.request.approve
hr.salary.profile.view
hr.salary.profile.update
```

Do not use role-based permission names like `hr.manager` or `hr.hrstaff`.

## Baseline permissions

Employee:

```text
hr.employee.profile.view
hr.employee.profile.create
hr.employee.profile.update
hr.employee.profile.delete
hr.employee.profile.view_sensitive
hr.employee.bank_info.view
hr.employee.bank_info.update
hr.employee.identity_document.view
```

Organization:

```text
hr.organization.department.view
hr.organization.department.create
hr.organization.department.update
hr.organization.department.delete
hr.organization.position.view
hr.organization.position.create
hr.organization.position.update
```

Leave:

```text
hr.leave.request.create
hr.leave.request.view_own
hr.leave.request.view_team
hr.leave.request.view_department
hr.leave.request.view_all
hr.leave.request.approve
hr.leave.request.reject
hr.leave.request.cancel
hr.leave.request.force_approve
hr.leave.request.force_cancel
hr.leave.balance.view
hr.leave.balance.adjust
hr.leave.policy.view
hr.leave.policy.manage
```

Approval:

```text
hr.approval.workflow.view
hr.approval.workflow.manage
hr.approval.request.view
hr.approval.request.action
```

Salary:

```text
hr.salary.profile.view_own
hr.salary.profile.view_all
hr.salary.profile.update
hr.salary.history.view
hr.salary.change_request.create
hr.salary.change_request.approve
```

Contract:

```text
hr.contract.view_own
hr.contract.view_all
hr.contract.create
hr.contract.update
hr.contract.terminate
hr.contract.file.view
```

Skill and Career:

```text
hr.skill.view
hr.skill.manage
hr.employee_skill.view
hr.employee_skill.update
hr.career_path.view
hr.career_path.manage
hr.promotion.request
hr.promotion.approve
hr.performance.review.view
hr.performance.review.update
```

Permission management:

```text
hr.permission.assign
hr.permission.assign_sensitive
hr.role.update
hr.user.deactivate
```

## Sensitive permissions

Mark these as `IsSensitive = true`.

```text
hr.salary.profile.view_all
hr.salary.profile.update
hr.salary.history.view
hr.salary.change_request.approve

hr.contract.view_all
hr.contract.create
hr.contract.update
hr.contract.terminate
hr.contract.file.view

hr.employee.profile.view_sensitive
hr.employee.bank_info.view
hr.employee.bank_info.update
hr.employee.identity_document.view

hr.leave.balance.adjust
hr.leave.request.force_approve
hr.leave.request.force_cancel

hr.organization.department.transfer
hr.organization.position.change
hr.employee.grade.change

hr.promotion.approve
hr.performance.review.view
hr.performance.review.update

hr.permission.assign_sensitive
hr.role.update
hr.user.deactivate
```

## Risk levels

```text
low
medium
high
critical
```

Recommended behavior:

```text
low:
  - normal assignment

medium:
  - confirmation popup
  - reason required

high:
  - reason required
  - password or OTP confirmation
  - notification to security/admin

critical:
  - reason required
  - password or OTP confirmation
  - second approver required
  - audit log required
  - notification to admin/security owner
```

## Sensitive assignment workflow

When assigning a sensitive permission:

1. Detect `IsSensitive` and `RiskLevelCode`.
2. Show warning in UI.
3. Require reason.
4. Require re-authentication for high/critical permissions.
5. Create approval request for critical permissions.
6. Do not apply permission until approval is completed.
7. Publish event after assignment.
8. Write audit log.

## Events

```text
SensitivePermissionAssignmentRequested
SensitivePermissionAssignmentApproved
SensitivePermissionAssignmentRejected
SensitivePermissionAssigned
SensitivePermissionRevoked
```
