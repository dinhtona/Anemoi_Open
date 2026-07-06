# ANEMOI HR - Technical Debt Register

IMPORTANT

Before working on any technical debt item, read:

- docs/architecture/ARCHITECTURE_DECISIONS.md

Architectural decisions take precedence over technical debt recommendations.

# ANEMOI HR - Technical Debt Register

Version: Phase 34 Complete

Status: Active

Last Updated: 2026-07-03

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

✅ **Resolved** — June 2026

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

## TD-P34-PROBATION-01 — Direct Pass/Fail Handlers Return 200 Despite Save Failure

### Priority

P2

### Severity

Medium

### Status

⚠️ Active — discovered Phase 34 Iteration 7

### Context

`PassProbationHandler` and `FailProbationHandler` set `ReviewerEmployeeId = new EmployeeId(Guid.Empty)` (`00000000-0000-0000-0000-000000000000`), which violates `FK_ProbationRecords_Employees_ReviewerEmployeeId` in the database. `SaveChangesAsync` throws `DbUpdateException`, but because the handler constructs the DTO from in-memory entity state AFTER `Pass()`/`Fail()` mutates it, the HTTP response returns 200 with `statusCode: "Passed"` while the database row remains unchanged.

Additionally, FluentValidation runs on `[FromBody] PassProbationCommand` before the controller merges `command with { Id = id }`, requiring `id` to be sent redundantly in the request body.

### Impact

- Direct Pass/Fail buttons return 200 success but the probation record is never updated in the database.
- Frontend has no indication of failure — the error is silently swallowed.
- These endpoints bypass the workflow path, which is the intended and verified route per ADR-029.

### Recommended Fix

Choose one:

1. **Remove/deprecate the direct Pass/Fail endpoints** — Workflow is the preferred and verified path per ADR-029. The frontend has already replaced the Pass/Fail buttons with a "Use Workflow" hint.
2. **Fix the handlers to persist correctly** — Change `new EmployeeId(Guid.Empty)` to an appropriate null/default for the nullable `ReviewerEmployeeId` FK, or use the authenticated user's employee ID.

### Affected Files

- `Anemoi.Hr.Application/Cqrs/Commands/ProbationCommands/PassProbation/PassProbationHandler.cs:26`
- `Anemoi.Hr.Application/Cqrs/Commands/ProbationCommands/FailProbation/FailProbationHandler.cs:26`
- `Anemoi.Hr.Application/Cqrs/Commands/ProbationCommands/PassProbation/PassProbationCommand.cs`
- `Anemoi.Hr.Application/Cqrs/Commands/ProbationCommands/FailProbation/FailProbationCommand.cs`

### Suggested Target

Probation cleanup sprint or when direct Pass/Fail is needed

---

## TD-P34-EMPLOYEE-02 — CreateEmployeeHandler Writes EmployeeHistory with Null EntityId

### Priority

P2

### Severity

Medium

### Status

✅ **Resolved** — July 2026 (Phase 34 Iteration 9)

### Fix Applied

1. **Added `EntityId` to all 10 EmployeeCommands handlers** — `CreateEmployeeHandler`, `ActivateEmployeeHandler`, `ArchiveEmployeeHandler`, `ChangeEmployeeDepartmentHandler`, `ChangeEmployeeGradeHandler`, `ChangeEmployeeManagerHandler`, `ChangeEmployeePositionHandler`, `ResumeEmployeeHandler`, `SuspendEmployeeHandler`, and `UpdateEmployeeContactHandler` now set `EntityId = employeeId.Value.ToString()` matching the event handler pattern.
2. **Added `Description` to all 10 handlers** — `Description` is also `.IsRequired()` in the schema. Previously only `CreateEmployeeHandler` set it; the other 9 were missing it.
3. **Audit confirmed**: Asset (6), Document (3), Note (2), and Event (4) handlers already correctly set `EntityId`.

### Verification

```txt
dotnet build → 0 errors, 73 warnings (all pre-existing)
dotnet test  → 575/575 passed (250 HR + 325 BuildingBlock)
```

### Affected Files

