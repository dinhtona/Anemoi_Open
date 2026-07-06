# Phase 32 — Architecture Review Report

## 1. Workflow Engine Adoption Review

### Summary

| Module | Workflow Used | WorkflowTargetStatusUpdater | Default Policy Steps | Status |
|--------|:---:|:---:|:---:|:---:|
| Leave | ✓ | LeaveWorkflowStatusUpdater | 2 (org hierarchy) | Complete |
| Overtime | ✓ | OvertimeWorkflowStatusUpdater | 2 (org hierarchy) | Complete |
| Payroll | ✓ | PayrollWorkflowStatusUpdater | 0 (definition required) | Complete |
| Recruitment Request | ✓ | RecruitmentWorkflowStatusUpdater | 0 (definition required) | Complete |
| Onboarding | ✗ | — | — | Self-contained lifecycle |
| Contract | ✗ | — | — | Simple lifecycle (ValueObject) |
| Promotion | ✗ | — | — | Direct action, no approval state |
| Transfer | ✗ | — | — | Direct action, no approval state |

### Findings

- **4/8 modules** integrate with the workflow engine (Leave, Overtime, Payroll, Recruitment Request).
- **Contract, Onboarding, Promotion, Transfer** do not use the workflow engine — justified by their simple/direct lifecycle patterns.
- **JobRequisition** has an independent Draft→Submitted→Approved status flow that bypasses the workflow engine (hard-coded status transitions in the controller).
- **RecruitmentRequest** is the only recruitment entity with workflow integration; other entities (Candidate, CandidateApplication, JobPosting, InterviewSchedule, HiringDecision) use independent stage transitions.
- All 4 existing `WorkflowTargetStatusUpdater` implementations handle their entity types correctly.

### Recommendations
- JobRequisition should be evaluated for workflow engine adoption in a future phase.
- No urgent workflow gaps identified.

---

## 2. Permission Audit

### Coverage

| Project | Controllers | Actions | HasPermission | Missing |
|---------|:---:|:---:|:---:|:---:|
| Anemoi.Hr.Api | 40 | ~190 | 100% | 0 |
| Anemoi.Centralize.Api | ~15 | ~60 | ~45 (with [Authorize]) | 9 |

### Findings

- **All 40 HR controllers** have `[HasPermission]` on every action — **100% coverage**.
- 9 Centralize controllers use `[Authorize]` or `[Authorize(Policy=...)]` instead of `[HasPermission]` — acceptable for non-HR operations.
- All permission constants referenced in controllers exist in `HrPermissions.cs`.
- 4 unused constants: `PositionChange`, `GradeChange`, `PayrollExport`, `WorkflowExecute` (defined but not referenced in any endpoint).
- 17 permissions are raw string literals in `HrPermissions.cs` (not registered in BuildingBlocks `Permissions.Definitions`) — functional but not discoverable via centralized permission UI.

---

## 3. Seed Data Consistency

| Check | Status |
|-------|--------|
| Employee ↔ User mapping via WorkEmail | Employee `IdentityUserId` may be null if `LinkEmployeesToIdentityUsers` was not called |
| Manager hierarchy intact | ✓ — Linh Nguyen manages 2 engineers; Mai Le manages 2 staff |
| Department references valid | ✓ |
| Position references valid | ✓ |
| Leave balances for all employees | ✓ — 6 employees with 15 days each |

### Finding
- The `LinkEmployeesToIdentityUsers` API must be called after seeding for ESS workflows to function properly (employee identity linking).

---

## 4. Frontend Route Audit

| Route Group | Pages | Load OK | Console Errors | Network Errors |
|:---|:---:|:---:|:---:|:---:|
| /hr/* | 39 | 39/39 | 0 (after fixes) | 0 (after fixes) |
| /ess/* | 8 | 8/8 | 0 | 0 |

### Findings Before Fixes
- Recruitment search endpoints returned 400 (non-nullable query params).
- Onboarding my-tasks endpoint returned 400 (non-nullable UserId).
- Recruitment requests page had missing i18n translations.
- Insurance reports page has a minor Select.Item warning (non-blocking).

---

## 5. Build & Test Results

| Check | Result |
|-------|--------|
| `dotnet build Anemoi.sln` | 0 errors, 39 warnings (pre-existing) |
| `npm run build` (cody-web-app) | 0 errors |
| `dotnet test` | 486/486 passed (0 failures) |

---

## 6. Technical Debt Observations

- **Pre-existing warnings**: 39 CS0618 deprecation warnings for obsolete `ApproveLeaveRequestCommand`/`RejectLeaveRequestCommand` etc. (should use workflow commands instead). Addressed in earlier phases but not fully migrated.
- **SearchCandidateApplicationsHandler** does not use `SearchTerm` in its filter query.
- **Seed data** uses `Guid.CreateVersion7()` for LeaveBalance IDs instead of `IdGenerator.NextGuid()`.
