# Phase 13 - Payroll Approval Workflow MVP

Status: Completed

---

# Objective

Introduce approval workflow before payroll finalization.

Payroll must be reviewed and approved before becoming immutable.

---

# Business Requirements

The system shall support:

- Payroll Review
- Payroll Approval
- Payroll Rejection
- Payroll Reopen before finalization
- Finalization after approval

---

# Payroll Lifecycle

Draft

↓

Calculated

↓

Reviewed

↓

Approved

↓

Finalized

---

# Status Meaning

## Draft

Payroll run exists but has not been calculated.

---

## Calculated

Payroll items have been generated.

---

## Reviewed

Payroll run has been reviewed by HR.

---

## Approved

Payroll run has been approved by authorized user.

---

## Finalized

Payroll is locked permanently.

---

# Commands

## SubmitPayrollForReviewCommand

Moves payroll from Calculated to Reviewed.

---

## ApprovePayrollRunCommand

Moves payroll from Reviewed to Approved.

---

## RejectPayrollRunCommand

Moves payroll back for correction.

---

## FinalizePayrollRunCommand

Locks payroll after approval.

---

## ReopenPayrollRunCommand

Allowed only before finalization.

---

# Business Rules

## Approval Required

Payroll cannot be finalized unless approved.

---

## Finalized Payroll Immutable

No modification after finalization.

---

## Permission Required

Approval requires explicit permission.

---

## Reopen Restriction

Finalized payroll cannot be reopened.

---

# Permissions

```text
hr.payroll.view
hr.payroll.calculate
hr.payroll.approve
```

---

# API Endpoints

POST /api/hr/payroll-runs/{id}/submit-review

POST /api/hr/payroll-runs/{id}/approve

POST /api/hr/payroll-runs/{id}/reject

POST /api/hr/payroll-runs/{id}/reopen

POST /api/hr/payroll-runs/{id}/finalize

---

# Frontend

Payroll Run Detail

Actions:

- Submit for Review
- Approve
- Reject
- Reopen
- Finalize

UI must show available actions based on:

- Payroll Status
- User Permissions

---

# Future Enhancements

- Multi-level approval
- Department-level approval
- CFO approval
- Approval comments
- Approval history timeline