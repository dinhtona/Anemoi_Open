# Phase 20 - Insurance Engine

Status: Planned

---

## Objective

Introduce insurance contribution calculation for payroll. Insurance values must be calculated during payroll and stored as immutable snapshots.

---

## Business Requirements

- Configure insurance rules
- Manage employee insurance participation
- Calculate employee contribution
- Calculate employer contribution
- Integrate insurance deductions into payroll
- Prepare insurance reports

---

## Domain Model

### InsuranceRule
Contribution rates, caps, and rule definitions.

### EmployeeInsuranceProfile
Employee participation settings.

### InsuranceContributionSnapshot
Immutable contribution result per payroll item.

---

## Workflow

```text
Salary / Insurance Base
    ↓
Insurance Engine
    ↓
PayrollItem Insurance Snapshot
    ↓
Payslip
    ↓
Insurance Report
```

---

## Business Rules / Architecture Notes

- Follow Clean Architecture and CQRS + MediatR.
- Keep domain rules in the domain/application layer, not in controllers.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.
- Employee and employer contribution must be separated.
- Reports must not recalculate historical insurance values.

---

## Commands

- CreateInsuranceRuleCommand
- UpdateInsuranceRuleCommand
- AssignEmployeeInsuranceProfileCommand
- CalculatePayrollInsuranceCommand

---

## Queries

- GetInsuranceRulesQuery
- GetEmployeeInsuranceProfileQuery
- GetPayrollInsuranceSnapshotsQuery

---

## Permissions

```text
hr.insurance.view
hr.insurance.manage
```

---

## API Endpoints

```text
GET    /api/hr/insurance-rules
POST   /api/hr/insurance-rules
PUT    /api/hr/insurance-rules/{id}
GET    /api/hr/employees/{id}/insurance-profile
POST   /api/hr/employees/{id}/insurance-profile
GET    /api/hr/payroll-runs/{id}/insurance-snapshots
```

---

## Frontend

- Insurance Rule Management
- Employee Insurance Profile
- Payroll Insurance Snapshot View

---

## Future Enhancements

- Country-specific insurance schemes
- Company contribution reports
- Insurance declaration export
