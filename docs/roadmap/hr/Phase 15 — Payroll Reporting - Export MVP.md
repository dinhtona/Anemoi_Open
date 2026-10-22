# Phase 15 - Payroll Reporting Export MVP

Status: In Progress

---

# Objective

Provide payroll reports and CSV export capability.

Reports must be based on payroll and payslip snapshots.

No report may trigger payroll recalculation.

---

# Business Requirements

The system shall support:

- Payroll Run Summary Report
- Payroll Item Detail Report
- Payslip Summary Report
- Paginated JSON report APIs
- CSV export APIs
- Export audit logging

---

# Critical Architecture Rule

Reports must never query:

```text
AttendanceRecord
```

Reports must read only:

```text
PayrollRun
PayrollItem
Payslip
```

---

# Report Types

## Payroll Run Summary

Purpose:

High-level finalized payroll overview.

Fields:

- PayrollRunId
- PayrollPeriod
- Status
- FinalizedAt
- EmployeeCount
- GrossPayTotal
- DeductionTotal
- NetPayTotal

---

## Payroll Item Detail

Purpose:

Employee-level payroll detail.

Fields:

- PayrollRunId
- EmployeeId
- EmployeeCode
- EmployeeName
- Department
- Position
- BaseSalary
- Allowances
- Deductions
- NetPay

---

## Payslip Summary

Purpose:

Payslip generation and payment visibility.

Fields:

- PayslipId
- PayrollRunId
- EmployeeCode
- EmployeeName
- GeneratedAt
- GrossPay
- NetPay

---

# Filters

## Payroll Run Summary

Uses:

```text
FinalizedFrom
FinalizedTo
```

---

## Payroll Item Detail

Uses:

```text
FinalizedFrom
FinalizedTo
```

---

## Payslip Summary

Uses:

```text
GeneratedFrom
GeneratedTo
```

---

# Architectural Principles

## Shared Projection Infrastructure

JSON and CSV export must share the same projection logic.

Do not duplicate query/filter logic.

---

## Snapshot-Only Reporting

Reports use persisted payroll snapshots only.

---

## No Generic Audit Framework Yet

Use a focused export audit entity.

Do not introduce full generic audit infrastructure.

---

# Domain Model

## Entity

ReportExportAuditLog

Tracks report export activity.

This is an Entity, not a ValueObject.

Reason:

It has identity and lifecycle.

---

# Suggested Namespace

Preferred:

```text
Domain/Reporting
```

or:

```text
Domain/Auditing
```

Avoid:

```text
Domain/Shared
```

unless project convention requires it.

---

# Constants

## ModuleCodes

Examples:

```text
HrPayroll
```

---

## ReportTypes

Examples:

```text
PayrollRunSummary
PayrollItemDetail
PayslipSummary
```

---

# Audit Captured Fields

ReportExportAuditLog should capture:

- Id
- ModuleCode
- ReportType
- ExportedByUserId
- ExportedByUserName
- ExportedAt
- FilterJson
- FileName
- RowCount

---

# Retention Policy

Report export audit logs must be retained for at least:

```text
5 years
```

Reason:

Future Tax and Insurance modules may rely on export traceability.

---

# Commands

## ExportPayrollRunSummaryCsvCommand

---

## ExportPayrollItemDetailCsvCommand

---

## ExportPayslipSummaryCsvCommand

---

# Queries

## GetPayrollRunSummaryReportQuery

---

## GetPayrollItemDetailReportQuery

---

## GetPayslipSummaryReportQuery

---

# Permissions

```text
hr.payroll.view
hr.payroll.export
```

---

# API Endpoints

GET /api/hr/payroll/reports/run-summary

GET /api/hr/payroll/reports/item-detail

GET /api/hr/payroll/reports/payslip-summary

GET /api/hr/payroll/reports/run-summary/export

GET /api/hr/payroll/reports/item-detail/export

GET /api/hr/payroll/reports/payslip-summary/export

---

# Frontend

Payroll Reports Page

Tabs:

- Run Summary
- Item Detail
- Payslip Summary

Features:

- Date Filters
- Pagination
- CSV Export
- Permission-Based Export Button

---

# Future Enhancements

- Excel Export
- PDF Report
- Scheduled Reports
- Email Delivery
- Tax Reports
- Insurance Reports
- BI Integration