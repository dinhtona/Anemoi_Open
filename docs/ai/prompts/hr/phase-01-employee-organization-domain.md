# Phase 01 Employee Organization Domain

Use this prompt with Codex.

```text
Read these files first:

- docs/ai/README.md
- docs/ai/core/architecture.md
- docs/ai/core/backend-rules.md
- docs/ai/core/frontend-rules.md when frontend work is included
- docs/ai/core/localization.md
- docs/ai/core/naming-conventions.md
- docs/ai/security/permission-model.md
- docs/ai/security/sensitive-permissions.md
- docs/ai/security/audit-log.md
- docs/ai/workflows/approval-workflow.md
- docs/ai/modules/hr/overview.md
- docs/ai/modules/hr/master-data.md
- docs/ai/modules/hr/employee.md
- docs/ai/modules/hr/leave.md
- docs/ai/modules/hr/payroll.md
- docs/ai/modules/hr/contract.md
- docs/ai/modules/hr/career-path.md

Task:
Implement Step 1 only for Employee & Organization: strongly typed IDs, domain entities, EF configurations, DbContext registration. Entities: Employee, Department, Position, history tables. Stop for review.

Requirements:
- Follow Clean Architecture.
- Follow CQRS.
- Use strongly typed IDs.
- Use Mapperly.
- Use FluentValidation.
- Use permission-based authorization.
- Follow localization rules.
- Consider sensitive permissions and audit logs where applicable.
- Do not jump to the next phase.

Output:
- Files created/modified.
- Assumptions made.
- Items needing review.
- Stop for user review.
```
