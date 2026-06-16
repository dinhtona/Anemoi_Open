# ANEMOI HR - Technical Debt Register

IMPORTANT

Before working on any technical debt item, read:

- docs/architecture/ARCHITECTURE_DECISIONS.md

Architectural decisions take precedence over technical debt recommendations.

# ANEMOI HR - Technical Debt Register

Version: After Phase 19 Approval

Status: Active

Last Updated: 2026-06-15

---

# Purpose

This document tracks known architectural, engineering, quality, and maintainability debt discovered during implementation and review of ANEMOI HR Phases 01–19.

Only unresolved debt should remain in this document.

Items that have already been fixed should be moved to the Historical Debt Resolution section.

---

# Priority Matrix

| Priority | Meaning                                                |
| -------- | ------------------------------------------------------ |
| P1       | High risk to architecture or long-term maintainability |
| P2       | Important quality or testing gap                       |
| P3       | Productivity or platform improvement                   |
| P4       | User experience or operational improvement             |
| P5       | Future optimization                                    |

---

# Active Technical Debt

---

## TD-001 — Domain Depends on Application Layer

### Priority

P1

### Severity

High

### Status

✅ **Resolved** — Sep 2026

### Fix Applied

1. **Removed `Anemoi.BuildingBlock.Application` dependency** from `Anemoi.Hr.Domain.csproj`.
2. **Domain factories now accept IDs externally** — 10 entity factory methods were refactored to take a strongly-typed ID as first parameter instead of calling `Guid.NewGuid()`.
3. **Application layer generates IDs** — All call sites use `IdGenerator.NextGuid()` (MassTransit sequential GUID) and pass the typed ID to the domain factory.

### Current Pattern

```txt
Application
  → IdGenerator.NextGuid()          (sequential GUID via MassTransit)
  → new XxxId(guid)                 (strongly-typed ID wrapper)
  → Domain.Factory(id, ...)         (entity creation, no ID generation)
```

### Entities Refactored

| Entity | Factory | ID Param |
|--------|---------|----------|
| ShiftTemplate | `Create(id, code, name, ...)` | `ShiftTemplateId` |
| EmployeeShiftAssignment | `Create(id, employeeId, template, ...)` | `EmployeeShiftAssignmentId` |
| CalendarException | `Create(id, exceptionDate, ...)` | `CalendarExceptionId` |
| WorkingCalendarRule | `Create(id, name, ...)` | `WorkingCalendarRuleId` |
| CompanyHoliday | `Create(id, holidayDate, ...)` | `CompanyHolidayId` |
| PublicHoliday | `Create(id, holidayDate, ...)` | `PublicHolidayId` |
| OvertimeRequest | `Create(id, employeeId, ...)` | `OvertimeRequestId` |
| PayslipEmailDelivery | `Create(id, payslipId, ...)` | `PayslipEmailDeliveryId` |
| PayslipDocument | `Create(id, payslipId, ...)` | `PayslipDocumentId` |
| ReportExportAuditLog | `Export(id, moduleCode, ...)` | `ReportExportAuditLogId` |

### IIdGenerator Abstraction

The `IIdGenerator` interface (briefly created in `BuildingBlock.Domain/Abstractions/`) was **removed** — it had no consumers. The Application layer uses the static `IdGenerator.NextGuid()` directly via `Anemoi.BuildingBlock.Application.Helpers`. No dead abstraction remains.

### Verification

```txt
dotnet build → 0 warnings, 0 errors
dotnet test  → 232/232 passed
```

---

## TD-002 — Excessive Build Warnings

### Priority

P1

### Severity

Medium

### Current State

Latest verification:

```txt
dotnet build

0 Errors
348 Warnings
```

### Problem

Large warning count hides important warnings.

### Risk

* Real issues become invisible
* Nullable defects remain unnoticed
* Technical debt compounds over time

### Recommended Fix

Audit all warnings and categorize:

```txt
Nullable warnings
Analyzer warnings
Obsolete API warnings
Performance warnings
Code quality warnings
```

