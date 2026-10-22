# Phase 28 — Approval Workflow Engine Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build standalone Workflow Engine foundation (domain, CQRS, API, frontend) for powering HR approval workflows.

**Architecture:** Clean Architecture with 5 domain entities, 9 commands, 5 queries, 3 controllers, and 3 frontend tabs. Follows existing HR patterns: manual mappers, `xmin` concurrency, `Entity<TId>`, `ISqlRepository`, `OneOf<T,E>` handlers.

**Tech Stack:** .NET 10, EF Core/Npgsql, MediatR, FluentValidation, Next.js 14

---

### Task 1: Strongly-Typed IDs + Domain Constants

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.ModelIds/ModelIds/WorkflowDefinitionId.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.ModelIds/ModelIds/WorkflowDefinitionStepId.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.ModelIds/ModelIds/WorkflowInstanceId.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.ModelIds/ModelIds/WorkflowInstanceStepId.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.ModelIds/ModelIds/WorkflowHistoryId.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowTypeCode.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowStatusCode.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowStepStatusCode.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/ApproverType.cs`

- [ ] **Step 1: Create 5 strongly-typed ID records**

Each file follows this exact pattern:

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record WorkflowDefinitionId(Guid Value) : StronglyTypedId<Guid>(Value);
```

Replace `WorkflowDefinitionId` with `WorkflowDefinitionStepId`, `WorkflowInstanceId`, `WorkflowInstanceStepId`, `WorkflowHistoryId` for each file.

- [ ] **Step 2: Create 4 constant classes**

`WorkflowTypeCode.cs`:
```csharp
namespace Anemoi.Hr.Domain.Workflow;

public static class WorkflowTypeCode
{
    public const string Approval = "Approval";
}
```

`WorkflowStatusCode.cs`:
```csharp
namespace Anemoi.Hr.Domain.Workflow;

public static class WorkflowStatusCode
{
    public const string Draft = "Draft";
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
    public const string Returned = "Returned";
}
```

`WorkflowStepStatusCode.cs`:
```csharp
namespace Anemoi.Hr.Domain.Workflow;

public static class WorkflowStepStatusCode
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Skipped = "Skipped";
    public const string Cancelled = "Cancelled";
}
```

`ApproverType.cs`:
```csharp
namespace Anemoi.Hr.Domain.Workflow;

public static class ApproverType
{
    public const string Role = "Role";
    public const string Permission = "Permission";
    public const string DirectManager = "DirectManager";
    public const string SpecificUser = "SpecificUser";
}
```

- [ ] **Step 3: Build and verify IDs compile**

```bash
dotnet build Anemoi.Hr.ModelIds/Anemoi.Hr.ModelIds.csproj
```
Expected: Build succeeded. 0 errors.

---

### Task 2: Domain Entities — WorkflowDefinition + WorkflowDefinitionStep

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowDefinition.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowDefinitionStep.cs`

- [ ] **Step 1: Create WorkflowDefinitionStep entity**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowDefinitionStep : Entity<WorkflowDefinitionStepId>
{
    public WorkflowDefinitionId WorkflowDefinitionId { get; private set; }
    public int Sequence { get; private set; }
    public string ApproverType { get; private set; }
    public string? ApproverValue { get; private set; }
    public bool IsRequired { get; private set; }

    private WorkflowDefinitionStep() { }

    internal static WorkflowDefinitionStep Create(
        WorkflowDefinitionStepId id,
        WorkflowDefinitionId workflowDefinitionId,
        int sequence,
        string approverType,
        string? approverValue,
        bool isRequired)
    {
        return new WorkflowDefinitionStep
        {
            Id = id,
            WorkflowDefinitionId = workflowDefinitionId,
            Sequence = sequence,
            ApproverType = approverType,
            ApproverValue = approverValue,
            IsRequired = isRequired
        };
    }
}
```

