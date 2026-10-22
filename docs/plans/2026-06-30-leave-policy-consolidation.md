# LeavePolicy Consolidation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Merge `MasterDataLeavePolicies` and `LeavePolicies` tables into a single `LeavePolicies` table using the `Domain.MasterData.LeavePolicy` entity, eliminating dual-entity bugs.

**Architecture:** Keep `Domain.MasterData.LeavePolicy` (Entity<TId>, xmin, FK to LeaveType, domain behavior), delete `Domain.Leaves.LeavePolicy` (ValueObject), rename table to `LeavePolicies`, update all navigation/repository references across Domain, Application, API, and Frontend.

**Tech Stack:** .NET 10, EF Core + PostgreSQL, Mapperly, MediatR, Next.js

---

### File Structure Map

```
DELETE:
  Domain/Leaves/LeavePolicy.cs                              # old ValueObject

MODIFY (Domain):
  Domain/Leaves/LeaveBalance.cs                              # LeavePolicy nav type
  Domain/Leaves/LeaveRequest.cs                              # LeavePolicy nav type
  Domain/Leaves/LeaveTransaction.cs                          # LeavePolicy nav type
  Domain/Leaves/LeaveAccrualRun.cs                           # LeavePolicy nav type

MODIFY (Infrastructure):
  Infrastructure/Persistence/HrDbContext.cs                  # DbSet types
  Infrastructure/Configurations/MasterDataModelMapping.cs    # ToTable name
  Infrastructure/Configurations/LeaveModelMapping.cs         # Remove old config + .WithMany()

MODIFY (Application):
  Application/Cqrs/Commands/EssCommands/.../SubmitMyLeaveRequestHandler.cs
  Application/Cqrs/Commands/LeaveRequestCommands/.../SubmitLeaveRequestHandler.cs
  Application/Mappings/EssMapper.cs

MODIFY (API):
  Api/Controllers/Leave/LeaveController.cs                    # Remove duplicate actions
  Api/Services/MonthlyLeaveAccrualWorker.cs                   # Repository type + field access

MODIFY (Infrastructure - Seed):
  Infrastructure/SeedData/HrDevSeedData.cs

CREATE:
  Infrastructure/Migrations/{timestamp}_ConsolidateLeavePolicies.cs

MODIFY (Frontend):
  cody-web-app/src/types/hr/leave.ts
  cody-web-app/src/services/hr/leaveService.ts
```

---

### Task 1: Delete Old ValueObject + Update Domain Navigation Types

**Files:**
- Delete: `Anemoi.Hr.Domain/Leaves/LeavePolicy.cs`
- Modify: `Anemoi.Hr.Domain/Leaves/LeaveBalance.cs`
- Modify: `Anemoi.Hr.Domain/Leaves/LeaveRequest.cs`
- Modify: `Anemoi.Hr.Domain/Leaves/LeaveTransaction.cs`
- Modify: `Anemoi.Hr.Domain/Leaves/LeaveAccrualRun.cs`
- Test: `dotnet build` + `dotnet test`

- [ ] **Step 1: Delete old LeavePolicy ValueObject**

Delete file `Anemoi.Hr.Domain/Leaves/LeavePolicy.cs` (removes ValueObject + List<T> navigation collections).

- [ ] **Step 2: Update LeaveBalance.LeavePolicy navigation type**

In `Anemoi.Hr.Domain/Leaves/LeaveBalance.cs`:

```csharp
// Line 8: add using
using Anemoi.Hr.Domain.MasterData;

// Line 23: change type from old LeavePolicy to new
public LeavePolicy LeavePolicy { get; set; }
```

Remove unused `using Anemoi.Hr.Domain.Leaves;` if no other Leaves types are used in this file (note: `LeaveTransaction` is still from `Domain.Leaves` — check actual imports).

- [ ] **Step 3: Update LeaveRequest.LeavePolicy navigation type**

In `Anemoi.Hr.Domain/Leaves/LeaveRequest.cs`:

```csharp
using Anemoi.Hr.Domain.MasterData;

public LeavePolicy LeavePolicy { get; set; }
```

- [ ] **Step 4: Update LeaveTransaction.LeavePolicy navigation type**

In `Anemoi.Hr.Domain/Leaves/LeaveTransaction.cs`:

```csharp
using Anemoi.Hr.Domain.MasterData;

public LeavePolicy LeavePolicy { get; set; }
```

- [ ] **Step 5: Update LeaveAccrualRun.LeavePolicy navigation type**

In `Anemoi.Hr.Domain/Leaves/LeaveAccrualRun.cs`:

```csharp
using Anemoi.Hr.Domain.MasterData;

public LeavePolicy LeavePolicy { get; set; }
```

- [ ] **Step 6: Verify build**

```bash
dotnet build Anemoi.sln
```
Expected: Build succeeds (compile errors expected from next tasks — infra configs still reference old type).

---

### Task 2: Update EF Configurations

**Files:**
- Modify: `Anemoi.Hr.Infrastructure/Persistence/HrDbContext.cs`
- Modify: `Anemoi.Hr.Infrastructure/Configurations/MasterDataModelMapping.cs`
- Modify: `Anemoi.Hr.Infrastructure/Configurations/LeaveModelMapping.cs`
- Test: `dotnet build`

- [ ] **Step 1: Update HrDbContext.cs**

In `HrDbContext.cs`:

```csharp
// Line 39: change DbSet type from old to new
public DbSet<Anemoi.Hr.Domain.MasterData.LeavePolicy> LeavePolicies { get; set; }

// Line 102: DELETE this line (MasterDataLeavePolicies goes away)
// public DbSet<Anemoi.Hr.Domain.MasterData.LeavePolicy> MasterDataLeavePolicies { get; set; }
```

- [ ] **Step 2: Update MasterDataModelMapping — rename table**

In `MasterDataModelMapping.cs`, `MasterDataLeavePolicyModelMapping.Configure`:

```csharp
// Line 30: change table name
builder.ToTable("LeavePolicies");
```

- [ ] **Step 3: Update LeaveModelMapping — remove old config + fix FKs**

In `LeaveModelMapping.cs`:

Delete the `Configure(EntityTypeBuilder<LeavePolicy> builder)` method (lines 15-28).

Update the remaining 4 `Configure` methods — change `.WithMany(x => x.LeaveBalances)` → `.WithMany()` etc.:

```csharp
// LeaveBalance (line 51-54):
builder.HasOne(x => x.LeavePolicy)
    .WithMany()  // was: .WithMany(x => x.LeaveBalances)
    .HasForeignKey(x => x.LeavePolicyId)
    .OnDelete(DeleteBehavior.Restrict);

// LeaveRequest (line 87-90):
builder.HasOne(x => x.LeavePolicy)
    .WithMany()  // was: .WithMany(x => x.LeaveRequests)
    .HasForeignKey(x => x.LeavePolicyId)
    .OnDelete(DeleteBehavior.Restrict);

// LeaveTransaction (line 119-122):
builder.HasOne(x => x.LeavePolicy)
    .WithMany()  // was: .WithMany(x => x.LeaveTransactions)
    .HasForeignKey(x => x.LeavePolicyId)
    .OnDelete(DeleteBehavior.Restrict);

// LeaveAccrualRun (line 150-153):
builder.HasOne(x => x.LeavePolicy)
    .WithMany()  // was: .WithMany(x => x.LeaveAccrualRuns)
    .HasForeignKey(x => x.LeavePolicyId)
    .OnDelete(DeleteBehavior.Restrict);
```

Remove `using Anemoi.Hr.Domain.Leaves;` (top of file) if no other Leaves types are imported (note: check if `LeaveBalance`, `LeaveRequest`, `LeaveTransaction`, `LeaveAccrualRun` import from `Domain.Leaves` — those must stay).

- [ ] **Step 4: Verify build**

