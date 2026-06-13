# ANEMOI HR — Phase 20: Insurance Engine

**Status:** FULLY APPROVED  
**Version:** 1.0  
**Date:** 2026-06-13  
**Supersedes:** None  

---

## 1. Overview

Build an Insurance Engine bounded context within `Anemoi.Hr`, following the same architectural pattern as the Tax Engine (Phase 19):

```
Configuration → Calculation → Snapshot → Audit → Reporting
```

The Insurance Engine is an independent bounded context. Payroll consumes Insurance snapshots. Insurance Engine must never depend on Payroll. Historical snapshots must never be mutated.

---

## 2. Architecture

- **Service:** `Anemoi.Hr` (same as Tax Engine)
- **Namespace root:** `Anemoi.Hr.Domain.Insurance`
- **Clean Architecture:** Domain → Application → Infrastructure → API
- **CQRS + MediatR:** All operations through Commands/Queries
- **Permissions:** `hr.insurance.view`, `hr.insurance.manage`, `hr.insurance.calculate`, `hr.insurance.report`

---

## 3. Domain Design

### 3.1 Strongly-Typed IDs

File: `Anemoi.Hr.ModelIds/ModelIds/InsuranceIds.cs`

```csharp
public sealed record InsuranceRuleSetId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record InsuranceContributionRuleId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record InsuranceCalculationSnapshotId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record InsuranceCalculationSnapshotItemId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record InsuranceAuditLogId(Guid Value) : StronglyTypedId<Guid>(Value);
```

### 3.2 InsuranceTypes Constants

File: `Anemoi.Hr.Domain/Insurance/InsuranceTypes.cs`

```csharp
public static class InsuranceTypes
{
    public const string SocialInsurance = "SocialInsurance";
    public const string HealthInsurance = "HealthInsurance";
    public const string UnemploymentInsurance = "UnemploymentInsurance";
}
```

### 3.3 InsuranceRuleSet (Aggregate Root)

File: `Anemoi.Hr.Domain/Insurance/InsuranceRuleSet.cs`

- Extends `Entity<InsuranceRuleSetId>` (has identity, lifecycle, persistence, audit, concurrency)
- Fields:
  - `InsuranceRuleSetId Id`
  - `string CountryCode`
  - `string InsuranceType`
  - `string Name`
  - `string Currency`
  - `DateOnly EffectiveFrom`
  - `DateOnly? EffectiveTo`
  - `string Status` — see `InsuranceRuleSetStatuses` constants
  - `int Version` — auto-incremented per `CountryCode + InsuranceType`
  - `DateTime CreatedAt`, `DateTime UpdatedAt`
  - `string CreatedBy`, `string UpdatedBy`
  - `List<InsuranceContributionRule> ContributionRules`

Business rules:
- Only `Active` rule sets used in calculations
- Active rule sets cannot be edited
- To change an active rule set: clone → Version +1 → activate new version
- No overlapping active rule sets for same `CountryCode + InsuranceType + EffectivePeriod`

### 3.4 InsuranceContributionRule

File: `Anemoi.Hr.Domain/Insurance/InsuranceContributionRule.cs`

- Extends `Entity<InsuranceContributionRuleId>` (has identity, lifecycle, persistence, audit, concurrency)
- Fields:
  - `InsuranceContributionRuleId Id`
  - `InsuranceRuleSetId RuleSetId`
  - `string ContributionType` — e.g. `"Standard"`, `"Additional"`, `"Voluntary"`
  - `decimal EmployeeRate` — e.g. `0.08m` for 8%
  - `decimal EmployerRate` — e.g. `0.175m` for 17.5%
  - `decimal? CeilingAmount` — max insurable salary (null = no cap)
  - `decimal? MinimumAmount` — floor salary (null = no floor)
  - `string SalaryBasis` — e.g. `"GrossSalary"`, `"ContractSalary"`
  - `int SortOrder`
  - `DateTime CreatedAt`, `DateTime UpdatedAt`
  - `InsuranceRuleSet RuleSet` (navigation)

Note: No `ApplicableTo` field. `EmployeeRate` and `EmployerRate` already define applicability.

### 3.5 InsuranceCalculationSnapshot (Immutable)

File: `Anemoi.Hr.Domain/Insurance/InsuranceCalculationSnapshot.cs`

