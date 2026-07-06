# Permission Model

Reuse the existing Identity role/permission system. Do not create a separate HR permission system.

Authorization is capability-based. Do not hard-code business role names such as Manager, Staff, HR, or DepartmentManager.

## Permission Format

```text
{module}.{resource}.{action}
```

Examples:

```text
hr.employee.view
hr.employee.create
hr.employee.update
hr.leave.request.create
hr.leave.request.approve
hr.salary.view_all
```

## Data Scope

HR requires data scope.

```text
Own
Team
Department
SubDepartments
Company
SpecificDepartment
SpecificEmployee
```

Examples:

```text
hr.leave.request.approve + Team
hr.employee.view + Department
hr.salary.view_all + Company
```

## Suggested Schema

```text
Permissions
- Id
- Code
- Name
- ModuleCode
- Description
- IsSensitive
- RiskLevel
- IsActive

RolePermissions
- RoleId
- PermissionId
- ScopeType
- DepartmentId nullable
- EmployeeId nullable
```

Protected endpoints must use permission authorization, for example `[HasPermission(HrPermissions.LeaveRequestApprove)]`.
