# Phase 26 — Onboarding Management Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement Onboarding Management module — reusable plan templates, snapshot-based instances, role-resolved task assignment, task-driven completion workflow.

**Architecture:** Template → Snapshot Instance → Tasks (same pattern as Payroll snapshots). Aggregate roots: OnboardingPlanTemplate, OnboardingInstance. Child entities: OnboardingTaskTemplate, OnboardingTask (aggregate-managed, no xmin). Clean Architecture + CQRS.

**Tech Stack:** .NET 10, ASP.NET Core, EF Core + PostgreSQL, MediatR, OneOf, Mapperly, FluentValidation, Next.js 16, React Query, shadcn/ui

---

## File Structure

### Backend — New Files (40+ files)

**Anemoi.Hr.ModelIds/ModelIds/**
- `OnboardingPlanTemplateId.cs` — strongly-typed ID
- `OnboardingTaskTemplateId.cs` — strongly-typed ID
- `OnboardingInstanceId.cs` — strongly-typed ID
- `OnboardingTaskId.cs` — strongly-typed ID

**Anemoi.Hr.Domain/Onboarding/**
- `OnboardingPlanTemplate.cs` — aggregate root
- `OnboardingTaskTemplate.cs` — child entity
- `OnboardingInstance.cs` — aggregate root
- `OnboardingTask.cs` — child entity
- `OnboardingPlanTemplateStatusCode.cs` — Active, Inactive
- `OnboardingInstanceStatusCode.cs` — Draft, InProgress, Completed, Cancelled
- `OnboardingTaskStatusCode.cs` — Pending, Completed, Skipped
- `AssigneeRoleCode.cs` — HR, Manager, IT, Employee
- `Events/OnboardingTaskAssignedDomainEvent.cs`
- `Events/OnboardingTaskCompletedDomainEvent.cs`
- `Events/OnboardingTaskSkippedDomainEvent.cs`
- `Events/OnboardingInstanceCompletedDomainEvent.cs`
- `Events/OnboardingInstanceCancelledDomainEvent.cs`

**Anemoi.Hr.Application/Configurations/**
- Modify: `HrBusinessErrorCodes.cs` — add HR_ONB_* codes
- Modify: `HrPermissions.cs` — add onboarding permissions
- Modify: `Permissions.cs` (BuildingBlocks) — add permission definitions

**Anemoi.Hr.Application/Mappings/**
- `OnboardingMapper.cs` — manual mapper

**Anemoi.Hr.Application/Responses/**
- `OnboardingPlanTemplateResponse.cs`
- `OnboardingTaskTemplateResponse.cs`
- `OnboardingInstanceResponse.cs`
- `OnboardingTaskResponse.cs`
- `OnboardingDashboardResponse.cs`

**Anemoi.Hr.Application/Cqrs/Commands/OnboardingCommands/**
- `CreateOnboardingPlanTemplate/` — Command, Handler, Validator
- `UpdateOnboardingPlanTemplate/` — Command, Handler, Validator
- `ActivateOnboardingPlanTemplate/` — Command, Handler
- `DeactivateOnboardingPlanTemplate/` — Command, Handler
- `StartOnboarding/` — Command, Handler, Validator
- `CompleteOnboardingTask/` — Command, Handler, Validator
- `ReopenOnboardingTask/` — Command, Handler, Validator
- `SkipOnboardingTask/` — Command, Handler, Validator
- `ReassignOnboardingTask/` — Command, Handler, Validator
- `CancelOnboarding/` — Command, Handler
- `ReopenOnboarding/` — Command, Handler
- `ForceCompleteOnboarding/` — Command, Handler, Validator

**Anemoi.Hr.Application/Cqrs/Queries/OnboardingQueries/**
- `GetOnboardingPlanTemplates/` — Query, Handler
- `GetOnboardingPlanTemplateById/` — Query, Handler
- `GetOnboardingInstances/` — Query, Handler
- `GetOnboardingInstanceById/` — Query, Handler
- `GetEmployeeOnboarding/` — Query, Handler
- `GetMyOnboardingTasks/` — Query, Handler
- `GetPendingOnboardingTasks/` — Query, Handler
- `GetOnboardingDashboard/` — Query, Handler

**Anemoi.Hr.Infrastructure/**
- `EntityConfigurations/OnboardingModelMapping.cs` — EF Core configs
- Modify: `Persistence/HrDbContext.cs` — add DbSets
- `ServiceInstallers/OnboardingServiceInstaller.cs` — DI

**Anemoi.Hr.Api/Controllers/**
- `OnboardingTemplatesController.cs`
- `OnboardingInstancesController.cs`
- `OnboardingTasksController.cs`

**Anemoi.Hr.Test/**
- `Domain/Onboarding/OnboardingPlanTemplateTests.cs`
- `Domain/Onboarding/OnboardingInstanceTests.cs`
- `Domain/Onboarding/OnboardingTaskTests.cs`
- `Application/Onboarding/CreateTemplateHandlerTests.cs`
- `Application/Onboarding/StartOnboardingHandlerTests.cs`
- `Application/Onboarding/CompleteTaskHandlerTests.cs`

### Frontend — New/Modified Files

**Pages:**
- `cody-web-app/src/app/[locale]/hr/onboarding/templates/page.tsx`
- `cody-web-app/src/app/[locale]/hr/onboarding/instances/page.tsx`
- `cody-web-app/src/app/[locale]/hr/onboarding/instances/[id]/page.tsx`
- `cody-web-app/src/app/[locale]/hr/onboarding/tasks/page.tsx`
- `cody-web-app/src/app/[locale]/ess/onboarding/page.tsx`

**Components:**
- `cody-web-app/src/components/hr/onboarding/` (template-form, instance-table, task-list, etc.)

**API Services & Hooks:**
- `cody-web-app/src/services/hr/onboarding.ts`
- `cody-web-app/src/hooks/hr/onboarding/` (useOnboardingTemplates, useOnboardingInstances, etc.)

**Localization:**
- Modify en.json and vi.json

---

### Task 1: Model IDs (4 files)

**Files:**
- Create: `Anemoi.Hr.ModelIds/ModelIds/OnboardingPlanTemplateId.cs`
- Create: `Anemoi.Hr.ModelIds/ModelIds/OnboardingTaskTemplateId.cs`
- Create: `Anemoi.Hr.ModelIds/ModelIds/OnboardingInstanceId.cs`
- Create: `Anemoi.Hr.ModelIds/ModelIds/OnboardingTaskId.cs`

- [ ] **Step 1: Create OnboardingPlanTemplateId.cs**

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record OnboardingPlanTemplateId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 2: Create OnboardingTaskTemplateId.cs**

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record OnboardingTaskTemplateId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 3: Create OnboardingInstanceId.cs**

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record OnboardingInstanceId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 4: Create OnboardingTaskId.cs**

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record OnboardingTaskId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 5: Verify build**

Run: `dotnet build Anemoi.Hr.ModelIds/Anemoi.Hr.ModelIds.csproj`
Expected: 0 errors

---

### Task 2: Domain Status Code Constants (4 files)

**Files:**
- Create: `Anemoi.Hr.Domain/Onboarding/OnboardingPlanTemplateStatusCode.cs`
- Create: `Anemoi.Hr.Domain/Onboarding/OnboardingInstanceStatusCode.cs`
- Create: `Anemoi.Hr.Domain/Onboarding/OnboardingTaskStatusCode.cs`
- Create: `Anemoi.Hr.Domain/Onboarding/AssigneeRoleCode.cs`

- [ ] **Step 1: Create OnboardingPlanTemplateStatusCode.cs**

```csharp
namespace Anemoi.Hr.Domain.Onboarding;

public static class OnboardingPlanTemplateStatusCode
{
    public const string Active = nameof(Active);
    public const string Inactive = nameof(Inactive);
}
```

- [ ] **Step 2: Create OnboardingInstanceStatusCode.cs**

```csharp
namespace Anemoi.Hr.Domain.Onboarding;

public static class OnboardingInstanceStatusCode
{
    public const string Draft = nameof(Draft);
    public const string InProgress = nameof(InProgress);
    public const string Completed = nameof(Completed);
    public const string Cancelled = nameof(Cancelled);
}
```

- [ ] **Step 3: Create OnboardingTaskStatusCode.cs**

```csharp
namespace Anemoi.Hr.Domain.Onboarding;

public static class OnboardingTaskStatusCode
{
    public const string Pending = nameof(Pending);
    public const string Completed = nameof(Completed);
    public const string Skipped = nameof(Skipped);
}
```

- [ ] **Step 4: Create AssigneeRoleCode.cs**

```csharp
namespace Anemoi.Hr.Domain.Onboarding;

public static class AssigneeRoleCode
{
    public const string Hr = nameof(Hr);
    public const string Manager = nameof(Manager);
    public const string It = "IT";
    public const string Employee = nameof(Employee);
}
```

- [ ] **Step 5: Verify build**

Run: `dotnet build Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj`
Expected: 0 errors

---

### Task 3: Domain Entity — OnboardingPlanTemplate + OnboardingTaskTemplate

**Files:**
- Create: `Anemoi.Hr.Domain/Onboarding/OnboardingPlanTemplate.cs`
- Create: `Anemoi.Hr.Domain/Onboarding/OnboardingTaskTemplate.cs`

- [ ] **Step 1: Create OnboardingTaskTemplate.cs (child entity)**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Onboarding;

public sealed class OnboardingTaskTemplate : ValueObject
{
    public OnboardingTaskTemplateId Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public string AssigneeType { get; private set; } // "Role" only in Phase 26
    public string? AssigneeRoleCode { get; private set; }
    public int OffsetDays { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsRequired { get; private set; }

    private OnboardingTaskTemplate() { }

    private OnboardingTaskTemplate(
        OnboardingTaskTemplateId id,
        string title,
        string? description,
        string assigneeType,
        string? assigneeRoleCode,
        int offsetDays,
        int sortOrder,
        bool isRequired)
    {
        Id = id;
        Title = title;
        Description = description;
        AssigneeType = assigneeType;
        AssigneeRoleCode = assigneeRoleCode;
        OffsetDays = offsetDays;
        SortOrder = sortOrder;
        IsRequired = isRequired;
    }

    public static OnboardingTaskTemplate Create(
        OnboardingTaskTemplateId id,
        string title,
        string? description,
        string? assigneeRoleCode,
        int offsetDays,
        int sortOrder,
        bool isRequired = true)
    {
        return new OnboardingTaskTemplate(
            id, title, description, "Role", assigneeRoleCode,
            offsetDays, sortOrder, isRequired);
    }

    public void UpdateDetails(
        string title,
        string? description,
        string? assigneeRoleCode,
        int offsetDays,
        int sortOrder,
        bool isRequired)
    {
        Title = title;
        Description = description;
        AssigneeRoleCode = assigneeRoleCode;
        OffsetDays = offsetDays;
        SortOrder = sortOrder;
        IsRequired = isRequired;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
```

- [ ] **Step 2: Create OnboardingPlanTemplate.cs (aggregate root)**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Onboarding;

public sealed class OnboardingPlanTemplate : ValueObject
{
    private readonly List<OnboardingTaskTemplate> _taskTemplates = [];

    public OnboardingPlanTemplateId Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Status { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string UpdatedBy { get; private set; }
    public IReadOnlyList<OnboardingTaskTemplate> TaskTemplates => _taskTemplates.AsReadOnly();

    private OnboardingPlanTemplate() { }

    private OnboardingPlanTemplate(
        OnboardingPlanTemplateId id,
        string name,
        string? description,
        string createdBy)
    {
        Id = id;
        Name = name;
        Description = description;
        Status = OnboardingPlanTemplateStatusCode.Active;
        Version = 1;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        UpdatedAt = CreatedAt;
        UpdatedBy = createdBy;
    }

    public static OnboardingPlanTemplate Create(
        OnboardingPlanTemplateId id,
        string name,
        string? description,
        string createdBy)
    {
        return new OnboardingPlanTemplate(id, name, description, createdBy);
    }

    public void AddTaskTemplate(OnboardingTaskTemplate taskTemplate)
    {
        _taskTemplates.Add(taskTemplate);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string name,
        string? description,
        string updatedBy,
        List<OnboardingTaskTemplate> updatedTasks)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;

        _taskTemplates.Clear();
        _taskTemplates.AddRange(updatedTasks);
    }

    public bool Activate()
    {
        if (Status == OnboardingPlanTemplateStatusCode.Active) return false;
        Status = OnboardingPlanTemplateStatusCode.Active;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public bool Deactivate()
    {
        if (Status == OnboardingPlanTemplateStatusCode.Inactive) return false;
        Status = OnboardingPlanTemplateStatusCode.Inactive;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public bool IsActive() => Status == OnboardingPlanTemplateStatusCode.Active;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj`
Expected: 0 errors

---

### Task 4: Domain Entity — OnboardingInstance + OnboardingTask

**Files:**
- Create: `Anemoi.Hr.Domain/Onboarding/OnboardingInstance.cs`
- Create: `Anemoi.Hr.Domain/Onboarding/OnboardingTask.cs`

- [ ] **Step 1: Create OnboardingTask.cs (child entity, snapshot, no xmin)**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Onboarding;

public sealed class OnboardingTask : ValueObject
{
    public OnboardingTaskId Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public string AssigneeType { get; private set; }
    public string? AssigneeRoleCode { get; private set; }
    public string? AssignedUserId { get; private set; }
    public string? AssignedUserDisplayNameSnapshot { get; private set; }
    public DateTime? AssignedAt { get; private set; }
    public string? AssignedBy { get; private set; }
    public DateTime? ReassignedAt { get; private set; }
    public string? ReassignedBy { get; private set; }
    public DateTime DueDate { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsRequired { get; private set; }
    public string Status { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? CompletedBy { get; private set; }
    public string? CompletedNotes { get; private set; }
    public DateTime? SkippedAt { get; private set; }
    public string? SkippedBy { get; private set; }
    public DateTime? ReopenedAt { get; private set; }
    public string? ReopenedBy { get; private set; }
    public string? ReopenedReason { get; private set; }

    private OnboardingTask() { }

    private OnboardingTask(
        OnboardingTaskId id,
        string title,
        string? description,
        string assigneeType,
        string? assigneeRoleCode,
        string? assignedUserId,
        string? assignedUserDisplayNameSnapshot,
        string assignedBy,
        DateTime dueDate,
        int sortOrder,
        bool isRequired)
    {
        Id = id;
        Title = title;
        Description = description;
        AssigneeType = assigneeType;
        AssigneeRoleCode = assigneeRoleCode;
        AssignedUserId = assignedUserId;
        AssignedUserDisplayNameSnapshot = assignedUserDisplayNameSnapshot;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
        DueDate = dueDate;
        SortOrder = sortOrder;
        IsRequired = isRequired;
        Status = OnboardingTaskStatusCode.Pending;
    }

    public static OnboardingTask Create(
        OnboardingTaskId id,
        string title,
        string? description,
        string? assigneeRoleCode,
        string? assignedUserId,
        string? assignedUserDisplayNameSnapshot,
        string assignedBy,
        DateTime dueDate,
        int sortOrder,
        bool isRequired = true)
    {
        return new OnboardingTask(
            id, title, description, "Role", assigneeRoleCode,
            assignedUserId, assignedUserDisplayNameSnapshot,
            assignedBy, dueDate, sortOrder, isRequired);
    }

    public bool Complete(string completedBy, string? notes = null)
    {
        if (Status != OnboardingTaskStatusCode.Pending) return false;
        Status = OnboardingTaskStatusCode.Completed;
        CompletedAt = DateTime.UtcNow;
        CompletedBy = completedBy;
        CompletedNotes = notes;
        return true;
    }

    public bool Skip(string skippedBy)
    {
        if (Status != OnboardingTaskStatusCode.Pending) return false;
        Status = OnboardingTaskStatusCode.Skipped;
        SkippedAt = DateTime.UtcNow;
        SkippedBy = skippedBy;
        return true;
    }

    public bool Reopen(string reopenedBy, string? reason = null)
    {
        if (Status == OnboardingTaskStatusCode.Pending) return false;
        Status = OnboardingTaskStatusCode.Pending;
        ReopenedAt = DateTime.UtcNow;
        ReopenedBy = reopenedBy;
        ReopenedReason = reason;
        return true;
    }

    public bool Reassign(string newUserId, string? newUserDisplayName, string reassignedBy)
    {
        if (Status != OnboardingTaskStatusCode.Pending) return false;
        AssignedUserId = newUserId;
        AssignedUserDisplayNameSnapshot = newUserDisplayName;
        ReassignedAt = DateTime.UtcNow;
        ReassignedBy = reassignedBy;
        return true;
    }

    public bool IsOverdue()
    {
        return Status == OnboardingTaskStatusCode.Pending && DueDate < DateTime.UtcNow;
    }

    public bool IsCompleted() => Status == OnboardingTaskStatusCode.Completed;
    public bool IsSkipped() => Status == OnboardingTaskStatusCode.Skipped;
    public bool IsPending() => Status == OnboardingTaskStatusCode.Pending;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
```

- [ ] **Step 2: Create OnboardingInstance.cs (aggregate root, with xmin)**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Onboarding;

public sealed class OnboardingInstance : ValueObject
{
    private readonly List<OnboardingTask> _tasks = [];

    public OnboardingInstanceId Id { get; private set; }
    public EmployeeId EmployeeId { get; private set; }
    public OnboardingPlanTemplateId? TemplateId { get; private set; }
    public string TemplateName { get; private set; }
    public int? TemplateVersion { get; private set; }
    public string Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? CompletedBy { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancelledBy { get; private set; }
    public DateTime? ForceCompletedAt { get; private set; }
    public string? ForceCompletedBy { get; private set; }
    public string? ForceCompleteReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string UpdatedBy { get; private set; }
    public IReadOnlyList<OnboardingTask> Tasks => _tasks.AsReadOnly();

    private OnboardingInstance() { }

    private OnboardingInstance(
        OnboardingInstanceId id,
        EmployeeId employeeId,
        OnboardingPlanTemplateId? templateId,
        string templateName,
        int? templateVersion,
        DateTime startDate,
        string createdBy)
    {
        Id = id;
        EmployeeId = employeeId;
        TemplateId = templateId;
        TemplateName = templateName;
        TemplateVersion = templateVersion;
        Status = OnboardingInstanceStatusCode.InProgress;
        StartDate = startDate;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        UpdatedAt = CreatedAt;
        UpdatedBy = createdBy;
    }

    public static OnboardingInstance Create(
        OnboardingInstanceId id,
        EmployeeId employeeId,
        OnboardingPlanTemplateId? templateId,
        string templateName,
        int? templateVersion,
        DateTime startDate,
        string createdBy)
    {
        return new OnboardingInstance(
            id, employeeId, templateId, templateName, templateVersion,
            startDate, createdBy);
    }

    public void AddTask(OnboardingTask task)
    {
        _tasks.Add(task);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CompleteTask(OnboardingTaskId taskId, string completedBy, string? notes = null)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null || !task.Complete(completedBy, notes)) return false;

        CheckAutoCompletion();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = completedBy;
        return true;
    }

    public bool SkipTask(OnboardingTaskId taskId, string skippedBy)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null || !task.Skip(skippedBy)) return false;

        CheckAutoCompletion();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = skippedBy;
        return true;
    }

    public bool ReopenTask(OnboardingTaskId taskId, string reopenedBy, string? reason = null)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null || !task.Reopen(reopenedBy, reason)) return false;

        // If instance was Completed, revert to InProgress
        if (Status == OnboardingInstanceStatusCode.Completed)
        {
            Status = OnboardingInstanceStatusCode.InProgress;
            CompletedAt = null;
            CompletedBy = null;
        }

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = reopenedBy;
        return true;
    }

    public bool ReassignTask(OnboardingTaskId taskId, string newUserId, string? newUserDisplayName, string reassignedBy)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null || !task.Reassign(newUserId, newUserDisplayName, reassignedBy)) return false;

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = reassignedBy;
        return true;
    }

    public bool Cancel(string cancelledBy)
    {
        if (Status != OnboardingInstanceStatusCode.InProgress) return false;
        Status = OnboardingInstanceStatusCode.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancelledBy = cancelledBy;
        UpdatedAt = CancelledAt.Value;
        UpdatedBy = cancelledBy;
        return true;
    }

    public bool Reopen(string reopenedBy)
    {
        if (Status != OnboardingInstanceStatusCode.Completed &&
            Status != OnboardingInstanceStatusCode.Cancelled) return false;
        Status = OnboardingInstanceStatusCode.InProgress;
        CompletedAt = null;
        CompletedBy = null;
        CancelledAt = null;
        CancelledBy = null;
        ForceCompletedAt = null;
        ForceCompletedBy = null;
        ForceCompleteReason = null;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = reopenedBy;
        return true;
    }

    public bool ForceComplete(string completedBy, string reason)
    {
        if (Status != OnboardingInstanceStatusCode.InProgress) return false;
        Status = OnboardingInstanceStatusCode.Completed;
        ForceCompletedAt = DateTime.UtcNow;
        ForceCompletedBy = completedBy;
        ForceCompleteReason = reason;
        CompletedAt = ForceCompletedAt;
        CompletedBy = completedBy;
        UpdatedAt = ForceCompletedAt.Value;
        UpdatedBy = completedBy;
        return true;
    }

    public double GetCompletionPercentage()
    {
        if (_tasks.Count == 0) return 0;
        return (double)_tasks.Count(t => t.IsCompleted() || t.IsSkipped()) / _tasks.Count * 100;
    }

    private void CheckAutoCompletion()
    {
        if (_tasks.Count > 0 && _tasks.All(t => t.IsCompleted() || t.IsSkipped()))
        {
            Status = OnboardingInstanceStatusCode.Completed;
            CompletedAt = DateTime.UtcNow;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj`
Expected: 0 errors

---

### Task 5: Domain Events (5 files)

**Files:**
- Create: `Anemoi.Hr.Domain/Onboarding/Events/OnboardingTaskAssignedDomainEvent.cs`
- Create: `Anemoi.Hr.Domain/Onboarding/Events/OnboardingTaskCompletedDomainEvent.cs`
- Create: `Anemoi.Hr.Domain/Onboarding/Events/OnboardingTaskSkippedDomainEvent.cs`
- Create: `Anemoi.Hr.Domain/Onboarding/Events/OnboardingInstanceCompletedDomainEvent.cs`
- Create: `Anemoi.Hr.Domain/Onboarding/Events/OnboardingInstanceCancelledDomainEvent.cs`

- [ ] **Step 1: Create all 5 domain event records**

```csharp
// OnboardingTaskAssignedDomainEvent.cs
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Onboarding.Events;

public sealed record OnboardingTaskAssignedDomainEvent(
    OnboardingTaskId TaskId,
    OnboardingInstanceId InstanceId,
    string? AssignedUserId,
    string AssignedBy,
    DateTime DueDate)
    : DomainEvent;

// OnboardingTaskCompletedDomainEvent.cs
public sealed record OnboardingTaskCompletedDomainEvent(
    OnboardingTaskId TaskId,
    OnboardingInstanceId InstanceId,
    string CompletedBy,
    DateTime CompletedAt)
    : DomainEvent;

// OnboardingTaskSkippedDomainEvent.cs
public sealed record OnboardingTaskSkippedDomainEvent(
    OnboardingTaskId TaskId,
    OnboardingInstanceId InstanceId,
    string SkippedBy,
    DateTime SkippedAt)
    : DomainEvent;

// OnboardingInstanceCompletedDomainEvent.cs
public sealed record OnboardingInstanceCompletedDomainEvent(
    OnboardingInstanceId InstanceId,
    EmployeeId EmployeeId,
    string CompletedBy,
    string CompletionType) // Auto / Manual / Force
    : DomainEvent;

// OnboardingInstanceCancelledDomainEvent.cs
public sealed record OnboardingInstanceCancelledDomainEvent(
    OnboardingInstanceId InstanceId,
    EmployeeId EmployeeId,
    string CancelledBy,
    DateTime CancelledAt)
    : DomainEvent;
```

- [ ] **Step 2: Verify build**

Run: `dotnet build`
Expected: 0 errors

---

### Task 6: Error Codes + Permissions (modify 3 files)

**Files:**
- Modify: `Anemoi.BuildingBlocks/Application/Authorization/Permissions.cs` — add onboarding definitions
- Modify: `Anemoi.Hr.Application/Configurations/HrPermissions.cs` — add onboarding permission props
- Modify: `Anemoi.Hr.Application/Configurations/HrBusinessErrorCodes.cs` — add HR_ONB_* constants

- [ ] **Step 1: Read existing files to understand exact patterns**

- [ ] **Step 2: Add onboarding permission definitions to Permissions.cs**

```csharp
// Add to Permissions.Definitions list after recruitment definitions
new Definition(HrOnboardingView, HrGroup, "hr.permission.onboarding.view"),
new Definition(HrOnboardingManage, HrGroup, "hr.permission.onboarding.manage"),
new Definition(HrOnboardingTaskComplete, HrGroup, "hr.permission.onboarding.task.complete"),
new Definition(HrOnboardingTaskManage, HrGroup, "hr.permission.onboarding.task.manage"),
```

```csharp
// Add permission constants
public const string HrOnboardingView = "hr.onboarding.view";
public const string HrOnboardingManage = "hr.onboarding.manage";
public const string HrOnboardingTaskComplete = "hr.onboarding.task.complete";
public const string HrOnboardingTaskManage = "hr.onboarding.task.manage";
```

- [ ] **Step 3: Add to HrPermissions.cs**

```csharp
public const string OnboardingView = Permissions.HrOnboardingView;
public const string OnboardingManage = Permissions.HrOnboardingManage;
public const string OnboardingTaskComplete = Permissions.HrOnboardingTaskComplete;
public const string OnboardingTaskManage = Permissions.HrOnboardingTaskManage;
```

- [ ] **Step 4: Add error codes to HrBusinessErrorCodes.cs**

```csharp
// Onboarding
public const string HrOnboardingTemplateNotFound = "HR_ONB_TEMPLATE_NOT_FOUND";
public const string HrOnboardingTemplateInactive = "HR_ONB_TEMPLATE_INACTIVE";
public const string HrOnboardingTemplateInUse = "HR_ONB_TEMPLATE_IN_USE";
public const string HrOnboardingInstanceNotFound = "HR_ONB_INSTANCE_NOT_FOUND";
public const string HrOnboardingInstanceInvalidStatus = "HR_ONB_INSTANCE_INVALID_STATUS";
public const string HrOnboardingInstanceAlreadyCompleted = "HR_ONB_INSTANCE_ALREADY_COMPLETED";
public const string HrOnboardingInstanceAlreadyCancelled = "HR_ONB_INSTANCE_ALREADY_CANCELLED";
public const string HrOnboardingInstanceNotInProgress = "HR_ONB_INSTANCE_NOT_IN_PROGRESS";
public const string HrOnboardingInstanceForceCompleteRequiresReason = "HR_ONB_INSTANCE_FORCE_COMPLETE_REQUIRES_REASON";
public const string HrOnboardingTaskNotFound = "HR_ONB_TASK_NOT_FOUND";
public const string HrOnboardingTaskInvalidStatus = "HR_ONB_TASK_INVALID_STATUS";
public const string HrOnboardingTaskAlreadyCompleted = "HR_ONB_TASK_ALREADY_COMPLETED";
public const string HrOnboardingTaskAlreadySkipped = "HR_ONB_TASK_ALREADY_SKIPPED";
public const string HrOnboardingEmployeeAlreadyOnboarding = "HR_ONB_EMPLOYEE_ALREADY_ONBOARDING";
public const string HrOnboardingEmployeeNotFound = "HR_ONB_EMPLOYEE_NOT_FOUND";
public const string HrOnboardingResolveRoleMissing = "HR_ONB_RESOLVE_ROLE_MISSING";
public const string HrOnboardingTemplateHasNoTasks = "HR_ONB_TEMPLATE_HAS_NO_TASKS";
public const string HrOnboardingTaskAlreadyReopened = "HR_ONB_TASK_ALREADY_REOPENED";
```

- [ ] **Step 5: Verify build**

Run: `dotnet build`
Expected: 0 errors

---

### Task 7: Response DTOs (5 files)

**Files:**
- Create: `Anemoi.Hr.Application/Responses/OnboardingPlanTemplateResponse.cs`
- Create: `Anemoi.Hr.Application/Responses/OnboardingTaskTemplateResponse.cs`
- Create: `Anemoi.Hr.Application/Responses/OnboardingInstanceResponse.cs`
- Create: `Anemoi.Hr.Application/Responses/OnboardingTaskResponse.cs`
- Create: `Anemoi.Hr.Application/Responses/OnboardingDashboardResponse.cs`

- [ ] **Step 1: Create OnboardingPlanTemplateResponse.cs**

```csharp
namespace Anemoi.Hr.Application.Responses;

public sealed class OnboardingPlanTemplateResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public List<OnboardingTaskTemplateResponse> TaskTemplates { get; set; } = [];
}
```

- [ ] **Step 2: Create OnboardingTaskTemplateResponse.cs**

```csharp
namespace Anemoi.Hr.Application.Responses;

public sealed class OnboardingTaskTemplateResponse
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? AssigneeRoleCode { get; set; }
    public int OffsetDays { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; }
}
```

- [ ] **Step 3: Create OnboardingInstanceResponse.cs**

```csharp
namespace Anemoi.Hr.Application.Responses;

public sealed class OnboardingInstanceResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public string? TemplateId { get; set; }
    public string TemplateName { get; set; }
    public int? TemplateVersion { get; set; }
    public string Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelledBy { get; set; }
    public string? ForceCompleteReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public double CompletionPercentage { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public List<OnboardingTaskResponse> Tasks { get; set; } = [];
}
```

- [ ] **Step 4: Create OnboardingTaskResponse.cs**

```csharp
namespace Anemoi.Hr.Application.Responses;

public sealed class OnboardingTaskResponse
{
    public string Id { get; set; }
    public string InstanceId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? AssigneeRoleCode { get; set; }
    public string? AssignedUserId { get; set; }
    public string? AssignedUserDisplayName { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime DueDate { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; }
    public string Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public string? CompletedNotes { get; set; }
    public DateTime? SkippedAt { get; set; }
    public string? SkippedBy { get; set; }
    public bool IsOverdue { get; set; }
}
```

- [ ] **Step 5: Create OnboardingDashboardResponse.cs**

```csharp
namespace Anemoi.Hr.Application.Responses;

public sealed class OnboardingDashboardResponse
{
    public int TotalActiveOnboardings { get; set; }
    public int TotalOverdueTasks { get; set; }
    public int TotalPendingTasks { get; set; }
    public int TotalUpcomingTasks { get; set; }
    public int CompletedThisMonth { get; set; }
    public double AverageCompletionRate { get; set; }
}
```

- [ ] **Step 6: Verify build**

Run: `dotnet build`
Expected: 0 errors

---

### Task 8: Mapper (1 file)

**Files:**
- Create: `Anemoi.Hr.Application/Mappings/OnboardingMapper.cs`

- [ ] **Step 1: Create OnboardingMapper.cs**

```csharp
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;

namespace Anemoi.Hr.Application.Mappings;

public sealed class OnboardingMapper
{
    public OnboardingPlanTemplateResponse ToResponse(OnboardingPlanTemplate template)
    {
        return new OnboardingPlanTemplateResponse
        {
            Id = template.Id.Value.ToString(),
            Name = template.Name,
            Description = template.Description,
            Status = template.Status,
            Version = template.Version,
            CreatedAt = template.CreatedAt,
            CreatedBy = template.CreatedBy,
            UpdatedAt = template.UpdatedAt,
            UpdatedBy = template.UpdatedBy,
            TaskTemplates = template.TaskTemplates.Select(ToResponse).ToList()
        };
    }

    public OnboardingTaskTemplateResponse ToResponse(OnboardingTaskTemplate taskTemplate)
    {
        return new OnboardingTaskTemplateResponse
        {
            Id = taskTemplate.Id.Value.ToString(),
            Title = taskTemplate.Title,
            Description = taskTemplate.Description,
            AssigneeRoleCode = taskTemplate.AssigneeRoleCode,
            OffsetDays = taskTemplate.OffsetDays,
            SortOrder = taskTemplate.SortOrder,
            IsRequired = taskTemplate.IsRequired
        };
    }

    public OnboardingInstanceResponse ToResponse(OnboardingInstance instance)
    {
        var tasks = instance.Tasks
            .OrderBy(t => t.SortOrder)
            .Select(ToResponse)
            .ToList();

        return new OnboardingInstanceResponse
        {
            Id = instance.Id.Value.ToString(),
            EmployeeId = instance.EmployeeId.Value.ToString(),
            TemplateId = instance.TemplateId?.Value.ToString(),
            TemplateName = instance.TemplateName,
            TemplateVersion = instance.TemplateVersion,
            Status = instance.Status,
            StartDate = instance.StartDate,
            CompletedAt = instance.CompletedAt,
            CompletedBy = instance.CompletedBy,
            CancelledAt = instance.CancelledAt,
            CancelledBy = instance.CancelledBy,
            ForceCompleteReason = instance.ForceCompleteReason,
            CreatedAt = instance.CreatedAt,
            CreatedBy = instance.CreatedBy,
            CompletionPercentage = instance.GetCompletionPercentage(),
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(t => t.Status == OnboardingTaskStatusCode.Completed || t.Status == OnboardingTaskStatusCode.Skipped),
            OverdueTasks = tasks.Count(t => t.IsOverdue),
            Tasks = tasks
        };
    }

    public OnboardingTaskResponse ToResponse(OnboardingTask task)
    {
        return new OnboardingTaskResponse
        {
            Id = task.Id.Value.ToString(),
            InstanceId = Guid.Empty.ToString(), // populated by caller if needed
            Title = task.Title,
            Description = task.Description,
            AssigneeRoleCode = task.AssigneeRoleCode,
            AssignedUserId = task.AssignedUserId,
            AssignedUserDisplayName = task.AssignedUserDisplayNameSnapshot,
            AssignedAt = task.AssignedAt,
            DueDate = task.DueDate,
            SortOrder = task.SortOrder,
            IsRequired = task.IsRequired,
            Status = task.Status,
            CompletedAt = task.CompletedAt,
            CompletedBy = task.CompletedBy,
            CompletedNotes = task.CompletedNotes,
            SkippedAt = task.SkippedAt,
            SkippedBy = task.SkippedBy,
            IsOverdue = task.IsOverdue()
        };
    }

    public OnboardingDashboardResponse ToDashboardResponse(
        int totalActive,
        int totalOverdue,
        int totalPending,
        int totalUpcoming,
        int completedThisMonth,
        double avgCompletionRate)
    {
        return new OnboardingDashboardResponse
        {
            TotalActiveOnboardings = totalActive,
            TotalOverdueTasks = totalOverdue,
            TotalPendingTasks = totalPending,
            TotalUpcomingTasks = totalUpcoming,
            CompletedThisMonth = completedThisMonth,
            AverageCompletionRate = avgCompletionRate
        };
    }
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build`
Expected: 0 errors

---

### Task 9: EF Core Configuration + DbContext

**Files:**
- Create: `Anemoi.Hr.Infrastructure/EntityConfigurations/OnboardingModelMapping.cs`
- Modify: `Anemoi.Hr.Infrastructure/Persistence/HrDbContext.cs`

- [ ] **Step 1: Create OnboardingModelMapping.cs**

```csharp
using Anemoi.Hr.Domain.Onboarding;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.EntityConfigurations;

public sealed class OnboardingPlanTemplateMapping : IEntityTypeConfiguration<OnboardingPlanTemplate>
{
    public void Configure(EntityTypeBuilder<OnboardingPlanTemplate> builder)
    {
        builder.ToTable("OnboardingPlanTemplates", "Hr");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new OnboardingPlanTemplateId(id));

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Version).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(200).IsRequired();

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasMany(x => x.TaskTemplates)
            .WithOne()
            .HasForeignKey("TemplateId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class OnboardingTaskTemplateMapping : IEntityTypeConfiguration<OnboardingTaskTemplate>
{
    public void Configure(EntityTypeBuilder<OnboardingTaskTemplate> builder)
    {
        builder.ToTable("OnboardingTaskTemplates", "Hr");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new OnboardingTaskTemplateId(id));

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.AssigneeType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AssigneeRoleCode).HasMaxLength(50);
        builder.Property(x => x.OffsetDays).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsRequired).IsRequired();
    }
}

public sealed class OnboardingInstanceMapping : IEntityTypeConfiguration<OnboardingInstance>
{
    public void Configure(EntityTypeBuilder<OnboardingInstance> builder)
    {
        builder.ToTable("OnboardingInstances", "Hr");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new OnboardingInstanceId(id));

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.TemplateId)
            .HasConversion(x => x!.Value, id => new OnboardingPlanTemplateId(id));
        builder.Property(x => x.TemplateName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CompletedBy).HasMaxLength(200);
        builder.Property(x => x.CancelledBy).HasMaxLength(200);
        builder.Property(x => x.ForceCompletedBy).HasMaxLength(200);
        builder.Property(x => x.ForceCompleteReason).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(200).IsRequired();

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();

        // Filtered unique index: only one active onboarding per employee
        builder.HasIndex("EmployeeId")
            .IsUnique()
            .HasFilter("\"Status\" IN ('Draft', 'InProgress')");
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.StartDate);

        builder.HasMany(x => x.Tasks)
            .WithOne()
            .HasForeignKey("InstanceId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class OnboardingTaskMapping : IEntityTypeConfiguration<OnboardingTask>
{
    public void Configure(EntityTypeBuilder<OnboardingTask> builder)
    {
        builder.ToTable("OnboardingTasks", "Hr");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new OnboardingTaskId(id));

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.AssigneeType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AssigneeRoleCode).HasMaxLength(50);
        builder.Property(x => x.AssignedUserId).HasMaxLength(200);
        builder.Property(x => x.AssignedUserDisplayNameSnapshot).HasMaxLength(300);
        builder.Property(x => x.AssignedBy).HasMaxLength(200);
        builder.Property(x => x.ReassignedBy).HasMaxLength(200);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CompletedBy).HasMaxLength(200);
        builder.Property(x => x.CompletedNotes).HasMaxLength(500);
        builder.Property(x => x.SkippedBy).HasMaxLength(200);
        builder.Property(x => x.ReopenedBy).HasMaxLength(200);
        builder.Property(x => x.ReopenedReason).HasMaxLength(500);

        builder.HasIndex(x => new { x.AssignedUserId, x.Status });
        builder.HasIndex(x => x.DueDate).HasFilter("\"Status\" = 'Pending'");
        builder.HasIndex(x => x.InstanceId);
    }
}
```

- [ ] **Step 2: Modify HrDbContext.cs — add DbSets**

Find the `public class HrDbContext` class and add after existing DbSets:
```csharp
public DbSet<OnboardingPlanTemplate> OnboardingPlanTemplates => Set<OnboardingPlanTemplate>();
public DbSet<OnboardingInstance> OnboardingInstances => Set<OnboardingInstance>();
```

Find the `OnModelCreating` method or the assembly marker configuration and ensure the onboarding mappings are picked up (they will be auto-discovered from the assembly if the assembly marker is used, but verify).

- [ ] **Step 3: Verify build**

Run: `dotnet build`
Expected: 0 errors

---

### Task 10: Commands — Template CRUD (4 command triads)

**Files:**
- Create: `Anemoi.Hr.Application/Cqrs/Commands/OnboardingCommands/CreateOnboardingPlanTemplate/` (3 files)
- Create: `Anemoi.Hr.Application/Cqrs/Commands/OnboardingCommands/UpdateOnboardingPlanTemplate/` (3 files)
- Create: `Anemoi.Hr.Application/Cqrs/Commands/OnboardingCommands/ActivateOnboardingPlanTemplate/` (2 files)
- Create: `Anemoi.Hr.Application/Cqrs/Commands/OnboardingCommands/DeactivateOnboardingPlanTemplate/` (2 files)

- [ ] **Step 1: Create CreateOnboardingPlanTemplateCommand**

```csharp
// Command
public sealed record CreateOnboardingPlanTemplateCommand(
    string Name,
    string? Description,
    List<CreateOnboardingTaskTemplateDto> TaskTemplates) : ICommandResult<OnboardingPlanTemplateResponse>
{
    [JsonIgnore]
    public string CreatedBy { get; set; }
}

public sealed record CreateOnboardingTaskTemplateDto(
    string Title,
    string? Description,
    string? AssigneeRoleCode,
    int OffsetDays,
    int SortOrder,
    bool IsRequired = true);

// Handler
public sealed class CreateOnboardingPlanTemplateHandler(
    ISqlRepository<OnboardingPlanTemplate> templateRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<CreateOnboardingPlanTemplateCommand, OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>> Handle(
        CreateOnboardingPlanTemplateCommand request, CancellationToken ct)
    {
        var id = new OnboardingPlanTemplateId(IdGenerator.NextGuid());
        var template = OnboardingPlanTemplate.Create(id, request.Name, request.Description, request.CreatedBy);

        foreach (var dto in request.TaskTemplates.OrderBy(t => t.SortOrder))
        {
            var taskId = new OnboardingTaskTemplateId(IdGenerator.NextGuid());
            var task = OnboardingTaskTemplate.Create(
                taskId, dto.Title, dto.Description, dto.AssigneeRoleCode,
                dto.OffsetDays, dto.SortOrder, dto.IsRequired);
            template.AddTaskTemplate(task);
        }

        await templateRepository.CreateOneAsync(template, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.ToResponse(template);
    }
}

// Validator
public sealed class CreateOnboardingPlanTemplateValidator : AbstractValidator<CreateOnboardingPlanTemplateCommand>
{
    public CreateOnboardingPlanTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValNameRequired);
        RuleFor(x => x.TaskTemplates)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.HrOnboardingTemplateHasNoTasks);
        RuleForEach(x => x.TaskTemplates).ChildRules(task =>
        {
            task.RuleFor(t => t.Title)
                .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValTitleRequired);
        });
    }
}
```

- [ ] **Step 2: Create UpdateOnboardingPlanTemplateCommand**

```csharp
// Command
public sealed record UpdateOnboardingPlanTemplateCommand(
    OnboardingPlanTemplateId Id,
    string Name,
    string? Description,
    List<UpdateOnboardingTaskTemplateDto> TaskTemplates) : ICommandResult<OnboardingPlanTemplateResponse>
{
    [JsonIgnore]
    public string UpdatedBy { get; set; }
}

public sealed record UpdateOnboardingTaskTemplateDto(
    string? Id,
    string Title,
    string? Description,
    string? AssigneeRoleCode,
    int OffsetDays,
    int SortOrder,
    bool IsRequired = true);

// Handler
public sealed class UpdateOnboardingPlanTemplateHandler(
    ISqlRepository<OnboardingPlanTemplate> templateRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<UpdateOnboardingPlanTemplateCommand, OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>> Handle(
        UpdateOnboardingPlanTemplateCommand request, CancellationToken ct)
    {
        var template = await templateRepository.GetOneByConditionAsync(
            x => x.Id == request.Id, ct);
        if (template == null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateNotFound);

        var updatedTasks = request.TaskTemplates
            .OrderBy(t => t.SortOrder)
            .Select(dto =>
            {
                var taskId = !string.IsNullOrEmpty(dto.Id)
                    ? new OnboardingTaskTemplateId(Guid.Parse(dto.Id))
                    : new OnboardingTaskTemplateId(IdGenerator.NextGuid());
                return OnboardingTaskTemplate.Create(
                    taskId, dto.Title, dto.Description, dto.AssigneeRoleCode,
                    dto.OffsetDays, dto.SortOrder, dto.IsRequired);
            }).ToList();

        template.UpdateDetails(request.Name, request.Description, request.UpdatedBy, updatedTasks);

        await templateRepository.UpdateOneAsync(template, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.ToResponse(template);
    }
}

// Validator
public sealed class UpdateOnboardingPlanTemplateValidator : AbstractValidator<UpdateOnboardingPlanTemplateCommand>
{
    public UpdateOnboardingPlanTemplateValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValTemplateIdRequired);
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValNameRequired);
        RuleFor(x => x.TaskTemplates)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.HrOnboardingTemplateHasNoTasks);
    }
}
```

- [ ] **Step 3: Create ActivateOnboardingPlanTemplateCommand**

```csharp
// Command
public sealed record ActivateOnboardingPlanTemplateCommand(
    OnboardingPlanTemplateId Id) : ICommandResult<OnboardingPlanTemplateResponse>
{
    [JsonIgnore]
    public string UpdatedBy { get; set; }
}

// Handler
public sealed class ActivateOnboardingPlanTemplateHandler(
    ISqlRepository<OnboardingPlanTemplate> templateRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<ActivateOnboardingPlanTemplateCommand, OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>> Handle(
        ActivateOnboardingPlanTemplateCommand request, CancellationToken ct)
    {
        var template = await templateRepository.GetOneByConditionAsync(
            x => x.Id == request.Id, ct);
        if (template == null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateNotFound);

        if (!template.Activate())
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateInactive);

        await templateRepository.UpdateOneAsync(template, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.ToResponse(template);
    }
}
```

- [ ] **Step 4: Create DeactivateOnboardingPlanTemplateCommand** (same pattern as Activate)

```csharp
// Command
public sealed record DeactivateOnboardingPlanTemplateCommand(
    OnboardingPlanTemplateId Id) : ICommandResult<OnboardingPlanTemplateResponse>
{
    [JsonIgnore]
    public string UpdatedBy { get; set; }
}

// Handler — same pattern, calls template.Deactivate()
```

- [ ] **Step 5: Verify build**

Run: `dotnet build`
Expected: 0 errors

---

### Task 11: Commands — Onboarding Start & Task Management (8 command triads)

**Files:**
- Create: `StartOnboarding/` (3 files)
- Create: `CompleteOnboardingTask/` (3 files)
- Create: `ReopenOnboardingTask/` (3 files)
- Create: `SkipOnboardingTask/` (3 files)
- Create: `ReassignOnboardingTask/` (3 files)
- Create: `CancelOnboarding/` (2 files)
- Create: `ReopenOnboarding/` (2 files)
- Create: `ForceCompleteOnboarding/` (3 files)

- [ ] **Step 1: Create StartOnboardingCommand**

```csharp
// Command
public sealed record StartOnboardingCommand(
    EmployeeId EmployeeId,
    OnboardingPlanTemplateId TemplateId,
    DateTime StartDate,
    Dictionary<string, string> RoleMappings) // e.g., {"Manager": "userId-guid", "IT": "userId-guid"}
    : ICommandResult<OnboardingInstanceResponse>
{
    [JsonIgnore]
    public string CreatedBy { get; set; }
}

// Handler
public sealed class StartOnboardingHandler(
    ISqlRepository<OnboardingPlanTemplate> templateRepository,
    ISqlRepository<OnboardingInstance> instanceRepository,
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<StartOnboardingCommand, OneOf<OnboardingInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingInstanceResponse, ErrorDetailResponse>> Handle(
        StartOnboardingCommand request, CancellationToken ct)
    {
        // Verify template exists and is active
        var template = await templateRepository.GetOneByConditionAsync(
            x => x.Id == request.TemplateId, ct);
        if (template == null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateNotFound);
        if (!template.IsActive())
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateInactive);
        if (template.TaskTemplates.Count == 0)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateHasNoTasks);

        // Verify employee exists
        var employee = await employeeRepository.GetOneByConditionAsync(
            x => x.Id == request.EmployeeId, ct);
        if (employee == null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingEmployeeNotFound);

        // Check for existing active onboarding (with DB unique index as backup)
        var existingOnboarding = await instanceRepository.GetOneByConditionAsync(
            x => x.EmployeeId == request.EmployeeId &&
                 (x.Status == OnboardingInstanceStatusCode.Draft ||
                  x.Status == OnboardingInstanceStatusCode.InProgress), ct);
        if (existingOnboarding != null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingEmployeeAlreadyOnboarding);

        // Create instance
        var instanceId = new OnboardingInstanceId(IdGenerator.NextGuid());
        var instance = OnboardingInstance.Create(
            instanceId, request.EmployeeId, request.TemplateId,
            template.Name, template.Version, request.StartDate, request.CreatedBy);

        // Snapshot tasks from template
        foreach (var taskTemplate in template.TaskTemplates.OrderBy(t => t.SortOrder))
        {
            var taskId = new OnboardingTaskId(IdGenerator.NextGuid());
            var dueDate = request.StartDate.AddDays(taskTemplate.OffsetDays);

            // Resolve role → user
            string? assignedUserId = null;
            string? assignedUserDisplayName = null;
            if (taskTemplate.AssigneeRoleCode != null &&
                request.RoleMappings.TryGetValue(taskTemplate.AssigneeRoleCode, out var resolvedUserId))
            {
                assignedUserId = resolvedUserId;
                // NOTE: display name resolution would require user lookup service
                // For Phase 26, store the userId as display name fallback
                assignedUserDisplayName = resolvedUserId;
            }
            else if (taskTemplate.AssigneeRoleCode != null)
            {
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingResolveRoleMissing);
            }

            var task = OnboardingTask.Create(
                taskId, taskTemplate.Title, taskTemplate.Description,
                taskTemplate.AssigneeRoleCode, assignedUserId, assignedUserDisplayName,
                request.CreatedBy, dueDate, taskTemplate.SortOrder, taskTemplate.IsRequired);

            instance.AddTask(task);
        }

        await instanceRepository.CreateOneAsync(instance, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.ToResponse(instance);
    }
}

// Validator
public sealed class StartOnboardingValidator : AbstractValidator<StartOnboardingCommand>
{
    public StartOnboardingValidator()
    {
        RuleFor(x => x.EmployeeId)
            .RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.TemplateId)
            .RequiredId(HrBusinessErrorCodes.ValTemplateIdRequired);
        RuleFor(x => x.StartDate)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValStartDateRequired);
    }
}
```

- [ ] **Step 2: Create CompleteOnboardingTaskCommand**

```csharp
// Command
public sealed record CompleteOnboardingTaskCommand(
    OnboardingTaskId TaskId,
    string? Notes) : ICommandResult<OnboardingInstanceResponse>
{
    [JsonIgnore]
    public string CompletedBy { get; set; }
}

// Handler
public sealed class CompleteOnboardingTaskHandler(
    ISqlRepository<OnboardingInstance> instanceRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<CompleteOnboardingTaskCommand, OneOf<OnboardingInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingInstanceResponse, ErrorDetailResponse>> Handle(
        CompleteOnboardingTaskCommand request, CancellationToken ct)
    {
        // Find instance containing this task
        var instance = await instanceRepository.GetOneByConditionAsync(
            x => x.Tasks.Any(t => t.Id == request.TaskId), ct);
        if (instance == null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTaskNotFound);

        if (instance.Status != OnboardingInstanceStatusCode.InProgress)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingInstanceNotInProgress);

        if (!instance.CompleteTask(request.TaskId, request.CompletedBy, request.Notes))
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTaskAlreadyCompleted);

        await instanceRepository.UpdateOneAsync(instance, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.ToResponse(instance);
    }
}

// Validator
public sealed class CompleteOnboardingTaskValidator : AbstractValidator<CompleteOnboardingTaskCommand>
{
    public CompleteOnboardingTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .RequiredId(HrBusinessErrorCodes.ValTaskIdRequired);
    }
}
```

- [ ] **Step 3: Create ReopenOnboardingTask, SkipOnboardingTask, ReassignOnboardingTask commands** (same pattern)

Reopen:
- Checks instance exists with task
- Calls `instance.ReopenTask(taskId, reopenedBy, reason)`

Skip:
- Calls `instance.SkipTask(taskId, skippedBy)`

Reassign:
- Calls `instance.ReassignTask(taskId, newUserId, newUserDisplayName, reassignedBy)`
- Validator: `RequiredId` for TaskId, `NotEmpty` for NewUserId

- [ ] **Step 4: Create CancelOnboardingCommand**

```csharp
// Command
public sealed record CancelOnboardingCommand(
    OnboardingInstanceId InstanceId) : ICommandResult<OnboardingInstanceResponse>
{
    [JsonIgnore]
    public string CancelledBy { get; set; }
}

// Handler
public sealed class CancelOnboardingHandler(
    ISqlRepository<OnboardingInstance> instanceRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<CancelOnboardingCommand, OneOf<OnboardingInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingInstanceResponse, ErrorDetailResponse>> Handle(
        CancelOnboardingCommand request, CancellationToken ct)
    {
        var instance = await instanceRepository.GetOneByConditionAsync(
            x => x.Id == request.InstanceId, ct);
        if (instance == null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingInstanceNotFound);

        if (!instance.Cancel(request.CancelledBy))
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingInstanceInvalidStatus);

        await instanceRepository.UpdateOneAsync(instance, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.ToResponse(instance);
    }
}
```

- [ ] **Step 5: Create ReopenOnboardingCommand and ForceCompleteOnboardingCommand** (same pattern)

Reopen: calls `instance.Reopen(reopenedBy)`
ForceComplete: calls `instance.ForceComplete(completedBy, reason)`, validator requires Reason

- [ ] **Step 6: Verify build**

Run: `dotnet build`
Expected: 0 errors

---

### Task 12: Queries (8 handlers)

**Files:**
- Create: 8 query folders with Query + Handler files

Implement the following queries using the pattern:
- `GetOnboardingPlanTemplatesQuery` — paginated, filterable by status, includes task templates
- `GetOnboardingPlanTemplateByIdQuery` — single template with tasks
- `GetOnboardingInstancesQuery` — paginated, filterable by employee, status, date range; include employee name via navigation
- `GetOnboardingInstanceByIdQuery` — instance with tasks, completion %, employee info
- `GetEmployeeOnboardingQuery` — active onboarding for an employee
- `GetMyOnboardingTasksQuery` — tasks assigned to current user (via AssignedUserId)
- `GetPendingOnboardingTasksQuery` — all pending + overdue tasks
- `GetOnboardingDashboardQuery` — aggregated stats

All queries use `ISqlRepository<T>` with `GetManyByConditionWithPaginationAsync` and Include patterns.

- [ ] **Step 1: Create all 8 queries with handlers**

- [ ] **Step 2: Verify build**

Run: `dotnet build`
Expected: 0 errors

---

### Task 13: DI Service Installer (1 file)

**Files:**
- Create: `Anemoi.Hr.Infrastructure/ServiceInstallers/OnboardingServiceInstaller.cs`

- [ ] **Step 1: Create installer**

```csharp
using Anemoi.Hr.Application.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.ServiceInstallers;

public sealed class OnboardingServiceInstaller
{
    public static void Install(IServiceCollection services)
    {
        services.AddScoped<OnboardingMapper>();
    }
}
```

- [ ] **Step 2: Register installer in Program.cs or DI config**

Find where other service installers (e.g., `RecruitmentServiceInstaller`) are registered and add:
```csharp
OnboardingServiceInstaller.Install(services);
```

- [ ] **Step 3: Verify build**

Run: `dotnet build`
Expected: 0 errors

---

### Task 14: API Controllers (3 files)

**Files:**
- Create: `Anemoi.Hr.Api/Controllers/OnboardingTemplatesController.cs`
- Create: `Anemoi.Hr.Api/Controllers/OnboardingInstancesController.cs`
- Create: `Anemoi.Hr.Api/Controllers/OnboardingTasksController.cs`

- [ ] **Step 1: Create OnboardingTemplatesController.cs**

```csharp
[ApiController]
[Route("api/hr/onboarding/templates")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class OnboardingTemplatesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.OnboardingView)]
    public async Task<PaginationResponse<OnboardingPlanTemplateResponse>> GetAll(
        [FromQuery] GetOnboardingPlanTemplatesQuery query, CancellationToken ct)
        => await sender.Send(query, ct);

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.OnboardingView)]
    public async Task<IActionResult> GetById([FromRoute] OnboardingPlanTemplateId id, CancellationToken ct)
    {
        var res = await sender.Send(new GetOnboardingPlanTemplateByIdQuery(id), ct);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.OnboardingManage)]
    public async Task<IActionResult> Create([FromBody] CreateOnboardingPlanTemplateCommand command, CancellationToken ct)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, ct);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.OnboardingManage)]
    public async Task<IActionResult> Update(
        [FromRoute] OnboardingPlanTemplateId id,
        [FromBody] UpdateOnboardingPlanTemplateCommand command, CancellationToken ct)
    {
        var res = await sender.Send(command with { Id = id, UpdatedBy = HttpContext.GetUserId() }, ct);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/activate")]
    [HasPermission(HrPermissions.OnboardingManage)]
    public async Task<IActionResult> Activate([FromRoute] OnboardingPlanTemplateId id, CancellationToken ct)
    {
        var res = await sender.Send(new ActivateOnboardingPlanTemplateCommand(id)
            { UpdatedBy = HttpContext.GetUserId() }, ct);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/deactivate")]
    [HasPermission(HrPermissions.OnboardingManage)]
    public async Task<IActionResult> Deactivate([FromRoute] OnboardingPlanTemplateId id, CancellationToken ct)
    {
        var res = await sender.Send(new DeactivateOnboardingPlanTemplateCommand(id)
            { UpdatedBy = HttpContext.GetUserId() }, ct);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
```

- [ ] **Step 2: Create OnboardingInstancesController.cs** with endpoints:
  - `POST /api/hr/onboarding/instances/start`
  - `GET /api/hr/onboarding/instances`
  - `GET /api/hr/onboarding/instances/{id}`
  - `POST /api/hr/onboarding/instances/{id}/cancel`
  - `POST /api/hr/onboarding/instances/{id}/reopen`
  - `POST /api/hr/onboarding/instances/{id}/force-complete`
  - `GET /api/hr/employees/{employeeId}/onboarding`

- [ ] **Step 3: Create OnboardingTasksController.cs** with endpoints:
  - `POST /api/hr/onboarding/tasks/{id}/complete`
  - `POST /api/hr/onboarding/tasks/{id}/reopen`
  - `POST /api/hr/onboarding/tasks/{id}/skip`
  - `POST /api/hr/onboarding/tasks/{id}/reassign`
  - `GET /api/hr/onboarding/my-tasks`
  - `GET /api/hr/onboarding/pending-tasks`
  - `GET /api/hr/onboarding/dashboard`

- [ ] **Step 4: Verify build**

Run: `dotnet build`
Expected: 0 errors

---

### Task 15: Backend Tests (6+ test files)

**Files:**
- Create: `Anemoi.Hr.Test/Domain/Onboarding/OnboardingPlanTemplateTests.cs`
- Create: `Anemoi.Hr.Test/Domain/Onboarding/OnboardingInstanceTests.cs`
- Create: `Anemoi.Hr.Test/Domain/Onboarding/OnboardingTaskTests.cs`
- Create: `Anemoi.Hr.Test/Application/Onboarding/CreateTemplateHandlerTests.cs`
- Create: `Anemoi.Hr.Test/Application/Onboarding/StartOnboardingHandlerTests.cs`
- Create: `Anemoi.Hr.Test/Application/Onboarding/CompleteTaskHandlerTests.cs`

- [ ] **Step 1: Create domain tests covering:**
  - Template: create, activate, deactivate, update details
  - Instance: create, complete task auto-completion logic, cancel, reopen, force complete, percentage calc
  - Task: create, complete, skip, reopen, reassign, overdue detection

- [ ] **Step 2: Create application tests covering:**
  - CreateTemplateHandler: valid creation, validation failures
  - StartOnboardingHandler: valid start, employee already onboarding, template inactive
  - CompleteTaskHandler: valid complete, task already completed, instance not in progress

- [ ] **Step 3: Run tests**

Run: `dotnet test`
Expected: All tests pass

---

### Task 16: Frontend — API Service + React Query Hooks

**Files:**
- Create: `cody-web-app/src/services/hr/onboarding.ts`
- Create: `cody-web-app/src/hooks/hr/onboarding/useOnboardingTemplates.ts`
- Create: `cody-web-app/src/hooks/hr/onboarding/useOnboardingInstances.ts`
- Create: `cody-web-app/src/hooks/hr/onboarding/useOnboardingTasks.ts`

- [ ] **Step 1: Create API service**

```typescript
// cody-web-app/src/services/hr/onboarding.ts
import api from '@/lib/api';

export const onboardingService = {
  // Templates
  getTemplates: (params?: any) => api.get('/hr/onboarding/templates', { params }),
  getTemplateById: (id: string) => api.get(`/hr/onboarding/templates/${id}`),
  createTemplate: (data: any) => api.post('/hr/onboarding/templates', data),
  updateTemplate: (id: string, data: any) => api.put(`/hr/onboarding/templates/${id}`, data),
  activateTemplate: (id: string) => api.post(`/hr/onboarding/templates/${id}/activate`),
  deactivateTemplate: (id: string) => api.post(`/hr/onboarding/templates/${id}/deactivate`),

  // Instances
  startOnboarding: (data: any) => api.post('/hr/onboarding/instances/start', data),
  getInstances: (params?: any) => api.get('/hr/onboarding/instances', { params }),
  getInstanceById: (id: string) => api.get(`/hr/onboarding/instances/${id}`),
  cancelOnboarding: (id: string) => api.post(`/hr/onboarding/instances/${id}/cancel`),
  reopenOnboarding: (id: string) => api.post(`/hr/onboarding/instances/${id}/reopen`),
  forceCompleteOnboarding: (id: string, data: any) => api.post(`/hr/onboarding/instances/${id}/force-complete`, data),
  getEmployeeOnboarding: (employeeId: string) => api.get(`/hr/employees/${employeeId}/onboarding`),

  // Tasks
  completeTask: (id: string, data?: any) => api.post(`/hr/onboarding/tasks/${id}/complete`, data),
  reopenTask: (id: string) => api.post(`/hr/onboarding/tasks/${id}/reopen`),
  skipTask: (id: string) => api.post(`/hr/onboarding/tasks/${id}/skip`),
  reassignTask: (id: string, data: any) => api.post(`/hr/onboarding/tasks/${id}/reassign`, data),
  getMyTasks: (params?: any) => api.get('/hr/onboarding/my-tasks', { params }),
  getPendingTasks: (params?: any) => api.get('/hr/onboarding/pending-tasks', { params }),
  getDashboard: () => api.get('/hr/onboarding/dashboard'),
};
```

- [ ] **Step 2: Create React Query hooks for each endpoint**

Follow existing patterns from other modules (e.g., `useQuery`, `useMutation`, query key conventions, cache invalidation).

---

### Task 17: Frontend — HR Pages (4 pages + components)

**Files:**
- Create: page files and component files for each route
- Components: table, dialog, form, detail view, task board

- [ ] **Step 1: Templates page** — table + create/edit dialog with task template dynamic rows
- [ ] **Step 2: Instances page** — table with status badges, progress bar, filter
- [ ] **Step 3: Instance detail page** — employee info, progress %, task list with actions (complete/skip/reopen/reassign)
- [ ] **Step 4: Task board page** — all pending/overdue tasks with filters

---

### Task 18: Frontend — ESS Page + Localization

**Files:**
- Create: ESS onboarding page
- Modify: en.json, vi.json

- [ ] **Step 1: Create ESS onboarding page** — employee's own onboarding checklist
- [ ] **Step 2: Add English translations for all UI strings**
- [ ] **Step 3: Add Vietnamese translations for all UI strings**
- [ ] **Step 4: Verify frontend build**

Run: `cd cody-web-app && npm run build`
Expected: 0 errors

---

### Task 19: Build Verification + Final Checks

- [ ] **Step 1: Full backend build**

Run: `dotnet build Anemoi.sln`
Expected: 0 errors, acceptable warnings

- [ ] **Step 2: Run all backend tests**

Run: `dotnet test`
Expected: All tests pass (existing + new)

- [ ] **Step 3: Frontend build**

Run: `cd cody-web-app && npm run build`
Expected: 0 errors

- [ ] **Step 4: Frontend lint**

Run: `cd cody-web-app && npm run lint`
Expected: 0 errors