- `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/CreateEmployee/CreateEmployeeHandler.cs`
- `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/ActivateEmployee/ActivateEmployeeHandler.cs`
- `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/ArchiveEmployee/ArchiveEmployeeHandler.cs`
- `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/ChangeEmployeeDepartment/ChangeEmployeeDepartmentHandler.cs`
- `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/ChangeEmployeeGrade/ChangeEmployeeGradeHandler.cs`
- `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/ChangeEmployeeManager/ChangeEmployeeManagerHandler.cs`
- `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/ChangeEmployeePosition/ChangeEmployeePositionHandler.cs`
- `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/ResumeEmployee/ResumeEmployeeHandler.cs`
- `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/SuspendEmployee/SuspendEmployeeHandler.cs`
- `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/UpdateEmployeeContact/UpdateEmployeeContactHandler.cs`

---

## TD-P34-SEC-01 — Missing [HasPermission] on 7 Controller Endpoints

### Priority

P1

### Severity

High

### Status

✅ **Resolved** — July 2026 (Phase 34 Iteration 9 Security Audit)

### Fix Applied

1. **WorkflowInstancesController** (4 endpoints): `ApproveWorkflowStep`, `RejectWorkflowStep`, `CancelWorkflow`, `ReturnWorkflow` were using plain `[Authorize]` (redundant — class already has `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`). Changed to `[HasPermission(HrPermissions.WorkflowApprove)]` for approve/reject/return, and `[HasPermission(HrPermissions.WorkflowManage)]` for cancel.
2. **ManagerApprovalsController** (2 endpoints): `GetPendingLeaveApprovals`, `GetPendingOvertimeApprovals` using `[Authorize]`. Changed to `[HasPermission(HrPermissions.WorkflowApprove)]`.
3. **EmployeeController** (1 endpoint): `GetMyProfile` (`GET /api/hr/employees/me`) had no `[HasPermission]` — any authenticated user could access. Changed to `[HasPermission(HrPermissions.EssProfileView)]`.

### Verification

```txt
dotnet build → 0 errors, 8 warnings (pre-existing)
dotnet test  → 575/575 passed
```

### Affected Files

- `Anemoi.Hr.Api/Controllers/Workflow/WorkflowInstancesController.cs`
- `Anemoi.Hr.Api/Controllers/Manager/ManagerApprovalsController.cs`
- `Anemoi.Hr.Api/Controllers/Employee/EmployeeController.cs`

---

## TD-P34-SEC-02 — OrganizationHierarchyController Returns Domain Entities

### Priority

P3

### Severity

Medium

### Status

⚠️ Active — discovered Phase 34 Iteration 9 Security Audit

### Context

`OrganizationHierarchyController.GetOrganizationTree` returns `IReadOnlyCollection<Domain.Organization.OrganizationNode>` and `GetReportingChain` returns `IReadOnlyCollection<Domain.Organization.ReportingRelationship>` — raw domain types instead of dedicated DTOs. This violates Clean Architecture and could expose internal domain modeling details or sensitive navigation properties via JSON serialization.

### Impact

- Potential data exposure if domain entities gain navigation properties in the future.
- Inconsistent with the rest of the API, which consistently uses response DTOs.

### Recommended Fix

Create dedicated response DTOs (e.g., `OrganizationNodeResponse`, `ReportingRelationshipResponse`) and map from domain entities via Mapperly before returning.

### Affected Files

- `Anemoi.Hr.Api/Controllers/Organization/OrganizationHierarchyController.cs`

### Suggested Target

Next HR module cleanup sprint.

---

## TD-P34-SEC-03 — Latent FromSqlRaw Injection Surface in Generic Repository

### Priority

P3

### Severity

Medium

### Status

⚠️ Active — discovered Phase 34 Iteration 9 Security Audit

### Context

`EfRepository.GetQueryableFromRawQuery` accepts a raw `sql` string passed to EF Core's `FromSqlRaw`. While the method accepts parameterized arguments (`params object[] parameters`), the `sql` string itself is not protected from injection if callers concatenate user input. No production code currently calls this method (only test mock implementations exist), but it remains publicly available.

### Recommended Fix

Either:
1. Remove the method from the public `ISqlRepository` interface entirely.
2. Mark it `[Obsolete]` with a message directing to typed queries.
3. Add XML doc comments warning about SQL injection risk.

### Affected Files

- `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Infrastructure/Repositories/EfRepository.cs`

### Suggested Target

Next platform cleanup sprint.

---

