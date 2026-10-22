# Phase 21 - Employee Self Service (ESS) — Design Document

**Status:** Planned  
**Date:** 2026-06-13  
**Supersedes:** None  

---

## 1. Overview

Provide an employee-facing portal for self-service HR operations. ESS reuses existing HR modules (leave, attendance, payslip, overtime, contract) while enforcing employee-scoped access control.

ESS is not a new domain aggregate. It is an application/API layer that exposes employee-scoped capabilities from existing modules.

---

## 2. Architecture

- **Service:** `Anemoi.Hr` (no new bounded context)
- **Pattern:** Application/API layer only — reuses existing Domain entities
- **Identity resolution:** EmployeeId resolved from the authenticated user (not from request params)
- **CQRS + MediatR:** All ESS operations through Commands/Queries
- **Permissions:** `ess.*` scope for employee-facing operations

---

## 3. Security & Access Control

- Employees cannot pass arbitrary `EmployeeId` for ESS queries
- Identity must be resolved server-side from the JWT/authenticated user
- Payslip access must be strictly employee-scoped
- Sensitive operations require explicit permissions
- Audit trail for all state-changing actions

---

## 4. API Design

Base route: `api/ess`

### Queries (employee-scoped reads)

| Method | Route | Permission |
|--------|-------|------------|
| GET | `profile` | `ess.profile.view` |
| GET | `leave/balance` | `ess.leave.view` |
| GET | `attendance/summary` | `ess.attendance.view` |
| GET | `payslips` | `ess.payslip.view` |
| GET | `payslips/{id}` | `ess.payslip.view` |
| GET | `contracts/current` | `ess.contract.view` |

### Commands (employee-initiated actions)

| Method | Route | Permission |
|--------|-------|------------|
| POST | `leave/requests` | `ess.leave.request` |
| POST | `overtime-requests` | `ess.overtime.request` |

---

## 5. Permissions

| Constant | Value |
|----------|-------|
| `EssProfileView` | `ess.profile.view` |
| `EssLeaveView` | `ess.leave.view` |
| `EssLeaveRequest` | `ess.leave.request` |
| `EssAttendanceView` | `ess.attendance.view` |
| `EssPayslipView` | `ess.payslip.view` |
| `EssOvertimeRequest` | `ess.overtime.request` |
| `EssContractView` | `ess.contract.view` |

---

## 6. CQRS Structure

### Commands

```
SubmitEssLeaveRequestCommand       — SubmitEssLeaveRequestHandler
SubmitEssOvertimeRequestCommand    — SubmitEssOvertimeRequestHandler
```

### Queries

```
GetMyProfileQuery                  — GetMyProfileHandler
GetMyLeaveBalanceQuery             — GetMyLeaveBalanceHandler
GetMyAttendanceSummaryQuery        — GetMyAttendanceSummaryHandler
GetMyPayslipsQuery                 — GetMyPayslipsHandler
GetMyPayslipDetailQuery            — GetMyPayslipDetailHandler
GetMyCurrentContractQuery          — GetMyCurrentContractHandler
```

---

## 7. Workflow

```
Authenticated User
    ↓
JWT → Resolve EmployeeId (server-side)
    ↓
Employee-Scoped ESS APIs
    ↓
Read: query existing module data filtered by EmployeeId
Write: submit requests with EmployeeId from identity (not request)
```

---

## 8. Frontend Pages

| Route | Description |
|-------|-------------|
| `/ess/profile` | View personal profile |
| `/ess/leave` | View leave balance + submit leave request |
| `/ess/attendance` | View attendance summary |
| `/ess/payslips` | View payslips list |
| `/ess/payslips/[id]` | View payslip detail |
| `/ess/overtime` | Submit overtime request |
| `/ess/contract` | View current contract summary |

---

## 9. Business Rules

| Rule | Detail |
|------|--------|
| EmployeeId resolution | Server-side from authenticated user, never from request body/params |
| Data scope | Read only the current employee's data |
| Reuse | Leverage existing HR modules — do not duplicate business logic |
| Audit | State-changing actions must be logged |
| Historical data | Preserve records affecting payroll, compliance, or employee history |
| Permission model | Explicit `ess.*` permissions, not role-based |

---

## 10. Future Enhancements

- Mobile ESS
- Employee document center
- Employee profile update request workflow
- ESS notifications

---

## 11. Dependencies

- Phase 11 (Payroll MVP) — payslip viewing
- Phase 12 (Attendance → Payroll Integration) — attendance summary
- Phase 14 (Payslip MVP) — payslip detail
- Phase 16 (Overtime Management) — overtime requests
- Phase 20 (Insurance Engine) — insurance contribution visibility
- Employee module — profile viewing
- Leave module — balance + requests
- Contract module — current contract