Reduce warning count to near zero.

### Suggested Target

Stabilization Sprint

---

## TD-003 — No Frontend Testing Infrastructure

### Priority

P2

### Severity

Medium

### Current State

Frontend currently relies on:

```txt
npm run lint
npm run build
```

No automated frontend test framework exists.

### Risk

* UI regressions
* Broken forms
* Query invalidation bugs
* Localization regressions

### Recommended Fix

Introduce:

```txt
Vitest
React Testing Library
MSW
```

Minimum coverage:

```txt
Forms
Hooks
Permissions
API Contracts
```

### Suggested Target

Frontend Quality Phase

---

## TD-004 — Frontend Repository Separation

### Priority

P2

### Severity

Medium

### Current State

Frontend exists under:

```txt
./cody-web-app
```

and is intentionally ignored from the backend repository.

### Risk

* Review confusion
* Missing CI validation
* Version synchronization issues
* Harder onboarding

### Recommended Fix

Document relationship explicitly:

```txt
README
Git Submodule
Monorepo Documentation
Repository Mapping Guide
```

### Suggested Target

DevOps Cleanup

---

## TD-005 — Localization Audit Across Existing Modules

### Priority

P4

### Severity

Medium

### Current State

Phase 19 revealed multiple hardcoded strings and mixed-language UI content.

### Risk

Similar issues may exist in:

```txt
Leave
Attendance
Payroll
Overtime
Shift
Calendar
Tax
```

### Recommended Fix

Perform localization audit:

```txt
Toast messages
Validation messages
Dialog text
Button labels
Table headers
Empty states
```

Remove hardcoded user-facing strings.

### Suggested Target

UI Stabilization Phase

---

## TD-006 — Frontend / Backend Contract Drift

### Priority

P3

### Severity

Medium

### Current State

Phase 19 exposed DTO mismatches between frontend and backend.

### Risk

* Runtime failures
* Broken forms
* 400 Bad Request errors
* Production regressions

### Recommended Fix

Generate frontend contracts from OpenAPI.

Possible approaches:

```txt
NSwag
OpenAPI Generator
Shared Contract Package
```

### Suggested Target

Platform Improvement Phase

---

## TD-007 — Missing End-to-End Test Coverage

### Priority

P2

### Severity

Medium

### Current State

Testing currently relies on:

```txt
Unit Tests
Build Verification
Manual Testing
```

### Risk

Cannot automatically detect:

```txt
Permission issues
Routing failures
Integration defects
Localization regressions
```

### Recommended Fix

Introduce:

```txt
Playwright
```

Target flows:

```txt
Leave
Attendance
Payroll
Overtime
Shift
Calendar
Tax
```

### Suggested Target

QA Automation Phase

---

## TD-008 — Snapshot Storage Growth

### Priority

P5

### Severity

Low

### Current State

Tax snapshots store:

```txt
RuleSetSnapshotJson
BracketSnapshotJson
DeductionSnapshotJson
CalculationResultJson
```

Future Insurance Engine may use similar patterns.

### Risk

Database growth over time.

### Recommended Fix

Future investigation:

```txt
JSONB compression
Archiving
Retention strategy
Historical partitioning
```

Not required for current scale.

### Suggested Target

Scale & Optimization Phase

---

## TD-009 — Permission Matrix Audit

### Priority

P3

### Severity

Medium

### Current State

Many permissions now exist:

```txt
leave.*
attendance.*
payroll.*
overtime.*
shift.*
calendar.*
tax.*
```

### Risk

Potential:

```txt
Unused permissions
Duplicate permissions
Orphan permissions
Missing route mapping
```

### Recommended Fix

Generate documentation:

```txt
Permission
→ Controller
→ Endpoint
→ Frontend Route
```

Validate permission coverage.

### Suggested Target

Security Review Phase

---

## TD-011 — Backend Validation Localization Strategy Cleanup

### Priority

P4

### Severity

Low

### Current State