- Extends `Entity<InsuranceCalculationSnapshotId>` (immutable persisted entity, not a ValueObject)
- Fields:
  - `InsuranceCalculationSnapshotId Id`
  - `EmployeeId? EmployeeId`
  - `string CountryCode`
  - `string InsuranceType`
  - `InsuranceRuleSetId RuleSetId`
  - `int RuleSetVersion`
  - `string Currency`
  - `DateOnly CalculationPeriodStart`
  - `DateOnly CalculationPeriodEnd`
  - `decimal InsurableSalarySnapshot`
  - `decimal TotalEmployeeContribution`
  - `decimal TotalEmployerContribution`
  - `decimal TotalContribution`
  - `string RuleSetSnapshotJson`
  - `string CalculationResultJson`
  - `DateTime CalculatedAt`
  - `string CalculatedBy`
  - `string SourceModule`
  - `string? SourceReferenceId` — `string?` (not `Guid?`) to support PayrollRunId, Government Filing, Import Job, Batch Code
  - `List<InsuranceCalculationSnapshotItem> Items`

Snapshot JSON fields:
- `RuleSetSnapshotJson`: frozen rule set metadata (id, country, type, name, effective dates, status, version, rates)
- `CalculationResultJson`: step-by-step calculation per contribution rule

### 3.6 InsuranceCalculationSnapshotItem

File: `Anemoi.Hr.Domain/Insurance/InsuranceCalculationSnapshotItem.cs`

- Extends `Entity<InsuranceCalculationSnapshotItemId>` (persisted entity for queryable breakdown)
- Fields:
  - `InsuranceCalculationSnapshotItemId Id`
  - `InsuranceCalculationSnapshotId SnapshotId`
  - `string InsuranceType`
  - `string ContributionType`
  - `decimal ContributionBase`
  - `decimal EmployeeRate`
  - `decimal EmployerRate`
  - `decimal EmployeeAmount`
  - `decimal EmployerAmount`
  - `decimal TotalAmount`
  - `int SortOrder`
  - `InsuranceCalculationSnapshot Snapshot` (navigation)

### 3.7 InsuranceRuleSetStatuses Constants

File: `Anemoi.Hr.Domain/Insurance/InsuranceRuleSetStatuses.cs`

```csharp
public static class InsuranceRuleSetStatuses
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Inactive = "Inactive";
}
```

### 3.8 InsuranceAuditLog

File: `Anemoi.Hr.Domain/Insurance/InsuranceAuditLog.cs`

Tracks: `Create`, `Update`, `Activate`, `Deactivate`, `Calculate` operations on rule sets. Follows existing audit patterns.

---

## 4. Calculation Flow

```
Input: EmployeeId, CountryCode, InsuranceType
       PeriodStart, PeriodEnd
       GrossSalarySnapshot, ContractSalarySnapshot
       SourceModule, SourceReferenceId

1. Find Active RuleSet (CountryCode + InsuranceType + EffectivePeriod overlap)
2. Load ContributionRules (sorted by SortOrder)
3. For each rule:
   a. Determine ContributionBase:
      - Start with appropriate salary (GrossSalary or ContractSalary)
      - Clamp(ContributionBase, MinimumAmount ?? 0, CeilingAmount ?? infinity)
   b. EmployeeAmount = ContributionBase × EmployeeRate
   c. EmployerAmount = ContributionBase × EmployerRate
   d. TotalAmount = EmployeeAmount + EmployerAmount
4. Create Snapshot + SnapshotItems
5. Persist (never update)
```

Rounding: `MidpointRounding.AwayFromZero`, 2 decimal places.

---

## 5. Database Design

### Tables

| Table | Entity |
|-------|--------|
| `hr_insurance_rule_sets` | InsuranceRuleSet (Entity) |
| `hr_insurance_contribution_rules` | InsuranceContributionRule (Entity) |
| `hr_insurance_calculation_snapshots` | InsuranceCalculationSnapshot (Entity) |
| `hr_insurance_calculation_snapshot_items` | InsuranceCalculationSnapshotItem (Entity) |
| `hr_insurance_audit_logs` | InsuranceAuditLog (Entity) |

### Conventions

