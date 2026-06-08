# Phase 06 - HR Dashboard

Status: Completed

---

# Objective

Provide a centralized HR dashboard that gives HR Administrators,
Managers, and Executives a real-time overview of workforce status.

The dashboard is intended to be a read-only analytical module.

No business transaction is performed from this module.

---

# Business Requirements

The system shall provide a consolidated dashboard displaying:

- Total Employee Count
- Active Employee Count
- Inactive Employee Count
- Employees Currently On Leave
- Pending Leave Requests
- Upcoming Leave Requests
- Department Distribution
- Contract Expiration Alerts

The dashboard must load quickly and avoid expensive transactional queries.

---

# Architectural Principles

## Read Model Only

Dashboard data must be generated from queries.

The dashboard must not modify any business data.

No Commands are allowed.

Only Queries are permitted.

---

## CQRS Compliance

Dashboard endpoints must be implemented through MediatR Queries.

Example:

GetDashboardSummaryQuery

GetDepartmentDistributionQuery

GetLeaveOverviewQuery

GetContractExpirationAlertsQuery

---

# Functional Areas

## Workforce Summary

Displays:

- Total Employees
- Active Employees
- Inactive Employees

### Calculation Rules

Active Employee:

Employee.Status == Active

Inactive Employee:

Employee.Status != Active

---

## Leave Overview

Displays:

- Employees On Leave Today
- Pending Leave Requests
- Upcoming Approved Leave Requests

Data Source:

LeaveRequest

LeaveBalance

---

## Department Distribution

Displays employee count grouped by department.

Example:

Engineering : 25

HR : 5

Finance : 8

Operations : 12

---

## Contract Expiration Alerts

Displays contracts nearing expiration.

Default Window:

30 Days

Configurable in future versions.

Data Source:

EmployeeContract

---

# Domain Scope

No Aggregate Root is introduced.

Dashboard is a projection-only module.

---

# Queries

## GetDashboardSummaryQuery

Returns:

- TotalEmployees
- ActiveEmployees
- InactiveEmployees
- EmployeesOnLeave
- PendingLeaveRequests

---

## GetDepartmentDistributionQuery

Returns:

Department Name

Employee Count

---

## GetContractExpirationAlertsQuery

Returns:

Employee

Contract Number

Expiration Date

Days Remaining

---

# Permissions

Required Permission:

hr.dashboard.view

---

# API Endpoints

GET /api/hr/dashboard/summary

GET /api/hr/dashboard/departments

GET /api/hr/dashboard/contracts

GET /api/hr/dashboard/leave-overview

---

# Frontend

Dashboard Page

Widgets:

- Summary Cards
- Department Chart
- Leave Chart
- Contract Alert Table

---

# Future Enhancements

Phase 16+

- Turnover Analytics
- Hiring Analytics
- Payroll Analytics
- Workforce Forecasting