HR validators use a mixed strategy:
- Some use stable error-code constants (`HrBusinessErrorCodes.ValXxx`, `HrBusinessErrorCodes.Xxx`) in `.WithMessage(...)` — ✅ already migrated.
- Some still use hard-coded English `.WithMessage("...")` — ⚠️ not yet migrated.

### Remaining English Messages (10 occurrences, 8 files)

| File | Message |
|---|---|
| `PayslipQueries/GetPayslipDocumentDownload/...` | `"PayslipId must not be empty."`, `"DocumentId must not be empty."` |
| `PayslipQueries/GetPayslipEmailDeliveries/...` | `"PayslipId must not be empty."` |
| `PayslipQueries/GetPayslipDocuments/...` | `"PayslipId must not be empty."` |
| `PayslipCommands/SendPayslipEmail/...` | `"PayslipId must not be empty."` |
| `PayslipCommands/GeneratePayslipPdf/...` | `"PayslipId must not be empty."` |
| `PayslipCommands/SendPayslipEmailsForPayrollRun/...` | `"PayrollRunId must not be empty."` |
| `PayslipCommands/GeneratePayslipPdfsForPayrollRun/...` | `"PayrollRunId must not be empty."` |
| `InsuranceCommands/CreateInsuranceContributionRule/...` | `"At least one rate must be greater than zero"` |
| `InsuranceCommands/UpdateInsuranceContributionRule/...` | `"At least one rate must be greater than zero"` |

### Problem

Mixed strategy: backend-localized English messages vs. frontend-localized error codes. Hard-coded English strings cannot be translated without a code change and violate ADR-017 (Localization Is Mandatory) and ADR-021 (Error Localization Strategy).

### Risk

- Non-English users see English validation messages.
- Frontend has no way to map these messages to localized text.
- Future validators might copy the wrong pattern.

### Recommended Fix

1. Create error-code constants for each English message (either `VAL_*` or dedicated constants).
2. Replace `.WithMessage("...")` with `.WithMessage(HrBusinessErrorCodes.ConstantName)`.
3. Add frontend mappings for the new error codes via `next-intl`.
4. Injecting `IStringLocalizer<SharedResource>` into validators is **not** recommended for HR validators per ADR-021.

### Exclusions

- Existing `SharedResource.resx` messages for BuildingBlock/Auth/Identity are **not** in scope.
- This task does not require injecting `IStringLocalizer` into HR validators.
- This task does not change the API response shape.

### Suggested Target

Next localization cleanup sprint

---

## TD-010 — Payroll Reporting Department/Position Fallback

### Priority

P3

### Severity

Medium

### Current State

Advanced Payroll Cost, Department Breakdown, and Employee Breakdown reports query the employee's current `PrimaryDepartment` and `PrimaryPosition` navigations because historical department and position snapshots are not persisted in `PayrollRun` or `Payslip` entities.

### Risk

If an employee transfers departments or positions, past period payroll reports will show their current department/position instead of their historical department/position at the time of the payroll run.

### Recommended Fix

Store `DepartmentId`, `DepartmentName`, `PositionId`, and `PositionName` snapshots directly in the `PayrollRun` entity at calculation time (Phase 24 or next optimization sprint).

BuildPayrollVariance fix not used full outer join

HR_PAYROLL_RUN_ALREADY_EXISTS employee payroll status Draft but can not re calculation payroll


### Suggested Target

Next Optimization Sprint

---

---

## TD-012 — DataChange Sensitivity Set to Low (No Workspace Context)

### Priority

P4

### Severity

Low

### Status

⚠️ Active — Temporary fix, revisit in multi-tenant production phase.

### Context

Phase N5.5 (Notification Platform Hardening) fixed a critical bug where all `DataChangeOccurredIntegrationEvent` events were silently dropped by the Centralize Gateway's `DataChangeOccurredIntegrationEventConsumer` because they used `WorkspaceId = null` with `Sensitivity = Medium/High`. The consumer drops any non-Low event without a workspace scope.

