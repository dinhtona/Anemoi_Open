# HR Domain Design Guide

## Aggregate roots

Recommended aggregate roots:

```text
Employee
Department
Position
LeavePolicy
LeaveRequest
LeaveBalance
ApprovalRequest
Contract
SalaryProfile
SalaryChangeRequest
Skill
EmployeeSkill
CareerPath
PromotionRequest
```

## Employee

Employee represents the internal employee profile. It must not be confused with Identity User.

```text
Employee
- EmployeeId
- UserId nullable/external identity id
- EmployeeCode
- FullName
- Email
- PhoneNumber
- WorkStatusCode
- JoinDate
- OfficialDate nullable
- ResignedDate nullable
- DirectManagerEmployeeId nullable
- PrimaryDepartmentId nullable
- PrimaryPositionId nullable
- CurrentGradeCode nullable
```

Rules:

- EmployeeCode must be unique.
- Email should be unique when present.
- Active employees should have a primary department and position after onboarding.
- Employee historical changes must be stored, not overwritten silently.

## Organization

Use real domain tables for departments and positions. Do not model departments as simple categories.

```text
Department
- DepartmentId
- Code
- Name
- ParentDepartmentId nullable
- ManagerEmployeeId nullable
- IsActive
```

```text
Position
- PositionId
- Code
- Name
- GradeCode nullable
- IsActive
```

```text
EmployeeDepartmentHistory
- Id
- EmployeeId
- DepartmentId
- PositionId
- StartDate
- EndDate nullable
- IsPrimary
- ChangeReasonCode
```

## Leave

Leave balance must be managed using a transaction ledger.

```text
LeavePolicy
- LeavePolicyId
- Code
- Name
- MonthlyAccrualDays default 1.25
- MaxAnnualDays default 15
- AllowCarryForward
- MaxCarryForwardDays nullable
- IsActive
```

```text
LeaveBalance
- LeaveBalanceId
- EmployeeId
- Year
- OpeningDays
- AccruedDays
- UsedDays
- PendingDays
- AdjustedDays
- RemainingDays
```

```text
LeaveTransaction
- LeaveTransactionId
- EmployeeId
- Year
- TypeCode
- Days
- SourceType
- SourceId nullable
- Description
- CreatedAt
```

```text
LeaveRequest
- LeaveRequestId
- EmployeeId
- LeaveTypeCode
- StartDate
- EndDate
- TotalDays
- Reason
- StatusCode
- CurrentApproverEmployeeId nullable
- SubmittedAt nullable
- CompletedAt nullable
```

```text
LeaveApproval
- LeaveApprovalId
- LeaveRequestId
- StepNo
- ApproverEmployeeId
- StatusCode
- Comment nullable
- ActionAt nullable
```

## Leave accrual

Monthly leave accrual rule:

- Add 1.25 leave days at the end of every month for eligible active employees.
- The value must come from LeavePolicy, not hard-coded in handlers.
- Prevent duplicate accrual by unique key `(EmployeeId, YearMonth)`.

```text
LeaveAccrualRun
- LeaveAccrualRunId
- EmployeeId
- YearMonth
- Days
- PolicyCode
- StatusCode
- CreatedAt
```

## Salary

Salary data is sensitive. Use immutable history.

```text
SalaryProfile
- SalaryProfileId
- EmployeeId
- CurrentBaseSalary
- CurrencyCode
- EffectiveFrom
```

```text
SalaryHistory
- SalaryHistoryId
- EmployeeId
- BaseSalary
- CurrencyCode
- EffectiveFrom
- EffectiveTo nullable
- SourceSalaryChangeRequestId nullable
```

```text
SalaryChangeRequest
- SalaryChangeRequestId
- EmployeeId
- OldSalary
- NewSalary
- CurrencyCode
- EffectiveDate
- ReasonCode
- StatusCode
```

Rules:

- Never update salary without creating SalaryHistory.
- Salary view/update permissions are sensitive.
- Salary changes must go through approval workflow.

## Contract

```text
Contract
- ContractId
- EmployeeId
- ContractTypeCode
- ContractNo
- StartDate
- EndDate nullable
- StatusCode
- FileId nullable
```

Rules:

- Contract expiry reminders should be event-driven.
- Contract termination should be sensitive and audited.

## Skill and career path

```text
Skill
- SkillId
- Code
- Name
- SkillCategoryCode
```

```text
EmployeeSkill
- EmployeeSkillId
- EmployeeId
- SkillId
- LevelCode
- VerifiedByEmployeeId nullable
- VerifiedAt nullable
```

```text
CareerPath
- CareerPathId
- Code
- Name
```

```text
CareerPathStep
- CareerPathStepId
- CareerPathId
- GradeCode
- RequiredSkillSetJson
- SortOrder
```
