# HR MasterData and Category Guide

## Purpose

Use MasterData/Category for shared lookup values that are stable, configurable, and reused across HR modules.

Do not use Category for complex business entities such as Department, Employee, Contract, LeaveRequest, or SalaryChangeRequest.

## Recommended schema

```text
CategoryGroup
- Id
- Code
- Name
- Description
- IsSystem
- IsActive
```

```text
Category
- Id
- GroupCode
- Code
- Name
- ParentId nullable
- SortOrder
- IsActive
- MetadataJson nullable
```

## Required HR category groups

```text
employee_status
employment_type
gender
marital_status
leave_type
leave_request_status
leave_transaction_type
approval_status
approval_target_type
approver_type
contract_type
contract_status
salary_change_reason
salary_change_status
department_change_reason
skill_category
skill_level
employee_grade
promotion_status
performance_review_status
document_type
sensitive_risk_level
permission_scope_type
```

## MetadataJson examples

Leave type:

```json
{
  "requiresBalance": true,
  "isPaid": true,
  "allowHalfDay": true,
  "maxConsecutiveDays": 5
}
```

Employee grade:

```json
{
  "rank": 4,
  "displayColor": "blue",
  "requiresApprovalForPromotion": true
}
```

Permission risk level:

```json
{
  "requiresReason": true,
  "requiresPasswordConfirmation": true,
  "requiresSecondApproval": true
}
```

## Naming rules

- GroupCode and Code must be stable wire values.
- Do not rename codes casually after they are used in DB/JWT/API.
- UI should localize display labels, not code values.
- Backend should return code values and optional localized names when needed.
