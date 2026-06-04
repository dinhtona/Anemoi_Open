# Payroll Management

Future concepts:

```text
SalaryProfile
SalaryHistory
SalaryComponent
Allowance
Deduction
PayrollPeriod
PayrollRun
Payslip
TaxProfile
InsuranceProfile
```

Rules:

- Salary history is immutable.
- Salary changes must be requested and approved.
- Do not directly overwrite current salary without SalaryChange history.
- Salary permissions are sensitive.
- Payroll run should be auditable.
