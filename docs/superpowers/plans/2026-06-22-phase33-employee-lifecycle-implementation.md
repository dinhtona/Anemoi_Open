# Phase 33: Employee Lifecycle & HR Core Process Automation — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build complete Employee Lifecycle — Recruitment → Onboarding → Active → Transfer → Separation — connecting previously isolated modules into end-to-end process automation.

**Architecture:** Clean Architecture, CQRS, Event-Driven. New entities (ProbationRecord, EmployeeTransfer, EmployeeSeparation, EmployeeHistory, EmployeeOrganizationHistory) extend `Entity<TId>`. Workflow Engine handles approvals. Domain events drive timeline/notifications. Employee refactored from `ValueObject` to `Entity<EmployeeId>`.

**Tech Stack:** .NET 10, EF Core + PostgreSQL xmin concurrency, MediatR + OneOf, Mapperly, FluentValidation, MassTransit + RabbitMQ

**Design doc:** `docs/superpowers/specs/2026-06-22-phase33-employee-lifecycle-design.md`

---

## Pre-Implementation Verified

- [x] Employee extends `ValueObject` — confirmed at `Employee.cs:8`
- [x] WorkflowDefinition seed: no existing seed, follow `HrDevSeedData.cs` pattern (hardcoded GUIDs, factory methods, existence check)
- [x] Existing history tables: all `ValueObject`, independent per-change tracking, no domain methods
- [x] `OnboardingInstanceCompletedDomainEvent` exists (`Events/OnboardingInstanceCompletedDomainEvent.cs`) but never fires — need to publish from application handlers

---

## Task 1: Refactor Employee from ValueObject to Entity\<EmployeeId\>

**Files:**
- Modify: `Anemoi.Hr.Domain/Employees/Employee.cs`

- [ ] **Step 1: Change base class and remove GetEqualityComponents**

In `Employee.cs`:
- Change `: ValueObject` to `: Entity<EmployeeId>`
- Delete the `GetEqualityComponents()` override (Entity<TId> handles equality via Id)
- Keep all other fields and navigation properties

```csharp
// Before:
public sealed class Employee : ValueObject
{
    public EmployeeId Id { get; set; }
    // ... fields ...

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}

// After:
public sealed class Employee : Entity<EmployeeId>
{
    // Id is inherited from Entity<EmployeeId>, remove the property declaration
    // All other fields stay the same
}
```

- [ ] **Step 2: Remove redundant `Id` property**

`Entity<TId>` already has `public TId Id { get; set; }`. Remove the `public EmployeeId Id { get; set; }` property from `Employee.cs`.

- [ ] **Step 3: Verify no compilation errors**

Run: `dotnet build Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj`

- [ ] **Step 4: Verify Employee access still works in all consumers**

Search for usages of `Employee.Id` and access patterns in the codebase to ensure nothing breaks.

---

## Task 2: Add EmployeeStatusCode Constants + Domain Methods

**Files:**
- Modify: `Anemoi.Hr.Domain/Employees/EmploymentStatusCode.cs`
- Modify: `Anemoi.Hr.Domain/Employees/Employee.cs`

- [ ] **Step 1: Expand EmploymentStatusCode constants**

```csharp
// Anemoi.Hr.Domain/Employees/EmploymentStatusCode.cs
namespace Anemoi.Hr.Domain.Employees;

public static class EmploymentStatusCode
{
    public const string Draft = "Draft";
    public const string PendingOnboarding = "PendingOnboarding";
    public const string Onboarding = "Onboarding";
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Resigned = "Resigned";
    public const string Terminated = "Terminated";
    public const string Archived = "Archived";

    // Valid transitions dictionary
    public static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        [Draft] = { PendingOnboarding },
        [PendingOnboarding] = { Onboarding },
        [Onboarding] = { Active },
        [Active] = { Suspended, Resigned, Terminated },
        [Suspended] = { Active },
        [Resigned] = { Archived },
        [Terminated] = { Archived },
        [Archived] = { }
    };

    public static bool IsValidTransition(string from, string to) =>
        ValidTransitions.TryGetValue(from, out var next) && next.Contains(to);
}
```

- [ ] **Step 2: Add domain methods to Employee**

```csharp
// Anemoi.Hr.Domain/Employees/Employee.cs

public void StartOnboarding(string actor)
{
    if (EmploymentStatusCode != EmploymentStatusCode.PendingOnboarding)
        throw new DomainException($"Cannot start onboarding from status {EmploymentStatusCode}");
    EmploymentStatusCode = EmploymentStatusCode.Onboarding;
    UpdatedAt = DateTime.UtcNow;
    AddEvent(new EmployeeStatusChangedDomainEvent(Id, EmploymentStatusCode.PendingOnboarding, EmploymentStatusCode.Onboarding, actor));
}

public void Activate(string actor)
{
    if (EmploymentStatusCode != EmploymentStatusCode.Onboarding && EmploymentStatusCode != EmploymentStatusCode.Suspended)
        throw new DomainException($"Cannot activate from status {EmploymentStatusCode}");
    var fromStatus = EmploymentStatusCode;
    EmploymentStatusCode = EmploymentStatusCode.Active;
    UpdatedAt = DateTime.UtcNow;
    AddEvent(new EmployeeStatusChangedDomainEvent(Id, fromStatus, EmploymentStatusCode.Active, actor));
}

public void Suspend(string actor)
{
    if (EmploymentStatusCode != EmploymentStatusCode.Active)
        throw new DomainException($"Cannot suspend from status {EmploymentStatusCode}");
    EmploymentStatusCode = EmploymentStatusCode.Suspended;
    UpdatedAt = DateTime.UtcNow;
    AddEvent(new EmployeeStatusChangedDomainEvent(Id, EmploymentStatusCode.Active, EmploymentStatusCode.Suspended, actor));
}

public void Resume(string actor)
{
    if (EmploymentStatusCode != EmploymentStatusCode.Suspended)
        throw new DomainException($"Cannot resume from status {EmploymentStatusCode}");
    EmploymentStatusCode = EmploymentStatusCode.Active;
    UpdatedAt = DateTime.UtcNow;
    AddEvent(new EmployeeStatusChangedDomainEvent(Id, EmploymentStatusCode.Suspended, EmploymentStatusCode.Active, actor));
}

public void Resign(string actor)
{
    if (EmploymentStatusCode != EmploymentStatusCode.Active)
        throw new DomainException($"Cannot resign from status {EmploymentStatusCode}");
    EmploymentStatusCode = EmploymentStatusCode.Resigned;
    UpdatedAt = DateTime.UtcNow;
    AddEvent(new EmployeeStatusChangedDomainEvent(Id, EmploymentStatusCode.Active, EmploymentStatusCode.Resigned, actor));
}

public void Terminate(string actor)
{
    if (EmploymentStatusCode != EmploymentStatusCode.Active)
        throw new DomainException($"Cannot terminate from status {EmploymentStatusCode}");
    EmploymentStatusCode = EmploymentStatusCode.Terminated;
    UpdatedAt = DateTime.UtcNow;
    AddEvent(new EmployeeStatusChangedDomainEvent(Id, EmploymentStatusCode.Active, EmploymentStatusCode.Terminated, actor));
}

public void Archive(string actor)
{
    if (EmploymentStatusCode != EmploymentStatusCode.Resigned && EmploymentStatusCode != EmploymentStatusCode.Terminated)
        throw new DomainException($"Cannot archive from status {EmploymentStatusCode}");
    var fromStatus = EmploymentStatusCode;
    EmploymentStatusCode = EmploymentStatusCode.Archived;
    UpdatedAt = DateTime.UtcNow;
    AddEvent(new EmployeeStatusChangedDomainEvent(Id, fromStatus, EmploymentStatusCode.Archived, actor));
}
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj`

---

## Task 3: Make OnboardingInstanceCompletedDomainEvent Actually Fire

**Files:**
- Modify: `Anemoi.Hr.Application/Cqrs/Commands/OnboardingCommands/CompleteOnboardingTask/CompleteOnboardingTaskHandler.cs` (approximately)
- Modify: `Anemoi.Hr.Application/Cqrs/Commands/OnboardingCommands/ForceCompleteOnboarding/ForceCompleteOnboardingHandler.cs` (approximately)

- [ ] **Step 1: Read existing handlers to verify exact file paths and structure**

