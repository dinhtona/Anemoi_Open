You are implementing Phase 15 - Payroll Reporting / Export MVP for the ANEMOI HR project.

Project context:

* Clean Architecture
* CQRS + MediatR
* ASP.NET Core
* PostgreSQL
* Domain-first design
* Strongly Typed IDs
* Snapshot-based payroll reporting
* No payroll recalculation during reporting
* Reports must never query AttendanceRecord
* Reports must read only PayrollRun, PayrollItem, and Payslip snapshot data

This task is to finalize and update the approved Phase 15 architecture baseline.

## Final Required Adjustments

### 1. Convert ReportExportAuditLog from ValueObject to Entity

`ReportExportAuditLog` must be implemented as a proper Domain Entity.

Reason:

* It has its own identity.
* It has lifecycle/history meaning.
* It represents an auditable export event.
* It must not inherit from `ValueObject`.

Follow the existing ANEMOI HR Entity pattern, including:

* Strongly Typed ID
* Private constructor if that is the project convention
* Static factory method if existing entities use it
* Domain-first validation style
* No unnecessary generic audit infrastructure

### 2. Reconsider Namespace Placement

Do not place `ReportExportAuditLog` under `Domain/Shared/`.

Prefer one of the following:

* `Domain/Auditing/`
* `Domain/Reporting/`

Choose the better option based on existing ANEMOI HR conventions.

Recommended default:

* Use `Domain/Auditing/` if this entity is considered cross-module export traceability.
* Use `Domain/Reporting/` if the project keeps reporting-related concepts together.

Explain the final decision.

### 3. Rename EF Migration

Rename the migration to a more explicit name.

Preferred names:

* `AddReportExportAuditLog`
* `AddPayrollReportingAudit`

Choose the clearer one based on the actual database changes.

Avoid vague migration names such as `AddPayrollReporting` if the migration only introduces the export audit log table.

### 4. Add Retention Policy Documentation

Add documentation stating:

* Report export audit records must be retained for at least 5 years.
* Future Tax and Insurance modules may rely on export traceability.
* These records should not be deleted casually.
* Any future cleanup job must respect the retention policy.

Place this documentation in the most appropriate location, such as:

* Architecture notes
* Entity XML comments
* Migration comments
* README / module documentation
* Reporting design document

### 5. Keep Approved Option C Design

The final architecture must keep Option C:

* Use `ReportExportAuditLog`
* Use `ModuleCodes` constants
* Use `ReportTypes` constants
* Use one shared export audit table
* Do not introduce generic `AuditLog` infrastructure yet
* Do not create module-specific tables such as `PayrollReportExportAuditLog`
* Avoid table proliferation

## Required Output

Regenerate and provide the following final baseline:

1. Final Architecture
2. Final File Structure
3. Final Entity Definition
4. Final EF Mapping
5. Final Migration Plan
6. Final Implementation Order

## Important Constraints

* Do not redesign the whole payroll reporting module.
* Do not introduce unnecessary abstraction.
* Do not create generic audit logging infrastructure.
* Do not query AttendanceRecord.
* Do not recalculate payroll data during reporting.
* Keep the solution simple, explicit, and compatible with future Tax and Insurance modules.
* Follow existing ANEMOI HR naming, folder, entity, EF mapping, and migration conventions.

This result should be treated as the final approved Phase 15 Payroll Reporting / Export MVP architecture baseline.