- PostgreSQL only
- `xmin` for optimistic concurrency on all tables
- snake_case table and column names
- JSON columns as `.HasColumnType("text")`
- IDs: `.HasConversion(x => x.Value, id => new XxxId(id))`
- Foreign keys with `OnDelete(DeleteBehavior.Cascade)` for child entities

### Indexes

- `hr_insurance_rule_sets`: `(country_code, insurance_type, effective_from, effective_to)`
- `hr_insurance_rule_sets`: `(country_code, insurance_type, status)` — for active lookup
- `hr_insurance_contribution_rules`: `(insurance_rule_set_id, sort_order)`
- `hr_insurance_calculation_snapshots`: `(employee_id, calculation_period_start, calculation_period_end)`
- `hr_insurance_calculation_snapshots`: `(source_module, source_reference_id)`
- `hr_insurance_calculation_snapshot_items`: `(snapshot_id, sort_order)`

---

## 6. API Design

### Base Route

```
api/hr/insurance
```

### Permissions

| Constant | Value |
|----------|-------|
| `HrInsuranceView` | `hr.insurance.view` |
| `HrInsuranceManage` | `hr.insurance.manage` |
| `HrInsuranceCalculate` | `hr.insurance.calculate` |
| `HrInsuranceReport` | `hr.insurance.report` |

### Endpoints

#### Rule Set Management (`hr.insurance.manage`)

| Method | Route | Command/Query |
|--------|-------|---------------|
| GET | `rule-sets?countryCode=&insuranceType=` | `GetInsuranceRuleSetsQuery` |
| GET | `rule-sets/{id}` | `GetInsuranceRuleSetDetailQuery` |
| POST | `rule-sets` | `CreateInsuranceRuleSetCommand` |
| PUT | `rule-sets/{id}` | `UpdateInsuranceRuleSetCommand` |
| POST | `rule-sets/{id}/activate` | `ActivateInsuranceRuleSetCommand` |
| POST | `rule-sets/{id}/deactivate` | `DeactivateInsuranceRuleSetCommand` |

#### Contribution Rule Management (`hr.insurance.manage`)

| Method | Route | Command/Query |
|--------|-------|---------------|
| POST | `rule-sets/{id}/contribution-rules` | `CreateInsuranceContributionRuleCommand` |
| PUT | `contribution-rules/{id}` | `UpdateInsuranceContributionRuleCommand` |
| DELETE | `contribution-rules/{id}` | `DeleteInsuranceContributionRuleCommand` |

#### Calculation (`hr.insurance.calculate`)

| Method | Route | Command |
|--------|-------|---------|
| POST | `calculate` | `CalculateInsuranceCommand` |

#### Snapshots (`hr.insurance.view`)

| Method | Route | Query |
|--------|-------|-------|
| GET | `calculation-snapshots?employeeId=&insuranceType=&sourceModule=&sourceReferenceId=` | `GetInsuranceCalculationSnapshotsQuery` |
| GET | `calculation-snapshots/{id}` | `GetInsuranceCalculationSnapshotDetailQuery` |

#### Reporting (`hr.insurance.report`)

| Method | Route | Query |
|--------|-------|-------|
| GET | `reports/contributions?periodStart=&periodEnd=&countryCode=&insuranceType=` | `GetInsuranceContributionReportQuery` |

---

## 7. Response DTOs

File: `Anemoi.Hr.Application/Responses/InsuranceResponses.cs`

- `InsuranceRuleSetResponse` — with nested `List<InsuranceContributionRuleResponse>`
- `InsuranceContributionRuleResponse`
- `InsuranceCalculationSnapshotResponse` — with nested `List<InsuranceCalculationSnapshotItemResponse>`
- `InsuranceCalculationSnapshotItemResponse`
- `CalculateInsuranceResponse` — SnapshotId, TotalEmployeeContribution, TotalEmployerContribution, TotalContribution, CalculationResultJson
- `CreateInsuranceRuleSetResponse` — InsuranceRuleSetId
- `CreateInsuranceContributionRuleResponse` — InsuranceContributionRuleId

---

## 8. Mapper

File: `Anemoi.Hr.Application/Mappings/InsuranceMapper.cs`

