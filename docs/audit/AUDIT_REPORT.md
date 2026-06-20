# ANEMOI HR Platform — Full System Audit Report

**Date:** June 19, 2026  
**Scope:** 2,263 C# files + 263 TypeScript files across 16 modules  
**Method:** Source code review + API analysis + Database migration review + Frontend analysis + Browser E2E via Chrome DevTools  
**Build:** ❌ FAILS — 1 compilation error

---

## Executive Summary

| Metric | Value |
|--------|-------|
| **Overall Score** | **52 / 100** |
| **Production Readiness** | **FAIL** |
| **Total Findings** | **~240** |
| **S1 (Critical)** | **19** |
| **S2 (High)** | **45** |
| **S3 (Medium)** | **~85** |
| **S4 (Low)** | **~91** |
| **Build Status** | ❌ 1 error, 8 warnings |
| **Browser E2E** | Auth works; all 21 pages accessible |

**Verdict: FAIL** — The platform has 19 critical and 45 high-severity issues. The build is broken. Tax and insurance deductions are not integrated into payroll (net pay is always incorrect). 7 of 9 recruitment pages render empty tables. Security credentials are hardcoded in source control. The new workflow-based approval path breaks leave balance tracking. These issues individually block production deployment.

---

## Functional Audit Matrix

| Module | Status | Score | Findings |
|--------|--------|-------|----------|
| **Auth & Authorization** | ❌ FAIL | 30/100 | 29 findings: 8 S1, 7 S2 |
| **Employee Management** | ❌ FAIL | 40/100 | 34 findings: 4 S1, 12 S2 |
| **Leave Management** | ❌ FAIL | 45/100 | 21 findings: 2 S1, 6 S2 |
| **Overtime Management** | ❌ FAIL | 45/100 | 19 findings: 2 S1, 4 S2 |
| **Attendance** | ⚠️ PARTIAL | 50/100 | 18 findings: 4 S2, 9 S3 |
| **Payroll & Payslip** | ❌ FAIL | 35/100 | 10 findings: 2 S2 (net pay always wrong) |
| **Tax Engine** | ⚠️ PARTIAL | 65/100 | 7 findings (mostly S3-S4) |
| **Insurance Engine** | ⚠️ PARTIAL | 55/100 | 9 findings (3 S2 — no calculation UI) |
| **Recruitment** | ❌ FAIL | 25/100 | 17 findings: 3 S1, 3 S2 |
| **Onboarding** | ⚠️ PARTIAL | 60/100 | 28 findings (mostly S3-S4) |
| **Workflow Engine** | ⚠️ PARTIAL | 55/100 | 18 findings (3 S2) |
| **Notification** | ⚠️ PARTIAL | 50/100 | 13 findings: 3 S1 |
| **Organization** | ⚠️ PARTIAL | 55/100 | 7 findings |
| **ESS** | ⚠️ PARTIAL | 60/100 | 15 findings (2 S2) |
| **Compensation** | ⚠️ PARTIAL | 70/100 | Minor issues |
| **Dashboard** | ✅ GOOD | 85/100 | Minor issues |
| **Contract** | ⚠️ PARTIAL | 70/100 | Minor issues |
| **Shift Management** | ✅ GOOD | 85/100 | Minor issues |
| **Calendar Management** | ✅ GOOD | 85/100 | Minor issues |

---

## Critical Findings (19 S1)

### S1-1 — Hardcoded Super Admin Password in Source Control
- **Module:** Auth
- **File:** `Anemoi.Identity/Anemoi.Identity.WorkerService/appsettings.json:33`
- **Description:** Password `Admin@12345` committed in plaintext
- **Fix:** Remove from config; use user-secrets/env vars

### S1-2 — Hardcoded Dev User Passwords in Source Control
- **Module:** Auth
- **File:** `Anemoi.Identity/Anemoi.Identity.WorkerService/appsettings.Development.json:13-37`
- **Description:** 5 test user accounts all share password `Password1` in plaintext
- **Fix:** Generate random passwords at seed time

### S1-3 — Hardcoded Database Credentials
- **Module:** Auth
- **File:** `Anemoi.Identity/Anemoi.Identity.WorkerService/appsettings.json:10`
- **Description:** PostgreSQL connection string with `Password=SomePassWord` in plaintext
- **Fix:** Use environment variables

