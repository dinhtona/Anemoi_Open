# ANEMOI HR - Architecture Decisions Record

IMPORTANT

This file is a mandatory architectural reference.

Before implementing or reviewing any feature, also review:

- docs/architecture/TECHNICAL_DEBT_REGISTER.md

Before changing an existing pattern, verify whether an Architecture Decision Record (ADR) already governs that area.

# ANEMOI HR - Architecture Decisions Record (ADR)

Version: After ADR-028 Approval

Status: Active

Last Updated: 2026-06-24

---

# Purpose

This document records architectural decisions that have been intentionally approved for ANEMOI HR.

Future implementations, reviewers, AI agents, and contributors should treat these decisions as established architecture unless a formal architecture review explicitly changes them.

This document exists to prevent accidental architectural drift.

---

# ADR-001 — Clean Architecture

## Status

Approved

## Decision

ANEMOI HR follows:

```txt
Domain
→ Application
→ Infrastructure
→ API
```

Dependency direction must always point inward.

Outer layers may depend on inner layers.

Inner layers must never depend on outer layers.

## Notes

Known exception:

```txt
Domain
→ BuildingBlock.Application
```

previously existed and was resolved as TD-001 (June 2026).

No new violations are allowed.

---

# ADR-002 — CQRS + MediatR

## Status

Approved

## Decision

Business operations must be implemented through:

```txt
Commands
Queries
Handlers
Validators
```

using MediatR.

Controllers must not contain business logic.

## Implications

Allowed:

```txt
Controller
→ MediatR
→ Handler
```

Not allowed:

```txt
Controller
→ DbContext
```

```txt
Controller
→ Business Logic
```

---

# ADR-003 — PostgreSQL Is The Primary Database

## Status

Approved

## Decision

PostgreSQL is the production database.

System design should prioritize PostgreSQL behavior.

## Implications

Prefer:

```txt
JSONB
xmin
PostgreSQL indexing
PostgreSQL migrations
```

Avoid SQL Server-specific features.

---

# ADR-004 — PostgreSQL xmin Concurrency Strategy

## Status

Approved

## Decision

Mutable aggregates use PostgreSQL xmin for optimistic concurrency.

## Examples

```txt
LeaveRequest
Promotion
OvertimeRequest
ShiftTemplate
EmployeeShiftAssignment
TaxRuleSet
```

## Not Allowed

```txt
SQL Server RowVersion
Timestamp concurrency columns
```

---

# ADR-005 — Historical Data Must Never Be Mutated

## Status

Approved

## Decision

Historical records are immutable.

Past payroll, tax, attendance, and payslip results must never change because of future configuration updates.

## Implications

Use snapshots.

Never recalculate historical results automatically.

---

# ADR-006 — Snapshot-Based Payroll Architecture

## Status

Approved

## Decision

Payroll calculations store snapshots.

Payroll must not depend on live configuration after finalization.

## Example

Snapshot:

```txt
Salary
Allowance
Attendance
Tax
Insurance
```

stored at calculation time.

Future changes must not affect historical payroll runs.

---

# ADR-007 — Attendance Is Consumed By Payroll Through Snapshots

## Status

Approved

## Decision

Payroll consumes attendance summaries.

Payroll must not directly recalculate attendance history.

## Reason

Historical reproducibility.

---

# ADR-008 — Payslips Are Immutable Historical Documents

## Status

Approved

## Decision

Generated payslips represent historical payroll output.

Publishing or future configuration changes must not alter historical payslips.

---

# ADR-009 — Tax Engine Is Independent From Payroll

## Status

Approved

## Decision

Tax Engine is a standalone bounded context.

Payroll consumes tax calculation snapshots.

Tax Engine does not belong to Payroll.

## Allowed

```txt
Payroll
→ Tax Calculation Snapshot
```

## Not Allowed

```txt
Payroll
→ Tax Rule Tables
→ Tax Brackets
→ Live Tax Logic
```

---

# ADR-010 — Tax Calculations Must Be Reproducible

## Status

Approved

## Decision

Tax calculations must be reproducible years later.

Every calculation stores:

```txt
RuleSet Snapshot
Bracket Snapshot
Deduction Snapshot
Calculation Result Snapshot
```