```csharp
[Mapper]
public partial class InsuranceMapper
{
    public InsuranceRuleSetResponse ToInsuranceRuleSetResponse(InsuranceRuleSet entity)    // manual
    public InsuranceContributionRuleResponse ToInsuranceContributionRuleResponse(...)       // manual
    public InsuranceCalculationSnapshotResponse ToInsuranceCalculationSnapshotResponse(...) // manual
    public InsuranceCalculationSnapshotItemResponse ToInsuranceCalculationSnapshotItemResponse(...) // manual
}
```

Manual mapping follows Tax pattern: null checks, `Id.Value.ToString()` for typed IDs.

---

## 9. Error Codes

Prefix: `HR_INSURANCE_`

Examples:
- `HR_INSURANCE_RULE_SET_NOT_FOUND`
- `HR_INSURANCE_RULE_SET_NOT_DRAFT`
- `HR_INSURANCE_RULE_SET_OVERLAPPING_PERIOD`
- `HR_INSURANCE_RULE_SET_NO_ACTIVE_RULES`
- `HR_INSURANCE_CONTRIBUTION_RULE_NOT_FOUND`
- `HR_INSURANCE_CALCULATION_INVALID_PERIOD`
- `HR_INSURANCE_CALCULATION_NEGATIVE_SALARY`
- `HR_INSURANCE_SNAPSHOT_NOT_FOUND`
- `HR_INSURANCE_SAVE_CHANGES_FAILED`

---

## 10. CQRS Structure

### Commands (`Anemoi.Hr.Application/Cqrs/Commands/InsuranceCommands/`)

```
CreateInsuranceRuleSet/        CreateInsuranceRuleSetCommand, Handler, Validator
UpdateInsuranceRuleSet/        UpdateInsuranceRuleSetCommand, Handler, Validator
ActivateInsuranceRuleSet/      ActivateInsuranceRuleSetCommand, Handler, Validator
DeactivateInsuranceRuleSet/    DeactivateInsuranceRuleSetCommand, Handler, Validator

CreateInsuranceContributionRule/   CreateInsuranceContributionRuleCommand, Handler, Validator
UpdateInsuranceContributionRule/   UpdateInsuranceContributionRuleCommand, Handler, Validator
DeleteInsuranceContributionRule/   DeleteInsuranceContributionRuleCommand, Handler, Validator

CalculateInsurance/            CalculateInsuranceCommand, Handler, Validator
```

### Queries (`Anemoi.Hr.Application/Cqrs/Queries/InsuranceQueries/`)

```
GetInsuranceRuleSets/          GetInsuranceRuleSetsQuery, Handler
GetInsuranceRuleSetDetail/     GetInsuranceRuleSetDetailQuery, Handler

GetInsuranceCalculationSnapshots/        GetInsuranceCalculationSnapshotsQuery, Handler
GetInsuranceCalculationSnapshotDetail/   GetInsuranceCalculationSnapshotDetailQuery, Handler

GetInsuranceContributionReport/          GetInsuranceContributionReportQuery, Handler
```

---

## 11. Validator Rules

### InsuranceRuleSet
- `CountryCode`: required, max 16 chars, uppercase trimmed
- `InsuranceType`: required, must be one of InsuranceTypes constants
- `Name`: required, max 128 chars
- `EffectiveFrom`: required
- `EffectiveTo`: must be >= EffectiveFrom if set

### InsuranceContributionRule
- `ContributionType`: required, max 64 chars
- `EmployeeRate`: 0 <= rate <= 1
- `EmployerRate`: 0 <= rate <= 1
- `CeilingAmount`: must be >= 0 if set
- `MinimumAmount`: must be >= 0 if set
- `SalaryBasis`: required
- At least one rate must be > 0

### CalculateInsurance
- `PeriodStart`, `PeriodEnd`: required, Start <= End
- `GrossSalarySnapshot`: >= 0
- `CountryCode`: required
- `InsuranceType`: required

---

## 12. Frontend

Location: `./cody-web-app`

### Pages

```
/[locale]/hr/insurance/rule-sets                — list rule sets
/[locale]/hr/insurance/rule-sets/[id]           — view/edit rule set detail
/[locale]/hr/insurance/calculation-snapshots     — list snapshots
/[locale]/hr/insurance/calculation-snapshots/[id] — view snapshot detail
/[locale]/hr/insurance/reports                   — contribution report (read-only)
```

### Technology

