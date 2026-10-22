# Phase 08 - Promotion Management

Status: Completed

---

# Objective

Manage employee career progression.

Support:

- Position Promotion
- Grade Promotion

Maintain complete audit history.

---

# Business Requirements

The system shall:

- Promote Employee
- Track Position History
- Track Grade History
- Prevent History Corruption

---

# Domain Model

## EmployeePositionHistory

Stores:

EmployeeId

PositionId

EffectiveFrom

EffectiveTo

---

## EmployeeGradeHistory

Stores:

EmployeeId

GradeCode

EffectiveFrom

EffectiveTo

---

# Architectural Principles

Promotion is historical.

Current Position and Grade are projections.

History is the source of truth.

---

# Commands

## PromoteEmployeeCommand

Input:

EmployeeId

PositionId

GradeCode

EffectiveDate

Reason

---

# Queries

## GetPromotionHistoryQuery

## GetPositionHistoryQuery

## GetGradeHistoryQuery

---

# Business Rules

## Grade Must Exist

Implemented through:

IEmployeeGradeLookup

---

## Effective Date Validation

Promotion date must not violate existing history.

---

## No Position History Overlap

---

## No Grade History Overlap

---

## Business Timezone Validation

Validation uses:

HrSettings.BusinessTimezone

instead of UTC.

---

# Stabilization Improvements

Added:

IEmployeeGradeLookup

Added:

Defensive Overlap Validation

Added:

Timezone-Aware Validation

---

# Permissions

hr.employee.promote

---

# API Endpoints

POST /api/hr/employees/{id}/promote

GET /api/hr/employees/{id}/promotion-history

GET /api/hr/employees/{id}/grade-history

GET /api/hr/employees/{id}/position-history

---

# Frontend

Employee Career Tab

Sections:

Current Position

Current Grade

Promotion Timeline

Promotion History

---

# Future Enhancements

- Promotion Approval Workflow
- Career Path Framework
- Competency Matrix
- Succession Planning