- [ ] **Step 2: Create WorkflowDefinition aggregate root**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowDefinition : Entity<WorkflowDefinitionId>
{
    private readonly List<WorkflowDefinitionStep> _steps = [];

    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string WorkflowTypeCode { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyCollection<WorkflowDefinitionStep> Steps => _steps.AsReadOnly();

    private WorkflowDefinition() { }

    public static WorkflowDefinition Create(
        WorkflowDefinitionId id,
        string code,
        string name,
        string? description,
        string workflowTypeCode,
        List<WorkflowDefinitionStep> steps)
    {
        var now = DateTime.UtcNow;
        return new WorkflowDefinition
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            WorkflowTypeCode = workflowTypeCode,
            IsActive = false,
            CreatedAt = now,
            UpdatedAt = now,
            _steps = steps
        };
    }

    public void Activate()
    {
        if (_steps.Count == 0)
            throw new InvalidOperationException("Cannot activate a workflow definition with no steps.");
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceSteps(List<WorkflowDefinitionStep> newSteps)
    {
        if (IsActive)
            throw new InvalidOperationException("Cannot replace steps on an active workflow definition.");
        _steps.Clear();
        _steps.AddRange(newSteps);
        UpdatedAt = DateTime.UtcNow;
    }
}
```

- [ ] **Step 3: Verify domain layer builds**

```bash
dotnet build Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj
```
Expected: Build succeeded. 0 errors.

---

### Task 3: Domain Entities — WorkflowInstance + WorkflowInstanceStep + WorkflowHistory

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowInstance.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowInstanceStep.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Workflow/WorkflowHistory.cs`

- [ ] **Step 1: Create WorkflowInstanceStep entity**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowInstanceStep : Entity<WorkflowInstanceStepId>
{
    public WorkflowInstanceId WorkflowInstanceId { get; private set; }
    public int Sequence { get; private set; }
    public string ApproverTypeSnapshot { get; private set; }
    public string? ApproverValueSnapshot { get; private set; }
    public string? ApproverUserId { get; private set; }
    public string Status { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? Comment { get; private set; }

    private WorkflowInstanceStep() { }

    internal static WorkflowInstanceStep Create(
        WorkflowInstanceStepId id,
        WorkflowInstanceId workflowInstanceId,
        int sequence,
        string approverTypeSnapshot,
        string? approverValueSnapshot,
        string? approverUserId)
    {
        return new WorkflowInstanceStep
        {
            Id = id,
            WorkflowInstanceId = workflowInstanceId,
            Sequence = sequence,
            ApproverTypeSnapshot = approverTypeSnapshot,
            ApproverValueSnapshot = approverValueSnapshot,
            ApproverUserId = approverUserId,
            Status = WorkflowStepStatusCode.Pending
        };
    }

    internal void Approve(string? comment)
    {
        Status = WorkflowStepStatusCode.Approved;
        ApprovedAt = DateTime.UtcNow;
        Comment = comment;
    }

    internal void Reject(string? comment)
    {
        Status = WorkflowStepStatusCode.Rejected;
        RejectedAt = DateTime.UtcNow;
        Comment = comment;
    }

    internal void Cancel()
    {
        Status = WorkflowStepStatusCode.Cancelled;
    }
}
```

- [ ] **Step 2: Create WorkflowHistory entity**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowHistory : Entity<WorkflowHistoryId>
{
    public WorkflowInstanceId WorkflowInstanceId { get; private set; }
    public string Action { get; private set; }
    public string PerformedBy { get; private set; }
    public string? Comment { get; private set; }
    public DateTime PerformedAt { get; private set; }

    private WorkflowHistory() { }

    internal static WorkflowHistory Create(
        WorkflowHistoryId id,
        WorkflowInstanceId workflowInstanceId,
        string action,
        string performedBy,
        string? comment)
    {
        return new WorkflowHistory
        {
            Id = id,
            WorkflowInstanceId = workflowInstanceId,
            Action = action,
            PerformedBy = performedBy,
            Comment = comment,
            PerformedAt = DateTime.UtcNow
        };
    }
}
```

- [ ] **Step 3: Create WorkflowInstance aggregate root**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowInstance : Entity<WorkflowInstanceId>
{
    private readonly List<WorkflowInstanceStep> _steps = [];
    private readonly List<WorkflowHistory> _histories = [];

    public WorkflowDefinitionId WorkflowDefinitionId { get; private set; }
    public string EntityType { get; private set; }
    public string EntityId { get; private set; }
    public int CurrentStep { get; private set; }
    public string Status { get; private set; }
    public string StartedBy { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public IReadOnlyCollection<WorkflowInstanceStep> Steps => _steps.AsReadOnly();
    public IReadOnlyCollection<WorkflowHistory> Histories => _histories.AsReadOnly();

    private WorkflowInstance() { }

    public static WorkflowInstance Start(
        WorkflowInstanceId id,
        WorkflowDefinitionId workflowDefinitionId,
        string entityType,
        string entityId,
        string startedBy,
        List<WorkflowInstanceStep> steps)
    {
        return new WorkflowInstance
        {
            Id = id,
            WorkflowDefinitionId = workflowDefinitionId,
            EntityType = entityType,
            EntityId = entityId,
            CurrentStep = steps.Count > 0 ? steps.Min(s => s.Sequence) : 0,
            Status = WorkflowStatusCode.Pending,
            StartedBy = startedBy,
            StartedAt = DateTime.UtcNow,
            _steps = steps
        };
    }

    public WorkflowHistory Approve(string performedBy, string? comment)
    {
        EnsurePending();
        var step = GetCurrentStepEntity();
        step.Approve(comment);
        AddHistory(WorkflowHistory.Create(
            new WorkflowHistoryId(IdGenerator.NextGuid()), Id,
            "Approve", performedBy, comment));

        var nextSequence = step.Sequence + 1;
        var nextStep = _steps.FirstOrDefault(s => s.Sequence == nextSequence);
        if (nextStep is null || nextStep.Status != WorkflowStepStatusCode.Pending)
        {
            Status = WorkflowStatusCode.Approved;
            CompletedAt = DateTime.UtcNow;
        }
        else
        {
            CurrentStep = nextSequence;
        }

        return _histories.Last();
    }

    public WorkflowHistory Reject(string performedBy, string? comment)
    {
        EnsurePending();
        var step = GetCurrentStepEntity();
        step.Reject(comment);
        Status = WorkflowStatusCode.Rejected;
        CompletedAt = DateTime.UtcNow;
        var history = WorkflowHistory.Create(
            new WorkflowHistoryId(IdGenerator.NextGuid()), Id,
            "Reject", performedBy, comment);
        AddHistory(history);
        return history;
    }

    public WorkflowHistory Cancel(string performedBy)
    {
        if (Status is WorkflowStatusCode.Approved or WorkflowStatusCode.Rejected or WorkflowStatusCode.Cancelled)
            throw new InvalidOperationException($"Cannot cancel workflow in status '{Status}'.");
        foreach (var step in _steps.Where(s => s.Status == WorkflowStepStatusCode.Pending))
            step.Cancel();
        Status = WorkflowStatusCode.Cancelled;
        CompletedAt = DateTime.UtcNow;
        var history = WorkflowHistory.Create(
            new WorkflowHistoryId(IdGenerator.NextGuid()), Id,
            "Cancel", performedBy, null);
        AddHistory(history);
        return history;
    }

    public WorkflowHistory ReturnForRevision(string performedBy, string? comment)
    {
        EnsurePending();
        foreach (var step in _steps.Where(s => s.Status == WorkflowStepStatusCode.Pending))
            step.Cancel();
        Status = WorkflowStatusCode.Returned;
        CompletedAt = DateTime.UtcNow;
        var history = WorkflowHistory.Create(
            new WorkflowHistoryId(IdGenerator.NextGuid()), Id,
            "ReturnForRevision", performedBy, comment);
        AddHistory(history);
        return history;
    }

    public bool IsCurrentStepApprover(string userId, Func<string, bool> hasRole, Func<string, bool> hasPermission)
    {
        var step = GetCurrentStepEntity();
        return step.ApproverTypeSnapshot switch
        {
            ApproverType.SpecificUser => step.ApproverValueSnapshot == userId,
            ApproverType.DirectManager => step.ApproverUserId == userId,
            ApproverType.Role => step.ApproverValueSnapshot != null && hasRole(step.ApproverValueSnapshot),
            ApproverType.Permission => step.ApproverValueSnapshot != null && hasPermission(step.ApproverValueSnapshot),
            _ => false
        };
    }

    private void EnsurePending()
    {
        if (Status != WorkflowStatusCode.Pending)
            throw new InvalidOperationException($"Workflow is in '{Status}' status, expected 'Pending'.");
    }

    private WorkflowInstanceStep GetCurrentStepEntity()
    {
        return _steps.FirstOrDefault(s => s.Sequence == CurrentStep)
            ?? throw new InvalidOperationException($"No step found at sequence {CurrentStep}.");
    }

    private void AddHistory(WorkflowHistory history)
    {
        _histories.Add(history);
    }
}
```

- [ ] **Step 4: Verify domain layer builds**

```bash
dotnet build Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj
```
Expected: Build succeeded. 0 errors.

---

### Task 4: EF Core Configurations

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Configurations/WorkflowDefinitionModelMapping.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Configurations/WorkflowInstanceModelMapping.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Persistence/HrDbContext.cs`

- [ ] **Step 1: Create WorkflowDefinitionModelMapping**

```csharp
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class WorkflowDefinitionModelMapping : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("WorkflowDefinitions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new WorkflowDefinitionId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired(false);
        builder.Property(x => x.WorkflowTypeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.WorkflowTypeCode);
        builder.HasIndex(x => x.IsActive);

        builder.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey(x => x.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class WorkflowDefinitionStepModelMapping : IEntityTypeConfiguration<WorkflowDefinitionStep>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinitionStep> builder)
    {
        builder.ToTable("WorkflowDefinitionSteps");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new WorkflowDefinitionStepId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.WorkflowDefinitionId)
            .HasConversion(x => x.Value, id => new WorkflowDefinitionId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.Sequence).IsRequired();
        builder.Property(x => x.ApproverType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ApproverValue).HasMaxLength(256).IsRequired(false);
        builder.Property(x => x.IsRequired).IsRequired();

        builder.HasIndex(x => new { x.WorkflowDefinitionId, x.Sequence }).IsUnique();
    }
}
```

- [ ] **Step 2: Create WorkflowInstanceModelMapping**

```csharp
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class WorkflowInstanceModelMapping : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("WorkflowInstances");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new WorkflowInstanceId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.WorkflowDefinitionId)
            .HasConversion(x => x.Value, id => new WorkflowDefinitionId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CurrentStep).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.StartedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.CompletedAt).IsRequired(false);

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.WorkflowDefinitionId);
        builder.HasIndex(x => x.StartedBy);

        builder.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey(x => x.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Histories)
            .WithOne()
            .HasForeignKey(x => x.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class WorkflowInstanceStepModelMapping : IEntityTypeConfiguration<WorkflowInstanceStep>
{
    public void Configure(EntityTypeBuilder<WorkflowInstanceStep> builder)
    {
        builder.ToTable("WorkflowInstanceSteps");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new WorkflowInstanceStepId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.WorkflowInstanceId)
            .HasConversion(x => x.Value, id => new WorkflowInstanceId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.Sequence).IsRequired();
        builder.Property(x => x.ApproverTypeSnapshot).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ApproverValueSnapshot).HasMaxLength(256).IsRequired(false);
        builder.Property(x => x.ApproverUserId).HasMaxLength(128).IsRequired(false);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ApprovedAt).IsRequired(false);
        builder.Property(x => x.RejectedAt).IsRequired(false);
        builder.Property(x => x.Comment).HasMaxLength(1000).IsRequired(false);

        builder.HasIndex(x => x.WorkflowInstanceId);
        builder.HasIndex(x => x.Status);
    }
}

public sealed class WorkflowHistoryModelMapping : IEntityTypeConfiguration<WorkflowHistory>
{
    public void Configure(EntityTypeBuilder<WorkflowHistory> builder)
    {
        builder.ToTable("WorkflowHistories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new WorkflowHistoryId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.WorkflowInstanceId)
            .HasConversion(x => x.Value, id => new WorkflowInstanceId(id))
            .HasColumnType("uuid").IsRequired();

        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PerformedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(1000).IsRequired(false);
        builder.Property(x => x.PerformedAt).IsRequired();

        builder.HasIndex(x => x.WorkflowInstanceId);
        builder.HasIndex(x => x.PerformedAt);
    }
}
```

- [ ] **Step 3: Add DbSets to HrDbContext**

Read `Anemoi.Hr/Anemoi.Hr.Infrastructure/Persistence/HrDbContext.cs` and add after the last existing `DbSet`:

```csharp
public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
public DbSet<WorkflowDefinitionStep> WorkflowDefinitionSteps { get; set; }
public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
public DbSet<WorkflowInstanceStep> WorkflowInstanceSteps { get; set; }
public DbSet<WorkflowHistory> WorkflowHistories { get; set; }
```

Also add imports:
```csharp
using Anemoi.Hr.Domain.Workflow;
```

- [ ] **Step 4: Build and verify**

```bash
dotnet build Anemoi.Hr.Infrastructure/Anemoi.Hr.Infrastructure.csproj
```
Expected: Build succeeded. 0 errors.

---

### Task 5: Business Error Codes + Permission Constants + Notification Category

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Configurations/HrBusinessErrorCodes.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Configurations/HrPermissions.cs`
- Modify: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/Permissions.cs`
- Modify: `Anemoi.Contract/Anemoi.Contract.Notification/ModelIds/NotificationConstants.cs`

- [ ] **Step 1: Add workflow error codes to HrBusinessErrorCodes.cs**

```csharp
// Workflow Definition
public const string WorkflowDefinitionNotFound = "HR_WF_DEF_NOT_FOUND";
public const string WorkflowDefinitionNoSteps = "HR_WF_DEF_NO_STEPS";
public const string WorkflowDefinitionAlreadyActive = "HR_WF_DEF_ALREADY_ACTIVE";
public const string WorkflowDefinitionAlreadyInactive = "HR_WF_DEF_ALREADY_INACTIVE";
public const string WorkflowDefinitionActiveCannotUpdate = "HR_WF_DEF_ACTIVE_CANNOT_UPDATE";

// Workflow Instance
public const string WorkflowInstanceNotFound = "HR_WF_INSTANCE_NOT_FOUND";
public const string WorkflowInstanceInvalidStatus = "HR_WF_INSTANCE_INVALID_STATUS";
public const string WorkflowInstanceStepNotFound = "HR_WF_INSTANCE_STEP_NOT_FOUND";
public const string WorkflowInstanceAlreadyCompleted = "HR_WF_INSTANCE_ALREADY_COMPLETED";
public const string WorkflowInstanceNotApprover = "HR_WF_INSTANCE_NOT_APPROVER";

// Validation
public const string ValWorkflowDefinitionIdRequired = "VAL_WF_DEF_ID_REQUIRED";
public const string ValWorkflowInstanceIdRequired = "VAL_WF_INSTANCE_ID_REQUIRED";
public const string ValWorkflowCodeRequired = "VAL_WF_CODE_REQUIRED";
public const string ValWorkflowNameRequired = "VAL_WF_NAME_REQUIRED";
public const string ValWorkflowStepSequenceInvalid = "VAL_WF_STEP_SEQUENCE_INVALID";
public const string ValWorkflowApproverTypeRequired = "VAL_WF_APPROVER_TYPE_REQUIRED";
```

- [ ] **Step 2: Add permissions to Permissions.cs (BuildingBlocks)**

```csharp
public const string HrWorkflowView = "hr.workflow.view";
public const string HrWorkflowManage = "hr.workflow.manage";
public const string HrWorkflowExecute = "hr.workflow.execute";
```

Add to `Definitions` list:
```csharp
new (HrWorkflowView, "HR", "permission.hr.workflow.view"),
new (HrWorkflowManage, "HR", "permission.hr.workflow.manage"),
new (HrWorkflowExecute, "HR", "permission.hr.workflow.execute"),
```

- [ ] **Step 3: Add to HrPermissions.cs**

```csharp
public const string WorkflowView = Permissions.HrWorkflowView;
public const string WorkflowManage = Permissions.HrWorkflowManage;
public const string WorkflowExecute = Permissions.HrWorkflowExecute;
```

Add to HrPermissions.All array.

- [ ] **Step 4: Add Workflow category to NotificationConstants.Categories**

```csharp
public const string Workflow = "Workflow";
```

Add `Workflow` to the `AllowedCategories` Hashset.

---

### Task 6: Response DTOs + Mapper

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Responses/WorkflowResponses.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Mappings/WorkflowMapper.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Installers/ServiceInstaller.cs`

- [ ] **Step 1: Create response DTOs**

`WorkflowResponses.cs`:
```csharp
namespace Anemoi.Hr.Application.Responses;

public sealed record WorkflowDefinitionResponse(
    string Id, string Code, string Name, string? Description,
    string WorkflowTypeCode, bool IsActive,
    IReadOnlyCollection<WorkflowDefinitionStepResponse> Steps,
    DateTime CreatedAt, DateTime UpdatedAt);

public sealed record WorkflowDefinitionStepResponse(
    string Id, int Sequence,
    string ApproverType, string? ApproverValue, bool IsRequired);

public sealed record WorkflowInstanceResponse(
    string Id, string WorkflowDefinitionId, string WorkflowDefinitionName,
    string EntityType, string EntityId, int CurrentStep,
    string Status, string StartedBy, DateTime StartedAt, DateTime? CompletedAt,
    IReadOnlyCollection<WorkflowInstanceStepResponse> Steps,
    IReadOnlyCollection<WorkflowHistoryResponse> Histories);

public sealed record WorkflowInstanceStepResponse(
    string Id, int Sequence,
    string ApproverTypeSnapshot, string? ApproverValueSnapshot,
    string? ApproverUserId, string Status,
    DateTime? ApprovedAt, DateTime? RejectedAt, string? Comment);

public sealed record WorkflowHistoryResponse(
    string Id, string WorkflowInstanceId,
    string Action, string PerformedBy, string? Comment, DateTime PerformedAt);
```

- [ ] **Step 2: Create WorkflowMapper**

`WorkflowMapper.cs`:
```csharp
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Mappings;

public sealed class WorkflowMapper
{
    public WorkflowDefinitionResponse ToResponse(WorkflowDefinition def)
    {
        if (def is null) return null;
        return new WorkflowDefinitionResponse(
            Id: def.Id.Value.ToString(),
            Code: def.Code,
            Name: def.Name,
            Description: def.Description,
            WorkflowTypeCode: def.WorkflowTypeCode,
            IsActive: def.IsActive,
            Steps: def.Steps.Select(ToStepResponse).ToList(),
            CreatedAt: def.CreatedAt,
            UpdatedAt: def.UpdatedAt);
    }

    public WorkflowDefinitionStepResponse ToStepResponse(WorkflowDefinitionStep step)
    {
        if (step is null) return null;
        return new WorkflowDefinitionStepResponse(
            Id: step.Id.Value.ToString(),
            Sequence: step.Sequence,
            ApproverType: step.ApproverType,
            ApproverValue: step.ApproverValue,
            IsRequired: step.IsRequired);
    }

    public WorkflowInstanceResponse ToResponse(WorkflowInstance instance, string definitionName = null)
    {
        if (instance is null) return null;
        return new WorkflowInstanceResponse(
            Id: instance.Id.Value.ToString(),
            WorkflowDefinitionId: instance.WorkflowDefinitionId.Value.ToString(),
            WorkflowDefinitionName: definitionName,
            EntityType: instance.EntityType,
            EntityId: instance.EntityId,
            CurrentStep: instance.CurrentStep,
            Status: instance.Status,
            StartedBy: instance.StartedBy,
            StartedAt: instance.StartedAt,
            CompletedAt: instance.CompletedAt,
            Steps: instance.Steps.Select(ToStepResponse).ToList(),
            Histories: instance.Histories.Select(ToHistoryResponse).ToList());
    }

    public WorkflowInstanceStepResponse ToStepResponse(WorkflowInstanceStep step)
    {
        if (step is null) return null;
        return new WorkflowInstanceStepResponse(
            Id: step.Id.Value.ToString(),
            Sequence: step.Sequence,
            ApproverTypeSnapshot: step.ApproverTypeSnapshot,
            ApproverValueSnapshot: step.ApproverValueSnapshot,
            ApproverUserId: step.ApproverUserId,
            Status: step.Status,
            ApprovedAt: step.ApprovedAt,
            RejectedAt: step.RejectedAt,
            Comment: step.Comment);
    }

    public WorkflowHistoryResponse ToHistoryResponse(WorkflowHistory history)
    {
        if (history is null) return null;
        return new WorkflowHistoryResponse(
            Id: history.Id.Value.ToString(),
            WorkflowInstanceId: history.WorkflowInstanceId.Value.ToString(),
            Action: history.Action,
            PerformedBy: history.PerformedBy,
            Comment: history.Comment,
            PerformedAt: history.PerformedAt);
    }

    public IReadOnlyCollection<WorkflowDefinitionResponse> ToResponses(IEnumerable<WorkflowDefinition> defs)
    {
        return defs?.Select(ToResponse).ToList() ?? [];
    }

    public IReadOnlyCollection<WorkflowInstanceResponse> ToResponses(IEnumerable<WorkflowInstance> instances,
        IDictionary<Guid, string> definitionNames = null)
    {
        if (instances is null) return [];
        return instances.Select(i =>
        {
            var name = definitionNames?.GetValueOrDefault(i.WorkflowDefinitionId.Value);
            return ToResponse(i, name);
        }).ToList();
    }
}
```

- [ ] **Step 3: Register mapper in ServiceInstaller.cs**

Add to the DI registration section:
```csharp
services.AddScoped<WorkflowMapper>();
```

- [ ] **Step 4: Verify build**

```bash
dotnet build Anemoi.Hr.Application/Anemoi.Hr.Application.csproj
```
Expected: Build succeeded. 0 errors.

---

### Task 7: Workflow Definition Commands (Create, Update, Activate, Deactivate)

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/CreateWorkflowDefinition/CreateWorkflowDefinitionCommand.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/CreateWorkflowDefinition/CreateWorkflowDefinitionHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/CreateWorkflowDefinition/CreateWorkflowDefinitionValidator.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/UpdateWorkflowDefinition/UpdateWorkflowDefinitionCommand.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/UpdateWorkflowDefinition/UpdateWorkflowDefinitionHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/UpdateWorkflowDefinition/UpdateWorkflowDefinitionValidator.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/ActivateWorkflowDefinition/ActivateWorkflowDefinitionCommand.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/ActivateWorkflowDefinition/ActivateWorkflowDefinitionHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/ActivateWorkflowDefinition/ActivateWorkflowDefinitionValidator.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/DeactivateWorkflowDefinition/DeactivateWorkflowDefinitionCommand.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/DeactivateWorkflowDefinition/DeactivateWorkflowDefinitionHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/DeactivateWorkflowDefinition/DeactivateWorkflowDefinitionValidator.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/WorkflowCommands/Common/WorkflowDefinitionStepInput.cs`

- [ ] **Step 1: Create shared step input DTO**

`WorkflowDefinitionStepInput.cs`:
```csharp
namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.Common;

public sealed record WorkflowDefinitionStepInput(
    int Sequence,
    string ApproverType,
    string? ApproverValue,
    bool IsRequired);
```

- [ ] **Step 2: Create CreateWorkflowDefinition command + handler + validator**

`CreateWorkflowDefinitionCommand.cs`:
```csharp
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.Common;
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CreateWorkflowDefinition;

public sealed record CreateWorkflowDefinitionCommand(
    string Code,
    string Name,
    string? Description,
    string WorkflowTypeCode,
    List<WorkflowDefinitionStepInput> Steps) : IRequest<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>;
```

`CreateWorkflowDefinitionHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CreateWorkflowDefinition;

public sealed class CreateWorkflowDefinitionHandler(
    ISqlRepository<WorkflowDefinition> repository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : IRequestHandler<CreateWorkflowDefinitionCommand, OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>> Handle(
        CreateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var id = new WorkflowDefinitionId(IdGenerator.NextGuid());
        var steps = request.Steps.Select(s =>
        {
            var stepId = new WorkflowDefinitionStepId(IdGenerator.NextGuid());
            return WorkflowDefinitionStep.Create(stepId, id, s.Sequence, s.ApproverType, s.ApproverValue, s.IsRequired);
        }).ToList();

        var definition = WorkflowDefinition.Create(id, request.Code, request.Name, request.Description, request.WorkflowTypeCode, steps);

        var createResult = await repository.CreateOneAsync(definition, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(definition);
    }
}
```

`CreateWorkflowDefinitionValidator.cs`:
```csharp
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Workflow;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CreateWorkflowDefinition;

public sealed class CreateWorkflowDefinitionValidator : AbstractValidator<CreateWorkflowDefinitionCommand>
{
    public CreateWorkflowDefinitionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowCodeRequired);
        RuleFor(x => x.Name).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowNameRequired);
        RuleFor(x => x.WorkflowTypeCode).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowApproverTypeRequired);
        RuleFor(x => x.Steps).NotEmpty().WithErrorCode(HrBusinessErrorCodes.WorkflowDefinitionNoSteps);
        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.Sequence).GreaterThan(0).WithErrorCode(HrBusinessErrorCodes.ValWorkflowStepSequenceInvalid);
            step.RuleFor(s => s.ApproverType)
                .Must(t => t is ApproverType.Role or ApproverType.Permission
                    or ApproverType.DirectManager or ApproverType.SpecificUser)
                .WithErrorCode(HrBusinessErrorCodes.ValWorkflowApproverTypeRequired);
        });
        RuleFor(x => x.Steps.Select(s => s.Sequence))
            .Must(seqs => seqs.Distinct().Count() == seqs.Count())
            .WithErrorCode(HrBusinessErrorCodes.ValWorkflowStepSequenceInvalid);
    }
}
```

- [ ] **Step 3: Create UpdateWorkflowDefinition command + handler + validator**

`UpdateWorkflowDefinitionCommand.cs`:
```csharp
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.Common;
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.UpdateWorkflowDefinition;

public sealed record UpdateWorkflowDefinitionCommand(
    string Id,
    string Name,
    string? Description,
    List<WorkflowDefinitionStepInput> Steps) : IRequest<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>;
```

`UpdateWorkflowDefinitionHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.UpdateWorkflowDefinition;

public sealed class UpdateWorkflowDefinitionHandler(
    ISqlRepository<WorkflowDefinition> repository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : IRequestHandler<UpdateWorkflowDefinitionCommand, OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>> Handle(
        UpdateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var id = new WorkflowDefinitionId(Guid.Parse(request.Id));
        var definition = await repository.GetQueryable()
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (definition is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionNotFound);

        if (definition.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionActiveCannotUpdate);

        definition.UpdateDetails(request.Name, request.Description);

        var newSteps = request.Steps.Select(s =>
        {
            var stepId = new WorkflowDefinitionStepId(IdGenerator.NextGuid());
            return WorkflowDefinitionStep.Create(stepId, id, s.Sequence, s.ApproverType, s.ApproverValue, s.IsRequired);
        }).ToList();

        definition.ReplaceSteps(newSteps);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(definition);
    }
}
```

`UpdateWorkflowDefinitionValidator.cs`:
```csharp
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Workflow;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.UpdateWorkflowDefinition;

public sealed class UpdateWorkflowDefinitionValidator : AbstractValidator<UpdateWorkflowDefinitionCommand>
{
    public UpdateWorkflowDefinitionValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowDefinitionIdRequired);
        RuleFor(x => x.Name).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowNameRequired);
        RuleFor(x => x.Steps).NotEmpty().WithErrorCode(HrBusinessErrorCodes.WorkflowDefinitionNoSteps);
        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.Sequence).GreaterThan(0).WithErrorCode(HrBusinessErrorCodes.ValWorkflowStepSequenceInvalid);
            step.RuleFor(s => s.ApproverType)
                .Must(t => t is ApproverType.Role or ApproverType.Permission
                    or ApproverType.DirectManager or ApproverType.SpecificUser)
                .WithErrorCode(HrBusinessErrorCodes.ValWorkflowApproverTypeRequired);
        });
    }
}
```

- [ ] **Step 4: Create ActivateWorkflowDefinition command + handler + validator**

`ActivateWorkflowDefinitionCommand.cs`:
```csharp
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ActivateWorkflowDefinition;

public sealed record ActivateWorkflowDefinitionCommand(string Id) : IRequest<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>;
```

`ActivateWorkflowDefinitionHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ActivateWorkflowDefinition;

public sealed class ActivateWorkflowDefinitionHandler(
    ISqlRepository<WorkflowDefinition> repository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : IRequestHandler<ActivateWorkflowDefinitionCommand, OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>> Handle(
        ActivateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var id = new WorkflowDefinitionId(Guid.Parse(request.Id));
        var definition = await repository.GetQueryable()
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (definition is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionNotFound);

        if (definition.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionAlreadyActive);

        try
        {
            definition.Activate();
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionNoSteps);
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(definition);
    }
}
```

`ActivateWorkflowDefinitionValidator.cs`:
```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ActivateWorkflowDefinition;

public sealed class ActivateWorkflowDefinitionValidator : AbstractValidator<ActivateWorkflowDefinitionCommand>
{
    public ActivateWorkflowDefinitionValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowDefinitionIdRequired);
    }
}
```

- [ ] **Step 5: Create DeactivateWorkflowDefinition command + handler + validator**

`DeactivateWorkflowDefinitionCommand.cs`:
```csharp
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.DeactivateWorkflowDefinition;

public sealed record DeactivateWorkflowDefinitionCommand(string Id) : IRequest<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>;
```

`DeactivateWorkflowDefinitionHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.DeactivateWorkflowDefinition;

public sealed class DeactivateWorkflowDefinitionHandler(
    ISqlRepository<WorkflowDefinition> repository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : IRequestHandler<DeactivateWorkflowDefinitionCommand, OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>> Handle(
        DeactivateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var id = new WorkflowDefinitionId(Guid.Parse(request.Id));
        var definition = await repository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (definition is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionNotFound);

        if (!definition.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionAlreadyInactive);

        definition.Deactivate();

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(definition);
    }
}
```

`DeactivateWorkflowDefinitionValidator.cs`:
```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.DeactivateWorkflowDefinition;

public sealed class DeactivateWorkflowDefinitionValidator : AbstractValidator<DeactivateWorkflowDefinitionCommand>
{
    public DeactivateWorkflowDefinitionValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowDefinitionIdRequired);
    }
}
```

- [ ] **Step 6: Build and verify**

```bash
dotnet build Anemoi.Hr.Application/Anemoi.Hr.Application.csproj
```
Expected: Build succeeded. 0 errors.

---

### Task 7b (Insert): IUserRolePermissionService Abstraction

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Abstractions/IUserRolePermissionService.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Services/UserRolePermissionService.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Installers/ServiceInstaller.cs`

- [ ] **Step 1: Create abstraction interface**

`IUserRolePermissionService.cs`:
```csharp
namespace Anemoi.Hr.Application.Abstractions;

public interface IUserRolePermissionService
{
    bool UserHasRole(string userId, string role);
    bool UserHasPermission(string userId, string permission);
}
```

- [ ] **Step 2: Create basic implementation (Phase 28 stub)**

`UserRolePermissionService.cs`:
```csharp
using Anemoi.Hr.Application.Abstractions;

namespace Anemoi.Hr.Infrastructure.Services;

public sealed class UserRolePermissionService : IUserRolePermissionService
{
    public bool UserHasRole(string userId, string role)
    {
        // Phase 28: stub — will integrate with Identity service in Phase 29+
        return false;
    }

    public bool UserHasPermission(string userId, string permission)
    {
        // Phase 28: stub — will integrate with Identity service in Phase 29+
        return false;
    }
}
```

- [ ] **Step 3: Register in ServiceInstaller**

```csharp
services.AddScoped<IUserRolePermissionService, UserRolePermissionService>();
```

### Task 8: Workflow Instance Commands (Start, Approve, Reject, Cancel, Return)

**Files:**
- Create: ... (same file list) ...

- [ ] **Step 1: Create StartWorkflow command + handler + validator**

`StartWorkflowCommand.cs`:
```csharp
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.StartWorkflow;

public sealed record StartWorkflowCommand(
    string DefinitionId,
    string EntityType,
    string EntityId,
    string StartedBy) : IRequest<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>;
```

`StartWorkflowHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.StartWorkflow;

public sealed class StartWorkflowHandler(
    ISqlRepository<WorkflowDefinition> definitionRepository,
    ISqlRepository<WorkflowInstance> instanceRepository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : IRequestHandler<StartWorkflowCommand, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        var definitionId = new WorkflowDefinitionId(Guid.Parse(request.DefinitionId));
        var definition = await definitionRepository.GetQueryable()
            .Include(x => x.Steps.OrderBy(s => s.Sequence))
            .FirstOrDefaultAsync(x => x.Id == definitionId, cancellationToken);

        if (definition is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionNotFound);

        if (!definition.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionNoSteps);

        var instanceId = new WorkflowInstanceId(IdGenerator.NextGuid());

        var instanceSteps = definition.Steps.Select(s =>
        {
            var stepId = new WorkflowInstanceStepId(IdGenerator.NextGuid());
            string? approverUserId = s.ApproverType switch
            {
                ApproverType.SpecificUser => s.ApproverValue,
                ApproverType.DirectManager => ResolveDirectManager(request.StartedBy),
                _ => null
            };
            return WorkflowInstanceStep.Create(stepId, instanceId, s.Sequence,
                s.ApproverType, s.ApproverValue, approverUserId);
        }).ToList();

        var instance = WorkflowInstance.Start(instanceId, definitionId,
            request.EntityType, request.EntityId, request.StartedBy, instanceSteps);

        var createResult = await instanceRepository.CreateOneAsync(instance, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(instance, definition.Name);
    }

    private static string? ResolveDirectManager(string userId)
    {
        // Phase 28: placeholder — will be implemented when integrated with employee data
        return null;
    }
}
```

`StartWorkflowValidator.cs`:
```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.StartWorkflow;

public sealed class StartWorkflowValidator : AbstractValidator<StartWorkflowCommand>
{
    public StartWorkflowValidator()
    {
        RuleFor(x => x.DefinitionId).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowDefinitionIdRequired);
        RuleFor(x => x.EntityType).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowApproverTypeRequired);
        RuleFor(x => x.EntityId).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowInstanceIdRequired);
    }
}
```

- [ ] **Step 2: Create ApproveWorkflowStep command + handler + validator**

`ApproveWorkflowStepCommand.cs`:
```csharp
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ApproveWorkflowStep;

