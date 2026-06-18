# ANEMOI HR - Architecture Decisions Record

IMPORTANT

This file is a mandatory architectural reference.

Before implementing or reviewing any feature, also review:

- docs/architecture/TECHNICAL_DEBT_REGISTER.md

Before changing an existing pattern, verify whether an Architecture Decision Record (ADR) already governs that area.

# ANEMOI HR - Architecture Decisions Record (ADR)

Version: After Phase 19 Approval

Status: Active

Last Updated: 2026-06-15

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

# Conclusion

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