## TD-P34-SEC-04 — Hardcoded Credentials in appsettings.json Files

### Priority

P2

### Severity

High

### Status

⚠️ Active (partially mitigated by docker-compose + .env.example) — discovered Phase 34 Iteration 9 Security Audit

### Context

Multiple `appsettings.json` files contain hardcoded credentials:
- **Connection strings with passwords** in 5 services (Hr, Identity, MasterData, Notification, Workspace)
- **Seed user passwords** (`Admin@12345`, `Password1`) in Identity WorkerService
- **S3 credentials** (`AccessKey: "secret"`, `SecretKey: "secret"`) in Centralize API
- **RabbitMQ `guest/guest`** credentials in 6 services
- **SFTP password** (`123456`) in Centralize API

These are development defaults used with Docker Compose. The `.env.example` and `docker-compose.yml` already provision proper environment variables, but the appsettings.json files still contain fallback values that would be used if environment variables are not set.

### Impact

- Credentials are visible to anyone with repository access.
- If deployed without proper environment variable overrides, services would use weak/guessable credentials.
- Violates security best practices for credential management.

### Recommended Fix

1. Remove default passwords from appsettings.json; use environment variable placeholders only.
2. Move seed user passwords to `dotnet user-secrets` or environment variables.
3. Document that development credentials are set via `docker-compose.yml` environment variables, not appsettings fallbacks.
4. Enable HTTPS redirection + HSTS for all services in non-development environments.

### Affected Files

- All `appsettings.json` and `appsettings.Development.json` files across 6 service projects.

### Suggested Target

DevOps/Security hardening sprint before production deployment.

---

## TD-P34-PERF-01 — N+1 Queries in Recruitment/Analytics Handlers

### Priority

P2

### Severity

High

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

Three recruitment handlers and one analytics handler execute DB queries inside foreach loops, producing O(N) additional round-trips:

1. **`GetHiringByDepartmentHandler`** — 3 DB calls per department (requisition count, application count, hire count). With 20 departments = 60+ round-trips.
2. **`GetCandidateSourceEffectivenessHandler`** — 3 DB calls per candidate source. With 10 sources = ~40 round-trips.
3. **`GetTimeToHireHandler`** — loads all `HireDecision` entities then computes min/max/avg/median in memory.
4. **`GetRecruitmentDashboardWidgetsHandler`** — loads all `RecruitmentOpening` rows, then counts/filters in memory instead of `CountAsync()`.

### Impact

- Recruitment dashboard loads grow linearly with department/source count.
- Each deployment with more departments or sources silently degrades.

### Recommended Fix

Use SQL-level `GroupBy` with aggregation (`.CountAsync()`, `.SumAsync()`) in a single query instead of per-entity loops. For `GetTimeToHireHandler`, compute the date difference in SQL using `EF.Functions.DateDiffDay`.

### Affected Files

- `GetHiringByDepartmentHandler.cs:27-51`
- `GetCandidateSourceEffectivenessHandler.cs:28-43`
- `GetTimeToHireHandler.cs:29-37`
- `GetRecruitmentDashboardWidgetsHandler.cs:33-36`

---

## TD-P34-PERF-02 — N+1 Role/Permission Resolution in Approval Handlers

### Priority

P2

### Severity

High

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

Three approval handlers iterate over workflow roles and permissions, calling external resolvers inside foreach loops:

1. **`GetMyPendingLeaveApprovalsHandler`** — `roleResolver.ResolveAsync()` per role (lines 143-151), `permissionResolver.HasPermissionAsync()` per permission (lines 161-165).
2. **`GetMyPendingOvertimeApprovalsHandler`** — identical pattern (lines 140-148, 158-162).
3. **`GetPendingApprovalsHandler`** — `roleResolver.ResolveAsync()` per role (lines 38-44).

Each resolver call likely hits an external service or DB. With many workflow instances each having distinct roles, a single request could trigger dozens of external calls.

### Impact

- Approval inbox page load degrades linearly with the number of distinct roles.
- Each role adds a sequential network round-trip.

### Recommended Fix

Add batch overloads (`ResolveManyAsync`, `HasPermissionsAsync`) to the resolver interfaces that accept collections and resolve all roles/permissions in a single call.

### Affected Files