Future rule changes must not alter historical results.

---

# ADR-011 — Progressive Tax Rules Are Database-Driven

## Status

Approved

## Decision

Tax brackets are stored in the database.

## Not Allowed

```txt
Hard-coded tax brackets
Hard-coded PIT rates
```

inside handlers.

---

# ADR-012 — Future Multi-Country Support

## Status

Approved

## Decision

Tax and Insurance engines must support:

```txt
CountryCode
TaxType
Effective Dates
Versioning
```

Future country-specific support should be configuration-driven where practical.

---

# ADR-013 — Permission-Based Security

## Status

Approved

## Decision

Features are protected through permissions.

Not roles.

## Example

```txt
hr.leave.view
hr.leave.manage

hr.payroll.view
hr.payroll.approve

hr.tax.view
hr.tax.manage
```

Controllers and frontend routes must respect permissions.

---

# ADR-014 — Auditability Is Mandatory

## Status

Approved

## Decision

Business-critical operations must be traceable.

## Examples

```txt
Payroll Approval
Payroll Export
Tax Calculation
Shift Assignment
Leave Approval
```

should preserve user and timestamp information.

---

# ADR-015 — Export Operations Must Be Audited

## Status

Approved

## Decision

Report exports must generate audit records.

## Current Implementation

```txt
ReportExportAuditLog
```

## Retention

Minimum:

```txt
5 years
```

---

# ADR-016 — Frontend Must Follow Existing Platform Patterns

## Status

Approved

## Decision

New modules must reuse existing patterns.

## Required

```txt
React Query
shadcn/ui
Permission Guards
Localization
Shared API Services
Shared Hooks
```

## Not Allowed

```txt
window.confirm()
Hard-coded strings
Direct fetch calls bypassing platform services
```

unless architecture review explicitly approves it.

---

# ADR-017 — Localization Is Mandatory

## Status

Approved

## Decision

All user-facing text must be localizable.

## Required

```txt
Buttons
Dialogs
Validation Messages
Toast Messages
Tables
Labels
```

## Not Allowed

Hard-coded English or Vietnamese strings inside feature components.

---

# ADR-018 — API Contracts Must Remain Stable

## Status

Approved

## Decision

Frontend and backend contracts should evolve through versioned DTOs.

Breaking changes require coordinated updates.

Future direction:

```txt
OpenAPI generation
Shared contracts
```

---

# ADR-019 — Historical Snapshots Take Priority Over Storage Optimization

## Status

Approved

## Decision

Storage duplication is acceptable when required for historical reproducibility.

## Examples

```txt
Tax snapshots
Payroll snapshots
Insurance snapshots
```

may duplicate configuration data intentionally.

## Reason

Correctness is more important than storage efficiency.

---

# ADR-020 — Future Engines Follow The Same Pattern

## Status

Approved

## Decision

Future modules should follow the established architecture:

```txt
Insurance Engine
ESS
PDF Payslip
Recruitment
Onboarding
Performance
Training
```

Core pattern:

```txt
Configuration
→ Calculation
→ Snapshot
→ Audit
→ Reporting
```

rather than direct recalculation.

---

# ADR-021 — Error Localization Strategy

## Status

Approved

## Context

Legacy modules (BuildingBlock, Auth, Identity) localized error messages server-side using `SharedResource.resx`. Newer HR modules (Payroll, Tax, Attendance, Leave, etc.) introduced a pattern where the backend returns a stable error code and the frontend handles translation via `next-intl`.

This created a mixed localization strategy.

## Decision

1. **Backend business modules return stable error codes**, not localized user-facing text.
2. **Frontend owns localization for HR business errors** — the frontend maps error codes to user-facing messages via `next-intl`.
3. **Error codes are API contracts** — they must be defined as constants (e.g., `HrBusinessErrorCodes`, `Val*` constants), not inline string literals.
4. **HR handlers and validators must use constants** — inline `"HR_SAVE_CHANGES_FAILED"` or `"VAL_AMOUNT_MUST_BE_POSITIVE"` inside `.Create()` or `.WithMessage()` is prohibited.
5. **`SharedResource.resx` remains allowed** for legacy BuildingBlock, Auth, Identity, and common infrastructure errors. No migration of `SharedResource.resx` is planned in this ADR.
6. **New HR business errors must not depend on backend resource localization** — do not add HR-specific keys to `SharedResource.resx` unless backend rendering requires them.
7. **FluentValidation `.WithMessage(...)` in HR should use error-code constants** — not English strings.
8. **English hard-coded validation messages are technical debt** unless the message is internal-only (logs, developer diagnostics).