public sealed record ApproveWorkflowStepCommand(
    string InstanceId,
    string? Comment,
    string PerformedBy) : IRequest<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>;
```

`ApproveWorkflowStepHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ApproveWorkflowStep;

public sealed class ApproveWorkflowStepHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowHistory> historyRepository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper,
    IUserRolePermissionService permissionService)
    : IRequestHandler<ApproveWorkflowStepCommand, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        ApproveWorkflowStepCommand request, CancellationToken cancellationToken)
    {
        var instanceId = new WorkflowInstanceId(Guid.Parse(request.InstanceId));
        var instance = await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .FirstOrDefaultAsync(x => x.Id == instanceId, cancellationToken);

        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        try
        {
            if (!instance.IsCurrentStepApprover(
                    request.PerformedBy,
                    role => permissionService.UserHasRole(request.PerformedBy, role),
                    perm => permissionService.UserHasPermission(request.PerformedBy, perm)))
                return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotApprover);

            var history = instance.Approve(request.PerformedBy, request.Comment);

            var historyCreateResult = await historyRepository.CreateOneAsync(history, cancellationToken);
            if (historyCreateResult.TryPickT1(out var exception, out _))
                return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
        catch (InvalidOperationException ex)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceInvalidStatus);
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(instance);
    }
}
```

`ApproveWorkflowStepValidator.cs`:
```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ApproveWorkflowStep;

