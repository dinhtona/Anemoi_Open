# Phase 23 - Advanced Payroll Reporting

## Overview

Phase 23 expands the payroll reporting capabilities by introducing advanced reporting, payroll analytics views, variance comparison, department breakdowns, employee breakdowns, delivery reporting, and CSV exports.

The implementation extends the reporting infrastructure created in Phase 15 and integrates with:

* Payroll MVP
* Attendance Integration
* Payroll Approval Workflow
* Payslip Management
* Tax Engine
* Insurance Engine
* PDF Payslip & Email Delivery

All reports operate exclusively on persisted snapshots and historical records.

---

## Goals

* Provide payroll cost reporting.
* Provide departmental payroll analysis.
* Provide employee-level payroll analysis.
* Compare payroll runs through variance reporting.
* Monitor payslip delivery status.
* Support CSV exports.
* Maintain export auditability.

---

## Functional Scope

### Payroll Cost Summary

Provides:

* Payroll run information
* Employee count
* Gross income
* Taxable income
* Employee tax
* Employee insurance
* Employer insurance
* Total deductions
* Net pay

Supports filtering by:

* Payroll run
* Payroll period
* Status
* Date range
* Department
* Position

---

### Department Breakdown

Grouped payroll reporting by department.

Metrics:

* Employee count
* Gross income
* Tax
* Insurance
* Net pay

---

### Employee Breakdown

Detailed payroll reporting per employee.

Includes:

* Employee information
* Department
* Position
* Gross income
* Taxable income
* Tax
* Insurance
* Net pay
* Payslip status

Supports pagination.

---

### Payroll Variance Report

Compares:

* Current payroll run
* Previous payroll run

Calculates:

* Gross income variance
* Tax variance
* Insurance variance
* Net pay variance

Supports:

* Employee comparison by EmployeeId
* IncludeRemovedEmployees option

Implemented through full outer join comparison logic.

---

### Payslip Delivery Report

Provides visibility into:

* PDF generation status
* Download availability
* Email delivery status
* Payslip publication status

Derived only from persisted records.

No synthetic calculations are introduced.

---

## CSV Export

Supported exports:

* Payroll Cost Summary
* Department Breakdown
* Employee Breakdown
* Variance Report
* Payslip Delivery Report

Every export automatically creates:

* ReportExportAuditLog

Audit data includes:

* Report type
* Export format
* Filters
* User
* Timestamp
* Row count

---

## Architecture

### Permissions

Added:

* hr.payroll.reporting.view
* hr.payroll.reporting.export

Integrated into:

* BuildingBlock permissions
* HR permissions

---

### Shared Query Layer

Introduced:

* PayrollReportingQueryExtensions

Responsibilities:

* Single source of truth for reporting queries.
* Shared by JSON endpoints and CSV exports.
* Prevents duplicated reporting logic.

Implemented projections:

* BuildPayrollCostSummary
* BuildPayrollCostDepartment
* BuildPayrollCostEmployee
* BuildPayrollVariance
* BuildPayslipDeliveryReport

---

### Application Layer

Added:

#### Queries

* GetPayrollCostSummaryReport
* GetPayrollCostDepartmentReport
* GetPayrollCostEmployeeReport
* GetPayrollVarianceReport
* GetPayslipDeliveryReport

#### CSV Exports

* ExportPayrollCostSummaryCsv
* ExportPayrollCostDepartmentCsv
* ExportPayrollCostEmployeeCsv
* ExportPayrollVarianceCsv
* ExportPayslipDeliveryCsv

Validators created for all requests.

---

### API Layer

Added ten new endpoints:

#### View

* Payroll Cost Summary
* Department Breakdown
* Employee Breakdown
* Variance
* Payslip Delivery

#### Export

* Summary CSV
* Department CSV
* Employee CSV
* Variance CSV
* Delivery CSV

All endpoints are permission protected.

---

### Frontend

Payroll Reporting page redesigned.

Features:

* Filter panel
* Summary reporting
* Department reporting
* Employee reporting
* Variance reporting
* Payslip delivery reporting
* CSV export actions

Supports:

* Pagination
* Loading states
* Empty states
* Error states
* Localization

Implemented using React Query and shadcn/ui.

---

## Verification

### Backend

Tests added:

* Summary calculations
* Department grouping
* Variance calculations
* Full outer join comparison
* Payslip delivery navigation

Results:

* 193/193 tests passed

---

### Frontend

Build:

* Passed

TypeScript:

* Passed

Lint:

* Passed

Reporting pages rendered successfully.

---

## Technical Debt

### TD-PAY-002

Department and Position reporting currently falls back to current employee assignments when historical snapshot values are unavailable.

Future enhancement:

* Snapshot department and position data into payroll runs for perfect historical reporting.

---

## Architectural Decisions

### ADR-023-001

Reports must never recalculate payroll values.

All reporting must consume:

* Payroll snapshots
* Tax snapshots
* Insurance snapshots
* Payslip records

as persisted historical data.

---

### ADR-023-002

JSON reports and CSV exports must share the same query source through PayrollReportingQueryExtensions to prevent reporting inconsistencies.

---

## Outcome

Phase 23 successfully transforms payroll reporting from operational reporting into analytical reporting, providing payroll cost visibility, variance analysis, delivery monitoring, and export auditability while preserving the snapshot-based payroll architecture.