### S1-4 — Hardcoded RabbitMQ Default Credentials
- **Module:** Auth
- **File:** `Anemoi.Identity/Anemoi.Identity.WorkerService/appsettings.json:16-17`
- **Description:** `guest/guest` credentials in source control
- **Fix:** Use strong credentials via env vars

### S1-5 — No Email Confirmation Requirement
- **Module:** Auth
- **File:** `Anemoi.Identity/Anemoi.Identity.Infrastructure/Installers/IdentityInstaller.cs:22-23`
- **Description:** Email and phone confirmation requirements commented out. Users activated immediately.
- **Fix:** Enable email confirmation, implement verification flow

### S1-6 — Lockout Disabled for External Auth Accounts
- **Module:** Auth
- **File:** `Anemoi.Identity/Anemoi.Identity.Application/Cqrs/Commands/RefreshTokenCommands/ExternalLogin/ExternalLoginHandler.cs:138`
- **Description:** `LockoutEnabled = false` for auto-provisioned external auth users
- **Fix:** Enable lockout by default

### S1-7 — Arbitrary Token Minting Without Authentication
- **Module:** Auth
- **File:** `Anemoi.Identity/Anemoi.Identity.Application/Cqrs/Commands/IdentityCommands/GenerateCustomToken/GenerateCustomTokenHandler.cs:27-33`
- **Description:** Generates signed JWTs with arbitrary claims from user input without authentication
- **Fix:** Add authorization gate; restrict claim types to allowlist

### S1-8 — Password Rehash Rejection Blocks Login
- **Module:** Auth
- **File:** `Anemoi.Identity/Anemoi.Identity.Application/Cqrs/Commands/RefreshTokenCommands/UserLogin/UserLoginHandler.cs:66-67`
- **Description:** `SuccessRehashNeeded` returns error instead of updating hash. Prevents password algorithm upgrades.
- **Fix:** Update password hash when rehash is needed

### S1-9 — GetMyProfile Missing Permission Check
- **Module:** Employee
- **File:** `Anemoi.Hr/Anemoi.Hr.Api/Controllers/Employee/EmployeeController.cs:57-65`
- **Description:** `GetMyProfile` endpoint has no `[HasPermission]` attribute
- **Fix:** Add `[HasPermission(HrPermissions.EssProfileView)]`

### S1-10 — LinkEmployeesToIdentityUsers Loads All Employees Into Memory
- **Module:** Employee
- **File:** `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/LinkEmployeesToIdentityUsers/LinkEmployeesToIdentityUsersHandler.cs:33-35`
- **Description:** `.ToListAsync()` without pagination loads entire employee table
- **Fix:** Process in batches; filter before materializing

### S1-11 — No Duplicate Email/EmployeeCode Validation
- **Module:** Employee
- **File:** (No validation exists)
- **Description:** No unique constraint checking for `WorkEmail` or `EmployeeCode`
- **Fix:** Add unique constraints + validation

### S1-12 — Workflow-Based Leave Approval Breaks Leave Balance
- **Module:** Leave
- **File:** `Anemoi.Hr/Anemoi.Hr.Application/WorkflowTargetStatusUpdaters/LeaveWorkflowStatusUpdater.cs:17-33`
- **Description:** Status updater changes status but never updates `LeaveBalance` (PendingDays, UsedDays, RemainingDays)
- **Fix:** Inject balance repositories and replicate old handler logic

### S1-13 — HR-Submitted Leave Requests Have Empty Approver
- **Module:** Leave
- **File:** `Anemoi.Hr/Anemoi.Hr.Application/Mappings/LeaveMapper.cs:39`
- **Description:** `ApproverEmployeeId = new EmployeeId(Guid.Empty)` — no approver set for HR-initiated leaves
- **Fix:** Add `ApproverEmployeeId` to `SubmitLeaveRequestCommand`

### S1-14 — Overtime Not Flowing Into Payroll
- **Module:** Overtime/Payroll
- **File:** `Anemoi.Hr/Anemoi.Hr.Application/Abstractions/IOvertimeSnapshotProvider.cs`
- **Description:** `IOvertimeSnapshotProvider` exists but is never consumed by payroll calculation
- **Fix:** Wire into `CalculatePayrollRunHandler`