```bash
dotnet build Anemoi.sln
```
Expected: Build succeeds.

---

### Task 3: Update Application Handlers & Worker

**Files:**
- Modify: `Anemoi.Hr.Application/Cqrs/Commands/EssCommands/SubmitMyLeaveRequest/SubmitMyLeaveRequestHandler.cs`
- Modify: `Anemoi.Hr.Application/Cqrs/Commands/LeaveRequestCommands/SubmitLeaveRequest/SubmitLeaveRequestHandler.cs`
- Modify: `Anemoi.Hr.Api/Services/MonthlyLeaveAccrualWorker.cs`
- Modify: `Anemoi.Hr.Application/Mappings/EssMapper.cs`
- Test: `dotnet build` + `dotnet test`

- [ ] **Step 1: Update SubmitMyLeaveRequestHandler**

In `SubmitMyLeaveRequestHandler.cs`:

```csharp
// Change using from:
using Anemoi.Hr.Domain.Leaves;
// To:
using Anemoi.Hr.Domain.MasterData;
```

(Line 13 `using Anemoi.Hr.Domain.Leaves;` — but note `LeaveBalance`, `LeaveRequest`, `LeaveTransaction`, `LeaveRequestStatusCode`, `LeaveBalanceTransactionType` are also from `Domain.Leaves`. These uses must stay. Split the using or add the new one separately.)

```csharp
using Anemoi.Hr.Domain.Leaves;        // keeps LeaveBalance, LeaveRequest, etc.
using Anemoi.Hr.Domain.MasterData;    // adds LeavePolicy
```

The `ISqlRepository<LeavePolicy>` on line 23 will now bind to `Domain.MasterData.LeavePolicy` because of the new using.

- [ ] **Step 2: Update SubmitLeaveRequestHandler**

Same change — add `using Anemoi.Hr.Domain.MasterData;`, keep `using Anemoi.Hr.Domain.Leaves;` for other types.

In `SubmitLeaveRequestHandler.cs`:

```csharp
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.Domain.MasterData;
```

- [ ] **Step 3: Update MonthlyLeaveAccrualWorker**

In `MonthlyLeaveAccrualWorker.cs`:

```csharp
// Change:
using Anemoi.Hr.Domain.Leaves;
// To:
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.Domain.MasterData;
```

Change field access (lines 84-85, 94, 107):

```csharp
// Old:
balance.AccruedDays += policy.MonthlyAccrualDays;
balance.RemainingDays += policy.MonthlyAccrualDays;
// ...
Days = policy.MonthlyAccrualDays,
// ...
AccruedDays = policy.MonthlyAccrualDays,

// New (AnnualEntitlement / 12):
balance.AccruedDays += policy.AnnualEntitlement / 12;
balance.RemainingDays += policy.AnnualEntitlement / 12;
// ...
Days = policy.AnnualEntitlement / 12,
// ...
AccruedDays = policy.AnnualEntitlement / 12,
```

- [ ] **Step 4: Update EssMapper**

In `EssMapper.cs` line 50:

```csharp
// Old:
LeaveTypeCode = balance.LeavePolicy?.LeaveTypeCode,
// New:
LeaveTypeCode = balance.LeavePolicy?.LeaveType?.Code,
```

Keep `using Anemoi.Hr.Domain.Leaves;` — it imports `LeaveBalance`, `LeaveRequest` etc.

- [ ] **Step 5: Verify build**

```bash
dotnet build Anemoi.sln
```
Expected: Build succeeds.

```bash
dotnet test Anemoi.sln
```
Expected: All tests pass.

---

### Task 4: Update Seed Data

**Files:**
- Modify: `Anemoi.Hr.Infrastructure/SeedData/HrDevSeedData.cs`
- Test: `dotnet build`

- [ ] **Step 1: Add LeaveType seed**

In `HrDevSeedData.cs`, add a new seed method and call it before `SeedLeavePolicyAsync`:

Add at line 44 area (after existing static IDs):