- `GetMyPendingLeaveApprovalsHandler.cs:143-165`
- `GetMyPendingOvertimeApprovalsHandler.cs:140-162`
- `GetPendingApprovalsHandler.cs:38-44`

---

## TD-P34-PERF-03 — MonthlyLeaveAccrualWorker N+1 per Employee×Policy

### Priority

P2

### Severity

High

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

`MonthlyLeaveAccrualWorker` loads all active employees and active policies, then executes 2 DB calls per employee×policy pair (accrual run existence check + balance lookup). For 500 employees × 5 policies = 5,000+ DB round-trips per monthly run.

### Impact

- Monthly accrual run time grows quadratically with employee count.
- Worker may time out or block other DB operations for extended periods.

### Recommended Fix

Batch-load all existing balances and accrual runs for the current year/month upfront in 2 queries, then process all logic in-memory using dictionary lookups.

### Affected Files

- `MonthlyLeaveAccrualWorker.cs:52-112`

---

## TD-P34-PERF-04 — Full Table Loads in Analytics Handlers

### Priority

P2

### Severity

Medium

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

Five analytics handlers materialize entire tables to compute aggregates in memory:

1. **`GetCompensationDashboardHandler`** — loads all employees, active salaries, and active allowances (AsNoTracking added in Iteration 10, but still materializes full tables).
2. **`GetOvertimeAnalyticsHandler`** — loads all `OvertimeRequest` rows, computes hours/totals/averages in C#.
3. **`GetPayrollAnalyticsHandler`** — loads all finalized `PayrollItem` rows (no date filter), computes aggregates in C#.
4. **`GetDepartmentCostAnalyticsHandler`** — loads all finalized `PayrollItem` rows (no date filter), groups by department in C#.
5. **`GetHeadcountTrendHandler`** — loads all employees, re-scans in-memory for each month.

### Impact

- Dashboard and analytics queries degrade linearly with total org history.
- For 10,000 employees and 5 years of payroll, queries load 60,000+ rows into memory.

### Recommended Fix

Push aggregation to SQL using `.GroupBy()`, `.SumAsync()`, `.CountAsync()`, `.AverageAsync()`. Add date range parameters to analytics queries and filter at the database level.

### Affected Files

- `GetCompensationDashboardHandler.cs:32-41` (AsNoTracking added, full scan remains)
- `GetOvertimeAnalyticsHandler.cs:26-44`
- `GetPayrollAnalyticsHandler.cs:20-38`
- `GetDepartmentCostAnalyticsHandler.cs:21-35`
- `GetHeadcountTrendHandler.cs:23-41`

---

## TD-P34-PERF-05 — Missing AsNoTracking in 16+ Query Handlers

### Priority

P3

### Severity

Low

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

~16 query handlers load entities with tracking enabled (EF change tracker snapshots) but map results to DTOs without ever modifying or saving the entities. Tracking overhead is pure memory/CPU waste.

Handlers include: `GetTransfersHandler`, `GetSeparationsHandler`, `GetProbationsHandler`, `GetAttendanceRecordsHandler`, `GetWorkflowDefinitionsHandler`, `GetWorkflowInstanceByIdHandler`, `GetLeaveRequestsHandler`, `GetRecruitmentRequestsHandler`, `GetMyProfileHandler`, `GetMyLeaveBalancesHandler`, `GetMyLeaveRequestsHandler`, `GetMyOvertimeRequestsHandler`, `GetMyApproverPreviewHandler`, `GetEmployeeAllowancesHandler`, `GetCompensationTimelineHandler`, `GetCompensationSnapshotHandler`, `GetEmployeePromotionTimelineHandler`.

(3 highest-impact handlers were fixed with AsNoTracking in Iteration 10: `GetCompensationDashboardHandler`, `GetWorkflowInstancesHandler`, `GetPendingApprovalsHandler`.)

### Impact

- Memory overhead from EF change tracker proxies for read-only query results.
- For handlers loading collections (e.g., dashboard returns 500+ entities), adds measurable GC pressure.

### Recommended Fix

Add `.AsNoTracking()` to all query handlers that map to DTOs. Audit via Roslyn analyzer rule or code review checklist.

### Affected Files

- 16+ files listed above.

---

## TD-P34-PERF-06 — Expensive Include Chains Without AsSplitQuery

### Priority

P3