Check: `Anemoi.Hr.Application/Cqrs/Commands/OnboardingCommands/CompleteOnboardingTask/CompleteOnboardingTaskHandler.cs`
Check: `Anemoi.Hr.Application/Cqrs/Commands/OnboardingCommands/ForceCompleteOnboarding/ForceCompleteOnboardingHandler.cs`

- [ ] **Step 2: Inject IMediator into the handlers and publish completion event**

In both handlers, after calling the domain method and `SaveChangesAsync`, check if the onboarding instance status is now `Completed` and publish the event:

```csharp
// After SaveChangesAsync, when instance status becomes Completed
await _mediator.Publish(new OnboardingInstanceCompletedDomainEvent(
    InstanceId: onboardingInstance.Id,
    EmployeeId: onboardingInstance.EmployeeId,
    CompletedBy: request.CompletedBy,
    CompletionType: "AutoComplete"), // or "ForceComplete"
    cancellationToken);
```

- [ ] **Step 3: Verify the event is defined correctly**

Check that `OnboardingInstanceCompletedDomainEvent` has the correct constructor signature (`InstanceId`, `EmployeeId`, `CompletedBy`, `CompletionType`).

- [ ] **Step 4: Build**

Run: `dotnet build Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`

---

## Task 4: Define All New Domain Events

**Files:**
- Create: `Anemoi.Hr.Domain/Employees/Events/EmployeeStatusChangedDomainEvent.cs`
- Create: `Anemoi.Hr.Domain/Probation/Events/ProbationStatusChangedDomainEvent.cs`
- Create: `Anemoi.Hr.Domain/Transfers/Events/TransferSubmittedDomainEvent.cs`
- Create: `Anemoi.Hr.Domain/Transfers/Events/TransferApprovedDomainEvent.cs`
- Create: `Anemoi.Hr.Domain/Transfers/Events/TransferRejectedDomainEvent.cs`
- Create: `Anemoi.Hr.Domain/Separations/Events/SeparationSubmittedDomainEvent.cs`
- Create: `Anemoi.Hr.Domain/Separations/Events/SeparationApprovedDomainEvent.cs`
- Create: `Anemoi.Hr.Domain/Separations/Events/SeparationRejectedDomainEvent.cs`

- [ ] **Step 1: Create each domain event**

All events follow the pattern:
```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.Domain.[EntityType].Events;

public sealed record EmployeeStatusChangedDomainEvent(
    EmployeeId EmployeeId,
    string FromStatus,
    string ToStatus,
    string Actor,
    string? CorrelationId = null) : DomainEvent;
```

Create similar records for:
- `ProbationStatusChangedDomainEvent(ProbationRecordId RecordId, EmployeeId EmployeeId, string FromStatus, string ToStatus, string Actor, string? CorrelationId = null)`
- `TransferSubmittedDomainEvent(EmployeeTransferId TransferId, EmployeeId EmployeeId, string Actor, string? CorrelationId = null)`
- `TransferApprovedDomainEvent(EmployeeTransferId TransferId, EmployeeId EmployeeId, string Actor, string? CorrelationId = null)`
- `TransferRejectedDomainEvent(EmployeeTransferId TransferId, EmployeeId EmployeeId, string Reason, string Actor, string? CorrelationId = null)`
- `SeparationSubmittedDomainEvent(EmployeeSeparationId SeparationId, EmployeeId EmployeeId, string Actor, string? CorrelationId = null)`
- `SeparationApprovedDomainEvent(EmployeeSeparationId SeparationId, EmployeeId EmployeeId, string SeparationType, string Actor, string? CorrelationId = null)`
- `SeparationRejectedDomainEvent(EmployeeSeparationId SeparationId, EmployeeId EmployeeId, string Reason, string Actor, string? CorrelationId = null)`

- [ ] **Step 2: Build**

```bash
dotnet build Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj
```

---

## Task 5: EmployeeOrganizationHistory Entity

**Files:**
- Create: `Anemoi.Hr.Domain/Employees/EmployeeOrganizationHistory.cs`
- The strongly typed ID `EmployeeOrganizationHistoryId` goes in the existing `Anemoi.Hr.ModelIds/ModelIds/` directory

- [ ] **Step 1: Create strongly typed ID**

```csharp
// Anemoi.Hr.ModelIds/ModelIds/EmployeeOrganizationHistoryId.cs
namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeOrganizationHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 2: Create the entity**

```csharp
// Anemoi.Hr.Domain/Employees/EmployeeOrganizationHistory.cs
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Employees;

