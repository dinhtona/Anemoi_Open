# Phase 23 - Advanced Payroll Reporting

Status: Implemented

---

## Objective

Expand payroll reporting beyond Phase 15 MVP reports with analytical payroll reports for HR, finance, and management.

---

## Business Requirements

- Department payroll cost report
- Payroll trend report
- Salary growth report
- Overtime cost report
- Tax report
- Insurance report
- Export-ready reporting foundation

---

## Domain Model

Reporting module extension based on finalized payroll snapshots.

Suggested report models:

- DepartmentPayrollCostReport
- PayrollTrendReport
- SalaryGrowthReport
- OvertimeCostReport
- TaxReport
- InsuranceReport

---

## Workflow

```text
Payroll Snapshots
    ↓
Report Projections
    ↓
JSON / CSV / Excel / BI
```

---

## Business Rules / Architecture Notes

- Follow Clean Architecture and CQRS + MediatR.
- Keep domain rules in the domain/application layer, not in controllers.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.
- Reports must use snapshots and finalized payroll data.
- Reports must not recalculate payroll, tax, insurance, attendance, or overtime.

---

## Commands

- ExportDepartmentPayrollCostReportCommand
- ExportPayrollTrendReportCommand
- ExportTaxReportCommand
- ExportInsuranceReportCommand

---

## Queries

- GetDepartmentPayrollCostReportQuery
- GetPayrollTrendReportQuery
- GetSalaryGrowthReportQuery
- GetOvertimeCostReportQuery
- GetTaxReportQuery
- GetInsuranceReportQuery

---

## Permissions

```text
hr.payroll.report.view
hr.payroll.report.export
```

---

## API Endpoints

```text
GET /api/hr/payroll/reports/department-cost
GET /api/hr/payroll/reports/trend
GET /api/hr/payroll/reports/salary-growth
GET /api/hr/payroll/reports/overtime-cost
GET /api/hr/payroll/reports/tax
GET /api/hr/payroll/reports/insurance
```

---

## Frontend

- Advanced Payroll Reports Page
- Report Filters
- Chart View
- Table View
- CSV Export

---

## Future Enhancements

- Excel export
- Scheduled reports
- Report templates
- BI connector
