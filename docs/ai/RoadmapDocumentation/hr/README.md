# ANEMOI HR Future Roadmap - Phase 16 to Phase 28

This folder contains planned roadmap/design documents for ANEMOI HR future modules.

## Planned Phases

| Phase | Module | Status |
|---|---|---|
| 16 | Overtime Management | Planned |
| 17 | Shift Management | Planned |
| 18 | Holiday & Working Calendar Engine | Planned |
| 19 | Tax Engine | Planned |
| 20 | Insurance Engine | Planned |
| 21 | Employee Self Service (ESS) | Planned |
| 22 | PDF Payslip & Email Delivery | Planned |
| 23 | Advanced Payroll Reporting | Planned |
| 24 | Workforce Analytics | Planned |
| 25 | Recruitment Management | Planned |
| 26 | Onboarding Management | Planned |
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