public sealed class ApproveWorkflowStepValidator : AbstractValidator<ApproveWorkflowStepCommand>
{
    public ApproveWorkflowStepValidator()
    {
        RuleFor(x => x.InstanceId).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowInstanceIdRequired);
    }
}
```

- [ ] **Step 3: Create RejectWorkflowStep command + handler + validator**

`RejectWorkflowStepCommand.cs`:
```csharp
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.RejectWorkflowStep;

public sealed record RejectWorkflowStepCommand(
    string InstanceId,
    string? Comment,
    string PerformedBy) : IRequest<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>;
```

`RejectWorkflowStepHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.RejectWorkflowStep;

public sealed class RejectWorkflowStepHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowHistory> historyRepository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper,
    IUserRolePermissionService permissionService)
    : IRequestHandler<RejectWorkflowStepCommand, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        RejectWorkflowStepCommand request, CancellationToken cancellationToken)
    {
        var instanceId = new WorkflowInstanceId(Guid.Parse(request.InstanceId));
        var instance = await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .FirstOrDefaultAsync(x => x.Id == instanceId, cancellationToken);

        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        try
        {
            if (!instance.IsCurrentStepApprover(
                    request.PerformedBy,
                    role => permissionService.UserHasRole(request.PerformedBy, role),
                    perm => permissionService.UserHasPermission(request.PerformedBy, perm)))
                return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotApprover);

            var history = instance.Reject(request.PerformedBy, request.Comment);

            var historyCreateResult = await historyRepository.CreateOneAsync(history, cancellationToken);
            if (historyCreateResult.TryPickT1(out var exception, out _))
                return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceInvalidStatus);
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(instance);
    }
}
```

`RejectWorkflowStepValidator.cs`:
```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.RejectWorkflowStep;

