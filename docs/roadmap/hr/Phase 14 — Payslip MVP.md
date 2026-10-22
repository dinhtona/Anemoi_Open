# Phase 14 - Payslip MVP

Status: Completed

---

# Objective

Generate employee payslips from finalized payroll.

Payslip is the employee-facing representation
of finalized payroll data.

---

# Business Requirements

The system shall support:

- Generate payslips for finalized payroll run
- View payslip list
- View payslip detail
- Prevent duplicate payslip generation
- Preserve immutable payroll snapshot

---

# Domain Model

## Aggregate Root

Payslip

Represents a generated salary statement
for one employee in one payroll run.

---

# Relationship

PayrollRun

↓

PayrollItem

↓

Payslip

---

# Snapshot Fields

Payslip stores:

- Employee Snapshot
- Payroll Period
- Base Salary
- Allowances
- Deductions
- Net Pay
- GeneratedAt

---

# Commands

## GeneratePayslipsForPayrollRunCommand

Generates payslips for all payroll items
in a finalized payroll run.

---

# Queries

## GetPayslipsQuery

## GetPayslipDetailQuery

## GetEmployeePayslipsQuery

---

# Business Rules

## Payroll Must Be Finalized

Payslips can only be generated when:

PayrollRun.Status == Finalized

---

## Idempotency

Running generation multiple times must not create duplicates.

---

## Unique Constraint

The following pair must be unique:

```text
PayrollRunId
EmployeeId
```

---

## Payslip Immutable

Generated payslip must not be recalculated.

---

# Permissions

```text
hr.payslip.view
hr.payroll.calculate
```

---

# API Endpoints

POST /api/hr/payroll-runs/{id}/payslips/generate

GET /api/hr/payslips

GET /api/hr/payslips/{id}

GET /api/hr/employees/{id}/payslips

---

# Frontend

Payroll Run Detail Drawer

Sections:

- Payslip Generation Status
- Generate Payslips Action
- Payslip List

Employee Detail

Tab:

- Payslips

---

# Future Enhancements

- PDF Payslip
- Email Delivery
- Employee Self-Service
- Digital Signature