### S1-15 — ESS Overtime Bypasses Workflow Engine
- **Module:** Overtime
- **File:** `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/EssCommands/SubmitMyOvertimeRequest/SubmitMyOvertimeRequestHandler.cs:82-85`
- **Description:** No `workflowEngine.StartAsync()` called for ESS-submitted overtime
- **Fix:** Add workflow engine call matching HR create handler

### S1-16 — 7 of 9 Recruitment Pages Display Empty Tables
- **Module:** Recruitment
- **File:** Multiple frontend pages in `cody-web-app/src/app/[locale]/(dashboard)/hr/recruitment/`
- **Description:** Candidates, Applications, Interviews, Hiring Decisions, Postings, Requisitions pages show empty tables with no data fetching
- **Fix:** Implement search hooks and wire to pages

### S1-17 — `useRequisitions()` Calls Wrong Endpoint With Empty String
- **Module:** Recruitment
- **File:** `cody-web-app/src/hooks/hr/useRecruitment.ts:37-43`
- **Description:** Hook calls `getRequisitionById("")` instead of the search endpoint; also `enabled: false`
- **Fix:** Replace with proper search hook

### S1-18 — CreateOfferDecisionHandler Has Wrong Stage Check
- **Module:** Recruitment
- **File:** `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/RecruitmentCommands/CreateOfferDecision/CreateOfferDecisionHandler.cs:38-41`
- **Description:** Checks for `Offer` stage but should check `Interview` stage; doesn't advance to `Offer`
- **Fix:** Fix stage check and add `MoveToStage`

### S1-19 — Notification Recruitment Recipient Resolution Mismatch
- **Module:** Notification
- **File:** `Anemoi.Notification/Anemoi.Notification.Application/Consumers/RecruitmentRequestConsumers.cs:37-43`
- **Description:** `ApproverUserId` field name implies UserId but it's passed to `ResolveApproverUserIdByEmployeeId()` which expects EmployeeId. Wrong user may be notified or notification silently dropped.
- **Fix:** Correct the field name or resolution logic

---

## Cross-Cutting Risks

### Security
1. **Hardcoded credentials in 3 config files** (S1-1 through S1-4)
2. **Arbitrary JWT token minting** via `GenerateCustomToken` (S1-7)
3. **No email confirmation** — anyone can register with any email (S1-5)
4. **External auth bypasses lockout** (S1-6)
5. **No token revocation on HR service** (S2)
6. **Workspace claim never validated** (S2)
7. **Weak password policy** (S2)

### Data Integrity
1. **Leave balance not updated via workflow approval** (S1-12)
2. **Payroll net pay always wrong** — tax/insurance deductions hardcoded to 0 (S2)
3. **Attendance PaidWorkingDays incorrectly calculated** (S2)
4. **Missing attendee unlock/recalculation** (S3)
5. **Overtime not in payroll calculation** (S1-14)

### Missing Features
1. **7/9 recruitment pages are empty shells** (S1-16)
2. **No employee CRUD** — only ConvertCandidateToEmployee (S2)
3. **No profile edit for ESS users** (S2)
4. **No insurance calculation UI** (S2)
5. **No batch payslip generation** (S3)

### Build & Quality
1. **Build broken** — test file missing parameter (S2)
2. **8 missing FluentValidation validators** in Payroll (S3)
3. **All GET endpoints lack OneOf error handling** across multiple controllers (S3)

### Architecture
1. **Domain entities inherit ValueObject instead of Entity** — across all modules (S3)
2. **No workflow→saga integration for onboarding** (S3)
3. **Role/Permission approvers never see pending approvals** (S2 in Workflow)
4. **Shared employee resolution code duplicated across 12 ESS handlers** (S4)

---

## Regression Risks

