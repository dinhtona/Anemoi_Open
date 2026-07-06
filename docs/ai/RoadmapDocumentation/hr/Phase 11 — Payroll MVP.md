# Phase 11 - Payroll MVP

Status: Completed

---

# Objective

Generate payroll snapshots from compensation data.

Payroll is the central financial module
of the HR platform.

---

# Architectural Philosophy

Payroll calculations must be snapshot-based.

Historical payroll data must never change.

---

# Critical Principle

Payroll reports must never depend on
current employee data.

Payroll reports must use snapshots.

---

# Domain Model

## Aggregate Root

PayrollRun

Represents a payroll cycle.

Example:

January 2026 Payroll

---

## Entity

PayrollItem

Represents payroll result
for one employee.

---

# Payroll Architecture

Employee

↓

Compensation

↓

Payroll Calculation

↓

PayrollRun

↓

PayrollItem

---

# Snapshot Principle

PayrollItem stores:

- Employee Snapshot
- Salary Snapshot
- Allowance Snapshot

at calculation time.

---

# Why Snapshot?

If employee salary changes later:

Payroll January 2026

must remain unchanged.

---

# Payroll Lifecycle

Draft

↓

Calculated

↓

Finalized

---

# Commands

## CalculatePayrollCommand

Generates payroll items.

---

## RecalculatePayrollCommand

Allowed only before finalization.

---

## FinalizePayrollCommand

Locks payroll permanently.

---

# Queries

## GetPayrollRunsQuery

## GetPayrollRunDetailQuery

## GetPayrollItemsQuery

---

# Business Rules

## Payroll Period Unique

Only one payroll run
per payroll period.

---

## Finalized Payroll Immutable

No modification allowed.

---

## Snapshot Required

Payroll calculation must create snapshots.

Direct references are forbidden.

---

# Permissions

hr.payroll.view

hr.payroll.calculate

---

# API Endpoints

GET /api/hr/payroll-runs

GET /api/hr/payroll-runs/{id}

POST /api/hr/payroll-runs/calculate

POST /api/hr/payroll-runs/{id}/finalize

---

# Frontend

Payroll Module

Screens:

- Payroll Run List
- Payroll Run Detail
- Payroll Item Detail

---

# Future Enhancements

Phase 12

Attendance Integration

Phase 13

Approval Workflow

Phase 14

Payslip Generation

Phase 15

Reporting & Export