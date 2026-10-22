# Audit Log Rules

HR data is sensitive. Important actions must be auditable, and audit records are append-only.

## Actions That Require Audit

```text
Employee personal or bank information changes
Salary view, change, adjustment, or export
Contract view, update, download, or termination
Leave balance adjustment
Force approve or cancel leave request
Department transfer
Position or grade change
Promotion approval
Performance review update
Payroll calculate, approve, lock, or export
Insurance rule activation or deactivation
Sensitive permission assignment
User deactivation
```

## Implementation Status

There is no shared generic `AuditLog` entity yet. Modules implement their own audit records. Existing examples:

- `Anemoi.Hr.Domain.Insurance.InsuranceAuditLog`
- `Anemoi.Hr.Domain.Reporting.ReportExportAuditLog`

When adding a sensitive module, add an audit entity or fields with at least:

```text
Actor user or employee identifier
Action code
Target entity identifier
Reason when required
Old/new values when applicable
Correlation identifier when available
Timestamp
```

Export audit records have a minimum retention of 5 years and must not be deleted casually.

## Rules

- Audit records are append-only. Do not update or delete them from normal application flows.
- Never store secrets, passwords, tokens, or full bank/identity document values in audit records.
- Sensitive permission assignment and force approve/cancel must record the reason.
- Verify the audit row in the database for sensitive actions before reporting completion.
