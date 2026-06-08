# Phase 10 - Compensation Management

Status: Completed

---

# Objective

Manage employee compensation structures.

The system shall support:

- Monthly Salary
- Daily Salary
- Allowances
- Salary Grades

Compensation is the foundation of payroll calculation.

---

# Business Requirements

Support:

- Salary Assignment
- Salary Change
- Allowance Assignment
- Grade-Based Compensation

---

# Domain Model

## Aggregate Root

EmployeeSalary

Represents current salary structure.

---

## Entities

### SalaryGrade

Stores salary ranges.

---

### EmployeeAllowance

Stores employee allowances.

---

### AllowanceType

Examples:

- Housing
- Transportation
- Meal
- Responsibility
- Custom

---

# Compensation Structure

Employee Compensation

= Base Salary

+ Allowances

---

# Supported Salary Types

## Monthly Salary

Fixed monthly compensation.

---

## Daily Salary

Compensation calculated by working days.

---

# Commands

## AssignSalaryCommand

---

## UpdateSalaryCommand

---

## AssignAllowanceCommand

---

## RemoveAllowanceCommand

---

# Queries

## GetSalaryDetailQuery

## GetAllowanceListQuery

## GetSalaryHistoryQuery

---

# Business Rules

## Single Active Salary

Only one active salary structure.

---

## Effective Date Validation

Salary periods must not overlap.

---

## Allowance Validation

Allowance amount cannot be negative.

---

## Salary Type Consistency

Daily and Monthly salary models
cannot coexist simultaneously.

---

# Permissions

hr.compensation.view

hr.compensation.manage

---

# API Endpoints

GET /api/hr/compensation

GET /api/hr/compensation/{employeeId}

POST /api/hr/compensation/salary

POST /api/hr/compensation/allowance

DELETE /api/hr/compensation/allowance/{id}

---

# Frontend

Employee Compensation Tab

Sections:

- Salary Information
- Allowance List
- Salary History

---

# Future Enhancements

- Tax Engine
- Insurance Engine
- Salary Review Workflow
- Compensation Approval Workflow