public sealed class RejectWorkflowStepValidator : AbstractValidator<RejectWorkflowStepCommand>
{
    public RejectWorkflowStepValidator()
    {
        RuleFor(x => x.InstanceId).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowInstanceIdRequired);
    }
}
```

- [ ] **Step 4: Create CancelWorkflow command + handler + validator**

`CancelWorkflowCommand.cs`:
```csharp
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CancelWorkflow;

public sealed record CancelWorkflowCommand(
    string InstanceId,
    string PerformedBy) : IRequest<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>;
```

`CancelWorkflowHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CancelWorkflow;

public sealed class CancelWorkflowHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowHistory> historyRepository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : IRequestHandler<CancelWorkflowCommand, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        CancelWorkflowCommand request, CancellationToken cancellationToken)
    {
        var instanceId = new WorkflowInstanceId(Guid.Parse(request.InstanceId));
        var instance = await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .FirstOrDefaultAsync(x => x.Id == instanceId, cancellationToken);

        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        try
        {
            var history = instance.Cancel(request.PerformedBy);

            var historyCreateResult = await historyRepository.CreateOneAsync(history, cancellationToken);
            if (historyCreateResult.TryPickT1(out var exception, out _))
                return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceInvalidStatus);
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(instance);
    }
}
```

`CancelWorkflowValidator.cs`:
```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CancelWorkflow;

public sealed class CancelWorkflowValidator : AbstractValidator<CancelWorkflowCommand>
{
    public CancelWorkflowValidator()
    {
        RuleFor(x => x.InstanceId).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowInstanceIdRequired);
    }
}
```

- [ ] **Step 5: Create ReturnWorkflow command + handler + validator**

`ReturnWorkflowCommand.cs`:
```csharp
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ReturnWorkflow;

public sealed record ReturnWorkflowCommand(
    string InstanceId,
    string? Comment,
    string PerformedBy) : IRequest<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>;
```

`ReturnWorkflowHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ReturnWorkflow;

public sealed class ReturnWorkflowHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowHistory> historyRepository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : IRequestHandler<ReturnWorkflowCommand, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        ReturnWorkflowCommand request, CancellationToken cancellationToken)
    {
        var instanceId = new WorkflowInstanceId(Guid.Parse(request.InstanceId));
        var instance = await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .FirstOrDefaultAsync(x => x.Id == instanceId, cancellationToken);

        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        try
        {
            var history = instance.ReturnForRevision(request.PerformedBy, request.Comment);

            var historyCreateResult = await historyRepository.CreateOneAsync(history, cancellationToken);
            if (historyCreateResult.TryPickT1(out var exception, out _))
                return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceInvalidStatus);
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(instance);
    }
}
```

`ReturnWorkflowValidator.cs`:
```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ReturnWorkflow;

public sealed class ReturnWorkflowValidator : AbstractValidator<ReturnWorkflowCommand>
{
    public ReturnWorkflowValidator()
    {
        RuleFor(x => x.InstanceId).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowInstanceIdRequired);
    }
}
```

- [ ] **Step 6: Build and verify**

```bash
dotnet build Anemoi.Hr.Application/Anemoi.Hr.Application.csproj
```
Expected: Build succeeded. 0 errors.

---

### Task 9: Workflow Queries (Definitions, Instances, Pending)

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/WorkflowQueries/GetWorkflowDefinitions/GetWorkflowDefinitionsQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/WorkflowQueries/GetWorkflowDefinitions/GetWorkflowDefinitionsHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/WorkflowQueries/GetWorkflowDefinitionById/GetWorkflowDefinitionByIdQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/WorkflowQueries/GetWorkflowDefinitionById/GetWorkflowDefinitionByIdHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/WorkflowQueries/GetWorkflowInstances/GetWorkflowInstancesQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/WorkflowQueries/GetWorkflowInstances/GetWorkflowInstancesHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/WorkflowQueries/GetWorkflowInstanceById/GetWorkflowInstanceByIdQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/WorkflowQueries/GetWorkflowInstanceById/GetWorkflowInstanceByIdHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/WorkflowQueries/GetPendingApprovals/GetPendingApprovalsQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/WorkflowQueries/GetPendingApprovals/GetPendingApprovalsHandler.cs`

- [ ] **Step 1: Create GetWorkflowDefinitions query + handler**

`GetWorkflowDefinitionsQuery.cs`:
```csharp
using Anemoi.Hr.Application.Responses;
using MediatR;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowDefinitions;

public sealed record GetWorkflowDefinitionsQuery(
    bool? IsActive,
    string? WorkflowTypeCode,
    int Page = 1,
    int PageSize = 20) : IRequest<PaginationResponse<WorkflowDefinitionResponse>>;
```

`GetWorkflowDefinitionsHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowDefinitions;

public sealed class GetWorkflowDefinitionsHandler(
    ISqlRepository<WorkflowDefinition> repository,
    WorkflowMapper mapper)
    : IRequestHandler<GetWorkflowDefinitionsQuery, PaginationResponse<WorkflowDefinitionResponse>>
{
    public async Task<PaginationResponse<WorkflowDefinitionResponse>> Handle(
        GetWorkflowDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable()
            .Include(x => x.Steps.OrderBy(s => s.Sequence))
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (!string.IsNullOrEmpty(request.WorkflowTypeCode))
            query = query.Where(x => x.WorkflowTypeCode == request.WorkflowTypeCode);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<WorkflowDefinitionResponse>(
            mapper.ToResponses(items), total, request.Page, request.PageSize);
    }
}
```

