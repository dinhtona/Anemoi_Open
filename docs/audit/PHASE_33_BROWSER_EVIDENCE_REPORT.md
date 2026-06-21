# Phase 33 — Browser Evidence Report

**Date:** 2026-06-21
**Method:** Chrome DevTools MCP + Direct API Calls

---

## Evidence Collected

### 1. Payroll Run Detail (Finalized)

**Screenshot:** Dialog showing:
- Employee: Anemoi Admin (DEV-ADMIN-001)
- Status: Finalized
- Attendance: 22 paid working days, 0 paid leave days, 0 unpaid leave days
- Base Salary: ₫5,000,000
- Net Amount: ₫5,000,000
- **Generate Payslips button visible**
- **Payslip section:** Employee=Anemoi Admin, Net Pay=₫5,000,000, Status=Published
- **Batch PDF** and **Batch Email** buttons visible
- **Approval history:** Submitted → Approved → Finalized (by user 01000000-...)

**API Evidence:**
- Payroll runs confirmed: 5 runs across different employees
- Khoa Do: SubmittedForApproval → Approved (after approve action)
- An Pham: Calculated → SubmittedForApproval (after submit action)

### 2. Payroll Run Detail (Submitted For Approval)

**Screenshot:** Dialog showing:
- Employee: Khoa Do (DEV-HR-002)
- Status: Submitted For Approval
- **Approve button** visible
- **Reject button** visible
- **Cancel Run button** visible
- Attendance snapshot shown

### 3. Notification Center

**Evidence:**
- 23 total notifications loaded
- 8 Overtime category notifications
- 15 Environment category notifications
- **0 Leave notifications**
- **0 notifications with action buttons**
- SignalR connection established to `/hubs/notification`

### 4. Workflow Definitions Page

**Evidence:**
- 5 workflow definitions visible
- "Quy trình tuyển dụng" (Recruitment) — Active, 4 steps, Target=RecruitmentRequest
- "Leave Approval Workflow" — Active, 3 steps
- 3 other definitions in Inactive status

### 5. Candidate Creation

**API Evidence:**
- Candidate created: `{"id":"01000000-...","candidateCode":"E2E-CAND-001","fullName":"E2E Test Candidate",...}`

### 6. System Data Snapshot

| Entity | Count | Details |
|--------|:-----:|---------|
| Employees | 6 | Codes: DEV-ADMIN-001 through DEV-HR-002 |
| Departments | 4 | Engineering, People Operations, System, Nhân sự |
| Positions | 5 | ENG-MGR, SWE, HR-MGR, HR-SPEC, SYS-ADMIN |
| Attendance Periods | 5 | 2026-04 through 2026-08-AUDIT |
| Payroll Periods | 4 | 2026-01, 2026-04, 2026-06, 2026-07 |
| Payroll Runs | 5 | Various statuses: Finalized, Approved, Submitted, Calculated, Cancelled |
| Leave Requests | 5 | 2 Approved, 3 Cancelled (all for admin) |
| Overtime Requests | 12 | 6 Approved, 5 Pending, 1 other |
| Contracts | 4 | Active contracts for Admin, Minh, Linh |
| Workflow Instances | 2 | Pending status |
| Workflow Defs | 5 | 2 Active, 3 Inactive |
| Notifications | 23 | Overtime + Environment (no Leave) |
| Notification Audits | 0 | No action audit records |