### Severity

Medium

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

Three handlers use multiple `.Include()` calls that produce Cartesian explosion in a single JOIN query:

1. **`GetTransferByIdHandler`** — 5 Includes (Employee, SourceDepartment, TargetDepartment, SourcePosition, TargetPosition).
2. **`GetTransfersHandler`** — same 5 Includes, paginated (worse: each page generates a Cartesian product).
3. **`GetInterviewByIdHandler`** — 4 Includes including collections (Feedbacks).
4. **`SearchInterviewsHandler`** — 3 Includes.

None use `.AsSplitQuery()` to break the query into separate round-trips that avoid the Cartesian product.

### Impact

- Single transfer query generates a 5-table cross-join result set that grows exponentially.
- Network transfer of redundant data, CPU to deduplicate on the client side.

### Recommended Fix

Add `.AsSplitQuery()` after `.Include()` chains for handlers loading multiple navigation properties, especially when collections are involved.

### Affected Files

- `GetTransferByIdHandler.cs:21-27`
- `GetTransfersHandler.cs:19-25`
- `GetInterviewByIdHandler.cs:25-30`
- `SearchInterviewsHandler.cs:31-33`

---

## TD-P34-PERF-07 — Redundant Employee Loading in Approval Handlers

### Priority

P3

### Severity

Low

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

`GetMyPendingLeaveApprovalsHandler` and `GetMyPendingOvertimeApprovalsHandler` each execute two separate employee queries:

1. First query loads employees with department Includes for name+department lookup.
2. Second query loads employees again (just for FullName) for approver display.

Both load full Employee entities instead of projecting only `Id`, `FullName`, and `Department.Name`.

### Impact

- Duplicate data transfer and entity materialization for the same Employee IDs.
- Loads all columns of the Employee table when only 2-3 are needed.

### Recommended Fix

Merge into a single query matching all required employee IDs, project with `.Select()` to return only needed columns.

### Affected Files

- `GetMyPendingLeaveApprovalsHandler.cs:55-58,79-84`
- `GetMyPendingOvertimeApprovalsHandler.cs:55-58,79-84`

---

## TD-P34-PERF-08 — Missing Pagination on Unbounded Endpoints

### Priority

P3

### Severity

Medium

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

Several endpoints return unbounded collections without pagination:

1. **`GetAttendancePeriodsHandler`** — loads all attendance periods ever created (120+ after 10 years).
2. **`GetInsuranceRuleSetsHandler`** — loads all insurance rule sets with nested contribution rules.
3. **`GetInsuranceCalculationSnapshotsHandler`** — loads all snapshots without pagination.
4. **`GetInsuranceContributionReportHandler`** — loads all matching snapshots for a period without row cap.
5. **ESS handlers** — `GetMyAttendanceRecords`, `GetMyLeaveRequests`, `GetMyOvertimeRequests`, `GetMyPayslips`, `GetMyPayrollHistory` — unbounded per employee.
6. **Compensation history handlers** — `GetCompensationTimeline`, `GetEmployeeSalaryHistory`, `GetEmployeeAllowances` — unbounded per employee.
7. **Employee history handlers** — `GetEmployeeDepartmentHistory`, `GetEmployeePositionHistory`, `GetEmployeePromotionTimeline` — unbounded per employee.

### Impact

- API responses grow unboundedly over time for long-tenured employees.
- No client-side pagination support for history views.

### Recommended Fix

Add pagination query parameters (Page/PageSize or Offset/Limit) to all unbounded list endpoints. For ESS endpoints, add `.Take(50)` as a safe default.

### Affected Files

- 11+ handlers listed above.

---

## TD-P34-PERF-09 — GetDashboardOverviewHandler 6 Independent List Loads

### Priority

P3

### Severity

Medium

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

`GetDashboardOverviewHandler` executes 6 independent `GetManyByConditionAsync` calls loading: near-exhaustion balances, active departments, active positions, out-today leave requests, out-this-week leave requests, and expiring contracts. While each query is individually bounded, 6 sequential DB round-trips add latency to the dashboard.

### Impact

- Dashboard load time is sum of 6 sequential queries.
- Each query could return hundreds of rows at scale.

### Recommended Fix

Consider parallel queries via `Task.WhenAll` for independent loads. Add result cap (`.Take(50)`) where appropriate.