- [ ] **Step 2: Create GetWorkflowDefinitionById query + handler**

`GetWorkflowDefinitionByIdQuery.cs`:
```csharp
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowDefinitionById;

public sealed record GetWorkflowDefinitionByIdQuery(string Id) : IRequest<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>;
```

`GetWorkflowDefinitionByIdHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowDefinitionById;

public sealed class GetWorkflowDefinitionByIdHandler(
    ISqlRepository<WorkflowDefinition> repository,
    WorkflowMapper mapper)
    : IRequestHandler<GetWorkflowDefinitionByIdQuery, OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>> Handle(
        GetWorkflowDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var id = new WorkflowDefinitionId(Guid.Parse(request.Id));
        var definition = await repository.GetQueryable()
            .Include(x => x.Steps.OrderBy(s => s.Sequence))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (definition is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionNotFound);

        return mapper.ToResponse(definition);
    }
}
```

- [ ] **Step 3: Create GetWorkflowInstances query + handler**

`GetWorkflowInstancesQuery.cs`:
```csharp
using Anemoi.Hr.Application.Responses;
using MediatR;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstances;

public sealed record GetWorkflowInstancesQuery(
    string? Status,
    string? EntityType,
    string? WorkflowDefinitionId,
    int Page = 1,
    int PageSize = 20) : IRequest<PaginationResponse<WorkflowInstanceResponse>>;
```

`GetWorkflowInstancesHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstances;

public sealed class GetWorkflowInstancesHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowDefinition> definitionRepository,
    WorkflowMapper mapper)
    : IRequestHandler<GetWorkflowInstancesQuery, PaginationResponse<WorkflowInstanceResponse>>
{
    public async Task<PaginationResponse<WorkflowInstanceResponse>> Handle(
        GetWorkflowInstancesQuery request, CancellationToken cancellationToken)
    {
        var query = instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrEmpty(request.EntityType))
            query = query.Where(x => x.EntityType == request.EntityType);
        if (!string.IsNullOrEmpty(request.WorkflowDefinitionId))
        {
            var defId = new WorkflowDefinitionId(Guid.Parse(request.WorkflowDefinitionId));
            query = query.Where(x => x.WorkflowDefinitionId == defId);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.StartedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var defIds = items.Select(x => x.WorkflowDefinitionId.Value).Distinct().ToList();
        var defNames = await definitionRepository.GetQueryable()
            .Where(d => defIds.Contains(d.Id.Value))
            .ToDictionaryAsync(d => d.Id.Value, d => d.Name, cancellationToken);

        return new PaginationResponse<WorkflowInstanceResponse>(
            mapper.ToResponses(items, defNames), total, request.Page, request.PageSize);
    }
}
```

- [ ] **Step 4: Create GetWorkflowInstanceById query + handler**

`GetWorkflowInstanceByIdQuery.cs`:
```csharp
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstanceById;

public sealed record GetWorkflowInstanceByIdQuery(string Id) : IRequest<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>;
```

`GetWorkflowInstanceByIdHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstanceById;

public sealed class GetWorkflowInstanceByIdHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowDefinition> definitionRepository,
    WorkflowMapper mapper)
    : IRequestHandler<GetWorkflowInstanceByIdQuery, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        GetWorkflowInstanceByIdQuery request, CancellationToken cancellationToken)
    {
        var id = new WorkflowInstanceId(Guid.Parse(request.Id));
        var instance = await instanceRepository.GetQueryable()
            .Include(x => x.Steps.OrderBy(s => s.Sequence))
            .Include(x => x.Histories.OrderByDescending(h => h.PerformedAt))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        var definition = await definitionRepository.GetQueryable()
            .FirstOrDefaultAsync(d => d.Id == instance.WorkflowDefinitionId, cancellationToken);

        return mapper.ToResponse(instance, definition?.Name);
    }
}
```

- [ ] **Step 5: Create GetPendingApprovals query + handler**

`GetPendingApprovalsQuery.cs`:
```csharp
using Anemoi.Hr.Application.Responses;
using MediatR;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetPendingApprovals;

public sealed record GetPendingApprovalsQuery(
    string UserId,
    int Page = 1,
    int PageSize = 20) : IRequest<PaginationResponse<WorkflowInstanceResponse>>;
```

`GetPendingApprovalsHandler.cs`:
```csharp
using Anemoi.BuildingBlock.Application;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetPendingApprovals;

public sealed class GetPendingApprovalsHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowDefinition> definitionRepository,
    WorkflowMapper mapper,
    IUserRolePermissionService permissionService)
    : IRequestHandler<GetPendingApprovalsQuery, PaginationResponse<WorkflowInstanceResponse>>
{
    public async Task<PaginationResponse<WorkflowInstanceResponse>> Handle(
        GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        var pendingInstances = await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .Where(x => x.Status == WorkflowStatusCode.Pending)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);

        var matched = pendingInstances.Where(instance =>
        {
            var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
            if (step is null) return false;

            return step.ApproverTypeSnapshot switch
            {
                ApproverType.SpecificUser => step.ApproverValueSnapshot == request.UserId,
                ApproverType.DirectManager => step.ApproverUserId == request.UserId,
                ApproverType.Role => step.ApproverValueSnapshot != null && permissionService.UserHasRole(request.UserId, step.ApproverValueSnapshot),
                ApproverType.Permission => step.ApproverValueSnapshot != null && permissionService.UserHasPermission(request.UserId, step.ApproverValueSnapshot),
                _ => false
            };
        }).ToList();

        var total = matched.Count;
        var pageItems = matched
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var defIds = pageItems.Select(x => x.WorkflowDefinitionId.Value).Distinct().ToList();
        var defNames = await definitionRepository.GetQueryable()
            .Where(d => defIds.Contains(d.Id.Value))
            .ToDictionaryAsync(d => d.Id.Value, d => d.Name, cancellationToken);

        return new PaginationResponse<WorkflowInstanceResponse>(
            mapper.ToResponses(pageItems, defNames), total, request.Page, request.PageSize);
    }
}
```

- [ ] **Step 6: Build and verify**

```bash
dotnet build Anemoi.Hr.Application/Anemoi.Hr.Application.csproj
```
Expected: Build succeeded. 0 errors.

---

### Task 10: API Controllers

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Api/Controllers/Workflow/WorkflowDefinitionsController.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Api/Controllers/Workflow/WorkflowInstancesController.cs`

- [ ] **Step 1: Create WorkflowDefinitionsController**

```csharp
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ActivateWorkflowDefinition;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CreateWorkflowDefinition;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.DeactivateWorkflowDefinition;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.UpdateWorkflowDefinition;
using Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowDefinitionById;
using Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowDefinitions;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MediatR;

namespace Anemoi.Hr.Api.Controllers.Workflow;