| Risk | From Module | To Module | Severity |
|------|-------------|-----------|----------|
| Workflow approval path (new) vs direct approval (old) conflict | Workflow/Leave/Overtime/Payroll | All with workflows | **HIGH** — dual approval path can cause errors |
| Tax/Insurance calculation disconnected from payroll | Tax/Insurance | Payroll | **HIGH** — net pay always wrong |
| Overtime snapshot not consumed by payroll | Overtime | Payroll | **HIGH** — overtime pay missing |
| Attendance summary calculator returns wrong values | Attendance | Payroll | **HIGH** — base pay wrong for absent/leave employees |
| Leave approval doesn't create attendance records | Leave | Attendance | **MEDIUM** — manual entry only |
| Manager change doesn't publish integration event | Organization | All | **MEDIUM** — stale data |
| No cascade delete/soft delete on employee | Employee | All | **MEDIUM** — data integrity risk |
| `ValueObject` inheritance vs `Entity` across modules | All | All | **LOW** — tracking issues in generic repos |

---

## Unimplemented Features

### Empty Screens / No Data
- **Recruitment Candidates** — `hr/recruitment/candidates/` — empty table
- **Recruitment Applications** — `hr/recruitment/applications/` — empty table
- **Recruitment Interviews** — `hr/recruitment/interviews/` — empty table
- **Recruitment Hiring Decisions** — `hr/recruitment/hiring-decisions/` — empty table
- **Recruitment Job Postings** — `hr/recruitment/postings/` — empty table
- **Recruitment Requisitions** — `hr/recruitment/requisitions/` — empty table

### Stub API / Missing Features
- **Insurance calculation** — no frontend calculate button (`useCalculateInsurance` hook missing)
- **ESS payslip download** — no download button for employees
- **ESS profile edit** — read-only profile page
- **Employee create/update/delete** — only `ConvertCandidateToEmployee` exists
- **Unlock attendance period** — no unlock endpoint exists
- **Batch payslip generation** — generates one payslip at a time
- **Template deletion** — no DELETE endpoint for onboarding templates or tax/insurance rule sets
- **Carry-forward leave balance** — carry-forward logic not implemented
- **Forgot password / Register** — no links on login page

### Dead Code
- `LeaveRequestStatusCode` frontend type has `ForceApproved`/`ForceCancelled` — not in backend
- `AttendancePeriodStatusCode` frontend has `'Calculated'` — not in backend
- `Draft` status for `OnboardingInstance` — defined but never used
- `QuickDeductionAmount` in TaxBracket — stored but never applied in calculation
- `WorkflowStepStatusCode.Skipped` — defined but never assigned
- `HrOnboardingInstanceForceCompleteRequiresReason` error code — defined but never used
- `EmployeeManagerHistory` entity — dead code, never populated
- `OvertimeRequest.Create().createdBy` parameter — never stored

### TODO/FIXME Items Found
- `employees/page.tsx:436` — `{/*TODO: Implement pagination controls */}` (controls exist but comment suggests incompleteness)

---

## Final Verdict

### ❌ FAIL

**Reasons:**

1. **19 Critical (S1) issues** — including hardcoded credentials in source control, arbitrary JWT minting, payroll net pay always wrong, leave balance corruption via workflow, and 7 empty recruitment pages
2. **45 High (S2) issues** — including broken payroll tax integration, incorrect attendance summary calculation, missing employee CRUD, no insurance calculation UI, and missing permission mappings
3. **Build is broken** — 1 compilation error in test project prevents CI/CD
4. **Dual approval paths** — old direct approval (marked `[Obsolete]`) and new workflow-based approval coexist; workflow path breaks leave balance
5. **Inconsistent domain modeling** — entities inherit `ValueObject` instead of `Entity` across all modules, violating DDD principles
6. **Payroll produces incorrect results** — tax and insurance deductions are hardcoded to zero; paid/unpaid leave days are hardcoded to zero in attendance summary
7. **Recruitment module is 70% empty** — 7 of 9 frontend pages display empty tables with no data fetching

**Recommended actions before production deployment:**

1. Remove hardcoded credentials from source control
2. Fix `GenerateCustomTokenHandler` authorization
3. Integrate tax/insurance into payroll calculation
4. Fix `LeaveWorkflowStatusUpdater` to update leave balances
5. Implement recruitment frontend data fetching
6. Fix build error in test project
7. Unify approval paths (remove obsolete commands or fix workflow)
8. Fix attendance summary calculator for paid/unpaid leave classification
9. Add employee CRUD endpoints
10. Enable email confirmation for registration
