# Phase 07 - Department Transfer

Status: Completed

---

# Objective

Track employee department movement while preserving
historical organizational structure.

Department changes must be auditable.

Historical data must never be lost.

---

# Business Requirements

The system shall allow:

- Department Transfer
- Department History Tracking
- Department Timeline Visualization

---

# Domain Model

## Aggregate Root

Employee

---

## Entity

EmployeeDepartmentHistory

Stores:

- EmployeeId
- DepartmentId
- EffectiveFrom
- EffectiveTo

---

# Architectural Principles

Department assignment is time-based.

Current department is derived from:

EmployeeDepartmentHistory

not from mutable overwrite operations.

---

# Transfer Workflow

Employee
    ↓
Transfer Request
    ↓
Validation
    ↓
Close Previous History
    ↓
Create New History Record
    ↓
Update Current Department

---

# Commands

## TransferEmployeeDepartmentCommand

Input:

EmployeeId

DepartmentId

EffectiveDate

Reason

---

# Queries

## GetDepartmentHistoryQuery

## GetDepartmentTimelineQuery

---

# Business Rules

## Employee Must Exist

---

## Department Must Exist

---

## Effective Date Validation

Transfer date cannot be earlier than current history.

---

## No History Overlap

The following is invalid:

History A

2025-01-01 ~ 2025-12-31

History B

2025-06-01 ~ NULL

---

## Concurrency Protection

Multiple transfers cannot be created simultaneously.

---

# Permissions

hr.employee.transfer

---

# API Endpoints

POST /api/hr/employees/{id}/transfer

GET /api/hr/employees/{id}/department-history

GET /api/hr/employees/{id}/department-timeline

---

# Frontend

Employee Detail

Tab:

Department History

Timeline View

Transfer Action

---

# Future Enhancements

- Bulk Transfer
- Organization Restructure Wizard
- Transfer Approval Workflow