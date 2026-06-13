# ANEMOI HR - Architecture Decisions Record

IMPORTANT

This file is a mandatory architectural reference.

Before implementing or reviewing any feature, also review:

- docs/architecture/TECHNICAL_DEBT_REGISTER.md

Before changing an existing pattern, verify whether an Architecture Decision Record (ADR) already governs that area.

# ANEMOI HR - Architecture Decisions Record (ADR)

Version: After Phase 19 Approval

Status: Active

Last Updated: 2026-06-13

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

currently exists and is tracked as Technical Debt TD-001.

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