```csharp
private static readonly LeaveTypeId AnnualLeaveTypeId =
    new(Guid.Parse("50000000-0000-0000-0000-000000000001"));
```

Add new method:

```csharp
private static async Task SeedLeaveTypesAsync(
    ISqlRepository<LeaveType> repository,
    DateTime now,
    CancellationToken cancellationToken)
{
    const string annualCode = LeaveTypeCode.Annual;
    if (await repository.ExistByConditionAsync(x => x.Code == annualCode, cancellationToken)) return;

    await repository.CreateOneAsync(LeaveType.Create(
        AnnualLeaveTypeId,
        annualCode,
        "Annual Leave",
        true,           // IsPaid
        true,           // RequiresApproval
        15m,            // AnnualEntitlement
        true,           // CarryForwardAllowed
        5m              // MaxCarryForwardDays
    ), cancellationToken);
}
```

In `SeedAsync` method, add `using Anemoi.Hr.Domain.MasterData;` and resolve + call:

```csharp
var leaveTypeRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<LeaveType>>();
await SeedLeaveTypesAsync(leaveTypeRepository, now, cancellationToken);
```

Add the repository resolution before `SeedLeavePolicyAsync`.

- [ ] **Step 2: Update SeedLeavePolicyAsync to use new entity**

Change `SeedLeavePolicyAsync` to create `Domain.MasterData.LeavePolicy`:

```csharp
private static async Task SeedLeavePolicyAsync(
    ISqlRepository<Anemoi.Hr.Domain.MasterData.LeavePolicy> repository,
    DateTime now,
    CancellationToken cancellationToken)
{
    const string annualLeaveCode = "DEV-ANNUAL";
    if (await repository.ExistByConditionAsync(x => x.Code == annualLeaveCode, cancellationToken)) return;

    await repository.CreateOneAsync(
        Anemoi.Hr.Domain.MasterData.LeavePolicy.Create(
            AnnualLeavePolicyId,
            annualLeaveCode,
            "Development Annual Leave",
            AnnualLeaveTypeId,
            "",     // ApplicableGradeCode — empty for all grades
            15m     // AnnualEntitlement
        ), cancellationToken);
}
```

Update the `SeedAsync` method to resolve the new repository type and pass `AnnualLeaveTypeId`:

```csharp
// Old line 66:
var leavePolicyRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<LeavePolicy>>();
// New:
var leavePolicyRepository = serviceScope.ServiceProvider
    .GetRequiredService<ISqlRepository<Anemoi.Hr.Domain.MasterData.LeavePolicy>>();
```

Remove `using Anemoi.Hr.Domain.Leaves;` if it's no longer needed (check if `LeaveBalance` is still used — it is, via `SeedLeaveBalancesAsync`, so keep the import).

- [ ] **Step 3: Update SeedLeaveBalancesAsync — remove old LeavePolicy using**

No change needed — `SeedLeaveBalancesAsync` already references `AnnualLeavePolicyId` by ID, not by navigation property.

- [ ] **Step 4: Verify build**

```bash
dotnet build Anemoi.sln
```
Expected: Build succeeds.

---

### Task 5: Cleanup LeaveController (Remove Duplicate Actions)

**Files:**
- Modify: `Anemoi.Hr.Api/Controllers/Leave/LeaveController.cs`
- Test: `dotnet build`

- [ ] **Step 1: Remove duplicate using directives**

Remove these usings (lines 5-6):

```csharp
// DELETE:
using Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.CreateLeavePolicy;
using Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.UpdateLeavePolicy;
```

Keep these usings (lines 15-16) — they are also used for response types:

```csharp
using Anemoi.Hr.Application.Cqrs.Queries.LeavePolicyQueries.GetLeavePolicies;
using Anemoi.Hr.Application.Cqrs.Queries.LeavePolicyQueries.GetLeavePolicy;
```

Actually wait — we're removing the actions, so we should remove all LeavePolicy-related usings:

```csharp
// DELETE ALL:
using Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.CreateLeavePolicy;
using Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.UpdateLeavePolicy;
using Anemoi.Hr.Application.Cqrs.Queries.LeavePolicyQueries.GetLeavePolicies;
using Anemoi.Hr.Application.Cqrs.Queries.LeavePolicyQueries.GetLeavePolicy;
```

- [ ] **Step 2: Remove LeavePolicy actions**

Remove these method blocks (roughly lines 35-71):

```csharp
// DELETE GetLeavePolicy action (lines 35-42)
// DELETE GetLeavePolicies action (lines 44-51)
// DELETE CreateLeavePolicy action (lines 53-61)
// DELETE UpdateLeavePolicy action (lines 63-71)
```

Keep all other actions (GetLeaveBalance, GetLeaveBalances, AdjustLeaveBalance, GetLeaveRequest, GetLeaveRequests, SubmitLeaveRequest, ApproveLeaveRequest, RejectLeaveRequest, CancelLeaveRequest, ForceApproveLeaveRequest, ForceCancelLeaveRequest, GetLeaveTransactions).

- [ ] **Step 3: Verify build**

```bash
dotnet build Anemoi.sln
```
Expected: Build succeeds.

---

### Task 6: EF Migration

**Files:**
- Create: `Anemoi.Hr.Infrastructure/Migrations/{timestamp}_ConsolidateLeavePolicies.cs`
- Create: `Anemoi.Hr.Infrastructure/Migrations/{timestamp}_ConsolidateLeavePolicies.Designer.cs`
- Modify: `Anemoi.Hr.Infrastructure/Migrations/HrDbContextModelSnapshot.cs`
- Test: `dotnet build`

- [ ] **Step 1: Add EF migration**

```bash
cd Anemoi.Hr/Anemoi.Hr.Infrastructure
dotnet ef migrations add ConsolidateLeavePolicies \
  --startup-project ../Anemoi.Hr.Api \
  --context HrDbContext
```

This generates the migration with automatic schema detection.

- [ ] **Step 2: Edit generated migration to add data migration**

In the generated `{timestamp}_ConsolidateLeavePolicies.cs`, add data migration logic in the `Up` method before the automatic schema changes (or review the auto-generated changes to ensure correctness). Key operations expected from EF detection:
1. Drop FK from `MasterDataLeavePolicies` to `LeaveTypes`
2. Drop old `MasterDataLeavePolicies` table
3. Add columns to `LeavePolicies`: `LeaveTypeId`, `ApplicableGradeCode`, `AnnualEntitlement`, `xmin`
4. Add FK from `LeavePolicies` to `LeaveTypes`
5. Drop old columns: `LeaveTypeCode`, `MonthlyAccrualDays`, `AnnualMaxDays`, `AllowCarryForward`, `MaxCarryForwardDays`

