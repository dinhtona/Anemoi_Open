# ANEMOI HR - Technical Debt Register

IMPORTANT

Before working on any technical debt item, read:

- docs/architecture/ARCHITECTURE_DECISIONS.md

Architectural decisions take precedence over technical debt recommendations.

# ANEMOI HR - Technical Debt Register

Version: After Phase 19 Approval

Status: Active

Last Updated: 2026-06-13

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

### Current State

The HR Domain project currently references:

```txt
Anemoi.BuildingBlock.Application
```

Example usage:

```csharp
IdGenerator.NextGuid()
```

inside domain entities.

### Problem

Violates Clean Architecture dependency direction.

Current dependency:

```txt
Domain
  ↓
Application
```

Domain should not depend on Application.

### Risk

* Architectural erosion
* Harder future modularization
* Harder domain isolation
* Harder unit testing

### Recommended Fix

Introduce abstraction:

```csharp
IIdGenerator
```

or

```csharp
IDomainGuidGenerator
```

and inject implementation from outer layers.

### Suggested Target

Architecture Cleanup Phase

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

# Historical Debt Resolved

The following items were discovered during implementation and later resolved.

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