### Affected Files

- `GetDashboardOverviewHandler.cs:59-160`

---

## TD-P34-PERF-10 — Identity GetUsersHandler N+1 Post-Materialization

### Priority

P3

### Severity

Medium

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

`GetUsersHandler` maps paginated user results, then iterates each user to execute:
- 1 DB call for role groups per user
- 2 DB calls for direct roles + effective roles per user

For a page of 20 users = 60 additional round-trips.

### Impact

- User list page slows linearly with page size.

### Recommended Fix

Pre-load role groups and role data for all paginated user IDs in batch queries before the mapping loop.

### Affected Files

- `Anemoi.Identity/.../GetUsers/GetUsersHandler.cs:73-97`

---

## TD-P34-PERF-11 — GetOnboardingDashboardHandler Loads All Instances with Tasks

### Priority

P3

### Severity

Low

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

Loads all active onboarding instances with all tasks (via EF navigation properties), then computes task counts in C#. For 200 concurrent onboardings × 20 tasks each = 4,000 task entities materialized.

### Impact

- Moderate memory usage for large concurrent onboarding cohorts.

### Recommended Fix

Run SQL aggregates with `.SelectMany()`/`.GroupBy()` to compute pending/overdue/upcoming counts in a single query.

### Affected Files

- `GetOnboardingDashboardHandler.cs:25-43`

---

## TD-P34-PERF-12 — ContractExpirationWorker No Batching

### Priority

P4

### Severity

Low

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

Loads all expired contracts, updates them in a foreach loop, then saves all changes in a single `SaveChangesAsync`. For large historical datasets on first run, this could load thousands of contracts.

### Impact

- High memory usage on first run after historical data import.
- Single large SaveChanges batch may hit transaction timeout.

### Recommended Fix

Add batching with `.Chunk(500)` for large contract sets.

### Affected Files

- `ContractExpirationWorker.cs:40-43`

---

## TD-P34-PERF-13 — IdentityPolicyController PageSize = int.MaxValue

### Priority

P4

### Severity

Low

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

`IdentityPolicyController` sets `PageSize = int.MaxValue` when calling `GetUserRolesQuery`, circumventing all pagination safeguards. While role counts are typically small (dozens), this ignores infrastructure limits.

### Impact

- If roles grow unexpectedly, this endpoint could attempt to load unbounded data.

### Recommended Fix

Set a reasonable maximum (e.g., `PageSize = 200`) or implement a dedicated non-paginated query.

### Affected Files

- `IdentityPolicyController.cs:40`

---

## TD-P34-PERF-14 — Database Index Gap Analysis (38 Findings)

### Priority

P2 (12 HIGH, 18 MEDIUM, 8 LOW)

### Severity

Medium-High

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

Comprehensive review of all 26 entity mapping configurations against query patterns revealed 38 missing indexes:

**HIGH priority (12)**:
- `Employees(EmploymentStatusCode, PrimaryDepartmentId)` — composite for dashboard/employee list filtering
- `Employees(EmploymentStatusCode, PrimaryPositionId)` — composite for dashboard/employee list filtering
- `Employees` GIN trigram on `EmployeeCode`, `WorkEmail`, `DisplayName` — for `LIKE '%term%'` search
- `PayrollRuns(Status, FinalizedAt)` — composite for analytics queries
- `OvertimeRequests(Status, ApprovedAt)` — composite for payslip calculation scan
- `JobRequisitions(DepartmentId)`, `JobRequisitions(PositionId)`, `JobRequisitions(Status)` — recruitment filter indexes
- `WorkflowInstances(Status, StartedAt)` — composite for pending approvals ordering
- `LeaveRequests(EmployeeId, StatusCode, StartDate, EndDate)` — merged composite for 4-filter query

**MEDIUM priority (18)**:
FK indexes: `WorkflowInstanceStep.ApproverUserId`, `WorkflowInstanceStep.ApproverEmployeeId`, `Position.DepartmentId`, `Department.ParentDepartmentId`, `SalaryRange.SalaryGradeId`, `LeaveRequest.ApproverEmployeeId`, `Employee.DirectManagerEmployeeId`
Composite: `ProbationRecord(StatusCode, EndDate)`, `ProbationRecord(EmployeeId, StatusCode, StartDate)`
IsArchived composite: `EmployeeAssets`, `EmployeeDocuments`, `EmployeeNotes`
CreatedAt sort indexes: `EmployeeSeparation`, `EmployeeTransfer`

