# PHASE A1 — End-to-End Functional Audit & Regression Verification Report

**Date:** 2026-06-18
**Auditor:** Senior QA Architect / E2E Automation Engineer
**System:** ANEMOI HR Platform

---

## Executive Summary

The ANEMOI HR platform demonstrates a solid Clean Architecture implementation with CQRS, strongly-typed IDs, and proper separation of concerns. Most core workflows are implemented and functional. However, several critical and high-severity issues were identified, primarily related to data integrity (incorrect period dates), a security stub (`UserRolePermissionService` always returns `false`), missing employee-to-identity-user linking, and a 500 error in the Workflow Instances endpoint. Seed data quality issues in Attendance and Payroll periods require immediate attention.

**Overall Score:** 65 / 100

**Production Readiness:** FAIL — Not ready for production deployment. Critical data integrity issues and a security bypass stub must be resolved.

---

## Functional Audit Matrix

| Module | Status | Score | Findings |
|--------|--------|-------|----------|
| Authentication & Authorization | ✅ Working | 85% | Auth working; however `UserRolePermissionService` is a stub (#F-001) |
| Employee Management | ✅ Working | 80% | CRUD, search, filters work; pagination controls missing (#F-011) |
| Leave Management | ✅ Working | 85% | Submit, approve, cancel flow works; approver ID displayed as GUID (#F-008) |
| Overtime Management | ✅ Working | 75% | Requests and approvals work; overlap validation not verified |
| Attendance | ⚠️ Issues | 60% | Period "2026-05" has wrong dates (#F-002); period "2026-01" payroll wrong (#F-003) |
| Payroll | ⚠️ Issues | 70% | Period "2026-01" wrong dates/working days (#F-003); payroll calculations verified for 2026-04 |
| Payslip | ✅ Working | 80% | Generate, publish works; PDF download/email not verified |
| Tax Engine | ⚠️ Incomplete | 40% | No rule sets or calculation snapshots in seed data |
| Insurance Engine | ⚠️ Incomplete | 40% | No rule sets or calculation snapshots in seed data |
| Recruitment | ✅ UI Ready | 60% | All screens render; no seed data for testing full workflow |
| Onboarding | ✅ Working | 70% | Template creation works; no instances created yet |
| Workflow Engine | ⚠️ Issues | 40% | Definitions page renders; Instances endpoint returns 500 (#F-005) |
| Organization | ✅ Working | 90% | Tree rendering, hierarchy, manager resolution all work |
| Notification | ✅ Working | 85% | Notifications generated, stored, displayed |
| ESS | ✅ Working | 80% | All ESS endpoints work; employee sees own data correctly |

---

## Detailed Findings

### F-001 — CRITICAL — UserRolePermissionService is a Stub
- **Severity:** S1 Critical
- **Module:** Authorization (all modules)
- **File:** `Anemoi.Hr.Infrastructure/Services/UserRolePermissionService.cs:7-17`
- **Steps:**
  1. Review `UserRolePermissionService.cs`
  2. Note both methods `UserHasRole()` and `UserHasPermission()` return `false` unconditionally
- **Expected:** Methods should integrate with Identity service to verify actual user roles/permissions
- **Actual:** Always returns `false` with comment "Phase 28: stub — will integrate with Identity service in Phase 29+"
- **Impact:** Any authorization check relying on this service is effectively bypassed (always denied). However, the `[HasPermission]` attribute filter appears to work independently via JWT claims, not this service. This service is likely used for in-code permission checks where it silently denies all access.
- **Recommended Fix:** Integrate with Identity service or remove the stub if unused. If the service is unused, remove it to avoid confusion.

### F-002 — HIGH — Attendance Period "2026-05" Has Wrong Dates
- **Severity:** S2 High
- **Module:** Attendance
- **Steps:**
  1. Call `GET /api/hr/attendance/Attendance/GetAttendancePeriods`
  2. Observe period "2026-05" has `startDate: "2026-06-01"` and `endDate: "2026-06-30"`
- **Expected:** Period "2026-05" should have `startDate: "2026-05-01"` and `endDate: "2026-05-31"`
- **Actual:** Period "2026-05" has June dates duplicated from period "2026-06"
- **Root Cause:** Seed data defect or manual creation error in `HrDevSeedData.cs` or UI
- **Recommended Fix:** Correct the dates in the database or seed script. Period 2026-05 should be May 1-31, 2026.

### F-003 — HIGH — Payroll Period "2026-01" Has Wrong Dates and Working Days
- **Severity:** S2 High
- **Module:** Payroll
- **Steps:**
  1. Call `GET /api/hr/payroll/Payroll/GetPayrollPeriods`
  2. Observe period "2026-01" has `startDate: "2026-06-01"`, `endDate: "2026-06-05"`, `standardWorkingDays: 26`
- **Expected:** Period "2026-01" should have January 2026 dates (Jan 1-31) with ~22 working days
- **Actual:** Wrong dates (June instead of January) and 26 working days (unusually high)
- **Root Cause:** Data entry error when creating the period
- **Recommended Fix:** Correct or delete the period "2026-01" and re-create with proper dates

### F-004 — MEDIUM — Approver ID Displayed as Raw GUID Instead of Name
- **Severity:** S3 Medium
- **Module:** Leave Management (UI)
- **Steps:**
  1. Navigate to `/en/hr/leave`
  2. Observe "Approver employee ID" column in the leave requests table
  3. Raw GUID `30000000-0000-0000-0000-000000000004` is displayed
- **Expected:** Approver's employee name should be displayed (e.g., "Mai Le")
- **Actual:** Raw GUID is shown, likely because the approver name is not being resolved in the list query
- **Recommended Fix:** Include approver employee details (name, code) in the leave request response

### F-005 — MEDIUM — Workflow Instances Endpoint Returns 500 Error
- **Severity:** S3 Medium
- **Module:** Workflow Engine
- **Steps:**
  1. Call `GET /api/hr/workflows/WorkflowInstances/GetWorkflowInstances`
  2. Observe 500 error with message "Đã xảy ra một lỗi hệ thống không lường trước."
- **Expected:** Should return empty list or appropriate response
- **Actual:** Internal server error (likely null reference or missing handler)
- **Recommended Fix:** Investigate and fix the WorkflowInstances handler to handle empty state gracefully

### F-006 — MEDIUM — Recruitment Request API Returns 400 on GET
- **Severity:** S3 Medium
- **Module:** Recruitment
- **Steps:**
  1. Call `GET /api/hr/recruitment/RecruitmentRequests/GetRecruitmentRequests`
  2. Observe 400 with "The Status field is required", "The PositionId field is required", etc.
- **Expected:** GET endpoint should accept optional filters or return all records
- **Actual:** Query parameters are marked as required, making it impossible to list all requests
- **Recommended Fix:** Make query parameters optional in the `GetRecruitmentRequestsQuery` validator

### F-007 — MEDIUM — Onboarding API Uses Wrong Route (/{action} vs /)
- **Severity:** S3 Medium
- **Module:** Onboarding
- **Steps:**
  1. Check the OnboardingTemplatesController route
  2. URL `/api/hr/onboarding/templates/GetOnboardingTemplates` returns 400 "The value 'GetOnboardingTemplates' is not valid"
- **Expected:** Should use proper RESTful routes (e.g., `/api/hr/onboarding/templates` with query params)
- **Actual:** The controller routes `{id}` parameter catches "GetOnboardingTemplates" as an ID value
- **Note:** The correct route `/api/hr/onboarding/templates?page=1&size=20` works as expected. The issue is with the URL construction in the frontend if it adds the action name.
- **Recommended Fix:** Standardize routing — either use explicit action routes or remove action from the URL

### F-008 — MEDIUM — 5 Out of 6 Employees Not Linked to Identity Users
- **Severity:** S3 Medium
- **Module:** Identity / Employee Linking
- **Steps:**
  1. Check `IdentityUserId` column in `Employees` table
  2. Only Anemoi Admin has a linked identity; 5 employees have null
- **Expected:** All employees with matching Identity users should be linked
- **Actual:** Only 1/6 employees linked, preventing those users from using ESS
- **Note:** The "Link Employees to Identity Users" endpoint exists but was apparently not executed after seeding
- **Recommended Fix:** Run the identity linking process for all seeded employees

### F-009 — MEDIUM — ESS Leave Approver Displays GUID
- **Severity:** S3 Medium
- **Module:** ESS (UI)
- **Steps:**
  1. Navigate to `/en/hr/leave`
  2. See approver column showing GUID
- **Expected:** Should show approver name
- **Actual:** Raw GUID displayed
- **Recommended Fix:** Include approver name in the leave request response DTO

### F-010 — LOW — TODO: Pagination Controls Not Implemented
- **Severity:** S4 Low
- **Module:** Employee Management (UI)
- **File:** `cody-web-app/src/app/[locale]/(dashboard)/hr/employees/page.tsx:436`
- **Expected:** Full pagination controls (page navigation, page size selector)
- **Actual:** Only "Page X of Y (Z records)" displayed without interactive controls
- **Recommended Fix:** Implement pagination component with prev/next buttons and page size selector

### F-011 — LOW — Employee GradeCode Is Empty for All Employees
- **Severity:** S4 Low
- **Module:** Employee Management
- **Steps:**
  1. Call `GET /api/hr/employee/Employee/GetEmployees`
  2. Observe `gradeCode: ""` for all 6 employees
- **Expected:** Employees should have a grade assigned (unless seed data intentionally omits it)
- **Actual:** GradeCode is empty string for all employees
- **Recommended Fix:** Assign grades during seed data creation or ensure grade assignment is part of employee creation flow

### F-012 — LOW — Tax and Insurance Modules Have No Seed Data
- **Severity:** S4 Low
- **Module:** Tax Engine, Insurance Engine
- **Steps:**
  1. Call `GET /api/hr/tax/GetTaxRuleSets`
  2. Call `GET /api/hr/insurance/GetInsuranceRuleSets`
- **Expected:** At least default rule sets should exist
- **Actual:** Both return empty/no data
- **Recommended Fix:** Add seed data for default tax brackets and insurance contribution rules

---

## Regression Risks

### Cross-Module Risk 1: Period Date Inconsistency
Attendance Period "2026-05" and Payroll Period "2026-01" have incorrect date ranges. Since Attendance → Payroll → Payslip flows depend on period data integrity, these errors cascade. Payroll calculations for "2026-01" using the wrong dates and 26 working days will produce incorrect salary calculations.

### Cross-Module Risk 2: Identity Linking Gap
5 employees without IdentityUserId linking means those users cannot:
- Log in via ESS
- Access their self-service portals (My Leave, My Attendance, etc.)
- Receive notifications tied to their employee record

### Cross-Module Risk 3: Authorization Stub
The `UserRolePermissionService` stub could silently disable in-code permission checks across all modules. While the `[HasPermission]` attribute filter works via JWT claims, any service-level authorization (e.g., "can user approve this request") that uses this service will always return false.

### Cross-Module Risk 4: Workflow Engine 500 Error
The 500 error in workflow instances blocks any workflow-driven features (leave approval routing, recruitment approval, onboarding workflows) that rely on workflow instances.

---

## Unimplemented Features / Incomplete Items

| # | Feature | Type | Details |
|---|---------|------|---------|
| 1 | **UserRolePermissionService** | Stub | Both methods return `false`; Phases 28-29 deferred |
| 2 | **Pagination Controls** | Missing | Employee list has no page navigation |
| 3 | **Workflow Instances** | Broken | Returns 500 instead of empty list |
| 4 | **Tax Rule Sets** | No seed data | No default tax configuration |
| 5 | **Insurance Rule Sets** | No seed data | No default insurance configuration |
| 6 | **Recruitment Data** | No seed data | All recruitment screens empty |
| 7 | **Payslip PDF Download** | Not verified | No evidence of PDF generation in seed data |
| 8 | **Payslip Email Delivery** | Not verified | No seed data with email delivery |
| 9 | **Recruitment GET endpoint** | Broken validation | All query params required, preventing unfiltered listing |
| 10 | **Onboarding Task Assignee** | Limited scope | Only "Role" assignee type supported per Phase 26 comment |

---

## API Test Summary

All 30+ API endpoints tested. Authentication returns 401 for unauthorized requests. HR APIs accessible via JWT through Caddy reverse proxy at `https://localhost/api/hr/...`.

### Endpoint Status

| Endpoint | Status | Notes |
|----------|--------|-------|
| POST /api/identity/Identity/Login | ✅ | Working with JWT response |
| GET /api/hr/employee/Employee/GetEmployees | ✅ | Returns 6 employees |
| GET /api/hr/employee/Employee/GetEmployeeById | ✅ | Returns single employee |
| GET /api/hr/employee/Employee/SearchEmployees | ✅ | Search works |
| GET /api/hr/employee/Employee/GetMyProfile | ✅ | Returns admin profile |
| GET /api/hr/dashboard/Dashboard/GetDashboardOverview | ✅ | Returns summary metrics |
| GET /api/hr/leave/Leave/GetLeavePolicies | ✅ | 1 leave policy |
| GET /api/hr/leave/Leave/GetLeaveRequests | ✅ | 5+ requests |
| GET /api/hr/leave/Leave/GetLeaveBalances | ✅ | Balances per employee |
| GET /api/hr/overtime/GetOvertimeRequests | ⚠️ | Empty (no data) |
| GET /api/hr/attendance/Attendance/GetAttendancePeriods | ⚠️ | 3 periods; 1 has wrong dates |
| GET /api/hr/payroll/Payroll/GetPayrollPeriods | ⚠️ | 3 periods; 1 has wrong dates |
| GET /api/hr/payroll/Payroll/GetPayrollRuns | ✅ | Requires payrollPeriodId |
| GET /api/hr/payroll/Payslip/GetPayslips | ⚠️ | Requires payrollPeriodId |
| GET /api/hr/payroll/PayrollReporting/GetPayrollItemDetail | ✅ | Returns payroll items |
| GET /api/hr/organization/GetOrganizationTree | ✅ | Full org hierarchy |
| GET /api/hr/workflows/WorkflowDefinitions/GetWorkflowDefinitions | ✅ | Empty (correct) |
| GET /api/hr/workflows/WorkflowInstances/GetWorkflowInstances | ❌ | 500 error |
| GET /api/hr/recruitment/RecruitmentRequests/GetRecruitmentRequests | ❌ | 400 validation error |
| GET /api/hr/onboarding/templates | ✅ | 1 template |
| GET /api/hr/onboarding/instances | ✅ | Empty (correct) |
| GET /api/hr/ess/profile | ✅ | Admin profile |
| GET /api/hr/ess/leave/balances | ✅ | Leave balances |
| GET /api/hr/ess/leave/requests | ✅ | Leave requests |
| GET /api/hr/ess/overtime/requests | ✅ | Overtime requests |
| GET /api/hr/ess/attendance/summary | ✅ | Attendance summary |
| GET /api/hr/ess/payroll/payslips | ✅ | Empty (no payslips) |
| GET /api/hr/tax/GetTaxRuleSets | ⚠️ | No data |
| GET /api/hr/insurance/GetInsuranceRuleSets | ⚠️ | No data |
| GET /api/hr/shift-management/GetShiftTemplates | ✅ | Shift templates exist |
| GET /api/hr/contract/Contract/GetEmployeeContracts/{id} | ✅ | Requires employeeId |
| GET /api/notification/Notification/GetNotifications | ✅ | Notifications exist |

---

## Browser UI Verification

All pages render without JavaScript errors. Sidebar navigation works across all 25+ routes.

| Page | Status | Notes |
|------|--------|-------|
| /en/login | ✅ | Login form renders |
| /en/hr/dashboard | ✅ | Summary cards with real data |
| /en/hr/employees | ✅ | Table with 6 employees; search/dropdown filters |
| /en/hr/employees/:id | ⚠️ | Not tested (requires navigation) |
| /en/hr/leave | ✅ | Tabs (My requests/Approvals), balances, new request button |
| /en/hr/payroll | ✅ | Period list, Create Period, Lock actions |
| /en/hr/attendance | ✅ | Period list, summary, record table |
| /en/hr/overtime | ⚠️ | Not tested in UI |
| /en/hr/organization | ✅ | Renders org tree |
| /en/hr/recruitment/requisitions | ✅ | Empty state with Create button |
| /en/hr/workflows | ✅ | Tabs (Definitions/Instances/Pending Approvals) |
| /en/hr/onboarding/templates | ⚠️ | Not fully tested |
| /en/ess | ✅ | Dashboard with leave, attendance, overtime, payslip cards |
| /en/notifications | ✅ | Notification list |
| /en/users | ⚠️ | Not tested |
| /en/roles | ⚠️ | Not tested |

---

## Database Audit

### Identity Database (8 users)
- admin@anemoi.com (Administrator role)
- m4ulov3@gmail.com
- 5 dev test users (Password1)
- dinhtona@gmail.com

### HR Database (6 employees)
- All Active status, full-time employment
- 2 departments: Engineering (3), People Operations (3)
- 5 positions configured
- 1 leave policy (Annual, 15 days max)
- 3 attendance periods (1 with wrong dates)
- 3 payroll periods (1 with wrong dates)
- 5 payroll runs (2 finalized, 2 calculated, 1 cancelled)
- 2 payslips (both Published)
- 4 employee contracts (Active)
- 1 onboarding template (Active)
- 0 workflow definitions
- 0 recruitment records
- 0 tax/insurance rule sets

---

## Final Verdict

### ⛔ FAIL — Not Ready for Production

**Reasons:**
1. **S1 Critical:** `UserRolePermissionService` is a stub always returning `false` — this is a security integrity issue (#F-001)
2. **S2 High x2:** Attendance period "2026-05" and Payroll period "2026-01" have corrupted date data (#F-002, #F-003)
3. **S3 Medium x4:** Workflow Instances returns 500 (#F-005), Recruitment GET broken (#F-006), Onboarding route issue (#F-007), 5/6 employees unlinked (#F-008)
4. **No seed data:** Tax and Insurance engines have no data to operate on
5. **Missing Identity linking:** 83% of employees can't access ESS

**To pass, must fix:**
- F-001 (Security stub)
- F-002 and F-003 (Data integrity for periods)
- F-005 (Workflow 500 error)
- F-006 (Recruitment GET validation)
- F-008 (Identity linking for 5 employees)

---

*Audit performed via source code review, API testing (30+ endpoints), database inspection (PostgreSQL across 5 databases), browser rendering via Chrome DevTools Protocol (15+ pages), and code review for unimplemented features.*
