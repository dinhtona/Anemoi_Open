# Phase 20 - Insurance Engine — Implementation Summary

**Status:** FULLY IMPLEMENTED  
**Date:** 2026-06-13  

---

## 1. Overview

Insurance Engine bounded context within `Anemoi.Hr`, following Tax Engine architectural pattern:

```
Configuration → Calculation → Snapshot → Audit → Reporting
```

Independent bounded context. Historical snapshots are immutable.

---

## 2. Architecture

- **Service:** `Anemoi.Hr`
- **Namespace root:** `Anemoi.Hr.Domain.Insurance`
- **Clean Architecture:** Domain → Application → Infrastructure → API
- **CQRS + MediatR:** 8 commands + 5 queries
- **Permissions:** `hr.insurance.view`, `hr.insurance.manage`, `hr.insurance.calculate`, `hr.insurance.report`

---

## 3. Domain Layer

### Strongly Typed IDs (`Anemoi.Hr.ModelIds/ModelIds/`)

| File | Kiểu |
|------|------|
| `InsuranceRuleSetId.cs` | `sealed record InsuranceRuleSetId(Guid Value) : StronglyTypedId<Guid>` |
| `InsuranceContributionRuleId.cs` | `sealed record InsuranceContributionRuleId(Guid Value) : StronglyTypedId<Guid>` |
| `InsuranceCalculationSnapshotId.cs` | `sealed record InsuranceCalculationSnapshotId(Guid Value) : StronglyTypedId<Guid>` |
| `InsuranceCalculationSnapshotItemId.cs` | `sealed record InsuranceCalculationSnapshotItemId(Guid Value) : StronglyTypedId<Guid>` |
| `InsuranceAuditLogId.cs` | `sealed record InsuranceAuditLogId(Guid Value) : StronglyTypedId<Guid>` |

### Constants

- `InsuranceTypes`: `SocialInsurance`, `HealthInsurance`, `UnemploymentInsurance`
- `InsuranceRuleSetStatuses`: `Draft`, `Active`, `Inactive`

### Entities (`Anemoi.Hr.Domain/Insurance/`)

| Entity | Extends | Key Fields |
|--------|---------|------------|
| `InsuranceRuleSet` | `Entity<InsuranceRuleSetId>` | CountryCode, InsuranceType, Name, Currency, EffectiveFrom/To, Status, Version |
| `InsuranceContributionRule` | `Entity<InsuranceContributionRuleId>` | RuleSetId, ContributionType, EmployeeRate, EmployerRate, CeilingAmount, MinimumAmount, SalaryBasis, SortOrder |
| `InsuranceCalculationSnapshot` | `Entity<InsuranceCalculationSnapshotId>` | EmployeeId, CountryCode, InsuranceType, RuleSetId, Version, InsurableSalarySnapshot, TotalEmployee/Employer/TotalContribution, RuleSetSnapshotJson, CalculationResultJson |
| `InsuranceCalculationSnapshotItem` | `Entity<InsuranceCalculationSnapshotItemId>` | SnapshotId, ContributionType, ContributionBase, EmployeeRate, EmployerRate, EmployeeAmount, EmployerAmount, TotalAmount |
| `InsuranceAuditLog` | `Entity<InsuranceAuditLogId>` | RuleSetId, Action, Description, PerformedBy, PerformedAt |

---

## 4. Calculation Logic

```
Input: EmployeeId, CountryCode, InsuranceType, PeriodStart, PeriodEnd, GrossSalarySnapshot
       
1. Find Active RuleSet matching CountryCode + InsuranceType + EffectivePeriod
2. Load ContributionRules sorted by SortOrder
3. For each rule:
   a. ContributionBase = appropriate salary (GrossSalary or ContractSalary)
   b. Clamp(ContributionBase, MinimumAmount ?? 0, CeilingAmount ?? INF)
   c. EmployeeAmount = Round(ContributionBase × EmployeeRate, 2, AwayFromZero)
   d. EmployerAmount = Round(ContributionBase × EmployerRate, 2, AwayFromZero)
4. Create immutable Snapshot + SnapshotItems
5. Store RuleSetSnapshotJson + CalculationResultJson as audit trail
```