Insert a ` migrationBuilder.Sql(...)` block to migrate data before schema changes that would drop data:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Copy MasterDataLeavePolicies data into LeavePolicies
    migrationBuilder.Sql(@"
        INSERT INTO ""LeavePolicies"" (""Id"", ""Code"", ""Name"", ""LeaveTypeId"", ""ApplicableGradeCode"",
            ""AnnualEntitlement"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"", ""xmin"")
        SELECT m.""Id"", m.""Code"", m.""Name"", m.""LeaveTypeId"", m.""ApplicableGradeCode"",
            m.""AnnualEntitlement"", m.""IsActive"", m.""CreatedAt"", m.""UpdatedAt"", m.""xmin""
        FROM ""MasterDataLeavePolicies"" m
        ON CONFLICT (""Id"") DO NOTHING;
    ");

    // 2. Map old LeavePolicies rows (seed data) — find LeaveTypeId by LeaveTypeCode
    migrationBuilder.Sql(@"
        UPDATE ""LeavePolicies"" lp
        SET
            ""LeaveTypeId"" = lt.""Id"",
            ""AnnualEntitlement"" = CASE
                WHEN lp.""AnnualMaxDays"" > 0 THEN lp.""AnnualMaxDays""
                ELSE lp.""MonthlyAccrualDays"" * 12
            END,
            ""ApplicableGradeCode"" = ''
        FROM ""LeaveTypes"" lt
        WHERE lt.""Code"" = lp.""LeaveTypeCode""
          AND lp.""LeaveTypeId"" IS NULL;
    ");

    // Then let auto-generated schema changes run (DropTable, AlterColumn, AddForeignKey, DropColumn)
    // Review auto-generated migration and ensure column drops come AFTER data copy.
}
```

- [ ] **Step 3: Verify build**

```bash
dotnet build Anemoi.sln
```
Expected: Build succeeds.

---

### Task 7: Frontend — Update ESS Leave Types & Service

**Files:**
- Modify: `cody-web-app/src/types/hr/leave.ts`
- Modify: `cody-web-app/src/services/hr/leaveService.ts`
- Test: `npm run build` or `npx tsc --noEmit`

- [ ] **Step 1: Update LeavePolicy type**

In `cody-web-app/src/types/hr/leave.ts`, update the `LeavePolicy` interface:

```typescript
export interface LeavePolicy {
  id: LeavePolicyId;
  code: string;
  name: string;
  leaveTypeId: string;
  leaveTypeCode: string;
  leaveTypeName: string;
  applicableGradeCode: string;
  annualEntitlement: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}
```

Replace all old fields (`monthlyAccrualDays`, `annualMaxDays`, `allowCarryForward`, `maxCarryForwardDays`) with new fields.

- [ ] **Step 2: Update leaveService endpoint**

In `cody-web-app/src/services/hr/leaveService.ts`, find `getLeavePolicies`:

```typescript
// Old:
getLeavePolicies: async (params: LeavePolicyListParams = {}): Promise<HrPaginatedResponse<LeavePolicy>> => {
    const { data } = await apiClient.get<HrPaginatedResponse<LeavePolicy>>(
      API_ENDPOINTS.hr.leave.policy.list,
      { params: compactParams(params) }
    );
    return data;
  },
```

Update to use the new RESTful endpoint:

```typescript
getLeavePolicies: async (params: LeavePolicyListParams = {}): Promise<HrPaginatedResponse<LeavePolicy>> => {
    const { data } = await apiClient.get<HrPaginatedResponse<LeavePolicy>>(
      API_ENDPOINTS.hr.leavePolicies.list,
      { params }
    );
    return data;
  },
```

Also add a `compactParams` or align params structure with what the new API expects. The new `GET /api/hr/leave-policies` likely uses query params from `GetLeavePoliciesQuery` — check `GetLeavePoliciesQuery.cs` for expected fields.

- [ ] **Step 3: Verify frontend build**

```bash
cd cody-web-app
npm run build
```
or:

```bash
npx tsc --noEmit
```
Expected: TypeScript compilation passes.

---

### Task 8: Integration Verification

- [ ] **Step 1: Rebuild entire solution**

```bash
dotnet build Anemoi.sln
```
Expected: Clean build, no warnings related to LeavePolicy.

- [ ] **Step 2: Run all tests**

```bash
dotnet test Anemoi.sln
```
Expected: All tests pass.

- [ ] **Step 3: Final review**

Run git diff to review all changes:

```bash
git diff --stat
```
Expected: ~15 files changed (4 domain, 3 infra config, 3 app handlers/mapper, 1 API, 1 seed, 2 migration, 2 frontend).

Verify no remaining references to `Domain.Leaves.LeavePolicy`:

```bash
rg "Domain\.Leaves\.LeavePolicy" --type cs
```
Expected: No matches (only `Domain.MasterData.LeavePolicy` remains).

Verify no remaining references to `MasterDataLeavePolicies`:

```bash
rg "MasterDataLeavePolicies" --type cs
```
Expected: No matches (table name changed to `LeavePolicies`).