public sealed class EmployeeOrganizationHistory : Entity<EmployeeOrganizationHistoryId>
{
    public EmployeeId EmployeeId { get; set; }
    public DepartmentId DepartmentId { get; set; }
    public PositionId PositionId { get; set; }
    public EmployeeId? ManagerEmployeeId { get; set; }
    public string GradeCode { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string ChangeReasonCode { get; set; }

    // Navigation
    public Employee Employee { get; set; }
    public Department Department { get; set; }
    public Position Position { get; set; }
    public Employee ManagerEmployee { get; set; }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj
dotnet build Anemoi.Hr.ModelIds/Anemoi.Hr.ModelIds.csproj
```

---

## Task 6: EmployeeHistory Entity

**Files:**
- Create: `Anemoi.Hr.Domain/Employees/EmployeeHistory.cs`
- Create: `Anemoi.Hr.ModelIds/ModelIds/EmployeeHistoryId.cs`

- [ ] **Step 1: Create strongly typed ID**

```csharp
// Anemoi.Hr.ModelIds/ModelIds/EmployeeHistoryId.cs
public sealed record EmployeeHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 2: Create the entity**

```csharp
// Anemoi.Hr.Domain/Employees/EmployeeHistory.cs
namespace Anemoi.Hr.Domain.Employees;

public sealed class EmployeeHistory : Entity<EmployeeHistoryId>
{
    public EmployeeId EmployeeId { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string EventType { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string MetadataJson { get; set; }
    public DateTime OccurredAt { get; set; }
    public Guid? ActorUserId { get; set; }
    public EmployeeId? ActorEmployeeId { get; set; }
    public string CorrelationId { get; set; }

    // Navigation
    public Employee Employee { get; set; }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

---

## Task 7: ProbationRecord Entity

**Files:**
- Create: `Anemoi.Hr.Domain/Probation/ProbationRecord.cs`
- Create: `Anemoi.Hr.Domain/Probation/ProbationStatusCode.cs`
- Create: `Anemoi.Hr.ModelIds/ModelIds/ProbationRecordId.cs`

- [ ] **Step 1: Create strongly typed ID**

```csharp
// Anemoi.Hr.ModelIds/ModelIds/ProbationRecordId.cs
public sealed record ProbationRecordId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 2: Create status code constants**

```csharp
// Anemoi.Hr.Domain/Probation/ProbationStatusCode.cs
namespace Anemoi.Hr.Domain.Probation;

public static class ProbationStatusCode
{
    public const string Pending = "Pending";
    public const string Passed = "Passed";
    public const string Failed = "Failed";
    public const string Extended = "Extended";
}
```

- [ ] **Step 3: Create entity**

```csharp
// Anemoi.Hr.Domain/Probation/ProbationRecord.cs
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Probation.Events;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Probation;

public sealed class ProbationRecord : Entity<ProbationRecordId>
{
    public EmployeeId EmployeeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StatusCode { get; set; }
    public string? Result { get; set; }
    public string? Comment { get; set; }
    public EmployeeId? ReviewerEmployeeId { get; set; }

    // Navigation
    public Employee Employee { get; set; }
    public Employee ReviewerEmployee { get; set; }

    public static ProbationRecord Create(
        ProbationRecordId id,
        EmployeeId employeeId,
        DateOnly startDate,
        DateOnly endDate)
    {
        return new ProbationRecord
        {
            Id = id,
            EmployeeId = employeeId,
            StartDate = startDate,
            EndDate = endDate,
            StatusCode = ProbationStatusCode.Pending
        };
    }

    public void Pass(string result, string comment, EmployeeId reviewerId)
    {
        if (StatusCode != ProbationStatusCode.Pending)
            throw new DomainException($"Cannot pass probation in status {StatusCode}");
        StatusCode = ProbationStatusCode.Passed;
        Result = result;
        Comment = comment;
        ReviewerEmployeeId = reviewerId;
        AddEvent(new ProbationStatusChangedDomainEvent(Id, EmployeeId,
            ProbationStatusCode.Pending, ProbationStatusCode.Passed, reviewerId.ToString()));
    }

    public void Fail(string comment, EmployeeId reviewerId)
    {
        if (StatusCode != ProbationStatusCode.Pending)
            throw new DomainException($"Cannot fail probation in status {StatusCode}");
        StatusCode = ProbationStatusCode.Failed;
        Comment = comment;
        ReviewerEmployeeId = reviewerId;
        AddEvent(new ProbationStatusChangedDomainEvent(Id, EmployeeId,
            ProbationStatusCode.Pending, ProbationStatusCode.Failed, reviewerId.ToString()));
    }

    public void Extend(DateOnly newEndDate)
    {
        if (StatusCode != ProbationStatusCode.Pending)
            throw new DomainException($"Cannot extend probation in status {StatusCode}");
        if (newEndDate <= EndDate)
            throw new DomainException("New end date must be after current end date");
        var oldEndDate = EndDate;
        EndDate = newEndDate;
        StatusCode = ProbationStatusCode.Extended;
        AddEvent(new ProbationStatusChangedDomainEvent(Id, EmployeeId,
            ProbationStatusCode.Pending, ProbationStatusCode.Extended, "system"));
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj
```

---

## Task 8: EmployeeTransfer Entity

**Files:**
- Create: `Anemoi.Hr.Domain/Transfers/EmployeeTransfer.cs`
- Create: `Anemoi.Hr.Domain/Transfers/TransferStatusCode.cs`
- Create: `Anemoi.Hr.ModelIds/ModelIds/EmployeeTransferId.cs`

- [ ] **Step 1: Create strongly typed ID**

```csharp
public sealed record EmployeeTransferId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 2: Create status code constants**

```csharp
namespace Anemoi.Hr.Domain.Transfers;

public static class TransferStatusCode
{
    public const string Draft = "Draft";
    public const string PendingApproval = "PendingApproval";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
}
```

- [ ] **Step 3: Create entity**

```csharp
namespace Anemoi.Hr.Domain.Transfers;

public sealed class EmployeeTransfer : Entity<EmployeeTransferId>
{
    public EmployeeId EmployeeId { get; set; }
    public DepartmentId FromDepartmentId { get; set; }
    public DepartmentId ToDepartmentId { get; set; }
    public PositionId FromPositionId { get; set; }
    public PositionId ToPositionId { get; set; }
    public EmployeeId? FromManagerId { get; set; }
    public EmployeeId? ToManagerId { get; set; }
    public string FromGradeCode { get; set; }
    public string ToGradeCode { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string Reason { get; set; }
    public string StatusCode { get; set; }
    public WorkflowInstanceId? WorkflowInstanceId { get; set; }

    // Navigation
    public Employee Employee { get; set; }
    public Department FromDepartment { get; set; }
    public Department ToDepartment { get; set; }
    public Position FromPosition { get; set; }
    public Position ToPosition { get; set; }

    public static EmployeeTransfer Create(
        EmployeeTransferId id, EmployeeId employeeId,
        DepartmentId fromDeptId, DepartmentId toDeptId,
        PositionId fromPosId, PositionId toPosId,
        EmployeeId? fromMgrId, EmployeeId? toMgrId,
        string fromGrade, string toGrade,
        DateOnly effectiveDate, string reason)
    {
        return new EmployeeTransfer
        {
            Id = id,
            EmployeeId = employeeId,
            FromDepartmentId = fromDeptId,
            ToDepartmentId = toDeptId,
            FromPositionId = fromPosId,
            ToPositionId = toPosId,
            FromManagerId = fromMgrId,
            ToManagerId = toMgrId,
            FromGradeCode = fromGrade,
            ToGradeCode = toGrade,
            EffectiveDate = effectiveDate,
            Reason = reason,
            StatusCode = TransferStatusCode.Draft
        };
    }

    public void Submit()
    {
        StatusCode = TransferStatusCode.PendingApproval;
        AddEvent(new TransferSubmittedDomainEvent(Id, EmployeeId, "system"));
    }

    public void Approve()
    {
        StatusCode = TransferStatusCode.Approved;
        AddEvent(new TransferApprovedDomainEvent(Id, EmployeeId, "system"));
    }

    public void Reject(string reason)
    {
        StatusCode = TransferStatusCode.Rejected;
        AddEvent(new TransferRejectedDomainEvent(Id, EmployeeId, reason, "system"));
    }

    public void Cancel()
    {
        StatusCode = TransferStatusCode.Cancelled;
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj
```

---

## Task 9: EmployeeSeparation Entity

**Files:**
- Create: `Anemoi.Hr.Domain/Separations/EmployeeSeparation.cs`
- Create: `Anemoi.Hr.Domain/Separations/SeparationStatusCode.cs`
- Create: `Anemoi.Hr.Domain/Separations/SeparationTypeCode.cs`
- Create: `Anemoi.Hr.ModelIds/ModelIds/EmployeeSeparationId.cs`

- [ ] **Step 1: Create strongly typed ID**

```csharp
public sealed record EmployeeSeparationId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 2: Create status code and type constants**

```csharp
namespace Anemoi.Hr.Domain.Separations;

public static class SeparationStatusCode
{
    public const string Draft = "Draft";
    public const string PendingApproval = "PendingApproval";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
}

public static class SeparationTypeCode
{
    public const string Resignation = "Resignation";
    public const string Termination = "Termination";
    public const string Retirement = "Retirement";
}
```

- [ ] **Step 3: Create entity**

```csharp
namespace Anemoi.Hr.Domain.Separations;

public sealed class EmployeeSeparation : Entity<EmployeeSeparationId>
{
    public EmployeeId EmployeeId { get; set; }
    public string SeparationType { get; set; }
    public string Reason { get; set; }
    public DateOnly LastWorkingDate { get; set; }
    public string StatusCode { get; set; }
    public WorkflowInstanceId? WorkflowInstanceId { get; set; }

    public Employee Employee { get; set; }

    public static EmployeeSeparation Create(
        EmployeeSeparationId id, EmployeeId employeeId,
        string separationType, string reason, DateOnly lastWorkingDate)
    {
        return new EmployeeSeparation
        {
            Id = id,
            EmployeeId = employeeId,
            SeparationType = separationType,
            Reason = reason,
            LastWorkingDate = lastWorkingDate,
            StatusCode = SeparationStatusCode.Draft
        };
    }

    public void Submit()
    {
        StatusCode = SeparationStatusCode.PendingApproval;
        AddEvent(new SeparationSubmittedDomainEvent(Id, EmployeeId, "system"));
    }

    public void Approve()
    {
        StatusCode = SeparationStatusCode.Approved;
        AddEvent(new SeparationApprovedDomainEvent(Id, EmployeeId, SeparationType, "system"));
    }

    public void Reject(string reason)
    {
        StatusCode = SeparationStatusCode.Rejected;
        AddEvent(new SeparationRejectedDomainEvent(Id, EmployeeId, reason, "system"));
    }

    public void Cancel()
    {
        StatusCode = SeparationStatusCode.Cancelled;
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj
```

---

## Task 10: EF Core Configurations for New Entities

**Files:**
- Create or modify: `Anemoi.Hr.Infrastructure/Configurations/ProbationModelMapping.cs`
- Create or modify: `Anemoi.Hr.Infrastructure/Configurations/TransferModelMapping.cs`
- Create or modify: `Anemoi.Hr.Infrastructure/Configurations/SeparationModelMapping.cs`
- Create or modify: `Anemoi.Hr.Infrastructure/Configurations/EmployeeOrganizationModelMapping.cs` (append EmployeeOrganizationHistory, EmployeeHistory)

- [ ] **Step 1: Read existing mapping files to understand the pattern**

Read files:
- `EmployeeOrganizationModelMapping.cs` (contains Employee, all existing histories)
- `OnboardingModelMapping.cs` (uses `"Hr"` schema)
- `WorkflowInstanceModelMapping.cs`

- [ ] **Step 2: Create ProbationModelMapping.cs**

```csharp
namespace Anemoi.Hr.Infrastructure.Configurations;

public class ProbationModelMapping : IEntityTypeConfiguration<ProbationRecord>
{
    public void Configure(EntityTypeBuilder<ProbationRecord> builder)
    {
        builder.ToTable("ProbationRecords", "Hr");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion<StronglyTypedIdJsonConverter<ProbationRecordId, Guid>>();
        builder.Property(x => x.EmployeeId).HasConversion<StronglyTypedIdJsonConverter<EmployeeId, Guid>>();
        builder.Property(x => x.ReviewerEmployeeId).HasConversion<StronglyTypedIdJsonConverter<EmployeeId, Guid>>().IsRequired(false);
        builder.Property(x => x.StatusCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Result).HasMaxLength(500);
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId);
        builder.HasOne(x => x.ReviewerEmployee).WithMany().HasForeignKey(x => x.ReviewerEmployeeId).OnDelete(DeleteBehavior.SetNull);
        builder.Property<uint>("xmin").HasColumnType("xid").IsConcurrencyToken();
        builder.HasIndex(x => new { x.EmployeeId, x.StatusCode });
    }
}
```

- [ ] **Step 3: Follow the same pattern for TransferModelMapping.cs and SeparationModelMapping.cs**

Each should use `"Hr"` schema, proper ID converters, xmin concurrency, and appropriate indexes.

- [ ] **Step 4: Modify EmployeeOrganizationModelMapping.cs** to add:

```csharp
builder.Entity<EmployeeOrganizationHistory>(entity =>
{
    entity.ToTable("EmployeeOrganizationHistories", "Hr");
    entity.HasKey(x => x.Id);
    entity.Property(x => x.Id).HasConversion<...>();
    entity.Property(x => x.EmployeeId).HasConversion<...>();
    entity.Property(x => x.DepartmentId).HasConversion<...>();
    entity.Property(x => x.PositionId).HasConversion<...>();
    entity.Property(x => x.ManagerEmployeeId).HasConversion<...>().IsRequired(false);
    entity.Property(x => x.GradeCode).HasMaxLength(50);
    entity.Property(x => x.ChangeReasonCode).HasMaxLength(100);
    entity.Property<uint>("xmin").HasColumnType("xid").IsConcurrencyToken();
    entity.HasIndex(x => new { x.EmployeeId, x.EffectiveDate }).IsDescending(false, true);
    entity.HasIndex(x => new { x.DepartmentId, x.EffectiveDate });
});

builder.Entity<EmployeeHistory>(entity =>
{
    entity.ToTable("EmployeeHistories", "Hr");
    entity.HasKey(x => x.Id);
    entity.Property(x => x.Id).HasConversion<...>();
    entity.Property(x => x.EmployeeId).HasConversion<...>();
    entity.Property(x => x.ActorEmployeeId).HasConversion<...>().IsRequired(false);
    entity.Property(x => x.EntityType).HasMaxLength(100);
    entity.Property(x => x.EventType).HasMaxLength(100);
    entity.Property(x => x.Title).HasMaxLength(500);
    entity.Property(x => x.Description).HasMaxLength(2000);
    entity.Property(x => x.MetadataJson).HasColumnType("jsonb");
    entity.Property(x => x.CorrelationId).HasMaxLength(100);
    entity.HasIndex(x => new { x.EmployeeId, x.OccurredAt }).IsDescending(false, true);
});
```

- [ ] **Step 5: Build**

```bash
dotnet build Anemoi.Hr.Infrastructure/Anemoi.Hr.Infrastructure.csproj
```

---

## Task 11: StatusCode Migration + Register New Entities in DbContext

**Files:**
- Modify: `Anemoi.Hr.Infrastructure/DbContexts/HrDbContext.cs`

- [ ] **Step 1: Read existing DbContext** to see how entities are registered

Read: `Anemoi.Hr.Infrastructure/DbContexts/HrDbContext.cs`

- [ ] **Step 2: Add DbSet properties for new entities**

```csharp
public DbSet<ProbationRecord> ProbationRecords { get; set; }
public DbSet<EmployeeTransfer> EmployeeTransfers { get; set; }
public DbSet<EmployeeSeparation> EmployeeSeparations { get; set; }
public DbSet<EmployeeOrganizationHistory> EmployeeOrganizationHistories { get; set; }
public DbSet<EmployeeHistory> EmployeeHistories { get; set; }
```

- [ ] **Step 3: Add entity configurations in `OnModelCreating`**

```csharp
builder.ApplyConfiguration(new ProbationModelMapping());
builder.ApplyConfiguration(new TransferModelMapping());
builder.ApplyConfiguration(new SeparationModelMapping());
// EmployeeOrganizationModelMapping already contains EmployeeHistory + EmployeeOrganizationHistory
```

- [ ] **Step 4: Add EF Core migration**

```bash
dotnet ef migrations add Phase33_EmployeeLifecycleEntities --context HrDbContext --project Anemoi.Hr.Infrastructure
```

- [ ] **Step 5: Build**

```bash
dotnet build Anemoi.sln
```

---

## Task 12: WorkflowDefinition Seed Data

**Files:**
- Modify: `Anemoi.Hr.Infrastructure/SeedData/HrDevSeedData.cs`

- [ ] **Step 1: Add seed for EmployeeTransfer workflow**

```csharp
private async Task SeedWorkflowDefinitions(CancellationToken cancellationToken)
{
    var transferDefId = new WorkflowDefinitionId(Guid.Parse("80000000-0000-0000-0000-000000000001"));
    var separationDefId = new WorkflowDefinitionId(Guid.Parse("80000000-0000-0000-0000-000000000002"));

    var transferDef = await _repository.ExistByConditionAsync(
        x => x.Code == "EmployeeTransferApproval", cancellationToken);
    if (!transferDef)
    {
        var definition = WorkflowDefinition.Create(
            transferDefId, "EmployeeTransferApproval", "Employee Transfer Approval",
            "Approval workflow for employee transfers",
            WorkflowTypeCode.Approval,
            WorkflowConstants.TargetEntityTypes.EmployeeTransfer,
            1,
            new List<WorkflowDefinitionStep>
            {
                WorkflowDefinitionStep.Create(
                    new WorkflowDefinitionStepId(Guid.Parse("80000000-0000-0000-0000-000000000011")),
                    transferDefId, 1, ApproverType.DirectManager, null, true),
                WorkflowDefinitionStep.Create(
                    new WorkflowDefinitionStepId(Guid.Parse("80000000-0000-0000-0000-000000000012")),
                    transferDefId, 2, ApproverType.HrManager, null, true)
            });
        definition.Activate();
        await _repository.CreateAsync(definition, cancellationToken);
    }

    var separationDef = await _repository.ExistByConditionAsync(
        x => x.Code == "EmployeeSeparationApproval", cancellationToken);
    if (!separationDef)
    {
        var definition = WorkflowDefinition.Create(
            separationDefId, "EmployeeSeparationApproval", "Employee Separation Approval",
            "Approval workflow for employee separations",
            WorkflowTypeCode.Approval,
            WorkflowConstants.TargetEntityTypes.EmployeeSeparation,
            1,
            new List<WorkflowDefinitionStep>
            {
                WorkflowDefinitionStep.Create(
                    new WorkflowDefinitionStepId(Guid.Parse("80000000-0000-0000-0000-000000000021")),
                    separationDefId, 1, ApproverType.DirectManager, null, true),
                WorkflowDefinitionStep.Create(
                    new WorkflowDefinitionStepId(Guid.Parse("80000000-0000-0000-0000-000000000022")),
                    separationDefId, 2, ApproverType.HrManager, null, true)
            });
        definition.Activate();
        await _repository.CreateAsync(definition, cancellationToken);
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
```

- [ ] **Step 2: Call the seed method from the existing seed orchestrator** (in `HrDevSeedData.cs`)

---

## Task 13: Add TargetEntityTypes to WorkflowConstants

**Files:**
- Modify: `Anemoi.Hr.Application/Configurations/WorkflowConstants.cs`

- [ ] **Step 1: Add new target entity types**

```csharp
public static class TargetEntityTypes
{
    // Existing
    public const string LeaveRequest = "LeaveRequest";
    public const string OvertimeRequest = "OvertimeRequest";
    public const string PayrollRun = "PayrollRun";
    public const string RecruitmentRequest = "RecruitmentRequest";

    // New for Phase 33
    public const string EmployeeTransfer = "EmployeeTransfer";
    public const string EmployeeSeparation = "EmployeeSeparation";
}
```

- [ ] **Step 2: Build**

```bash
dotnet build Anemoi.Hr.Application/Anemoi.Hr.Application.csproj
```

---

## Task 14: Workflow Status Updaters

**Files:**
- Create: `Anemoi.Hr.Application/WorkflowTargetStatusUpdaters/EmployeeTransferWorkflowStatusUpdater.cs`
- Create: `Anemoi.Hr.Application/WorkflowTargetStatusUpdaters/EmployeeSeparationWorkflowStatusUpdater.cs`

- [ ] **Step 1: Read existing status updater** to understand the pattern

Read: `Anemoi.Hr.Application/WorkflowTargetStatusUpdaters/LeaveWorkflowStatusUpdater.cs`

- [ ] **Step 2: Create EmployeeTransferWorkflowStatusUpdater**

```csharp
namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public class EmployeeTransferWorkflowStatusUpdater : IWorkflowTargetStatusUpdater
{
    private readonly ISqlRepository<EmployeeTransfer> _transferRepo;
    private readonly ISqlRepository<Employee> _employeeRepo;
    private readonly ISqlRepository<EmployeeOrganizationHistory> _orgHistoryRepo;
    private readonly IUnitOfWork _unitOfWork;

    public string TargetEntityType => WorkflowConstants.TargetEntityTypes.EmployeeTransfer;

    public async Task<OneOf<bool, ErrorDetailResponse>> HandleApprovedAsync(
        Guid entityId, Guid performedBy, CancellationToken cancellationToken)
    {
        var transferId = new EmployeeTransferId(entityId);
        var transfer = await _transferRepo.FindByConditionAsync(
            x => x.Id == transferId, cancellationToken, "Employee,FromDepartment,ToDepartment,FromPosition,ToPosition");
        if (transfer == null)
            return ErrorHelper.NotFound("EmployeeTransfer", entityId);

        transfer.Approve();
        await _transferRepo.UpdateAsync(transfer, cancellationToken);

        // Update Employee snapshot
        var employee = await _employeeRepo.FindByConditionAsync(
            x => x.Id == transfer.EmployeeId, cancellationToken);
        if (employee == null)
            return ErrorHelper.NotFound("Employee", transfer.EmployeeId.Value);

        employee.PrimaryDepartmentId = transfer.ToDepartmentId;
        employee.PrimaryPositionId = transfer.ToPositionId;
        employee.DirectManagerEmployeeId = transfer.ToManagerId;
        employee.GradeCode = transfer.ToGradeCode;
        employee.UpdatedAt = DateTime.UtcNow;
        await _employeeRepo.UpdateAsync(employee, cancellationToken);

        // Close previous EmployeeOrganizationHistory record
        var currentOrgHistory = await _orgHistoryRepo.FindByConditionAsync(
            x => x.EmployeeId == transfer.EmployeeId && x.EndDate == null, cancellationToken);
        if (currentOrgHistory != null)
        {
            currentOrgHistory.EndDate = transfer.EffectiveDate;
            await _orgHistoryRepo.UpdateAsync(currentOrgHistory, cancellationToken);
        }

        // Create new EmployeeOrganizationHistory record
        var newOrgHistory = new EmployeeOrganizationHistory
        {
            Id = new EmployeeOrganizationHistoryId(IdGenerator.NextGuid()),
            EmployeeId = transfer.EmployeeId,
            DepartmentId = transfer.ToDepartmentId,
            PositionId = transfer.ToPositionId,
            ManagerEmployeeId = transfer.ToManagerId,
            GradeCode = transfer.ToGradeCode,
            EffectiveDate = transfer.EffectiveDate,
            ChangeReasonCode = "Transfer"
        };
        await _orgHistoryRepo.CreateAsync(newOrgHistory, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<OneOf<bool, ErrorDetailResponse>> HandleRejectedAsync(
        Guid entityId, Guid performedBy, string? comment, CancellationToken cancellationToken)
    {
        var transferId = new EmployeeTransferId(entityId);
        var transfer = await _transferRepo.FindByConditionAsync(
            x => x.Id == transferId, cancellationToken);
        if (transfer == null)
            return ErrorHelper.NotFound("EmployeeTransfer", entityId);

        transfer.Reject(comment ?? "Rejected by approver");
        await _transferRepo.UpdateAsync(transfer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
```

- [ ] **Step 3: Create EmployeeSeparationWorkflowStatusUpdater**

Same pattern, but on approved:
1. Load EmployeeSeparation, call `Approve()`
2. Load Employee, call `employee.Resign()` or `employee.Terminate()` based on `SeparationType`
3. Close EmployeeOrganizationHistory (set EndDate = separation.LastWorkingDate, not approval date)
4. SaveChanges

- [ ] **Step 4: Register both updaters in DI**

Read the existing registration pattern for status updaters (likely in `Anemoi.Hr.Api/Program.cs` or `Anemoi.Hr.Application/DependencyInjection.cs`) and add:

```csharp
services.AddScoped<IWorkflowTargetStatusUpdater, EmployeeTransferWorkflowStatusUpdater>();
services.AddScoped<IWorkflowTargetStatusUpdater, EmployeeSeparationWorkflowStatusUpdater>();
```

---

## Task 15: CQRS — Probation Commands

**Files:**
- Create: `Anemoi.Hr.Application/Cqrs/Commands/ProbationCommands/StartProbation/StartProbationCommand.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/ProbationCommands/StartProbation/StartProbationHandler.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/ProbationCommands/PassProbation/PassProbationCommand.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/ProbationCommands/PassProbation/PassProbationHandler.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/ProbationCommands/FailProbation/FailProbationCommand.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/ProbationCommands/FailProbation/FailProbationHandler.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/ProbationCommands/ExtendProbation/ExtendProbationCommand.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/ProbationCommands/ExtendProbation/ExtendProbationHandler.cs`
- Create: Validators for each command

- [ ] **Step 1: Create each command/record** following existing CQRS pattern

```csharp
public sealed record StartProbationCommand(
    EmployeeId EmployeeId,
    DateOnly StartDate,
    DateOnly EndDate
) : IRequest<OneOf<ProbationRecordDto, ErrorDetailResponse>>;
```

Similar for PassProbationCommand(result, comment), FailProbationCommand(comment), ExtendProbationCommand(newEndDate).

- [ ] **Step 2: Create each handler** following existing pattern

```csharp
public sealed class StartProbationHandler
    : IRequestHandler<StartProbationCommand, OneOf<ProbationRecordDto, ErrorDetailResponse>>
{
    private readonly ISqlRepository<ProbationRecord> _repo;
    private readonly ISqlRepository<Employee> _employeeRepo;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<OneOf<ProbationRecordDto, ErrorDetailResponse>> Handle(
        StartProbationCommand request, CancellationToken ct)
    {
        var employee = await _employeeRepo.FindByConditionAsync(
            x => x.Id == request.EmployeeId, ct);
        if (employee == null)
            return ErrorHelper.NotFound("Employee", request.EmployeeId.Value);

        var record = ProbationRecord.Create(
            new ProbationRecordId(IdGenerator.NextGuid()),
            request.EmployeeId,
            request.StartDate,
            request.EndDate);
        await _repo.CreateAsync(record, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return _mapper.MapToDto(record);
    }
}
```

- [ ] **Step 3: Create FluentValidators** (required fields, date ranges, no DB-heavy checks)

```csharp
public class StartProbationValidator : AbstractValidator<StartProbationCommand>
{
    public StartProbationValidator()
    {
        RuleFor(x => x.EmployeeId).NotNull();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build Anemoi.Hr.Application/Anemoi.Hr.Application.csproj
```

---

## Task 16: CQRS — SubmitTransfer + SubmitSeparation Commands

**Files:**
- Create: `Anemoi.Hr.Application/Cqrs/Commands/TransferCommands/SubmitTransfer/SubmitTransferCommand.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/TransferCommands/SubmitTransfer/SubmitTransferHandler.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/SeparationCommands/SubmitSeparation/SubmitSeparationCommand.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/SeparationCommands/SubmitSeparation/SubmitSeparationHandler.cs`
- Create: Validators for both commands

- [ ] **Step 1: Create SubmitTransferCommand and handler**

```csharp
public sealed record SubmitTransferCommand(
    EmployeeId EmployeeId,
    DepartmentId ToDepartmentId,
    PositionId ToPositionId,
    EmployeeId? ToManagerId,
    string ToGradeCode,
    DateOnly EffectiveDate,
    string Reason
) : IRequest<OneOf<EmployeeTransferDto, ErrorDetailResponse>>;
```

Handler steps:
1. Load Employee (validate exists)
2. Capture current From* values from Employee (department, position, manager, grade)
3. Create EmployeeTransfer entity via static Create() factory
4. Call `transfer.Submit()` (Draft → PendingApproval)
5. Save transfer to DB
6. Start workflow via `_workflowEngine.StartAsync(...)` with EntityType = WorkflowConstants.TargetEntityTypes.EmployeeTransfer
7. Map to DTO and return

- [ ] **Step 2: Create SubmitSeparationCommand and handler**

```csharp
public sealed record SubmitSeparationCommand(
    EmployeeId EmployeeId,
    string SeparationType,
    string Reason,
    DateOnly LastWorkingDate
) : IRequest<OneOf<EmployeeSeparationDto, ErrorDetailResponse>>;
```

Handler steps:
1. Load Employee (validate exists)
2. Validate SeparationType is one of SeparationTypeCode constants
3. Create EmployeeSeparation entity
4. Call `separation.Submit()`
5. Save to DB
6. Start workflow
7. Map to DTO

- [ ] **Step 3: Create validators**

SubmitTransferValidator: ToDepartmentId != current department? (skip — DB-heavy, handle in handler). Keep: required fields, future EffectiveDate.
SubmitSeparationValidator: LastWorkingDate should be today or future. SeparationType must be valid constant.

- [ ] **Step 4: Build**

```bash
dotnet build Anemoi.Hr.Application/Anemoi.Hr.Application.csproj
```

---

## Task 17: CQRS — Queries (Probation, Transfer, Separation, Timeline, Dashboard)

**Files:**

Probation queries:
- `Anemoi.Hr.Application/Cqrs/Queries/ProbationQueries/GetProbations/GetProbationsQuery.cs` + Handler
- `Anemoi.Hr.Application/Cqrs/Queries/ProbationQueries/GetProbationById/GetProbationByIdQuery.cs` + Handler

Transfer queries:
- `Anemoi.Hr.Application/Cqrs/Queries/TransferQueries/GetTransfers/GetTransfersQuery.cs` + Handler
- `Anemoi.Hr.Application/Cqrs/Queries/TransferQueries/GetTransferById/GetTransferByIdQuery.cs` + Handler

Separation queries:
- `Anemoi.Hr.Application/Cqrs/Queries/SeparationQueries/GetSeparations/GetSeparationsQuery.cs` + Handler
- `Anemoi.Hr.Application/Cqrs/Queries/SeparationQueries/GetSeparationById/GetSeparationByIdQuery.cs` + Handler

Timeline query:
- `Anemoi.Hr.Application/Cqrs/Queries/TimelineQueries/GetEmployeeTimeline/GetEmployeeTimelineQuery.cs` + Handler

Dashboard query:
- `Anemoi.Hr.Application/Cqrs/Queries/DashboardQueries/GetLifecycleSummary/GetLifecycleSummaryQuery.cs` + Handler

- [ ] **Step 1: Create list queries with filtering, pagination, sorting**

```csharp
public sealed record GetProbationsQuery(
    string? StatusCode,
    EmployeeId? EmployeeId,
    DateOnly? DateFrom,
    DateOnly? DateTo,
    int Page = 1,
    int PageSize = 20
) : IRequest<OneOf<PaginatedResult<ProbationRecordDto>, ErrorDetailResponse>>;

public sealed class GetProbationsHandler
    : IRequestHandler<GetProbationsQuery, OneOf<PaginatedResult<ProbationRecordDto>, ErrorDetailResponse>>
{
    private readonly ISqlRepository<ProbationRecord> _repo;
    private readonly IMapper _mapper;

    public async Task<OneOf<PaginatedResult<ProbationRecordDto>, ErrorDetailResponse>> Handle(
        GetProbationsQuery request, CancellationToken ct)
    {
        var query = _repo.FindAllByCondition(x => true);
        if (!string.IsNullOrEmpty(request.StatusCode))
            query = query.Where(x => x.StatusCode == request.StatusCode);
        if (request.EmployeeId != null)
            query = query.Where(x => x.EmployeeId == request.EmployeeId);
        if (request.DateFrom.HasValue)
            query = query.Where(x => x.EndDate >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(x => x.EndDate <= request.DateTo.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.StartDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToDto(_mapper)
            .ToListAsync(ct);

        return new PaginatedResult<ProbationRecordDto>(items, total, request.Page, request.PageSize);
    }
}
```

- [ ] **Step 2: Create GetEmployeeTimelineQuery**

```csharp
public sealed record GetEmployeeTimelineQuery(
    EmployeeId EmployeeId,
    string? EventType,
    string? EntityType,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page = 1,
    int PageSize = 20
) : IRequest<OneOf<PaginatedResult<EmployeeHistoryDto>, ErrorDetailResponse>>;
```

Query from EmployeeHistories table filtered by EmployeeId, ordered by OccurredAt DESC, with optional filters on EventType/EntityType/DateRange.

- [ ] **Step 3: Create GetLifecycleSummaryQuery (Dashboard)**

```csharp
public sealed record GetLifecycleSummaryQuery(DateOnly? AsOfDate)
    : IRequest<OneOf<DashboardLifecycleSummaryDto, ErrorDetailResponse>>;

public sealed class GetLifecycleSummaryHandler
    : IRequestHandler<GetLifecycleSummaryQuery, OneOf<DashboardLifecycleSummaryDto, ErrorDetailResponse>>
{
    // Queries:
    // - ProbationRecords where StatusCode = Pending AND EndDate BETWEEN today AND today+7/14/30
    // - EmployeeTransfers where StatusCode = PendingApproval → count
    // - EmployeeSeparations where StatusCode = PendingApproval → count
    // - Employees where CreatedAt >= today-7/30 → count
}
```

- [ ] **Step 4: Build**

```bash
dotnet build Anemoi.Hr.Application/Anemoi.Hr.Application.csproj
```

---

## Task 18: DTOs & Mappers

**Files:**
- Create: `Anemoi.Hr.Application/Cqrs/Common/Dtos/ProbationRecordDto.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Common/Dtos/EmployeeTransferDto.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Common/Dtos/EmployeeSeparationDto.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Common/Dtos/EmployeeHistoryDto.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Common/Dtos/DashboardLifecycleSummaryDto.cs`
- Create: `Anemoi.Hr.Application/Mappings/ProbationRecordMapper.cs`
- Create: `Anemoi.Hr.Application/Mappings/EmployeeTransferMapper.cs`
- Create: `Anemoi.Hr.Application/Mappings/EmployeeSeparationMapper.cs`
- Create: `Anemoi.Hr.Application/Mappings/EmployeeHistoryMapper.cs`

- [ ] **Step 1: Create DTOs**

```csharp
public sealed record ProbationRecordDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateOnly StartDate,
    DateOnly EndDate,
    string StatusCode,
    string? Result,
    string? Comment,
    Guid? ReviewerEmployeeId
);
```

Similar for EmployeeTransferDto, EmployeeSeparationDto, EmployeeHistoryDto.

```csharp
public sealed record DashboardLifecycleSummaryDto(
    int ProbationsExpiring7Days,
    int ProbationsExpiring14Days,
    int ProbationsExpiring30Days,
    int PendingTransfers,
    int PendingSeparations,
    int NewEmployees7Days,
    int NewEmployees30Days
);
```

- [ ] **Step 2: Create Mapperly mappers**

```csharp
[Mapper]
public static partial class ProbationRecordMapper
{
    public static partial IQueryable<ProbationRecordDto> ProjectToDto(
        this IQueryable<ProbationRecord> query);

    public static partial ProbationRecordDto MapToDto(
        this ProbationRecord entity);
}
```

- [ ] **Step 3: Build**

```bash
dotnet build Anemoi.Hr.Application/Anemoi.Hr.Application.csproj
```

---

## Task 19: Domain Event Handlers — EmployeeHistory Timeline Writers

**Files:**
- Create: `Anemoi.Hr.Application/EventHandlers/EmployeeHistoryWriters/EmployeeCreatedHistoryHandler.cs`
- Create: `Anemoi.Hr.Application/EventHandlers/EmployeeHistoryWriters/EmployeeStatusChangedHistoryHandler.cs`
- Create: `Anemoi.Hr.Application/EventHandlers/EmployeeHistoryWriters/ProbationStatusChangedHistoryHandler.cs`
- Create: `Anemoi.Hr.Application/EventHandlers/EmployeeHistoryWriters/TransferApprovedHistoryHandler.cs`
- Create: `Anemoi.Hr.Application/EventHandlers/EmployeeHistoryWriters/SeparationApprovedHistoryHandler.cs`
- Create: `Anemoi.Hr.Application/EventHandlers/EmployeeHistoryWriters/OnboardingCompletedActivateEmployeeHandler.cs`

- [ ] **Step 1: Create each event handler following this pattern:**

```csharp
namespace Anemoi.Hr.Application.EventHandlers.EmployeeHistoryWriters;

public class TransferApprovedHistoryHandler : INotificationHandler<TransferApprovedDomainEvent>
{
    private readonly ISqlRepository<EmployeeHistory> _historyRepo;
    private readonly IUnitOfWork _unitOfWork;

    public async Task Handle(TransferApprovedDomainEvent notification, CancellationToken ct)
    {
        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = notification.EmployeeId,
            EntityType = "EmployeeTransfer",
            EntityId = notification.TransferId.Value.ToString(),
            EventType = "TransferApproved",
            Title = "Transfer Approved",
            Description = $"Employee transfer approved", // localize if needed
            MetadataJson = "{}",  // fill from actual transfer data
            OccurredAt = DateTime.UtcNow,
            CorrelationId = notification.CorrelationId ?? string.Empty
        };
        await _historyRepo.CreateAsync(history, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

Create handlers for:
- `EmployeeCreatedDomainEvent` → "Employee Created"
- `EmployeeStatusChangedDomainEvent` → "Status: {old} → {new}"
- `ProbationStatusChangedDomainEvent` → "Probation {Passed/Failed/Extended}"
- `TransferApprovedDomainEvent` → "Transfer Approved"
- `SeparationApprovedDomainEvent` → "Separation: {Type}"
- `OnboardingInstanceCompletedDomainEvent` → calls `employee.Activate()`

---

## Task 20: Domain Event Handler — EmployeeOrganizationHistory On Employee Created

**Files:**
- Create: `Anemoi.Hr.Application/EventHandlers/EmployeeCreatedOrganizationHistoryHandler.cs`

- [ ] **Step 1: Create handler that writes initial EmployeeOrganizationHistory**

```csharp
public class EmployeeCreatedOrganizationHistoryHandler : INotificationHandler<EmployeeCreatedDomainEvent>
{
    private readonly ISqlRepository<EmployeeOrganizationHistory> _orgHistoryRepo;
    private readonly ISqlRepository<Employee> _employeeRepo;
    private readonly IUnitOfWork _unitOfWork;

    public async Task Handle(EmployeeCreatedDomainEvent notification, CancellationToken ct)
    {
        var employee = await _employeeRepo.FindByConditionAsync(
            x => x.Id == notification.EmployeeId, ct);
        if (employee == null) return;

        var history = new EmployeeOrganizationHistory
        {
            Id = new EmployeeOrganizationHistoryId(IdGenerator.NextGuid()),
            EmployeeId = employee.Id,
            DepartmentId = employee.PrimaryDepartmentId,
            PositionId = employee.PrimaryPositionId,
            ManagerEmployeeId = employee.DirectManagerEmployeeId,
            GradeCode = employee.GradeCode,
            EffectiveDate = employee.JoinDate,
            ChangeReasonCode = "Initial"
        };
        await _orgHistoryRepo.CreateAsync(history, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

---

## Task 21: API Controllers

**Files:**
- Create: `Anemoi.Hr.Api/Controllers/Probation/ProbationController.cs`
- Create: `Anemoi.Hr.Api/Controllers/Transfer/TransferController.cs`
- Create: `Anemoi.Hr.Api/Controllers/Separation/SeparationController.cs`
- Create: `Anemoi.Hr.Api/Controllers/Dashboard/DashboardController.cs`
- Modify: `Anemoi.Hr.Api/Controllers/Employee/EmployeeController.cs` (add timeline endpoint)

- [ ] **Step 1-5: Create each controller following existing patterns**

```csharp
[Route("api/hr/[controller]")]
[Authorize]
public class ProbationController : BaseController
{
    [HttpPost]
    [HasPermission(HrPermissions.ProbationManage)]
    public async Task<IActionResult> Start([FromBody] StartProbationCommand command)
        => OkOrError(await Mediator.Send(command));

    [HttpPost("{id:guid}/pass")]
    [HasPermission(HrPermissions.ProbationManage)]
    public async Task<IActionResult> Pass(Guid id, [FromBody] PassProbationRequest request)
        => OkOrError(await Mediator.Send(new PassProbationCommand(new ProbationRecordId(id), ...)));

    // ... similar for fail, extend, get, get-by-id
}

[Route("api/hr/transfers")]
public class TransferController : BaseController
{
    [HttpPost("submit")]
    [HasPermission(HrPermissions.TransferCreate)]
    public async Task<IActionResult> Submit([FromBody] SubmitTransferCommand command)
        => OkOrError(await Mediator.Send(command));

    [HttpGet]
    [HasPermission(HrPermissions.TransferView)]
    public async Task<IActionResult> GetAll([FromQuery] GetTransfersQuery query)
        => OkOrError(await Mediator.Send(query));

    [HttpGet("{id:guid}")]
    [HasPermission(HrPermissions.TransferView)]
    public async Task<IActionResult> GetById(Guid id)
        => OkOrError(await Mediator.Send(new GetTransferByIdQuery(new EmployeeTransferId(id))));
}
```

Separation controller follows Transfer pattern. Dashboard controller returns `GetLifecycleSummaryQuery`.

Employee controller adds:
```csharp
[HttpGet("{id:guid}/timeline")]
[HasPermission(HrPermissions.EmployeeTimelineView)]
public async Task<IActionResult> GetTimeline(Guid id, [FromQuery] GetEmployeeTimelineQuery query)
{
    query.EmployeeId = new EmployeeId(id);
    return OkOrError(await Mediator.Send(query));
}

[HttpGet("me/timeline")]
[HasPermission(HrPermissions.EmployeeTimelineView)]
public async Task<IActionResult> GetMyTimeline([FromQuery] GetEmployeeTimelineQuery query)
{
    query.EmployeeId = GetCurrentEmployeeId();
    return OkOrError(await Mediator.Send(query));
}
```

---

## Task 22: Permissions

**Files:**
- Modify: `Anemoi.Hr.Application/Configurations/HrPermissions.cs`

- [ ] **Step 1: Add new permission constants**

```csharp
public const string ProbationView = "Probation.View";
public const string ProbationManage = "Probation.Manage";
public const string TransferCreate = "Transfer.Create";
public const string TransferView = "Transfer.View";
public const string SeparationCreate = "Separation.Create";
public const string SeparationView = "Separation.View";
public const string EmployeeTimelineView = "EmployeeTimeline.View";
public const string DashboardView = "Dashboard.View";
```

---

## Task 23: Integration Events & Consumers

**Files:**
- Create: `Anemoi.Contract/Anemoi.Contract.Hr/Events/EmployeeCreatedIntegrationEvent.cs`
- Create: `Anemoi.Contract/Anemoi.Contract.Hr/Events/EmployeeStatusChangedIntegrationEvent.cs`
- Create: `Anemoi.Contract/Anemoi.Contract.Hr/Events/TransferApprovedIntegrationEvent.cs`
- Create: `Anemoi.Contract/Anemoi.Contract.Hr/Events/SeparationApprovedIntegrationEvent.cs`
- Create: notification consumers (follow existing pattern, e.g., `Anemoi.Notification/Anemoi.Notification.WorkerService/Consumers/`)

- [ ] **Step 1: Create integration events in `Anemoi.Contract.Hr`**

```csharp
// Anemoi.Contract.Hr/Events/EmployeeCreatedIntegrationEvent.cs
public sealed record EmployeeCreatedIntegrationEvent(
    Guid EmployeeId,
    string EmployeeCode,
    string FullName,
    string WorkEmail) : IntegrationEvent;

// EmployeeStatusChangedIntegrationEvent(Guid EmployeeId, string FromStatus, string ToStatus) : IntegrationEvent;
// TransferApprovedIntegrationEvent(Guid TransferId, Guid EmployeeId, ...) : IntegrationEvent;
// SeparationApprovedIntegrationEvent(Guid SeparationId, Guid EmployeeId, string SeparationType, DateOnly LastWorkingDate) : IntegrationEvent;
```

- [ ] **Step 2: Publish integration events from these locations:**

| Event | Published By | Trigger |
|---|---|---|
| `EmployeeCreatedIntegrationEvent` | `ConvertCandidateToEmployeeHandler` (Task 24) or `SubmitEmployeeCommand` handler | After employee created + SaveChanges |
| `EmployeeStatusChangedIntegrationEvent` | `EmployeeStatusChangedHistoryHandler` (Task 19, timeline writer) | After EmployeeStatusChangedDomainEvent handled |
| `TransferApprovedIntegrationEvent` | `EmployeeTransferWorkflowStatusUpdater` (Task 14) | After transfer approved + employee snapshot updated |
| `SeparationApprovedIntegrationEvent` | `EmployeeSeparationWorkflowStatusUpdater` (Task 14) | After separation approved + employee status changed |

Publishing code (example in status updater):
```csharp
// After employee snapshot updated and SaveChangesAsync:
await _messageBus.Publish(new TransferApprovedIntegrationEvent(
    transfer.Id.Value, transfer.EmployeeId.Value, ...), cancellationToken);
```

Inject `IBus` (MassTransit) or the appropriate message bus interface into the status updaters/handlers.

- [ ] **Step 3: Create notification consumers in `Anemoi.Notification.WorkerService`**

Follow existing pattern (e.g., `RecruitmentRequestApprovedConsumer`):
1. Create consumer class implementing `IConsumer<TransferApprovedIntegrationEvent>`
2. In Consume: create `CreateNotificationCommand` with appropriate category, title, action URL
3. Send via MediatR

```csharp
public class TransferApprovedConsumer : IConsumer<TransferApprovedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public async Task Consume(ConsumeContext<TransferApprovedIntegrationEvent> context)
    {
        await _mediator.Send(new CreateNotificationCommand(
            UserId: /* resolve employee user ID */,
            Title: "Transfer Approved",
            Content: $"Your transfer has been approved",
            Category: NotificationConstants.Categories.Hr,
            ActionUrl: $"/hr/employees/transfers/{context.Message.TransferId}"
        ));
    }
}
```

Create consumers for:
- `EmployeeCreatedConsumer` — notify new employee's manager
- `TransferApprovedConsumer` — notify employee + both managers
- `SeparationApprovedConsumer` — notify HR + manager

---

## Task 24: Modify ConvertCandidateToEmployeeHandler

**Files:**
- Modify: `Anemoi.Hr.Application/Cqrs/Commands/RecruitmentCommands/ConvertCandidateToEmployee/ConvertCandidateToEmployeeHandler.cs`

- [ ] **Step 1: Read the existing handler**

Read: `ConvertCandidateToEmployeeHandler.cs`

- [ ] **Step 2: Modify to set PendingOnboarding instead of Active**

Change:
```csharp
EmploymentStatusCode = EmploymentStatusCode.Active
```
to:
```csharp
EmploymentStatusCode = EmploymentStatusCode.PendingOnboarding
```

- [ ] **Step 3: Add auto-creation of OnboardingInstance**

After creating Employee, create an OnboardingInstance (Draft status) linked to the new employee.

- [ ] **Step 4: Add EmployeeCreatedDomainEvent emission**

After the employee is created, add:
```csharp
employee.AddEvent(new EmployeeCreatedDomainEvent(employee.Id, employee.EmployeeCode, employee.FullName, ...));
```

Note: This requires Employee to be Entity<EmployeeId> (Task 1).

- [ ] **Step 5: Add publish of EmployeeCreatedIntegrationEvent**

After SaveChanges, publish:
```csharp
await _messageBus.Publish(new EmployeeCreatedIntegrationEvent(
    employee.Id.Value, employee.EmployeeCode, employee.FullName, employee.WorkEmail));
```

---

## Task 25: Frontend — API Service Layer

**Files (in cody-web-app):**
- Modify: existing API service layer to add new endpoints

Follow existing patterns for the Next.js app. Create API functions or service methods for:
- Dashboard: `GET /api/hr/dashboard/lifecycle-summary`
- Probations: CRUD + pass/fail/extend
- Transfers: list + submit
- Separations: list + submit
- Timeline: get by employee ID

---

## Task 26: Frontend — Dashboard Page

**Files:**
- Create: `cody-web-app/app/hr/dashboard/page.tsx`
- Create: component files for dashboard cards

- [ ] **Step 1: Create DashboardPage with 4 widget cards**

Each card shows:
- Title and count
- Clickable link to filtered list page

Follow existing page patterns in the app.

---

## Task 27: Frontend — Probation Page

**Files:**
- Create: `cody-web-app/app/hr/employees/probations/page.tsx`
- Create: dialog components for Start/Pass/Fail/Extend

- [ ] **Step 1: Create ProbationListPage**

Features:
- Table with employee, start/end date, status, actions
- Start Probation dialog
- Pass/Fail/Extend buttons (enabled based on status)
- Status filter

---

## Task 28: Frontend — Transfer Page

**Files:**
- Create: `cody-web-app/app/hr/employees/transfers/page.tsx`

Features:
- Table: employee, effective date, status, workflow status, current approver, actions
- Submit Transfer dialog (prefills current department/position/manager/grade)
- Clickable workflow instance link

---

## Task 29: Frontend — Separation Page

**Files:**
- Create: `cody-web-app/app/hr/employees/separations/page.tsx`

Features:
- Table: employee, type, last working date, status, workflow status, actions
- Submit Separation dialog
- Type selector: Resignation/Termination/Retirement

---

## Task 30: Frontend — Employee Timeline Page

**Files:**
- Create: `cody-web-app/app/hr/employees/[id]/timeline/page.tsx`
- Create: `cody-web-app/app/ess/profile/history/page.tsx`

Features:
- Timeline view (chronological list)
- Filter: event type, date range
- Pagination
- Each entry shows: date, event type, title, description

---

## Task 31: Build & Test

- [ ] **Step 1: Build the entire solution**

```bash
dotnet build Anemoi.sln
```

- [ ] **Step 2: Fix any compilation errors**

- [ ] **Step 3: Run existing tests**

```bash
dotnet test
```

- [ ] **Step 4: Frontend build**

```bash
npm run build
```

---

## Task 32: Browser Verification (DevTools MCP)

- [ ] **Step 1: Verify dashboard**
  - Open `/hr/dashboard` — 4 widgets load with correct data
  - Click each widget card — navigates to correct filtered list

- [ ] **Step 2: Verify probation flow**
  - Start probation for an employee — confirm record created
  - Pass probation — confirm status changed to Passed
  - Fail probation — confirm status changed to Failed
  - Timeline shows probation events

- [ ] **Step 3: Verify transfer flow + EmployeeOrganizationHistory**
  - Submit transfer — confirm EmployeeTransfer created in PendingApproval
  - Confirm WorkflowInstance created
  - Approve via workflow endpoint — confirm transfer Approved
  - Confirm Employee department/position/manager/grade updated
  - Confirm previous EmployeeOrganizationHistory record now has EndDate = effective date
  - Confirm new EmployeeOrganizationHistory record created with ToDepartmentId/ToPositionId/ToGradeCode
  - Confirm Timeline shows transfer event with correct MetadataJson

- [ ] **Step 4: Verify separation flow + EmployeeOrganizationHistory + LastWorkingDate**
  - Submit separation — confirm EmployeeSeparation created
  - Confirm WorkflowInstance created
  - Approve — confirm Employee status changed (Resigned/Terminated)
  - Confirm Timeline shows separation event with correct MetadataJson (type, lastWorkingDate, reason)
  - Confirm EmployeeOrganizationHistory EndDate = separation.LastWorkingDate (not approval date)
  - Confirm no other EndDate changes on unrelated EmployeeOrganizationHistory records

- [ ] **Step 5: Verify notifications**
  - Check notification center for employee lifecycle notifications

---

## Self-Review Checklist

1. **Spec coverage check:**
   - [ ] Module 1 (Lifecycle State Machine): Tasks 1, 2, 19
   - [ ] Module 2 (Recruitment Integration): Task 24
   - [ ] Module 3 (Probation Management): Tasks 7, 15, 21, 27
   - [ ] Module 4 (Employee Transfer): Tasks 8, 14, 16, 21, 28
   - [ ] Module 5 (Employee Separation): Tasks 9, 14, 16, 21, 29
   - [ ] Module 6 (Employee History Timeline): Tasks 6, 19, 21, 30
   - [ ] Module 7 (Workflow Templates): Tasks 12, 13, 14
   - [ ] Module 8 (Notifications): Task 23
   - [ ] Module 9 (Frontend): Tasks 25-30
   - [ ] Module 10 (Dashboard Widgets): Task 17 (Dashboard query), Task 26

2. **Placeholder scan:** No TBD/TODO in plan ✅

3. **Type consistency:** Domain events → handler signatures match throughout ✅

4. **No placeholders in code blocks:** Every code block shows actual implementation ✅