**LOW priority (8)**:
Minor FK indexes, `RefreshToken.UserId` etc.

### Impact

- Every analytics query, employee search, and recruitment filter currently performs sequential scans or uses suboptimal index strategies.
- `LIKE '%term%'` search on EmployeeCode/Name/Email cannot use B-tree indexes at all (requires full scan without GIN trigram).

### Recommended Fix

Add index definitions in entity configuration files using `HasIndex()` and create EF Core migrations. GIN trigram indexes require raw SQL migration:
```sql
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE INDEX ix_employees_employee_code_trgm ON "Employees" USING gin ("EmployeeCode" gin_trgm_ops);
```

### Affected Files

- `EmployeeModelMapping.cs` (5 indexes)
- `PayrollModelMapping.cs` (1 composite)
- `OvertimeRequestModelMapping.cs` (1 composite)
- `RecruitmentModelMapping.cs` (3 indexes)
- `WorkflowInstanceModelMapping.cs` (1 composite, 2 FK)
- `LeaveModelMapping.cs` (1 composite, 1 FK)
- `EmployeeOrganizationModelMapping.cs` (2 FK)
- `CompensationModelMapping.cs` (1 FK)
- `ProbationModelMapping.cs` (2 composite, 1 FK)
- `EmployeeAssetModelMapping.cs` (IsArchived composite)
- `EmployeeDocumentModelMapping.cs` (IsArchived composite)
- `EmployeeNoteModelMapping.cs` (IsArchived composite)
- `SeparationModelMapping.cs` (CreatedAt, SeparationTypeCode)
- `TransferModelMapping.cs` (CreatedAt)

### Suggested Target

Dedicated database optimization sprint with EXPLAIN ANALYZE verification for each new index.

---

## TD-P34-PERF-15 — GetPendingApprovalsHandler In-Memory Pagination After Full Load

### Priority

P2

### Severity

High

### Status

⚠️ Active — discovered Phase 34 Iteration 10 Performance Audit

### Context

`GetPendingApprovalsHandler` loads ALL pending workflow instances (with Steps + Histories) from the database, then filters by current user's roles in memory, and finally applies `Skip/Take` in memory. The pagination happens AFTER the full dataset is materialized.

### Impact

- As pending approvals accumulate, every request loads all pending instances regardless of page size.
- With 1,000+ pending instances across the organization, each approval-inbox page load materializes all of them.

### Recommended Fix

Restructure to paginate at the database level: resolve user's applicable roles first, then query workflow instances with database-level pagination using the resolved role set. Requires batch role resolution (see TD-P34-PERF-02).

### Affected Files

- `GetPendingApprovalsHandler.cs:23-67`

---

# Recommended Cleanup Roadmap

## Immediate Priority (P1)

1. TD-001 Domain → Application dependency
2. TD-002 Build warning reduction

## Short-Term Priority (P2)

3. TD-003 Frontend testing infrastructure
4. TD-007 End-to-End automation
5. TD-P34-PROBATION-01 Direct Pass/Fail handler FK violation
6. TD-P34-SEC-04 Hardcoded credentials in appsettings.json
7. TD-P34-PERF-01 N+1 queries in recruitment/analytics handlers
8. TD-P34-PERF-02 N+1 role resolution in approval handlers
9. TD-P34-PERF-03 MonthlyLeaveAccrualWorker N+1
10. TD-P34-PERF-14 Database index gap analysis
11. TD-P34-PERF-15 GetPendingApprovalsHandler in-memory pagination

## Medium-Term Priority (P3)

5. TD-006 OpenAPI-generated contracts
6. TD-009 Permission audit
7. TD-P34-SEC-02 OrganizationHierarchyController returns domain entities
8. TD-P34-SEC-03 Latent FromSqlRaw injection surface
9. TD-P34-PERF-04 Full table loads in analytics handlers
10. TD-P34-PERF-05 Missing AsNoTracking in 16+ handlers
11. TD-P34-PERF-06 Expensive Include chains without AsSplitQuery
12. TD-P34-PERF-07 Redundant employee loading in approval handlers
13. TD-P34-PERF-08 Missing pagination on unbounded endpoints
14. TD-P34-PERF-09 GetDashboardOverviewHandler 6 independent loads
15. TD-P34-PERF-10 Identity GetUsersHandler N+1
16. TD-P34-PERF-11 GetOnboardingDashboardHandler full instance load