---

## 5. Database

### Tables (`hr_` prefix, snake_case, xmin concurrency)

| Table | Cascade |
|-------|---------|
| `hr_insurance_rule_sets` | — |
| `hr_insurance_contribution_rules` | Cascade on RuleSet |
| `hr_insurance_calculation_snapshots` | — |
| `hr_insurance_calculation_snapshot_items` | Cascade on Snapshot |
| `hr_insurance_audit_logs` | SetNull on RuleSet |

### Key Indexes
- Rule sets: `(country_code, insurance_type, effective_from, effective_to)`
- Rule sets: `(country_code, insurance_type, status)` — for active lookup
- Snapshots: `(employee_id, calculation_period_start, calculation_period_end)`
- Snapshots: `(source_module, source_reference_id)` — for Payroll integration

---

## 6. API Endpoints

Base route: `api/hr/insurance`

| Method | Route | Permission |
|--------|-------|------------|
| GET | `rule-sets` | `hr.insurance.view` |
| GET | `rule-sets/{id}` | `hr.insurance.view` |
| POST | `rule-sets` | `hr.insurance.manage` |
| PUT | `rule-sets/{id}` | `hr.insurance.manage` |
| POST | `rule-sets/{id}/activate` | `hr.insurance.manage` |
| POST | `rule-sets/{id}/deactivate` | `hr.insurance.manage` |
| POST | `rule-sets/{id}/contribution-rules` | `hr.insurance.manage` |
| PUT | `contribution-rules/{id}` | `hr.insurance.manage` |
| DELETE | `contribution-rules/{id}` | `hr.insurance.manage` |
| POST | `calculate` | `hr.insurance.calculate` |
| GET | `calculation-snapshots` | `hr.insurance.view` |
| GET | `calculation-snapshots/{id}` | `hr.insurance.view` |
| GET | `reports/contributions` | `hr.insurance.report` |

---

## 7. Frontend

### Pages (6 routes)

| Route | Description |
|-------|-------------|
| `/hr/insurance` | Redirect to rule-sets |
| `/hr/insurance/rule-sets` | List + create + activate/deactivate rule sets |
| `/hr/insurance/rule-sets/[id]` | Detail + manage contribution rules |
| `/hr/insurance/calculation-snapshots` | Browse calculation history |
| `/hr/insurance/calculation-snapshots/[id]` | Snapshot detail with item breakdown |
| `/hr/insurance/reports` | Read-only contribution report |

### Technology
- React Query with cache invalidation
- shadcn/ui components
- Permission guards
- Full en/vi localization

---

## 8. Tests

- **13 unit tests** (179 total backend tests pass)
- Coverage: rule set CRUD, contribution rule CRUD, active rule set edit rejection, overlapping activation rejection, ceiling/minimum/rounding calculation, snapshot+item creation, snapshot/report queries, snapshot detail with items

---

## 9. Verification Gates

| Gate | Result |
|------|--------|
| `dotnet build` | 0 errors |
| `dotnet test` | 179/179 pass |
| `npm run lint` | 0 errors |
| `npm run build` | 6 insurance routes compiled |

---

## 10. Business Rules

| Rule | Detail |
|------|--------|
| Rule set lookup | Active + CountryCode + InsuranceType + EffectivePeriod overlap |
| Salary basis | GrossSalary or ContractSalary per rule |
| Contribution base | `Clamp(salary, min, ceiling)` |
| Rounding | 2 decimals, `MidpointRounding.AwayFromZero` |
| Snapshot immutability | Never update after creation |
| Versioning | Auto-incremented per CountryCode+InsuranceType |
| Active rule editing | Not allowed — clone + activate new version |