The fix changed all publishers (Leave, Overtime, Payroll, Payslip consumers) to use `Sensitivity = Low`, which causes the consumer to broadcast to `Clients.All`.

### Risk

- `Sensitivity = Low` with `Clients.All` broadcasts cache invalidation events to every connected client, regardless of workspace membership.
- Events contain only `Resource`, `EntityId`, `Action`, `QueryTags`, `OccurredAt` — no PII, salary, leave reason, or sensitive data — but the broadcast scope is still wider than ideal.
- In a multi-tenant production deployment, tenant A clients receive data-change signals for tenant B resources.

### Recommended Fix

1. Propagate `WorkspaceId` through all consumer pipelines (requires workspace context in HR integration events).
2. Restore `Sensitivity = Medium` (or `High` where appropriate) for workspace-scoped events.
3. `DataChangeOccurredIntegrationEventConsumer` already correctly routes workspace-scoped events to `Workspace-{WorkspaceId}` SignalR groups.
4. Audit all `JoinWorkspace` calls on the frontend to ensure clients join the correct workspace groups.

### Suggested Target

Multi-tenant Production Hardening Phase

---

---

## Phase 16

### PostgreSQL Concurrency Alignment

Resolved:

```txt
SQL Server RowVersion
→ PostgreSQL xmin
```

---

### Manual Mapping

Resolved:

```txt
Manual mapping
→ Mapperly
```

---

### Duplicate DI Registrations

Resolved.

---

## Phase 18

### Working Calendar Representation

Resolved:

```txt
WorkingDays string
→ 7 boolean weekday fields
```

---

## Phase 19

### Native Browser Confirm Dialogs

Resolved:

```txt
window.confirm()
→ shadcn confirmation dialogs
```

---

### Mixed English / Vietnamese UI

Resolved.

---

### Tax Calculation Contract Mismatch

Resolved.

---

### Snapshot Missing RuleSet Data

Resolved.

---

## Phase 22

### Frontend Error Translation System

Resolved:
- **Centralized Error Parsing**: Consolidated raw/duplicated API error parsing (e.g., `error.response?.data?.code`) into the centralized `extractApiErrorCode()` helper.
- **Centralized Translation Pipeline**: Routed all user-facing API error notifications through `translateApiError()`, utilizing a root-level `"errors"` block instead of module-specific error namespaces.
- **Complete Error Code Coverage**: Synced all 231 backend `HR_*` and `VAL_*` codes from `HrBusinessErrorCodes.cs` into `en.json` and `vi.json` catalogs.
- **Import/Code Cleanup**: Removed unused `isAxiosError` and `ApiError` imports/declarations from 11 frontend components.

---

# Recommended Cleanup Roadmap

## Immediate Priority (P1)

1. TD-001 Domain → Application dependency
2. TD-002 Build warning reduction

## Short-Term Priority (P2)

3. TD-003 Frontend testing infrastructure
4. TD-007 End-to-End automation

## Medium-Term Priority (P3)

5. TD-006 OpenAPI-generated contracts
6. TD-009 Permission audit

## Long-Term Priority (P4-P5)

7. TD-005 Localization audit
8. TD-008 Snapshot storage optimization
9. TD-011 Validation localization strategy cleanup

---

# Notes

This register intentionally excludes:

* Completed features
* Approved architectural decisions
* Future roadmap items that are not yet debt

Only known unresolved technical debt should remain in this document.


Phase 22 Status:
COMPLETED WITH ACCEPTED TECH DEBT

TD-PDF-001: Vietnamese accents require embedded Unicode font support.
TD-STOR-001: Local file storage should be replaced by durable object storage before production multi-instance deployment.


# 2026-06-15
Audit actor fields currently stored as string:
- ApprovedBy
- RejectedBy
- CancelledBy

Recommended:
- UserId (Guid / Strongly Typed UserId)

Reason:
- Type safety
- FK capability
- Consistency with the rest of the domain model
- Eliminate Guid -> string conversions