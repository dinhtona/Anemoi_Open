# ANEMOI HR Roadmap - Phase 16 to Phase 28

This folder contains concise roadmap/design documents for ANEMOI HR modules.

Detailed design specs, execution plans, and completion reports may exist under `docs/superpowers/`, but this folder is the compact HR roadmap index. Use ADRs and `docs/ai/core/*` for platform-wide rules.

Note: some `docs/superpowers/` files use execution-plan phase numbers that do not match this HR roadmap numbering. Treat this file as the source for HR roadmap phase names.

## Phase Index

| Phase | Module | Status |
|---|---|---|
| 16 | Overtime Management | Implemented |
| 17 | Shift Management | Implemented |
| 18 | Holiday & Working Calendar Engine | Planned |
| 19 | Tax Engine | Planned |
| 20 | Insurance Engine | Implemented |
| 21 | Employee Self Service (ESS) | Planned |
| 22 | PDF Payslip & Email Delivery | Implemented |
| 23 | Advanced Payroll Reporting | Implemented |
| 24 | Workforce Analytics | Designed |
| 25 | Recruitment Management | Implemented |
| 26 | Onboarding Management | Designed |
| 27 | Performance Management | Planned |
| 28 | Training Management | Planned |

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

Several planned modules will benefit from a shared Enterprise Approval Framework.

Strong candidates:

- Overtime
- Shift Change Request
- Recruitment Job Requisition
- Performance Review
- Promotion
- Transfer
- Contract Renewal

A dedicated approval framework phase may be inserted before Recruitment or Performance if needed.
