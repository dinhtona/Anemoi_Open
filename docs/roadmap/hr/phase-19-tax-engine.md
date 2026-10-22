# Phase 19 - Tax Engine

Status: Planned

---

## Objective

Introduce tax calculation support for payroll. The tax engine must calculate tax deductions and preserve tax snapshots inside payroll results.

---

## Business Requirements

- Configure tax rules
- Configure tax brackets
- Calculate taxable income
- Apply payroll tax deduction
- Store tax snapshots
- Prepare future tax reporting

---

## Domain Model

### TaxRule
Tax rule set.

### TaxBracket
Progressive tax bracket.

### TaxCalculationSnapshot
Immutable tax result stored during payroll calculation.

---

## Workflow

```text
Gross Income
    ↓
Tax Engine
    ↓
PayrollItem Tax Snapshot
    ↓
Payslip
    ↓
Tax Report
```

---

## Business Rules / Architecture Notes

- Follow Clean Architecture and CQRS + MediatR.
- Keep domain rules in the domain/application layer, not in controllers.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.
- Tax must be snapshot-based. Historical payroll reports must not recalculate tax from latest tax rules.

---

## Commands

- CreateTaxRuleCommand
- UpdateTaxRuleCommand
- ActivateTaxRuleCommand
- CalculatePayrollTaxCommand

---

## Queries

- GetTaxRulesQuery
- GetActiveTaxRuleQuery
- GetPayrollTaxSnapshotQuery

---

## Permissions

```text
hr.tax.view
hr.tax.manage
```

---

## API Endpoints

```text
GET    /api/hr/tax-rules
POST   /api/hr/tax-rules
PUT    /api/hr/tax-rules/{id}
POST   /api/hr/tax-rules/{id}/activate
GET    /api/hr/payroll-runs/{id}/tax-snapshots
```

---

## Frontend

- Tax Rule Management
- Tax Bracket Editor
- Payroll Tax Snapshot View

---

## Future Enhancements

- Country-specific tax engines
- Dependent deduction rules
- Annual tax reconciliation
- Tax declaration export