- React Query hooks (service layer)
- shadcn/ui components
- Permission guards (`hr.insurance.*`)
- Existing localization patterns (vi-VN, en-US)

### MVP scope
- CRUD for rule sets and contribution rules
- Snapshot viewing
- Read-only report page
- No dashboard or analytics charts

---

## 13. Permissions Registration

### BuildingBlocks (`Permissions.cs`)

```csharp
public const string HrInsuranceView = "hr.insurance.view";
public const string HrInsuranceManage = "hr.insurance.manage";
public const string HrInsuranceCalculate = "hr.insurance.calculate";
public const string HrInsuranceReport = "hr.insurance.report";
```

### Hr Application (`HrPermissions.cs`)

```csharp
public const string InsuranceView = Permissions.HrInsuranceView;
public const string InsuranceManage = Permissions.HrInsuranceManage;
public const string InsuranceCalculate = Permissions.HrInsuranceCalculate;
public const string InsuranceReport = Permissions.HrInsuranceReport;
```

Add to `All` list. Add `InsuranceManage` to `SensitivePermissions` with risk level `"High"`.

---

## 14. Business Rules Summary

| Rule | Detail |
|------|--------|
| Rule set lookup | Active + CountryCode + InsuranceType + EffectivePeriod overlap |
| Salary basis | GrossSalary or ContractSalary per rule configuration |
| Contribution base | `Clamp(salary, MinimumAmount ?? 0, CeilingAmount ?? INF)` |
| Employee contribution | `ContributionBase × EmployeeRate` per rule |
| Employer contribution | `ContributionBase × EmployerRate` per rule |
| Rounding | 2 decimals, `MidpointRounding.AwayFromZero` |
| Snapshot immutability | Never update after creation |
| Versioning | Rule set Version auto-increments per CountryCode+InsuranceType |
| Active rule editing | Not allowed — must clone and activate new version |

---

## 15. Plan Corrections Applied

The following corrections were applied to this plan per architecture review:

| # | Correction | Detail |
|---|-----------|--------|
| 1 | **Entities, not ValueObjects** | All persisted domain objects now extend `Entity<TId>` instead of `ValueObject` |
| 2 | **InsuranceAuditLog renamed** | `InsuranceCalculationAuditLog` → `InsuranceAuditLog`; table `hr_insurance_audit_logs` |
| 3 | **InsuranceRuleSetStatuses** | Added constants class: Draft, Active, Inactive |
| 4 | **SourceReferenceId as string?** | Kept as `string?` (not `Guid?`) for future flexibility |
| 5 | **InsuranceCalculationSnapshotItem kept** | Required for queryable reporting without JSON parsing |
| 6 | **Two JSON fields only** | `RuleSetSnapshotJson` and `CalculationResultJson` only; no separate contribution/ceiling JSON |

---

## 16. Implementation Order

1. **Domain & ModelIds** — Entities, IDs, InsuranceTypes constants
2. **EF Core Configuration** — Entity mappings, DbContext, migration
3. **Application Layer** — Response DTOs, Mapper, Error Codes, Permissions
4. **CQRS Commands** — Rule set + contribution rule CRUD commands
5. **CQRS Query** — Rule set queries, snapshot queries, report query
6. **Calculation Command** — `CalculateInsuranceCommand` + handler
7. **API Controller** — All endpoints with permission attributes
8. **Tests** — Unit tests for calculation logic, command handlers
9. **Frontend** — Pages, services, hooks, locale files
10. **Verification** — `dotnet build`, `dotnet test`, `npm run lint`, `npm run build`

---

## 17. Verification Gates

All must pass before phase completion:
- `dotnet build` — 0 errors
- `dotnet test` — all existing + new tests pass
- `npm run lint` — 0 errors
- `npm run build` — 0 errors

---

## 18. Future Extensibility

| Need | Built-in Support |
|------|-----------------|
| New countries | New InsuranceRuleSet with CountryCode |
| New insurance types | New InsuranceTypes constant + rule set |
| Voluntary insurance | ContributionType field |
| Government filing | Snapshot data provides full audit trail |
| Payroll integration | SourceModule + SourceReferenceId linking |
| Employer cost reporting | EmployerContribution stored per snapshot |
| Regional ceilings | CeilingAmount with CeilingRule metadata |