## Exceptions

Legacy error codes that already use `SharedResource.resx` (e.g., `IDE_01`, `AUE_01`, `ROE_01`, `VAL_REQUIRED`, `UnhandledError`) are exempt from this ADR. They may continue using server-side localization until a future architecture review decides otherwise.

## Implications

- Frontend must maintain a mapping of `HR_*` / `VAL_*` / `REPORT_*` error codes to localized messages.
- Backend must never assume the frontend displays error codes directly. The frontend maps them.
- New validators in HR must use `WithErrorCode(HrBusinessErrorCodes.SomeConstant)` or `WithMessage(HrBusinessErrorCodes.SomeConstant)`, never English strings.

---

# Architecture Review Rule

Before introducing a new pattern, contributors must ask:

```txt
Does this violate an existing ADR?
```

If yes:

* Create a new architecture review.
* Document the reason.
* Explicitly supersede the previous ADR.

Do not silently replace approved architecture.

---

## ADR-025 — Publish Integration Events Before SaveChangesAsync

### Status
Accepted (Phase N6 — 2026-06-16)

### Context
HR command handlers published integration events via `IPublishEndpoint.Publish()` **after** `SaveChangesAsync()`. With MassTransit's bus outbox (`UseBusOutbox()`), the outbox transport uses a separate `IScopedDbContextFactory` instance, placing domain changes and outbox messages in separate transactions. If the outbox save failed after the domain save succeeded, events were silently lost.

### Decision
Move all `Publish()` calls to **before** `SaveChangesAsync()` in every HR command handler that publishes integration events. This ensures:
- If the bus outbox transport fails to persist the `OutboxMessage`, the exception propagates before domain changes commit.
- The handler can return an error to the caller, and the client can retry.

### Trade-offs
- Does NOT achieve true transactional outbox (separate DbContext instances), but closes the window of silent event loss.
- Minor behavioral change: integration events are now serialized before domain data is committed. In the unlikely event of a save failure after successful publish, the outbox contains an orphaned message that will be delivered. Consumers must be idempotent (they are — see ADR-026).

---

## ADR-026 — Multi-Layer Consumer Idempotency for Notifications

### Status
Accepted (Phase N6 — 2026-06-16)

### Context
Integration events published by the HR service (leave submitted/approved/rejected/cancelled, overtime created/approved/rejected/cancelled, payslip published/cancelled, payroll run submitted/approved/rejected/finalized) are consumed by the Notification service to create `NotificationHistory` records. Duplicate delivery of the same event (due to broker retries, consumer crashes, or outbox redelivery) must not create duplicate notifications.

### Decision
Implement a 3-layer idempotency strategy:

1. **Transport layer (MassTransit Inbox)** — `InboxState` table with unique constraint on `(MessageId, ConsumerId)` prevents the same message from being delivered to the same consumer type twice.

2. **Application layer (deterministic DeduplicationKey)** — Each consumer builds a deterministic dedup key: `{entity_type}:{entity_id}:{action}:{user_id}`. The `CreateNotificationHandler` checks for an existing record with the same `UserId + DeduplicationKey` before inserting.

3. **Database layer (filtered unique index)** — `NotificationHistory` has a filtered unique index on `(UserId, DeduplicationKey)` that prevents duplicate rows at the database level, with application-level recovery for concurrent insert races.

### Trade-offs
- Slight insert overhead from the dedup pre-check query and index maintenance.
- `DeduplicationKey` is stored on every `NotificationHistory` row, adding storage cost.
- The 3-layer defense ensures correctness under all failure modes (broker redelivery, consumer crash-restart, concurrent inserts).

---

---

## ADR-027 — Three-Scope Architecture Separation