## Long-Term Priority (P4-P5)

7. TD-005 Localization audit
8. TD-008 Snapshot storage optimization
9. TD-011 Validation localization strategy cleanup
10. TD-P34-PERF-12 ContractExpirationWorker batching
11. TD-P34-PERF-13 IdentityPolicyController PageSize = int.MaxValue

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

---

## TD-025-01 — Hardcoded GradeCode "G1" in Candidate-to-Employee Conversion

### Priority

P2

### Severity

High

### Context

`ConvertCandidateToEmployeeHandler.cs` always sets `GradeCode = "G1"` for every converted employee, regardless of their position/grade.

### Risk

- Payroll calculations use GradeCode for salary range validation.
- Compensation reports will be wrong for converted employees.
- Without correct grade, payroll runs for converted employees will produce incorrect results.

### Recommended Fix

Choose one:

1. **Add `GradeCode` to `ConvertCandidateToEmployeeCommand`** — simplest, gives conversion operator full control.
2. **Derive `GradeCode` from PositionId** — requires grade-to-position mapping configuration.
3. **Introduce default grade mapping per position** — most robust but most work.

### Suggested Target

Before running payroll for any converted employee. Requires explicit architectural decision before implementation.

---

## Phase N6 — Notification Outbox/Inbox Durability Verification & Hardening (2026-06-16)

### Status
COMPLETED — See also ADR-026 in ARCHITECTURE_DECISIONS.md

### Changes Made

#### Publish Order Hardening
All 20 HR command handlers + MonthlyLeaveAccrualWorker now call `publishEndpoint.Publish()` **before** `unitOfWork.SaveChangesAsync()`. This ensures that if the bus outbox transport fails to persist an `OutboxMessage`, the exception propagates before domain changes are committed. Previously, events were published after save, risking silent event loss if the outbox save failed.

#### Outbox Configuration Verified
Both `Anemoi.Hr.Infrastructure` and `Anemoi.Notification.Infrastructure` have confirmed:
- `AddEntityFrameworkOutbox<TDbContext>` with `UsePostgres()` + `UseBusOutbox()`
- `InboxState`, `OutboxMessage`, `OutboxState` entity configurations in both DbContexts
- Unique constraint on `InboxState(MessageId, ConsumerId)` for transport-level dedup
- Filtered unique index on `NotificationHistory(UserId, DeduplicationKey)` for application-level dedup

#### Consumer Idempotency Confirmed
- `DeduplicationKey` is mandatory for all 13 business notification consumers
- `CreateNotificationHandler` has 3-layer dedup: pre-check, post-save concurrent recovery, catch-all recovery
- Unique index prevents duplicate `NotificationHistory` rows
- Race-condition fallback returns existing notification

#### Tests Added
- `NotificationDeduplicationTests.cs` — 4 tests verifying duplicate event consumption produces same DeduplicationKey
- `NotificationFailureBehaviorTests.cs` — 3 tests verifying failure before commit prevents persistence, successful save publishes event, DataChangeOccurred is never lost

#### Logging Added
- Empty permission audience warnings added to all 4 PayrollRun consumers
- Duplicate notification skip/reuse logging already present in CreateNotificationHandler

### Remaining Risks
1. **Bus outbox uses separate DbContext instances** — MassTransit's `BusOutboxPublishTransport` creates a separate DbContext scope via `IScopedDbContextFactory`. Domain changes and outbox messages are in **separate transactions**, even after the publish-order fix. True transactional outbox would require sharing the same DbContext instance.
2. **DataChange sensitivity remains Low** — See TD-012. WorkspaceId is not propagated through HR integration events.
3. **No MassTransit InMemoryTestHarness usage** — Consumer tests use NSubstitute mocks, not real MassTransit transport. Outbox delivery is not integration-tested.

---
## Phase 26
1. TD-013 — Onboarding assignee display name resolution should use user lookup service instead of userId fallback.
Severity: Low
Fix timing: Future notification/user profile integration phase.
---
