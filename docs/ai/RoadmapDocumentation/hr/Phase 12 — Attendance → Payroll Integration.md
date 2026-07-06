# Phase 12 - Attendance Payroll Integration

Status: Completed

---

# Objective

Integrate attendance results into payroll calculation.

Attendance data may influence payroll,
but payroll must preserve its own snapshot.

---

# Core Principle

Attendance is an input source.

Payroll is the financial source of truth.

Reports must not read AttendanceRecord directly.

---

# Business Requirements

The system shall support:

- Attendance-based payroll calculation
- Working day calculation
- Absence deduction
- Late / early leave calculation foundation
- Payroll input generation from attendance results

---

# Architecture

AttendanceRecord

↓

Attendance Summary / Projection

↓

Payroll Calculation

↓

PayrollItem Snapshot

---

# Forbidden Dependency

Payroll Reporting must never query:

```text
AttendanceRecord
```

Reports must use:

```text
PayrollRun
PayrollItem
Payslip
```

---

# Domain Scope

No attendance recalculation during reporting.

Payroll calculation consumes attendance-derived results only.

---

# Commands

## GeneratePayrollFromAttendanceCommand

Generates payroll items using attendance summary.

---

## RecalculatePayrollWithAttendanceCommand

Allowed only before payroll finalization.

---

# Queries

## GetPayrollAttendanceInputsQuery

Returns attendance-derived values used for payroll.

---

# Business Rules

## Payroll Period Required

Attendance summary must match payroll period.

---

## Snapshot Required

Attendance values used during payroll must be copied into PayrollItem.

---

## No Recalculation After Finalization

Finalized payroll must never query attendance again.

---

# PayrollItem Snapshot Fields

Examples:

- WorkingDays
- PaidLeaveDays
- UnpaidLeaveDays
- AbsenceDays
- LateMinutes
- EarlyLeaveMinutes

---

# Permissions

Inherited from payroll:

```text
hr.payroll.view
hr.payroll.calculate
```

---

# API Endpoints

GET /api/hr/payroll-runs/{id}/attendance-inputs

POST /api/hr/payroll-runs/{id}/calculate-from-attendance

POST /api/hr/payroll-runs/{id}/recalculate-from-attendance

---

# Frontend

Payroll Run Detail

Sections:

- Attendance Input Summary
- Attendance-Based Payroll Items
- Calculation Warnings

---

# Future Enhancements

- Overtime Management
- Shift Management
- Holiday Engine
- Attendance Worker