### Status

Approved (2026-06-24)

### Context

Leave and Overtime modules mixed personal data, approval data, and organization-wide data in a single HR page. This violated least-privilege visibility and created no clear pattern for future modules. The Workflow Engine (Phase 28–30) requires a clean separation between who submits, who approves, and who administers.

### Decision

Every business module must explicitly define three scopes before implementation:

#### 1. Employee Scope (ESS)

Route pattern: `/ess/*`

- Personal data only: My Requests, My Balances, My Status
- Self-service operations
- Readonly display of current approver (resolved by Workflow Engine)
- Never: pending approvals, organization-wide data, manual approver selection

#### 2. Approval Scope (Manager)

Route pattern: `/manager/approvals`

- Only items requiring the current user's approval
- Data routed exclusively by Workflow Engine
- Visibility based on approval permissions and workflow responsibilities, never role names
- Must work with all approver types: DirectManager, DepartmentManager, WorkflowRole, Permission-based, SpecificUser, future types

#### 3. HR/Admin Scope (Organization)

Route pattern: `/hr/*`

- Organization-wide data, reporting, administration, configuration
- Must not become an approval inbox
- May observe workflow progress (CurrentApprover, WorkflowStatus) without approval authority

### Key Rule: CanObserveWorkflow ≠ CanApproveWorkflow

- Approval Scope: Can approve, reject, observe own inbox
- HR Scope: May observe workflow state on entity records without approval authority
- Future permission `hr.workflow.view` may govern workflow observation

### Shared Workflow Read Model: IWorkflowQueryService

To avoid duplicating WorkflowInstance → WorkflowInstanceStep → Employee joins across modules, all workflow observation queries must use a shared read model.

**Batch API (avoids N+1):**

```csharp
public interface IWorkflowQueryService
{
    Task<Dictionary<Guid, WorkflowSummaryResponse>> GetWorkflowSummariesAsync(
        string entityType,
        IReadOnlyCollection<Guid> entityIds,
        CancellationToken ct);
}

public sealed record WorkflowSummaryResponse(
    string? CurrentApproverName,
    string? CurrentStepName,
    string? WorkflowStatus);
```

Batch-load for N entity IDs in 1–3 queries instead of N per-entity queries. Consumers call once with all entity IDs from their result set and join the returned dictionary in-memory.

Consumers:
- ESS queries (Leave, Overtime)
- HR queries (Leave, Overtime)
- Future module queries (Recruitment, Transfer, Separation, Probation)

Manager approval handlers may query WorkflowInstance directly since they already load workflow data as part of the pending approvals resolution. All other consumers go through `IWorkflowQueryService`.

### Implications

- Add this rule to AGENTS.md Architecture Rules
- Add scope definitions to every future module specification
- A feature is not complete until all three scopes are defined and verified in browser validation

---

## ADR-028 — Approval Inbox Centralization

### Status

Approved (2026-06-24)

### Context

Without a centralization rule, each business module could create its own approval page (e.g., `/leave/pending`, `/overtime/pending`, `/recruitment/pending`). This fragments the approval experience, duplicates UI code, and forces users to check multiple locations for pending actions.

### Decision

`/manager/approvals` is the single approval inbox for the entire platform.

#### Forbidden patterns

```txt
/leave/pending
/overtime/pending
/recruitment/pending
/probation/pending
/separation/pending
/transfer/pending
```

Business modules may expose only:

- **ESS (`/ess/*`)**: My Requests — user's own submissions
- **HR (`/hr/*`)**: All Records — organization-wide data

Approval inbox belongs exclusively to the Approval Scope.

#### Tab-based entity views

The single inbox uses tabs for entity-specific filtering (Leave, Overtime, Probation, etc.). The "All" tab is the default view, showing everything requiring the user's action.

#### Future unification

Entity-specific manager API endpoints (`/api/hr/manager/approvals/leave`, `/overtime`) are acceptable for the current implementation phase. Future direction is a single unified endpoint:

```txt
GET /api/hr/workflow/pending-approvals
```

with a polymorphic response containing:
```csharp
EntityType          // discriminator
EntityId
WorkflowInstanceId
DisplayTitle
SubmittedBy
SubmittedAt
CurrentStep
Status
```

