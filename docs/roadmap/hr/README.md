# ANEMOI HR Roadmap - Phase 06 to Phase 28

This folder contains concise roadmap/design documents for ANEMOI HR modules.

Detailed design specs may exist under `docs/specs/`, but this folder is the compact HR roadmap index. Use ADRs and `docs/ai/core/*` for platform-wide rules.

Treat this file as the source for HR roadmap phase names.

## Phase Index

| Phase | Module | Status |
|---|---|---|
| 06 | HR Dashboard | Implemented |
| 07 | Department Transfer | Implemented |
| 08 | Promotion Management | Implemented |
| 09 | Contract Management | Implemented |
| 10 | Compensation Management | Implemented |
| 11 | Payroll MVP | Implemented |
| 12 | Attendance → Payroll Integration | Implemented |
| 13 | Payroll Approval Workflow MVP | Implemented |
| 14 | Payslip MVP | Implemented |
| 15 | Payroll Reporting - Export MVP | Implemented |
| 16 | Overtime Management | Implemented |
| 17 | Shift Management | Implemented |
| 18 | Holiday & Working Calendar Engine | Implemented |
| 19 | Tax Engine | Implemented |
| 20 | Insurance Engine | Implemented |
| 21 | Employee Self Service (ESS) | Implemented |
| 22 | PDF Payslip & Email Delivery | Implemented |
| 23 | Advanced Payroll Reporting | Implemented |
| 24 | Workforce Analytics | Implemented |
| 25 | Recruitment Management | Implemented |
| 26 | Onboarding Management | Implemented |
| 27 | Performance Management | Planned |
| 28 | Training Management | Planned |

Status reflects the code on disk (`Anemoi.Hr.Api/Controllers/` plus the matching
route under `cody-web-app`), not the original phase plan. Verify against code
before relying on a status row.

## Architecture Principles

- Clean Architecture
- CQRS + MediatR
- Domain-first design
- Strongly Typed IDs
- Snapshot-based payroll
- Explicit business validation
- Auditability
- Historical preservation
- Low coupling between bounded contexts

## Note About Approval Workflow

The shared approval framework already exists and is binding:

- ADR-027 — three-scope separation (ESS, Approval, HR/Admin).
- ADR-028 — approval inbox centralized under `/manager/approvals`.
- ADR-029 — every required workflow type is definition-bound; hierarchy fallback
  is not allowed in production.

Module work does not need a new approval phase. A new workflow-enabled entity
type is added to `RequiredEntityTypes` with a seed definition (AGENTS.md §6).

Performance (Phase 27) and Training (Phase 28) are not implemented. Phases 29+
(universal workflow engine, organization hierarchy, employee lifecycle,
employee administration, bulk import) are recorded in `docs/specs/` and
`docs/architecture/ARCHITECTURE_DECISIONS.md`, not in this index.