[ApiController]
[Route("api/hr/workflows/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class WorkflowDefinitionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.WorkflowView)]
    [ProducesResponseType(typeof(PaginationResponse<WorkflowDefinitionResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<WorkflowDefinitionResponse>> GetWorkflowDefinitions(
        [FromQuery] GetWorkflowDefinitionsQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.WorkflowView)]
    [ProducesResponseType(typeof(WorkflowDefinitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkflowDefinitionById(
        [FromRoute] string id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetWorkflowDefinitionByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowManage)]
    [ProducesResponseType(typeof(WorkflowDefinitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateWorkflowDefinition(
        [FromBody] CreateWorkflowDefinitionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut]
    [HasPermission(HrPermissions.WorkflowManage)]
    [ProducesResponseType(typeof(WorkflowDefinitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateWorkflowDefinition(
        [FromBody] UpdateWorkflowDefinitionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowManage)]
    [ProducesResponseType(typeof(WorkflowDefinitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateWorkflowDefinition(
        [FromBody] ActivateWorkflowDefinitionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowManage)]
    [ProducesResponseType(typeof(WorkflowDefinitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateWorkflowDefinition(
        [FromBody] DeactivateWorkflowDefinitionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
```

- [ ] **Step 2: Create WorkflowInstancesController**

```csharp
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ApproveWorkflowStep;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CancelWorkflow;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.RejectWorkflowStep;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ReturnWorkflow;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.StartWorkflow;
using Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstanceById;
using Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstances;
using Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetPendingApprovals;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MediatR;
using Anemoi.BuildingBlock.Api;

namespace Anemoi.Hr.Api.Controllers.Workflow;

[ApiController]
[Route("api/hr/workflows/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class WorkflowInstancesController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.WorkflowManage)]
    [ProducesResponseType(typeof(WorkflowInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartWorkflow(
        [FromBody] StartWorkflowCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { StartedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowExecute)]
    [ProducesResponseType(typeof(WorkflowInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveWorkflowStep(
        [FromBody] ApproveWorkflowStepCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { PerformedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowExecute)]
    [ProducesResponseType(typeof(WorkflowInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectWorkflowStep(
        [FromBody] RejectWorkflowStepCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { PerformedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowExecute)]
    [ProducesResponseType(typeof(WorkflowInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelWorkflow(
        [FromBody] CancelWorkflowCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { PerformedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.WorkflowExecute)]
    [ProducesResponseType(typeof(WorkflowInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReturnWorkflow(
        [FromBody] ReturnWorkflowCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { PerformedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.WorkflowView)]
    [ProducesResponseType(typeof(PaginationResponse<WorkflowInstanceResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<WorkflowInstanceResponse>> GetWorkflowInstances(
        [FromQuery] GetWorkflowInstancesQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.WorkflowView)]
    [ProducesResponseType(typeof(WorkflowInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkflowInstanceById(
        [FromRoute] string id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetWorkflowInstanceByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.WorkflowExecute)]
    [ProducesResponseType(typeof(PaginationResponse<WorkflowInstanceResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<WorkflowInstanceResponse>> GetPendingApprovals(
        [FromQuery] GetPendingApprovalsQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query with { UserId = HttpContext.GetUserId() }, cancellationToken);
    }
}
```

- [ ] **Step 3: Build and verify**

```bash
dotnet build Anemoi.Hr.Api/Anemoi.Hr.Api.csproj
```
Expected: Build succeeded. 0 errors.

---

### Task 11: Frontend — Types, API Endpoints, Permissions, Service

**Files:**
- Create: `cody-web-app/src/types/hr/workflow.ts`
- Create: `cody-web-app/src/services/hr/workflowService.ts`
- Modify: `cody-web-app/src/constants/permissions.ts`
- Modify: `cody-web-app/src/constants/api-endpoints.ts`

- [ ] **Step 1: Create TypeScript types**

`workflow.ts`:
```typescript
export interface WorkflowDefinition {
  id: string;
  code: string;
  name: string;
  description: string | null;
  workflowTypeCode: string;
  isActive: boolean;
  steps: WorkflowDefinitionStep[];
  createdAt: string;
  updatedAt: string;
}

export interface WorkflowDefinitionStep {
  id: string;
  sequence: number;
  approverType: string;
  approverValue: string | null;
  isRequired: boolean;
}

export interface WorkflowInstance {
  id: string;
  workflowDefinitionId: string;
  workflowDefinitionName: string;
  entityType: string;
  entityId: string;
  currentStep: number;
  status: string;
  startedBy: string;
  startedAt: string;
  completedAt: string | null;
  steps: WorkflowInstanceStep[];
  histories: WorkflowHistory[];
}

export interface WorkflowInstanceStep {
  id: string;
  sequence: number;
  approverTypeSnapshot: string;
  approverValueSnapshot: string | null;
  approverUserId: string | null;
  status: string;
  approvedAt: string | null;
  rejectedAt: string | null;
  comment: string | null;
}

export interface WorkflowHistory {
  id: string;
  workflowInstanceId: string;
  action: string;
  performedBy: string;
  comment: string | null;
  performedAt: string;
}
```

- [ ] **Step 2: Add workflow permissions**

In `permissions.ts`, add:
```typescript
HR_WORKFLOW_VIEW: "hr.workflow.view",
HR_WORKFLOW_MANAGE: "hr.workflow.manage",
HR_WORKFLOW_EXECUTE: "hr.workflow.execute",
```

In `ROUTE_PERMISSIONS`, add:
```typescript
"/hr/workflows": [PERMISSIONS.HR_WORKFLOW_VIEW],
```

- [ ] **Step 3: Add workflow API endpoints**

In `api-endpoints.ts`, add under `hr:`:
```typescript
workflow: {
  definitions: {
    getAll: "/api/hr/workflows/WorkflowDefinitions/GetWorkflowDefinitions",
    getById: (id: string) => `/api/hr/workflows/WorkflowDefinitions/GetWorkflowDefinitionById/${id}`,
    create: "/api/hr/workflows/WorkflowDefinitions/CreateWorkflowDefinition",
    update: "/api/hr/workflows/WorkflowDefinitions/UpdateWorkflowDefinition",
    activate: "/api/hr/workflows/WorkflowDefinitions/ActivateWorkflowDefinition",
    deactivate: "/api/hr/workflows/WorkflowDefinitions/DeactivateWorkflowDefinition",
  },
  instances: {
    start: "/api/hr/workflows/WorkflowInstances/StartWorkflow",
    approve: "/api/hr/workflows/WorkflowInstances/ApproveWorkflowStep",
    reject: "/api/hr/workflows/WorkflowInstances/RejectWorkflowStep",
    cancel: "/api/hr/workflows/WorkflowInstances/CancelWorkflow",
    return: "/api/hr/workflows/WorkflowInstances/ReturnWorkflow",
    getAll: "/api/hr/workflows/WorkflowInstances/GetWorkflowInstances",
    getById: (id: string) => `/api/hr/workflows/WorkflowInstances/GetWorkflowInstanceById/${id}`,
    pending: "/api/hr/workflows/WorkflowInstances/GetPendingApprovals",
  },
},
```

- [ ] **Step 4: Create workflow service**

`workflowService.ts`:
```typescript
import apiClient from "@/lib/apiClient";
import { API_ENDPOINTS } from "@/constants/api-endpoints";
import {
  WorkflowDefinition,
  WorkflowInstance,
} from "@/types/hr/workflow";

export const workflowService = {
  // Definitions
  getDefinitions: async (params?: Record<string, unknown>) => {
    const { data } = await apiClient.get(API_ENDPOINTS.hr.workflow.definitions.getAll, { params });
    return data;
  },
  getDefinitionById: async (id: string): Promise<WorkflowDefinition> => {
    const { data } = await apiClient.get(API_ENDPOINTS.hr.workflow.definitions.getById(id));
    return data;
  },
  createDefinition: async (payload: Record<string, unknown>): Promise<WorkflowDefinition> => {
    const { data } = await apiClient.post(API_ENDPOINTS.hr.workflow.definitions.create, payload);
    return data;
  },
  updateDefinition: async (payload: Record<string, unknown>): Promise<WorkflowDefinition> => {
    const { data } = await apiClient.put(API_ENDPOINTS.hr.workflow.definitions.update, payload);
    return data;
  },
  activateDefinition: async (id: string): Promise<WorkflowDefinition> => {
    const { data } = await apiClient.post(API_ENDPOINTS.hr.workflow.definitions.activate, { id });
    return data;
  },
  deactivateDefinition: async (id: string): Promise<WorkflowDefinition> => {
    const { data } = await apiClient.post(API_ENDPOINTS.hr.workflow.definitions.deactivate, { id });
    return data;
  },

  // Instances
  startWorkflow: async (payload: Record<string, unknown>): Promise<WorkflowInstance> => {
    const { data } = await apiClient.post(API_ENDPOINTS.hr.workflow.instances.start, payload);
    return data;
  },
  approveStep: async (payload: Record<string, unknown>): Promise<WorkflowInstance> => {
    const { data } = await apiClient.post(API_ENDPOINTS.hr.workflow.instances.approve, payload);
    return data;
  },
  rejectStep: async (payload: Record<string, unknown>): Promise<WorkflowInstance> => {
    const { data } = await apiClient.post(API_ENDPOINTS.hr.workflow.instances.reject, payload);
    return data;
  },
  cancel: async (payload: Record<string, unknown>): Promise<WorkflowInstance> => {
    const { data } = await apiClient.post(API_ENDPOINTS.hr.workflow.instances.cancel, payload);
    return data;
  },
  returnForRevision: async (payload: Record<string, unknown>): Promise<WorkflowInstance> => {
    const { data } = await apiClient.post(API_ENDPOINTS.hr.workflow.instances.return, payload);
    return data;
  },
  getInstances: async (params?: Record<string, unknown>) => {
    const { data } = await apiClient.get(API_ENDPOINTS.hr.workflow.instances.getAll, { params });
    return data;
  },
  getInstanceById: async (id: string): Promise<WorkflowInstance> => {
    const { data } = await apiClient.get(API_ENDPOINTS.hr.workflow.instances.getById(id));
    return data;
  },
  getPendingApprovals: async (params?: Record<string, unknown>) => {
    const { data } = await apiClient.get(API_ENDPOINTS.hr.workflow.instances.pending, { params });
    return data;
  },
};
```

---

### Task 12: Frontend — Workflows Page

**Files:**
- Create: `cody-web-app/src/app/[locale]/(dashboard)/hr/workflows/page.tsx`
- Modify: Sidebar component (add HR Workflows menu item)
- Modify: Localization files (en.json, vi.json)

- [ ] **Step 1: Create tabbed workflows page**

Create `page.tsx` with three tabs:
- **Definitions tab**: table of workflow definitions with activate/deactivate actions
- **Instances tab**: table of workflow instances with status badges
- **Pending Approvals tab**: table of pending approvals with approve/reject buttons

The page should use the existing UI component patterns from other HR pages (Shadcn Table, Tabs, Badge, Button, Dialog).

- [ ] **Step 2: Add sidebar menu item**

In the sidebar nav config, add under HR group:
```typescript
{ href: "/hr/workflows", label: "hrWorkflows", icon: CheckCheck },
```

- [ ] **Step 3: Add localization keys**

In `en.json`:
```json
"hrWorkflows": "Workflows",
"hrWorkflows.tab.definitions": "Definitions",
"hrWorkflows.tab.instances": "Instances",
"hrWorkflows.tab.pending": "Pending Approvals",
"hrWorkflows.definitions.code": "Code",
"hrWorkflows.definitions.name": "Name",
"hrWorkflows.definitions.type": "Type",
"hrWorkflows.definitions.active": "Active",
"hrWorkflows.definitions.inactive": "Inactive",
"hrWorkflows.definitions.steps": "Steps",
"hrWorkflows.instances.entityType": "Entity Type",
"hrWorkflows.instances.entityId": "Entity ID",
"hrWorkflows.instances.status": "Status",
"hrWorkflows.instances.startedBy": "Started By",
"hrWorkflows.instances.startedAt": "Started At",
"hrWorkflows.actions.approve": "Approve",
"hrWorkflows.actions.reject": "Reject",
"hrWorkflows.actions.cancel": "Cancel",
"hrWorkflows.actions.return": "Return for Revision",
```

In `vi.json`, add Vietnamese translations for the same keys.

---

### Task 13: Domain Unit Tests

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Test/Domain/Workflow/WorkflowDefinitionTests.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Test/Domain/Workflow/WorkflowInstanceTests.cs`

- [ ] **Step 1: Write WorkflowDefinition tests**

```csharp
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;

namespace Anemoi.Hr.Test.Domain.Workflow;

public sealed class WorkflowDefinitionTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var id = new WorkflowDefinitionId(Guid.NewGuid());
        var steps = new List<WorkflowDefinitionStep>
        {
            WorkflowDefinitionStep.Create(
                new WorkflowDefinitionStepId(Guid.NewGuid()), id, 1,
                ApproverType.Role, "HR_Manager", true)
        };

        var def = WorkflowDefinition.Create(id, "REQ-001", "Test Workflow", null,
            WorkflowTypeCode.Approval, steps);

        def.Code.Should().Be("REQ-001");
        def.Name.Should().Be("Test Workflow");
        def.IsActive.Should().BeFalse();
        def.Steps.Should().HaveCount(1);
    }

    [Fact]
    public void Activate_WithSteps_ShouldSetActive()
    {
        var id = new WorkflowDefinitionId(Guid.NewGuid());
        var steps = new List<WorkflowDefinitionStep>
        {
            WorkflowDefinitionStep.Create(
                new WorkflowDefinitionStepId(Guid.NewGuid()), id, 1,
                ApproverType.Role, "HR_Manager", true)
        };
        var def = WorkflowDefinition.Create(id, "TST", "Test", null,
            WorkflowTypeCode.Approval, steps);

        def.Activate();

        def.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WithoutSteps_ShouldThrow()
    {
        var id = new WorkflowDefinitionId(Guid.NewGuid());
        var def = WorkflowDefinition.Create(id, "TST", "Test", null,
            WorkflowTypeCode.Approval, []);

        Action act = () => def.Activate();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deactivate_ShouldSetInactive()
    {
        var id = new WorkflowDefinitionId(Guid.NewGuid());
        var steps = new List<WorkflowDefinitionStep>
        {
            WorkflowDefinitionStep.Create(
                new WorkflowDefinitionStepId(Guid.NewGuid()), id, 1,
                ApproverType.Role, "HR_Manager", true)
        };
        var def = WorkflowDefinition.Create(id, "TST", "Test", null,
            WorkflowTypeCode.Approval, steps);
        def.Activate();

        def.Deactivate();

        def.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ReplaceSteps_WhenActive_ShouldThrow()
    {
        var id = new WorkflowDefinitionId(Guid.NewGuid());
        var steps = new List<WorkflowDefinitionStep>
        {
            WorkflowDefinitionStep.Create(
                new WorkflowDefinitionStepId(Guid.NewGuid()), id, 1,
                ApproverType.Role, "HR_Manager", true)
        };
        var def = WorkflowDefinition.Create(id, "TST", "Test", null,
            WorkflowTypeCode.Approval, steps);
        def.Activate();

        Action act = () => def.ReplaceSteps([]);
        act.Should().Throw<InvalidOperationException>();
    }
}
```

- [ ] **Step 2: Write WorkflowInstance tests**

```csharp
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;

namespace Anemoi.Hr.Test.Domain.Workflow;

public sealed class WorkflowInstanceTests
{
    private static (WorkflowInstance Instance, WorkflowInstanceId Id) CreatePendingInstance(int stepCount = 1)
    {
        var defId = new WorkflowDefinitionId(Guid.NewGuid());
        var instanceId = new WorkflowInstanceId(Guid.NewGuid());
        var steps = Enumerable.Range(1, stepCount).Select(i =>
            WorkflowInstanceStep.Create(
                new WorkflowInstanceStepId(Guid.NewGuid()), instanceId, i,
                ApproverType.SpecificUser, "user-1", "user-1")).ToList();

        var instance = WorkflowInstance.Start(instanceId, defId,
            "TestEntity", "entity-1", "requester-1", steps);
        return (instance, instanceId);
    }

    [Fact]
    public void Start_ShouldSetPending()
    {
        var (instance, _) = CreatePendingInstance();
        instance.Status.Should().Be(WorkflowStatusCode.Pending);
        instance.CurrentStep.Should().Be(1);
    }

    [Fact]
    public void Approve_LastStep_ShouldComplete()
    {
        var (instance, _) = CreatePendingInstance(1);

        instance.Approve("user-1", null);

        instance.Status.Should().Be(WorkflowStatusCode.Approved);
        instance.CurrentStep.Should().Be(1);
        instance.CompletedAt.Should().NotBeNull();
        instance.Steps.First().Status.Should().Be(WorkflowStepStatusCode.Approved);
    }

    [Fact]
    public void Approve_FirstOfMultipleSteps_ShouldAdvance()
    {
        var (instance, _) = CreatePendingInstance(3);

        instance.Approve("user-1", null);

        instance.Status.Should().Be(WorkflowStatusCode.Pending);
        instance.CurrentStep.Should().Be(2);
    }

    [Fact]
    public void Approve_AllSteps_ShouldComplete()
    {
        var (instance, _) = CreatePendingInstance(3);

        instance.Approve("user-1", null);
        instance.Approve("user-1", null);
        instance.Approve("user-1", null);

        instance.Status.Should().Be(WorkflowStatusCode.Approved);
        instance.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_ShouldSetRejected()
    {
        var (instance, _) = CreatePendingInstance(3);

        instance.Reject("user-1", "Not approved");

        instance.Status.Should().Be(WorkflowStatusCode.Rejected);
        instance.CompletedAt.Should().NotBeNull();
        instance.Steps.First().Status.Should().Be(WorkflowStepStatusCode.Rejected);
    }

    [Fact]
    public void Cancel_ShouldSetCancelled()
    {
        var (instance, _) = CreatePendingInstance(2);

        instance.Cancel("requester-1");

        instance.Status.Should().Be(WorkflowStatusCode.Cancelled);
        instance.CompletedAt.Should().NotBeNull();
        instance.Steps.Should().AllSatisfy(s => s.Status.Should().Be(WorkflowStepStatusCode.Cancelled));
    }

    [Fact]
    public void Cancel_AlreadyApproved_ShouldThrow()
    {
        var (instance, _) = CreatePendingInstance(1);
        instance.Approve("user-1", null);

        Action act = () => instance.Cancel("requester-1");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReturnForRevision_ShouldSetReturned()
    {
        var (instance, _) = CreatePendingInstance(2);

        instance.ReturnForRevision("user-1", "Please revise");

        instance.Status.Should().Be(WorkflowStatusCode.Returned);
        instance.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_NotCurrentStepApprover_ShouldFail()
    {
        var (instance, _) = CreatePendingInstance(1);

        var result = instance.IsCurrentStepApprover("wrong-user",
            _ => false, _ => false);

        result.Should().BeFalse();
    }

    [Fact]
    public void Approve_CurrentSpecificUser_ShouldPass()
    {
        var (instance, _) = CreatePendingInstance(1);

        var result = instance.IsCurrentStepApprover("user-1",
            _ => false, _ => false);

        result.Should().BeTrue();
    }

    [Fact]
    public void Approve_WithRole_ShouldPass()
    {
        var defId = new WorkflowDefinitionId(Guid.NewGuid());
        var instanceId = new WorkflowInstanceId(Guid.NewGuid());
        var step = WorkflowInstanceStep.Create(
            new WorkflowInstanceStepId(Guid.NewGuid()), instanceId, 1,
            ApproverType.Role, "HR_Manager", null);
        var instance = WorkflowInstance.Start(instanceId, defId,
            "Test", "e-1", "requester-1", [step]);

        var result = instance.IsCurrentStepApprover("any-user",
            role => role == "HR_Manager", _ => false);

        result.Should().BeTrue();
    }

    [Fact]
    public void Approve_WithPermission_ShouldPass()
    {
        var defId = new WorkflowDefinitionId(Guid.NewGuid());
        var instanceId = new WorkflowInstanceId(Guid.NewGuid());
        var step = WorkflowInstanceStep.Create(
            new WorkflowInstanceStepId(Guid.NewGuid()), instanceId, 1,
            ApproverType.Permission, "hr.workflow.execute", null);
        var instance = WorkflowInstance.Start(instanceId, defId,
            "Test", "e-1", "requester-1", [step]);

        var result = instance.IsCurrentStepApprover("any-user",
            _ => false, perm => perm == "hr.workflow.execute");

        result.Should().BeTrue();
    }
}
```

- [ ] **Step 3: Run tests and verify**

```bash
dotnet test Anemoi.Hr.Test/Anemoi.Hr.Test.csproj --filter "FullyQualifiedName~Workflow"
```
Expected: All tests pass.

---

### Task 14: Final Build Verification

- [ ] **Step 1: Full solution build**

```bash
dotnet build Anemoi.sln
```
Expected: Build succeeded. 0 errors. 0 warnings.

- [ ] **Step 2: Full test run**

```bash
dotnet test
```
Expected: All tests pass.

- [ ] **Step 3: Frontend build**

```bash
npm run build
```
Expected: Build succeeded. 0 errors.