Do not implement the unified endpoint yet. The entity-specific endpoints remain as a transitional pattern.

#### Future combined permission

Current sidebar visibility logic (`hr.leave.request.approve` OR `hr.overtime.approve`) requires updating the route mapping for each new workflow type. Future direction is a single combined permission:

```txt
approval.inbox.view
```

or

```txt
workflow.approval.view
```

This permission would grant access to the entire `/manager/approvals` approval center without per-entity enumeration in the sidebar. Do not implement yet — existing per-entity permissions remain sufficient for the current phase.

---

## ADR-029 — Workflow Definition-Bound Architecture

### Status

Approved (2026-06-30)

### Context

The Phase 29 spec originally allowed hierarchy-driven workflow building (no WorkflowDefinition needed) for LeaveRequest and OvertimeRequest when no active WorkflowDefinition was found. PayrollRun and RecruitmentRequest required a definition (GetStepCount == 0). This created two issues:

1. **Inconsistent behavior** — some entity types could silently fall back to org hierarchy without a definition, while others could not. This made the system unpredictable in production without seed data.

2. **Definition coupling** — The Workflow Engine's `StartAsync` had a defense-in-depth guard (`IsRequiredEntityType && DefinitionId is null`) that applied to ALL required types, while the builder's check (`RequiresDefinition`) only applied to the subset with GetStepCount == 0. This inconsistency between builder and engine guards created confusion about whether hierarchy fallback was actually allowed for a given type.

### Decision

ALL required business workflow types are definition-bound:

- LeaveRequest
- OvertimeRequest
- PayrollRun
- RecruitmentRequest
- EmployeeTransfer
- EmployeeSeparation
- ProbationRecord

`WorkflowConstants.DefaultPolicy.RequiresDefinition(entityType)` returns `IsRequiredEntityType(entityType)` — not `GetStepCount(entityType) == 0` as the original spec stated. This means every type in `RequiredEntityTypes` must have an active `WorkflowDefinition` before submissions can succeed.

### Why not use GetStepCount == 0?

The spec's original `RequiresDefinition` implementation (checking `GetStepCount == 0`) was correct in intent but effectively meaningless with the engine's defense-in-depth guard. The `BuildFromHierarchyAsync` path was already dead for all required types due to the engine guard. Hardening `RequiresDefinition` to match `IsRequiredEntityType` eliminates the misleading code path and makes the architecture explicit: every required type is definition-bound.

### BuildFromHierarchyAsync Status

`BuildFromHierarchyAsync` is **preview-only / test-only**. It must never be used for production workflow routing. The method is preserved to support:
- Development/testing scenarios without a WorkflowDefinition
- Future non-required entity types that might use hierarchy-based routing

Any new type added to `RequiredEntityTypes` MUST also have a corresponding seed `WorkflowDefinition` in `HrDevSeedData.cs`.

### Guard Architecture

Two independent guards protect the definition-bound rule:

1. **Builder guard** (`WorkflowBuilder.BuildAsync`): If `RequiresDefinition(entityType)` is true and no active definition is found, returns `HR_WF_DEF_REQUIRES_DEFINITION` immediately before attempting hierarchy fallback.

2. **Engine guard** (`WorkflowEngine.StartAsync`): After the builder succeeds, if `IsRequiredEntityType(entityType)` is true and `result.DefinitionId` is null, returns `WorkflowDefinitionRequiresDefinition`. This is defense-in-depth — it catches any path where the builder might produce steps without binding a definition.

### Implications

- Every new workflow-enabled entity type must be added to `RequiredEntityTypes` AND have a seed definition.
- Changes to `RequiresDefinition` / `IsRequiredEntityType` logic require architecture review.
- The `DefaultPolicy.GetStepCount` values are informational only (used for the preview-only hierarchy path).
- `BuildFromHierarchyAsync` may be removed in a future phase if no non-required entity types need it.

---

ANEMOI HR prioritizes:

```txt
Correctness
Auditability
Historical Reproducibility
Maintainability
```

over:

```txt
Premature Optimization
Framework Trends
Short-Term Convenience
```

All future phases should align with these principles.
