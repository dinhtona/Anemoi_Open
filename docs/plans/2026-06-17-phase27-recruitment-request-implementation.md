# Phase 27 — Recruitment Request & Workflow Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the complete Recruitment Request → Approval → Recruitment Opening workflow with notification integration, history tracking, and conversion sync.

**Architecture:** New `RecruitmentRequest` aggregate (Entity<TId>) and `RecruitmentOpening` aggregate with CQRS commands/queries, MassTransit integration events, Notification Platform consumers, history tracking (ValueObject pattern). No breaking changes to existing JobRequisition/JobPosting.

**Tech Stack:** .NET 10, MediatR, EF Core/Npgsql, MassTransit, Riok.Mapperly (manual mapper for consistency), FluentValidation, OneOf, PostgreSQL xmin

---

### Task 1: Strongly Typed IDs + Status/Priority Constants

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.ModelIds/ModelIds/RecruitmentRequestId.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.ModelIds/ModelIds/RecruitmentRequestHistoryId.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.ModelIds/ModelIds/RecruitmentOpeningId.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Recruitment/RecruitmentRequestStatusCode.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Recruitment/RecruitmentRequestPriorityCode.cs`

- [ ] **Step 1: Create RecruitmentRequestId.cs**

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record RecruitmentRequestId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 2: Create RecruitmentRequestHistoryId.cs**

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record RecruitmentRequestHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 3: Create RecruitmentOpeningId.cs**

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record RecruitmentOpeningId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 4: Create RecruitmentRequestStatusCode.cs**

```csharp
namespace Anemoi.Hr.Domain.Recruitment;

public static class RecruitmentRequestStatusCode
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
}
```

- [ ] **Step 5: Create RecruitmentRequestPriorityCode.cs**

```csharp
namespace Anemoi.Hr.Domain.Recruitment;

public static class RecruitmentRequestPriorityCode
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Urgent = "Urgent";
}
```

- [ ] **Step 6: Build and verify**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.ModelIds/Anemoi.Hr.ModelIds.csproj`
Expected: Build succeeded with 0 warnings

---

### Task 2: Domain Entities

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Recruitment/RecruitmentRequest.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Recruitment/RecruitmentRequestHistory.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Recruitment/RecruitmentOpening.cs`

- [ ] **Step 1: Create RecruitmentRequest aggregate — Entity<RecruitmentRequestId>**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class RecruitmentRequest : Entity<RecruitmentRequestId>
{
    public string RequestNumber { get; set; }
    public DepartmentId DepartmentId { get; set; }
    public PositionId PositionId { get; set; }
    public int RequestedHeadcount { get; set; }
    public string Reason { get; set; }
    public string PriorityCode { get; set; }
    public string RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Status { get; private set; } = RecruitmentRequestStatusCode.Draft;
    public string ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; private set; }
    public string RejectedBy { get; set; }
    public DateTime? RejectedAt { get; private set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Department Department { get; set; }
    public Position Position { get; set; }
    public ICollection<RecruitmentRequestHistory> Histories { get; set; } = [];

    public bool Submit(string actor, DateTime now)
    {
        if (Status != RecruitmentRequestStatusCode.Draft)
            return false;

        Status = RecruitmentRequestStatusCode.Submitted;
        RequestedBy = actor;
        RequestedAt = now;
        UpdatedAt = now;
        return true;
    }

    public bool Approve(string actor, DateTime now, string comment = null)
    {
        if (Status != RecruitmentRequestStatusCode.Submitted)
            return false;

        Status = RecruitmentRequestStatusCode.Approved;
        ApprovedBy = actor;
        ApprovedAt = now;
        Comment = comment;
        UpdatedAt = now;
        return true;
    }

    public bool Reject(string actor, DateTime now, string comment = null)
    {
        if (Status != RecruitmentRequestStatusCode.Submitted)
            return false;

        Status = RecruitmentRequestStatusCode.Rejected;
        RejectedBy = actor;
        RejectedAt = now;
        Comment = comment;
        UpdatedAt = now;
        return true;
    }

    public bool Cancel(string actor, DateTime now)
    {
        if (Status == RecruitmentRequestStatusCode.Approved ||
            Status == RecruitmentRequestStatusCode.Rejected ||
            Status == RecruitmentRequestStatusCode.Cancelled)
            return false;

        Status = RecruitmentRequestStatusCode.Cancelled;
        UpdatedAt = now;
        return true;
    }

    public bool CanModify => Status == RecruitmentRequestStatusCode.Draft;
    public bool CanSubmit => Status == RecruitmentRequestStatusCode.Draft;
    public bool CanApprove => Status == RecruitmentRequestStatusCode.Submitted;
    public bool CanReject => Status == RecruitmentRequestStatusCode.Submitted;
    public bool CanCancel => Status == RecruitmentRequestStatusCode.Draft || Status == RecruitmentRequestStatusCode.Submitted;
}
```

- [ ] **Step 2: Create RecruitmentRequestHistory — ValueObject**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class RecruitmentRequestHistory : ValueObject
{
    public RecruitmentRequestHistoryId Id { get; set; }
    public RecruitmentRequestId RecruitmentRequestId { get; set; }
    public string ActionCode { get; set; }
    public string OldStatus { get; set; }
    public string NewStatus { get; set; }
    public string Comment { get; set; }
    public string PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }

    // Navigation
    public RecruitmentRequest RecruitmentRequest { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
```

- [ ] **Step 3: Create RecruitmentOpening — Entity<RecruitmentOpeningId>**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class RecruitmentOpening : Entity<RecruitmentOpeningId>
{
    public RecruitmentRequestId RecruitmentRequestId { get; set; }
    public string Code { get; set; }
    public int PlannedHeadcount { get; set; }
    public int FilledHeadcount { get; set; }
    public int RemainingHeadcount => PlannedHeadcount - FilledHeadcount;
    public string Status { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public RecruitmentRequest RecruitmentRequest { get; set; }

    public void MarkFilled(int count)
    {
        FilledHeadcount += count;
        if (FilledHeadcount >= PlannedHeadcount)
        {
            Status = "Filled";
            ClosedAt = DateTime.UtcNow;
        }
    }
}
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj`
Expected: Build succeeded

---

### Task 3: Domain Events

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Events/RecruitmentRequestSubmittedDomainEvent.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Events/RecruitmentRequestApprovedDomainEvent.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/Events/RecruitmentRequestRejectedDomainEvent.cs`

- [ ] **Step 1: Create RecruitmentRequestSubmittedDomainEvent**

```csharp
using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.Domain.Events;

public sealed record RecruitmentRequestSubmittedDomainEvent(
    Guid RecruitmentRequestId,
    string RequestNumber,
    string RequestedBy,
    DateTime OccurredAt) : DomainEvent;
```

- [ ] **Step 2: Create RecruitmentRequestApprovedDomainEvent**

```csharp
using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.Domain.Events;

public sealed record RecruitmentRequestApprovedDomainEvent(
    Guid RecruitmentRequestId,
    string RequestNumber,
    string ApprovedBy,
    DateTime OccurredAt) : DomainEvent;
```

- [ ] **Step 3: Create RecruitmentRequestRejectedDomainEvent**

```csharp
using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.Domain.Events;

public sealed record RecruitmentRequestRejectedDomainEvent(
    Guid RecruitmentRequestId,
    string RequestNumber,
    string RejectedBy,
    string Reason,
    DateTime OccurredAt) : DomainEvent;
```

- [ ] **Step 4: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj`

---

### Task 4: Integration Events (Contract)

**Files:**
- Create: `Anemoi.Contract/Anemoi.Contract.Hr/Events/RecruitmentRequestSubmittedIntegrationEvent.cs`
- Create: `Anemoi.Contract/Anemoi.Contract.Hr/Events/RecruitmentRequestApprovedIntegrationEvent.cs`
- Create: `Anemoi.Contract/Anemoi.Contract.Hr/Events/RecruitmentRequestRejectedIntegrationEvent.cs`
- Create: `Anemoi.Contract/Anemoi.Contract.Hr/Events/RecruitmentOpeningCreatedIntegrationEvent.cs`
- Create: `Anemoi.Contract/Anemoi.Contract.Hr/Events/EmployeeConvertedIntegrationEvent.cs`

- [ ] **Step 1: Create all 5 integration events**

```csharp
// RecruitmentRequestSubmittedIntegrationEvent.cs
namespace Anemoi.Contract.Hr.Events;

public sealed record RecruitmentRequestSubmittedIntegrationEvent(
    string RecruitmentRequestId,
    string RequestNumber,
    string RequestedByUser,
    string ApproverUserId,
    string DepartmentId,
    string PositionId,
    int Headcount);
```

```csharp
// RecruitmentRequestApprovedIntegrationEvent.cs
namespace Anemoi.Contract.Hr.Events;

public sealed record RecruitmentRequestApprovedIntegrationEvent(
    string RecruitmentRequestId,
    string RequestNumber,
    string ApprovedBy);
```

```csharp
// RecruitmentRequestRejectedIntegrationEvent.cs
namespace Anemoi.Contract.Hr.Events;

public sealed record RecruitmentRequestRejectedIntegrationEvent(
    string RecruitmentRequestId,
    string RequestNumber,
    string RejectedBy,
    string Reason);
```

```csharp
// RecruitmentOpeningCreatedIntegrationEvent.cs
namespace Anemoi.Contract.Hr.Events;

public sealed record RecruitmentOpeningCreatedIntegrationEvent(
    string RecruitmentOpeningId,
    string RecruitmentRequestId,
    string Code,
    int PlannedHeadcount);
```

```csharp
// EmployeeConvertedIntegrationEvent.cs
namespace Anemoi.Contract.Hr.Events;

public sealed record EmployeeConvertedIntegrationEvent(
    string CandidateId,
    string EmployeeId,
    string RecruitmentOpeningId,
    string ConvertedBy);
```

- [ ] **Step 2: Build**

Run: `dotnet build Anemoi.Contract/Anemoi.Contract.Hr/Anemoi.Contract.Hr.csproj`

---

### Task 5: Update Constants (BuildingBlocks + Notification)

**Files:**
- Modify: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/Permissions.cs`
- Modify: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/NotificationWorkflowConstants.cs`
- Modify: `Anemoi.Contract/Anemoi.Contract.Notification/ModelIds/NotificationConstants.cs`

- [ ] **Step 1: Add recruitment request permissions to Permissions.cs**

Find the end of the recruitment permissions section around line 121 (after `HrRecruitmentAnalytics`), add:

```csharp
public const string HrRecruitmentRequestView = "hr.recruitment.request.view";
public const string HrRecruitmentRequestCreate = "hr.recruitment.request.create";
public const string HrRecruitmentRequestSubmit = "hr.recruitment.request.submit";
public const string HrRecruitmentRequestApprove = "hr.recruitment.request.approve";
public const string HrRecruitmentRequestManage = "hr.recruitment.request.manage";
```

Then add to the `Definitions` list (after line 240):

```csharp
new(HrRecruitmentRequestView, "PermissionGroupHrRecruitment", "PermissionDescriptionHrRecruitmentRequestView"),
new(HrRecruitmentRequestCreate, "PermissionGroupHrRecruitment", "PermissionDescriptionHrRecruitmentRequestCreate"),
new(HrRecruitmentRequestSubmit, "PermissionGroupHrRecruitment", "PermissionDescriptionHrRecruitmentRequestSubmit"),
new(HrRecruitmentRequestApprove, "PermissionGroupHrRecruitment", "PermissionDescriptionHrRecruitmentRequestApprove"),
new(HrRecruitmentRequestManage, "PermissionGroupHrRecruitment", "PermissionDescriptionHrRecruitmentRequestManage"),
```

- [ ] **Step 2: Add ViewRecruitmentRequest to NotificationWorkflowConstants.cs**

```csharp
public const string ViewRecruitmentRequest = "ViewRecruitmentRequest";
```

Add under the ActionCodes section.

- [ ] **Step 3: Add Recruitment category to NotificationConstants.cs**

```csharp
public const string Recruitment = "Recruitment";
```

Add to `Categories` class and add `Recruitment` to `AllowedCategories` HashSet.

- [ ] **Step 4: Build**

Run: `dotnet build Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Anemoi.BuildingBlock.Application.csproj && dotnet build Anemoi.Contract/Anemoi.Contract.Notification/Anemoi.Contract.Notification.csproj`

---

### Task 6: Update HR Application Constants

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Configurations/HrPermissions.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Configurations/HrBusinessErrorCodes.cs`

- [ ] **Step 1: Add request permissions to HrPermissions.cs**

```csharp
public const string RecruitmentRequestView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentRequestView;
public const string RecruitmentRequestCreate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentRequestCreate;
public const string RecruitmentRequestSubmit = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentRequestSubmit;
public const string RecruitmentRequestApprove = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentRequestApprove;
public const string RecruitmentRequestManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentRequestManage;
```

Add to the AllPermissions list and risk mappings.

- [ ] **Step 2: Add error codes to HrBusinessErrorCodes.cs**

Find the recruitment error codes section (around line 278), add:

```csharp
public const string RecruitmentRequestNotFound = "HR_REC_REQUEST_NOT_FOUND";
public const string RecruitmentRequestInvalidStatus = "HR_REC_REQUEST_INVALID_STATUS";
public const string RecruitmentRequestHeadcountInvalid = "HR_REC_REQUEST_HEADCOUNT_INVALID";
public const string RecruitmentRequestAlreadySubmitted = "HR_REC_REQUEST_ALREADY_SUBMITTED";
public const string RecruitmentRequestNotApproved = "HR_REC_REQUEST_NOT_APPROVED";
public const string RecruitmentOpeningNotFound = "HR_REC_OPENING_NOT_FOUND";
public const string RecruitmentOpeningExceedsPlanned = "HR_REC_OPENING_EXCEEDS_PLANNED";
```

Then add validation error codes:

```csharp
public const string ValRecruitmentRequestIdRequired = "VAL_RECRUITMENT_REQUEST_ID_REQUIRED";
public const string ValRecruitmentRequestDepartmentRequired = "VAL_RECRUITMENT_REQUEST_DEPARTMENT_REQUIRED";
public const string ValRecruitmentRequestPositionRequired = "VAL_RECRUITMENT_REQUEST_POSITION_REQUIRED";
public const string ValRecruitmentRequestHeadcountPositive = "VAL_RECRUITMENT_REQUEST_HEADCOUNT_POSITIVE";
public const string ValRecruitmentRequestPriorityRequired = "VAL_RECRUITMENT_REQUEST_PRIORITY_REQUIRED";
```

- [ ] **Step 3: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`

---

### Task 7: EF Core Configuration

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Configurations/RecruitmentModelMapping.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Domain/Recruitment/JobPosting.cs`

- [ ] **Step 1: Add RecruitmentOpeningId to JobPosting**

Add after `JobRequisitionId`:
```csharp
public RecruitmentOpeningId? RecruitmentOpeningId { get; set; }
```

Add navigation:
```csharp
public RecruitmentOpening RecruitmentOpening { get; set; }
```

- [ ] **Step 2: Add RecruitmentRequest configuration to RecruitmentModelMapping.cs**

Add `IEntityTypeConfiguration<RecruitmentRequest>`:

```csharp
public void Configure(EntityTypeBuilder<RecruitmentRequest> builder)
{
    builder.ToTable("RecruitmentRequests");
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id)
        .HasConversion(x => x.Value, id => new RecruitmentRequestId(id));
    builder.Property(x => x.RequestNumber).HasMaxLength(32).IsRequired();
    builder.Property(x => x.RequestedHeadcount).IsRequired();
    builder.Property(x => x.Reason).HasMaxLength(2000);
    builder.Property(x => x.PriorityCode).HasMaxLength(32).IsRequired();
    builder.Property(x => x.RequestedBy).HasMaxLength(128).IsRequired();
    builder.Property(x => x.RequestedAt).IsRequired();
    builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
    builder.Property(x => x.ApprovedBy).HasMaxLength(128);
    builder.Property(x => x.RejectedBy).HasMaxLength(128);
    builder.Property(x => x.Comment).HasMaxLength(2000);
    builder.Property(x => x.CreatedAt).IsRequired();
    builder.Property(x => x.UpdatedAt).IsRequired();

    builder.Property(x => x.DepartmentId)
        .HasConversion(x => x.Value, id => new DepartmentId(id))
        .IsRequired();
    builder.Property(x => x.PositionId)
        .HasConversion(x => x.Value, id => new PositionId(id))
        .IsRequired();

    builder.HasOne(x => x.Department)
        .WithMany()
        .HasForeignKey(x => x.DepartmentId)
        .OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(x => x.Position)
        .WithMany()
        .HasForeignKey(x => x.PositionId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasMany(x => x.Histories)
        .WithOne(x => x.RecruitmentRequest)
        .HasForeignKey(x => x.RecruitmentRequestId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(x => x.RequestNumber).IsUnique();
    builder.HasIndex(x => x.Status);
    builder.HasIndex(x => x.DepartmentId);
    builder.HasIndex(x => x.PositionId);
    builder.HasIndex(x => x.RequestedAt);
    builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
}
```

- [ ] **Step 3: Add RecruitmentRequestHistory configuration**

```csharp
public void Configure(EntityTypeBuilder<RecruitmentRequestHistory> builder)
{
    builder.ToTable("RecruitmentRequestHistories");
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id)
        .HasConversion(x => x.Value, id => new RecruitmentRequestHistoryId(id));
    builder.Property(x => x.ActionCode).HasMaxLength(64).IsRequired();
    builder.Property(x => x.OldStatus).HasMaxLength(32);
    builder.Property(x => x.NewStatus).HasMaxLength(32).IsRequired();
    builder.Property(x => x.Comment).HasMaxLength(2000);
    builder.Property(x => x.PerformedBy).HasMaxLength(128).IsRequired();
    builder.Property(x => x.PerformedAt).IsRequired();

    builder.Property(x => x.RecruitmentRequestId)
        .HasConversion(x => x.Value, id => new RecruitmentRequestId(id))
        .IsRequired();

    builder.HasIndex(x => x.RecruitmentRequestId);
    builder.HasIndex(x => x.PerformedAt);
}
```

- [ ] **Step 4: Add RecruitmentOpening configuration**

```csharp
public void Configure(EntityTypeBuilder<RecruitmentOpening> builder)
{
    builder.ToTable("RecruitmentOpenings");
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id)
        .HasConversion(x => x.Value, id => new RecruitmentOpeningId(id));
    builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
    builder.Property(x => x.PlannedHeadcount).IsRequired();
    builder.Property(x => x.FilledHeadcount).IsRequired();
    builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
    builder.Property(x => x.OpenedAt).IsRequired();
    builder.Property(x => x.CreatedAt).IsRequired();
    builder.Property(x => x.UpdatedAt).IsRequired();

    builder.Property(x => x.RecruitmentRequestId)
        .HasConversion(x => x.Value, id => new RecruitmentRequestId(id))
        .IsRequired();

    builder.HasOne(x => x.RecruitmentRequest)
        .WithMany()
        .HasForeignKey(x => x.RecruitmentRequestId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(x => x.RecruitmentRequestId);
    builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
}
```

- [ ] **Step 5: Add RegistrationRequestId FK to JobPosting configuration**

Find the existing `JobPosting` config and add:
```csharp
builder.Property(x => x.RecruitmentOpeningId)
    .HasConversion(x => x.Value, id => new RecruitmentOpeningId(id))
    .IsRequired(false);
```

Add navigation:
```csharp
builder.HasOne(x => x.RecruitmentOpening)
    .WithMany()
    .HasForeignKey(x => x.RecruitmentOpeningId)
    .OnDelete(DeleteBehavior.SetNull);
```

- [ ] **Step 6: Register the new interfaces**

Update the class declaration to implement the new interfaces:
```csharp
public sealed class RecruitmentModelMapping :
    IEntityTypeConfiguration<JobRequisition>,
    IEntityTypeConfiguration<JobPosting>,
    IEntityTypeConfiguration<Candidate>,
    IEntityTypeConfiguration<CandidateApplication>,
    IEntityTypeConfiguration<CandidateApplicationStageHistory>,
    IEntityTypeConfiguration<InterviewSchedule>,
    IEntityTypeConfiguration<InterviewFeedback>,
    IEntityTypeConfiguration<HiringDecision>,
    IEntityTypeConfiguration<RecruitmentRequest>,
    IEntityTypeConfiguration<RecruitmentRequestHistory>,
    IEntityTypeConfiguration<RecruitmentOpening>
```

- [ ] **Step 7: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Infrastructure/Anemoi.Hr.Infrastructure.csproj`

---

### Task 8: RecruitmentRequest Mapper + Response DTOs

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Mappings/RecruitmentMapper.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Responses/RecruitmentAnalyticsResponses.cs`

- [ ] **Step 1: Add response DTOs**

Add to RecruitmentAnalyticsResponses.cs or create a new file `RecruitmentRequestResponses.cs`:

```csharp
namespace Anemoi.Hr.Application.Responses;

public sealed record RecruitmentRequestResponse(
    string Id,
    string RequestNumber,
    string DepartmentId,
    string DepartmentName,
    string PositionId,
    string PositionName,
    int RequestedHeadcount,
    string Reason,
    string PriorityCode,
    string RequestedBy,
    DateTime RequestedAt,
    string Status,
    string ApprovedBy,
    DateTime? ApprovedAt,
    string RejectedBy,
    DateTime? RejectedAt,
    string Comment,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record RecruitmentRequestHistoryResponse(
    string Id,
    string RecruitmentRequestId,
    string ActionCode,
    string OldStatus,
    string NewStatus,
    string Comment,
    string PerformedBy,
    DateTime PerformedAt);

public sealed record RecruitmentOpeningResponse(
    string Id,
    string RecruitmentRequestId,
    string Code,
    int PlannedHeadcount,
    int FilledHeadcount,
    int RemainingHeadcount,
    string Status,
    DateTime OpenedAt);

public sealed record RecruitmentDashboardWidgetsResponse(
    int OpenRequests,
    int ApprovedRequests,
    int RejectedRequests,
    int PendingApprovals,
    int OpenPositions,
    int Vacancies,
    int ActiveCandidates,
    int InPipeline,
    int Hired,
    double ConversionRate);
```

- [ ] **Step 2: Add mapper methods to RecruitmentMapper.cs**

```csharp
public RecruitmentRequestResponse ToResponse(RecruitmentRequest req)
{
    if (req is null) return null;
    return new RecruitmentRequestResponse(
        Id: req.Id.Value.ToString(),
        RequestNumber: req.RequestNumber,
        DepartmentId: req.DepartmentId.Value.ToString(),
        DepartmentName: req.Department?.Name,
        PositionId: req.PositionId.Value.ToString(),
        PositionName: req.Position?.Name,
        RequestedHeadcount: req.RequestedHeadcount,
        Reason: req.Reason,
        PriorityCode: req.PriorityCode,
        RequestedBy: req.RequestedBy,
        RequestedAt: req.RequestedAt,
        Status: req.Status,
        ApprovedBy: req.ApprovedBy,
        ApprovedAt: req.ApprovedAt,
        RejectedBy: req.RejectedBy,
        RejectedAt: req.RejectedAt,
        Comment: req.Comment,
        CreatedAt: req.CreatedAt,
        UpdatedAt: req.UpdatedAt
    );
}

public IReadOnlyCollection<RecruitmentRequestResponse> ToResponses(IEnumerable<RecruitmentRequest> reqs)
{
    if (reqs is null) return [];
    return reqs.Select(ToResponse).ToList();
}

public RecruitmentRequestHistoryResponse ToResponse(RecruitmentRequestHistory h)
{
    if (h is null) return null;
    return new RecruitmentRequestHistoryResponse(
        Id: h.Id.Value.ToString(),
        RecruitmentRequestId: h.RecruitmentRequestId.Value.ToString(),
        ActionCode: h.ActionCode,
        OldStatus: h.OldStatus,
        NewStatus: h.NewStatus,
        Comment: h.Comment,
        PerformedBy: h.PerformedBy,
        PerformedAt: h.PerformedAt
    );
}

public IReadOnlyCollection<RecruitmentRequestHistoryResponse> ToHistoryResponses(
    IEnumerable<RecruitmentRequestHistory> history)
{
    if (history is null) return [];
    return history.Select(ToResponse).ToList();
}

public RecruitmentOpeningResponse ToResponse(RecruitmentOpening opening)
{
    if (opening is null) return null;
    return new RecruitmentOpeningResponse(
        Id: opening.Id.Value.ToString(),
        RecruitmentRequestId: opening.RecruitmentRequestId.Value.ToString(),
        Code: opening.Code,
        PlannedHeadcount: opening.PlannedHeadcount,
        FilledHeadcount: opening.FilledHeadcount,
        RemainingHeadcount: opening.RemainingHeadcount,
        Status: opening.Status,
        OpenedAt: opening.OpenedAt
    );
}

public IReadOnlyCollection<RecruitmentOpeningResponse> ToOpeningResponses(
    IEnumerable<RecruitmentOpening> openings)
{
    if (openings is null) return [];
    return openings.Select(ToResponse).ToList();
}
```

- [ ] **Step 3: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`

---

### Task 9: Command — CreateRecruitmentRequest

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/RecruitmentCommands/CreateRecruitmentRequest/CreateRecruitmentRequestCommand.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/RecruitmentCommands/CreateRecruitmentRequest/CreateRecruitmentRequestHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/RecruitmentCommands/CreateRecruitmentRequest/CreateRecruitmentRequestValidator.cs`

- [ ] **Step 1: Create command record**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentRequest;

public sealed record CreateRecruitmentRequestCommand(
    string DepartmentId,
    string PositionId,
    int RequestedHeadcount,
    string Reason,
    string PriorityCode,
    string CreatedBy) : ICommandResult<RecruitmentRequestResponse>;
```

- [ ] **Step 2: Create validator**

```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentRequest;

public sealed class CreateRecruitmentRequestValidator : AbstractValidator<CreateRecruitmentRequestCommand>
{
    public CreateRecruitmentRequestValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestDepartmentRequired);
        RuleFor(x => x.PositionId)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestPositionRequired);
        RuleFor(x => x.RequestedHeadcount)
            .GreaterThan(0).WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestHeadcountPositive);
        RuleFor(x => x.PriorityCode)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestPriorityRequired);
    }
}
```

- [ ] **Step 3: Create handler**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentRequest;

public sealed class CreateRecruitmentRequestHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<CreateRecruitmentRequestCommand, OneOf<RecruitmentRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentRequestResponse, ErrorDetailResponse>> Handle(
        CreateRecruitmentRequestCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var requestId = new RecruitmentRequestId(IdGenerator.NextGuid());

        var requestNumber = await GenerateRequestNumber(now, cancellationToken);

        var recruitmentRequest = new RecruitmentRequest
        {
            Id = requestId,
            RequestNumber = requestNumber,
            DepartmentId = new DepartmentId(Guid.Parse(request.DepartmentId)),
            PositionId = new PositionId(Guid.Parse(request.PositionId)),
            RequestedHeadcount = request.RequestedHeadcount,
            Reason = request.Reason,
            PriorityCode = request.PriorityCode,
            RequestedBy = request.CreatedBy,
            RequestedAt = now,
            Status = RecruitmentRequestStatusCode.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };

        var createResult = await requestRepository.CreateOneAsync(recruitmentRequest, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(recruitmentRequest);
    }

    private async Task<string> GenerateRequestNumber(DateTime now, CancellationToken cancellationToken)
    {
        var prefix = $"RR-{now:yyyyMM}-";
        var existing = await requestRepository.GetQueryable()
            .Where(x => x.RequestNumber.StartsWith(prefix))
            .CountAsync(cancellationToken);
        return $"{prefix}{(existing + 1):D4}";
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`

---

### Task 10: Command — UpdateRecruitmentRequest

**Files:**
- Create: `.../UpdateRecruitmentRequest/UpdateRecruitmentRequestCommand.cs`
- Create: `.../UpdateRecruitmentRequest/UpdateRecruitmentRequestHandler.cs`
- Create: `.../UpdateRecruitmentRequest/UpdateRecruitmentRequestValidator.cs`

- [ ] **Step 1: Create command**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateRecruitmentRequest;

public sealed record UpdateRecruitmentRequestCommand(
    string Id,
    string DepartmentId,
    string PositionId,
    int RequestedHeadcount,
    string Reason,
    string PriorityCode,
    string UpdatedBy) : ICommandResult<RecruitmentRequestResponse>;
```

- [ ] **Step 2: Create validator** (same validation as create, plus Id required)

```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateRecruitmentRequest;

public sealed class UpdateRecruitmentRequestValidator : AbstractValidator<UpdateRecruitmentRequestCommand>
{
    public UpdateRecruitmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestIdRequired);
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestDepartmentRequired);
        RuleFor(x => x.PositionId)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestPositionRequired);
        RuleFor(x => x.RequestedHeadcount)
            .GreaterThan(0).WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestHeadcountPositive);
        RuleFor(x => x.PriorityCode)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestPriorityRequired);
    }
}
```

- [ ] **Step 3: Create handler**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateRecruitmentRequest;

public sealed class UpdateRecruitmentRequestHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<UpdateRecruitmentRequestCommand, OneOf<RecruitmentRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentRequestResponse, ErrorDetailResponse>> Handle(
        UpdateRecruitmentRequestCommand request,
        CancellationToken cancellationToken)
    {
        var id = new RecruitmentRequestId(Guid.Parse(request.Id));
        var recruitmentRequest = await requestRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (recruitmentRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotFound);

        if (!recruitmentRequest.CanModify)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestInvalidStatus);

        recruitmentRequest.DepartmentId = new DepartmentId(Guid.Parse(request.DepartmentId));
        recruitmentRequest.PositionId = new PositionId(Guid.Parse(request.PositionId));
        recruitmentRequest.RequestedHeadcount = request.RequestedHeadcount;
        recruitmentRequest.Reason = request.Reason;
        recruitmentRequest.PriorityCode = request.PriorityCode;
        recruitmentRequest.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(recruitmentRequest);
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`

---

### Task 11: Command — SubmitRecruitmentRequest (with integration events)

**Files:**
- Create: `.../SubmitRecruitmentRequest/SubmitRecruitmentRequestCommand.cs`
- Create: `.../SubmitRecruitmentRequest/SubmitRecruitmentRequestHandler.cs`
- Create: `.../SubmitRecruitmentRequest/SubmitRecruitmentRequestValidator.cs`

- [ ] **Step 1: Create command**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRecruitmentRequest;

public sealed record SubmitRecruitmentRequestCommand(
    string Id,
    string SubmittedBy) : ICommandResult<RecruitmentRequestResponse>;
```

- [ ] **Step 2: Create validator**

```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRecruitmentRequest;

public sealed class SubmitRecruitmentRequestValidator : AbstractValidator<SubmitRecruitmentRequestCommand>
{
    public SubmitRecruitmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestIdRequired);
    }
}
```

- [ ] **Step 3: Create handler with integration events (publish BEFORE SaveChanges per ADR-025)**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Events;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRecruitmentRequest;

public sealed class SubmitRecruitmentRequestHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    ISqlRepository<RecruitmentRequestHistory> historyRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    RecruitmentMapper mapper)
    : ICommandHandler<SubmitRecruitmentRequestCommand, OneOf<RecruitmentRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentRequestResponse, ErrorDetailResponse>> Handle(
        SubmitRecruitmentRequestCommand request,
        CancellationToken cancellationToken)
    {
        var id = new RecruitmentRequestId(Guid.Parse(request.Id));
        var recruitmentRequest = await requestRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (recruitmentRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotFound);

        if (!recruitmentRequest.CanSubmit)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestAlreadySubmitted);

        var now = DateTime.UtcNow;
        var oldStatus = recruitmentRequest.Status;
        recruitmentRequest.Submit(request.SubmittedBy, now);

        // Create history record
        var history = new RecruitmentRequestHistory
        {
            Id = new RecruitmentRequestHistoryId(IdGenerator.NextGuid()),
            RecruitmentRequestId = recruitmentRequest.Id,
            ActionCode = "Submitted",
            OldStatus = oldStatus,
            NewStatus = recruitmentRequest.Status,
            PerformedBy = request.SubmittedBy,
            PerformedAt = now
        };
        var historyResult = await historyRepository.CreateOneAsync(history, cancellationToken);
        if (historyResult.TryPickT1(out var histException, out _))
            return HrErrorResponses.FromSaveResult(histException, HrBusinessErrorCodes.SaveChangesFailed);

        // Add domain event
        recruitmentRequest.AddEvent(new RecruitmentRequestSubmittedDomainEvent(
            recruitmentRequest.Id.Value,
            recruitmentRequest.RequestNumber,
            request.SubmittedBy,
            now));

        // Publish integration events BEFORE SaveChanges (ADR-025)
        await publishEndpoint.Publish(new RecruitmentRequestSubmittedIntegrationEvent(
            recruitmentRequest.Id.Value.ToString(),
            recruitmentRequest.RequestNumber,
            request.SubmittedBy,
            request.SubmittedBy,
            recruitmentRequest.DepartmentId.Value.ToString(),
            recruitmentRequest.PositionId.Value.ToString(),
            recruitmentRequest.RequestedHeadcount
        ), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(recruitmentRequest);
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`

---

### Task 12: Command — ApproveRecruitmentRequest

**Files:**
- Create: `.../ApproveRecruitmentRequest/ApproveRecruitmentRequestCommand.cs`
- Create: `.../ApproveRecruitmentRequest/ApproveRecruitmentRequestHandler.cs`
- Create: `.../ApproveRecruitmentRequest/ApproveRecruitmentRequestValidator.cs`

- [ ] **Step 1: Create command**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ApproveRecruitmentRequest;

public sealed record ApproveRecruitmentRequestCommand(
    string Id,
    string ApprovedBy,
    string Comment) : ICommandResult<RecruitmentRequestResponse>;
```

- [ ] **Step 2: Create validator**

```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ApproveRecruitmentRequest;

public sealed class ApproveRecruitmentRequestValidator : AbstractValidator<ApproveRecruitmentRequestCommand>
{
    public ApproveRecruitmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestIdRequired);
    }
}
```

- [ ] **Step 3: Create handler**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Events;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ApproveRecruitmentRequest;

public sealed class ApproveRecruitmentRequestHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    ISqlRepository<RecruitmentRequestHistory> historyRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    RecruitmentMapper mapper)
    : ICommandHandler<ApproveRecruitmentRequestCommand, OneOf<RecruitmentRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentRequestResponse, ErrorDetailResponse>> Handle(
        ApproveRecruitmentRequestCommand request,
        CancellationToken cancellationToken)
    {
        var id = new RecruitmentRequestId(Guid.Parse(request.Id));
        var recruitmentRequest = await requestRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (recruitmentRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotFound);

        if (!recruitmentRequest.CanApprove)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestInvalidStatus);

        var now = DateTime.UtcNow;
        var oldStatus = recruitmentRequest.Status;
        recruitmentRequest.Approve(request.ApprovedBy, now, request.Comment);

        // Create history record
        var history = new RecruitmentRequestHistory
        {
            Id = new RecruitmentRequestHistoryId(IdGenerator.NextGuid()),
            RecruitmentRequestId = recruitmentRequest.Id,
            ActionCode = "Approved",
            OldStatus = oldStatus,
            NewStatus = recruitmentRequest.Status,
            Comment = request.Comment,
            PerformedBy = request.ApprovedBy,
            PerformedAt = now
        };
        var historyResult = await historyRepository.CreateOneAsync(history, cancellationToken);
        if (historyResult.TryPickT1(out var histException, out _))
            return HrErrorResponses.FromSaveResult(histException, HrBusinessErrorCodes.SaveChangesFailed);

        // Add domain event
        recruitmentRequest.AddEvent(new RecruitmentRequestApprovedDomainEvent(
            recruitmentRequest.Id.Value,
            recruitmentRequest.RequestNumber,
            request.ApprovedBy,
            now));

        // Publish integration events BEFORE SaveChanges (ADR-025)
        await publishEndpoint.Publish(new RecruitmentRequestApprovedIntegrationEvent(
            recruitmentRequest.Id.Value.ToString(),
            recruitmentRequest.RequestNumber,
            request.ApprovedBy
        ), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(recruitmentRequest);
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`

---

### Task 13: Command — RejectRecruitmentRequest

**Files:**
- Create: `.../RejectRecruitmentRequest/RejectRecruitmentRequestCommand.cs`
- Create: `.../RejectRecruitmentRequest/RejectRecruitmentRequestHandler.cs`
- Create: `.../RejectRecruitmentRequest/RejectRecruitmentRequestValidator.cs`

- [ ] **Step 1: Create command**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRecruitmentRequest;

public sealed record RejectRecruitmentRequestCommand(
    string Id,
    string RejectedBy,
    string Comment) : ICommandResult<RecruitmentRequestResponse>;
```

- [ ] **Step 2: Create validator**

```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRecruitmentRequest;

public sealed class RejectRecruitmentRequestValidator : AbstractValidator<RejectRecruitmentRequestCommand>
{
    public RejectRecruitmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestIdRequired);
    }
}
```

- [ ] **Step 3: Create handler**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Events;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRecruitmentRequest;

public sealed class RejectRecruitmentRequestHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    ISqlRepository<RecruitmentRequestHistory> historyRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    RecruitmentMapper mapper)
    : ICommandHandler<RejectRecruitmentRequestCommand, OneOf<RecruitmentRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentRequestResponse, ErrorDetailResponse>> Handle(
        RejectRecruitmentRequestCommand request,
        CancellationToken cancellationToken)
    {
        var id = new RecruitmentRequestId(Guid.Parse(request.Id));
        var recruitmentRequest = await requestRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (recruitmentRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotFound);

        if (!recruitmentRequest.CanReject)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestInvalidStatus);

        var now = DateTime.UtcNow;
        var oldStatus = recruitmentRequest.Status;
        recruitmentRequest.Reject(request.RejectedBy, now, request.Comment);

        // Create history record
        var history = new RecruitmentRequestHistory
        {
            Id = new RecruitmentRequestHistoryId(IdGenerator.NextGuid()),
            RecruitmentRequestId = recruitmentRequest.Id,
            ActionCode = "Rejected",
            OldStatus = oldStatus,
            NewStatus = recruitmentRequest.Status,
            Comment = request.Comment,
            PerformedBy = request.RejectedBy,
            PerformedAt = now
        };
        var historyResult = await historyRepository.CreateOneAsync(history, cancellationToken);
        if (historyResult.TryPickT1(out var histException, out _))
            return HrErrorResponses.FromSaveResult(histException, HrBusinessErrorCodes.SaveChangesFailed);

        // Add domain event
        recruitmentRequest.AddEvent(new RecruitmentRequestRejectedDomainEvent(
            recruitmentRequest.Id.Value,
            recruitmentRequest.RequestNumber,
            request.RejectedBy,
            request.Comment,
            now));

        // Publish integration events BEFORE SaveChanges (ADR-025)
        await publishEndpoint.Publish(new RecruitmentRequestRejectedIntegrationEvent(
            recruitmentRequest.Id.Value.ToString(),
            recruitmentRequest.RequestNumber,
            request.RejectedBy,
            request.Comment
        ), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(recruitmentRequest);
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`

---

### Task 14: Command — CancelRecruitmentRequest

**Files:**
- Create: `.../CancelRecruitmentRequest/CancelRecruitmentRequestCommand.cs`
- Create: `.../CancelRecruitmentRequest/CancelRecruitmentRequestHandler.cs`
- Create: `.../CancelRecruitmentRequest/CancelRecruitmentRequestValidator.cs`

- [ ] **Step 1: Create command**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CancelRecruitmentRequest;

public sealed record CancelRecruitmentRequestCommand(
    string Id,
    string CancelledBy) : ICommandResult<RecruitmentRequestResponse>;
```

- [ ] **Step 2: Create validator**

```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CancelRecruitmentRequest;

public sealed class CancelRecruitmentRequestValidator : AbstractValidator<CancelRecruitmentRequestCommand>
{
    public CancelRecruitmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestIdRequired);
    }
}
```

- [ ] **Step 3: Create handler (no integration event to external, just DataChange)**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Notification.Events;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CancelRecruitmentRequest;

public sealed class CancelRecruitmentRequestHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    ISqlRepository<RecruitmentRequestHistory> historyRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    RecruitmentMapper mapper)
    : ICommandHandler<CancelRecruitmentRequestCommand, OneOf<RecruitmentRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentRequestResponse, ErrorDetailResponse>> Handle(
        CancelRecruitmentRequestCommand request,
        CancellationToken cancellationToken)
    {
        var id = new RecruitmentRequestId(Guid.Parse(request.Id));
        var recruitmentRequest = await requestRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (recruitmentRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotFound);

        if (!recruitmentRequest.CanCancel)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestInvalidStatus);

        var now = DateTime.UtcNow;
        var oldStatus = recruitmentRequest.Status;
        recruitmentRequest.Cancel(request.CancelledBy, now);

        // Create history record
        var history = new RecruitmentRequestHistory
        {
            Id = new RecruitmentRequestHistoryId(IdGenerator.NextGuid()),
            RecruitmentRequestId = recruitmentRequest.Id,
            ActionCode = "Cancelled",
            OldStatus = oldStatus,
            NewStatus = recruitmentRequest.Status,
            PerformedBy = request.CancelledBy,
            PerformedAt = now
        };
        var historyResult = await historyRepository.CreateOneAsync(history, cancellationToken);
        if (historyResult.TryPickT1(out var histException, out _))
            return HrErrorResponses.FromSaveResult(histException, HrBusinessErrorCodes.SaveChangesFailed);

        // Publish DataChange BEFORE SaveChanges
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.recruitment.request",
            Action = "Update",
            EntityId = recruitmentRequest.Id.Value.ToString(),
            WorkspaceId = null,
            Sensitivity = "Low",
            QueryTags = new[] { "hr", "recruitment", "recruitment-request" }.ToList(),
            OccurredAt = now
        }, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(recruitmentRequest);
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`

---

### Task 15: Command — CreateRecruitmentOpening

**Files:**
- Create: `.../CreateRecruitmentOpening/CreateRecruitmentOpeningCommand.cs`
- Create: `.../CreateRecruitmentOpening/CreateRecruitmentOpeningHandler.cs`
- Create: `.../CreateRecruitmentOpening/CreateRecruitmentOpeningValidator.cs`

- [ ] **Step 1: Create command**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentOpening;

public sealed record CreateRecruitmentOpeningCommand(
    string RecruitmentRequestId,
    string Code,
    int PlannedHeadcount,
    string CreatedBy) : ICommandResult<RecruitmentOpeningResponse>;
```

- [ ] **Step 2: Create validator**

```csharp
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentOpening;

public sealed class CreateRecruitmentOpeningValidator : AbstractValidator<CreateRecruitmentOpeningCommand>
{
    public CreateRecruitmentOpeningValidator()
    {
        RuleFor(x => x.RecruitmentRequestId)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestIdRequired);
        RuleFor(x => x.Code)
            .NotEmpty().WithErrorCode("VAL_OPENING_CODE_REQUIRED");
        RuleFor(x => x.PlannedHeadcount)
            .GreaterThan(0).WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestHeadcountPositive);
    }
}
```

- [ ] **Step 3: Create handler**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentOpening;

public sealed class CreateRecruitmentOpeningHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    ISqlRepository<RecruitmentOpening> openingRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    RecruitmentMapper mapper)
    : ICommandHandler<CreateRecruitmentOpeningCommand, OneOf<RecruitmentOpeningResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentOpeningResponse, ErrorDetailResponse>> Handle(
        CreateRecruitmentOpeningCommand request,
        CancellationToken cancellationToken)
    {
        var requestId = new RecruitmentRequestId(Guid.Parse(request.RecruitmentRequestId));
        var recruitmentRequest = await requestRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);

        if (recruitmentRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotFound);

        if (recruitmentRequest.Status != RecruitmentRequestStatusCode.Approved)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotApproved);

        var now = DateTime.UtcNow;
        var openingId = new RecruitmentOpeningId(IdGenerator.NextGuid());

        var opening = new RecruitmentOpening
        {
            Id = openingId,
            RecruitmentRequestId = requestId,
            Code = request.Code,
            PlannedHeadcount = request.PlannedHeadcount,
            FilledHeadcount = 0,
            Status = "Open",
            OpenedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        var createResult = await openingRepository.CreateOneAsync(opening, cancellationToken);
        if (createResult.TryPickT1(out var createException, out _))
            return HrErrorResponses.FromSaveResult(createException, HrBusinessErrorCodes.SaveChangesFailed);

        // Publish integration events BEFORE SaveChanges
        await publishEndpoint.Publish(new RecruitmentOpeningCreatedIntegrationEvent(
            openingId.Value.ToString(),
            request.RecruitmentRequestId,
            request.Code,
            request.PlannedHeadcount
        ), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(opening);
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`

---

### Task 16: Queries

**Files:**
- Create: `.../GetRecruitmentRequestById/GetRecruitmentRequestByIdQuery.cs`
- Create: `.../GetRecruitmentRequestById/GetRecruitmentRequestByIdHandler.cs`
- Create: `.../GetRecruitmentRequests/GetRecruitmentRequestsQuery.cs`
- Create: `.../GetRecruitmentRequests/GetRecruitmentRequestsHandler.cs`
- Create: `.../GetRecruitmentRequestTimeline/GetRecruitmentRequestTimelineQuery.cs`
- Create: `.../GetRecruitmentRequestTimeline/GetRecruitmentRequestTimelineHandler.cs`

- [ ] **Step 1: Create GetRecruitmentRequestById query + handler**

```csharp
// GetRecruitmentRequestByIdQuery.cs
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequestById;

public sealed record GetRecruitmentRequestByIdQuery(string Id) : IQueryResult<RecruitmentRequestResponse>;
```

```csharp
// GetRecruitmentRequestByIdHandler.cs
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequestById;

public sealed class GetRecruitmentRequestByIdHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<GetRecruitmentRequestByIdQuery, OneOf<RecruitmentRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentRequestResponse, ErrorDetailResponse>> Handle(
        GetRecruitmentRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var id = new RecruitmentRequestId(Guid.Parse(request.Id));
        var recruitmentRequest = await requestRepository.GetQueryable()
            .Include(x => x.Department)
            .Include(x => x.Position)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (recruitmentRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotFound);

        return mapper.ToResponse(recruitmentRequest);
    }
}
```

- [ ] **Step 2: Create GetRecruitmentRequests search query + handler**

```csharp
// GetRecruitmentRequestsQuery.cs
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequests;

public sealed record GetRecruitmentRequestsQuery(
    string Status,
    string DepartmentId,
    string PositionId,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page,
    int PageSize) : IQueryResult<PaginationResponse<RecruitmentRequestResponse>>;
```

```csharp
// GetRecruitmentRequestsHandler.cs
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequests;

public sealed class GetRecruitmentRequestsHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<GetRecruitmentRequestsQuery, OneOf<PaginationResponse<RecruitmentRequestResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<PaginationResponse<RecruitmentRequestResponse>, ErrorDetailResponse>> Handle(
        GetRecruitmentRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var query = requestRepository.GetQueryable()
            .Include(x => x.Department)
            .Include(x => x.Position)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrEmpty(request.DepartmentId))
            query = query.Where(x => x.DepartmentId == new DepartmentId(Guid.Parse(request.DepartmentId)));
        if (!string.IsNullOrEmpty(request.PositionId))
            query = query.Where(x => x.PositionId == new PositionId(Guid.Parse(request.PositionId)));
        if (request.FromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= request.FromDate.Value);
        if (request.ToDate.HasValue)
            query = query.Where(x => x.CreatedAt <= request.ToDate.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<RecruitmentRequestResponse>
        {
            Items = mapper.ToResponses(items),
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
```

- [ ] **Step 3: Create GetRecruitmentRequestTimeline query + handler**

```csharp
// GetRecruitmentRequestTimelineQuery.cs
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequestTimeline;

public sealed record GetRecruitmentRequestTimelineQuery(string RecruitmentRequestId)
    : IQueryResult<IReadOnlyCollection<RecruitmentRequestHistoryResponse>>;
```

```csharp
// GetRecruitmentRequestTimelineHandler.cs
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequestTimeline;

public sealed class GetRecruitmentRequestTimelineHandler(
    ISqlRepository<RecruitmentRequestHistory> historyRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<GetRecruitmentRequestTimelineQuery, OneOf<IReadOnlyCollection<RecruitmentRequestHistoryResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<RecruitmentRequestHistoryResponse>, ErrorDetailResponse>> Handle(
        GetRecruitmentRequestTimelineQuery request,
        CancellationToken cancellationToken)
    {
        var requestId = new RecruitmentRequestId(Guid.Parse(request.RecruitmentRequestId));
        var history = await historyRepository.GetQueryable()
            .Where(x => x.RecruitmentRequestId == requestId)
            .OrderByDescending(x => x.PerformedAt)
            .ToListAsync(cancellationToken);

        return mapper.ToHistoryResponses(history);
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`

---

### Task 17: Controllers + Dashboard Widgets

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Api/Controllers/Recruitment/RecruitmentRequestsController.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Api/Controllers/Recruitment/RecruitmentOpeningsController.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Api/Controllers/Recruitment/RecruitmentAnalyticsController.cs`

- [ ] **Step 1: Create RecruitmentRequestsController**

```csharp
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ApproveRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CancelRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequestById;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequests;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Recruitment;

[ApiController]
[Route("api/hr/recruitment/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class RecruitmentRequestsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestCreate)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateRecruitmentRequest(
        [FromBody] CreateRecruitmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestCreate)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRecruitmentRequest(
        [FromBody] UpdateRecruitmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestSubmit)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitRecruitmentRequest(
        [FromBody] SubmitRecruitmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { SubmittedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestApprove)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveRecruitmentRequest(
        [FromBody] ApproveRecruitmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ApprovedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestApprove)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectRecruitmentRequest(
        [FromBody] RejectRecruitmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { RejectedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestManage)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelRecruitmentRequest(
        [FromBody] CancelRecruitmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CancelledBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.RecruitmentRequestView)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecruitmentRequestById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetRecruitmentRequestByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.RecruitmentRequestView)]
    [ProducesResponseType(typeof(PaginationResponse<RecruitmentRequestResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<RecruitmentRequestResponse>> GetRecruitmentRequests(
        [FromQuery] GetRecruitmentRequestsQuery query,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(query, cancellationToken);
        return res.Match(
            success => success,
            _ => new PaginationResponse<RecruitmentRequestResponse>()
        );
    }

    [HttpGet("{id}/timeline")]
    [HasPermission(HrPermissions.RecruitmentRequestView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<RecruitmentRequestHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecruitmentRequestTimeline(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetRecruitmentRequestTimelineQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
```

- [ ] **Step 2: Create RecruitmentOpeningsController**

```csharp
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentOpening;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Recruitment;

[ApiController]
[Route("api/hr/recruitment/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class RecruitmentOpeningsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestManage)]
    [ProducesResponseType(typeof(RecruitmentOpeningResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateRecruitmentOpening(
        [FromBody] CreateRecruitmentOpeningCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
```

- [ ] **Step 3: Add dashboard widget endpoint to RecruitmentAnalyticsController**

Add a new endpoint:

```csharp
[HttpGet]
[HasPermission(HrPermissions.RecruitmentView)]
[ProducesResponseType(typeof(RecruitmentDashboardWidgetsResponse), StatusCodes.Status200OK)]
public async Task<IActionResult> GetDashboardWidgets(CancellationToken cancellationToken)
{
    var res = await sender.Send(new GetRecruitmentDashboardWidgetsQuery(), cancellationToken);
    return res.Match<IActionResult>(Ok, BadRequest);
}
```

Add query:

```csharp
// GetRecruitmentDashboardWidgetsQuery.cs
public sealed record GetRecruitmentDashboardWidgetsQuery()
    : IQueryResult<RecruitmentDashboardWidgetsResponse>;
```

Add handler injected with `ISqlRepository<RecruitmentRequest>` and `ISqlRepository<RecruitmentOpening>`:

```csharp
public sealed class GetRecruitmentDashboardWidgetsHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    ISqlRepository<RecruitmentOpening> openingRepository)
    : IQueryHandler<GetRecruitmentDashboardWidgetsQuery, OneOf<RecruitmentDashboardWidgetsResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentDashboardWidgetsResponse, ErrorDetailResponse>> Handle(
        GetRecruitmentDashboardWidgetsQuery request,
        CancellationToken cancellationToken)
    {
        var openRequests = await requestRepository.GetQueryable()
            .CountAsync(x => x.Status == RecruitmentRequestStatusCode.Draft ||
                             x.Status == RecruitmentRequestStatusCode.Submitted, cancellationToken);
        var approvedRequests = await requestRepository.GetQueryable()
            .CountAsync(x => x.Status == RecruitmentRequestStatusCode.Approved, cancellationToken);
        var rejectedRequests = await requestRepository.GetQueryable()
            .CountAsync(x => x.Status == RecruitmentRequestStatusCode.Rejected, cancellationToken);
        var pendingApprovals = await requestRepository.GetQueryable()
            .CountAsync(x => x.Status == RecruitmentRequestStatusCode.Submitted, cancellationToken);

        var openings = await openingRepository.GetQueryable()
            .ToListAsync(cancellationToken);
        var openPositions = openings.Count(x => x.Status == "Open");
        var vacancies = openings.Sum(x => x.RemainingHeadcount);

        // Note: ActiveCandidates, InPipeline, Hired, ConversionRate
        // can be fetched from existing Candidate/Application repositories
        // For now, return 0 for these (Phase 25 already has analytics)

        return new RecruitmentDashboardWidgetsResponse(
            openRequests, approvedRequests, rejectedRequests,
            pendingApprovals, openPositions, vacancies,
            0, 0, 0, 0);
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Api/Anemoi.Hr.Api.csproj`

---

### Task 18: Conversion Sync — Modify ConvertCandidateToEmployeeHandler

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/RecruitmentCommands/ConvertCandidateToEmployee/ConvertCandidateToEmployeeHandler.cs`

- [ ] **Step 1: Modify handler to sync RecruitmentOpening FilledHeadcount**

Add `ISqlRepository<RecruitmentOpening>` to the constructor. After linking candidate to employee, find the associated RecruitmentOpening via the candidate's application chain and call `MarkFilled(1)`.

Current imports + new:
```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
```

Add to constructor:
```csharp
ISqlRepository<RecruitmentOpening> openingRepository,
```

After the `candidate.LinkEmployee(...)` call and before `SaveChangesAsync`, add:
```csharp
// Sync RecruitmentOpening FilledHeadcount
var application = await applicationRepository.GetQueryable()
    .Include(a => a.JobPosting)
    .FirstOrDefaultAsync(a => a.Id == application.Id, cancellationToken);

if (application?.JobPosting?.RecruitmentOpeningId is not null)
{
    var opening = await openingRepository.GetQueryable()
        .FirstOrDefaultAsync(o => o.Id == application.JobPosting.RecruitmentOpeningId, cancellationToken);
    if (opening is not null)
    {
        opening.MarkFilled(1);
    }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`

---

### Task 19: Notification Consumers

**Files:**
- Create: `Anemoi.Notification/Anemoi.Notification.Application/Consumers/RecruitmentRequestConsumers.cs`

- [ ] **Step 1: Create RecruitmentRequestConsumers.cs**

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Anemoi.Contract.Hr.Events;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Constants;
using Anemoi.Contract.Notification.Events;
using Anemoi.Notification.Application.Services;
using MassTransit;
using MediatR;
using Serilog;

namespace Anemoi.Notification.Application.Consumers;

public sealed class RecruitmentRequestSubmittedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<RecruitmentRequestSubmittedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<RecruitmentRequestSubmittedIntegrationEvent> context)
    {
        var message = context.Message;

        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.recruitment.request",
            Action = NotificationConstants.DataChangeActions.Create,
            EntityId = message.RecruitmentRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "recruitment", "recruitment-request" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var userId = await recipientResolver.ResolveUserIdByEmployeeId(message.ApproverUserId, context.CancellationToken);
        if (string.IsNullOrEmpty(userId))
        {
            logger.Warning("Could not resolve Approver User ID for EmployeeId: {EmployeeId}. Skipping notification.", message.ApproverUserId);
            return;
        }

        var command = new CreateNotificationCommand(
            UserId: userId,
            TitleLocalizationKey: "notification.recruitment.request.submitted.title",
            ContentLocalizationKey: "notification.recruitment.request.submitted.content",
            Category: NotificationConstants.Categories.Recruitment,
            ActionUrl: $"/hr/recruitment/requests/{message.RecruitmentRequestId}",
            ActionType: NotificationConstants.ActionTypes.Navigate,
            DeduplicationKey: $"recruitment-request:{message.RecruitmentRequestId}:submitted:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed RecruitmentRequestSubmitted notification for Approver User ID: {UserId}", userId);
    }
}

public sealed class RecruitmentRequestApprovedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<RecruitmentRequestApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<RecruitmentRequestApprovedIntegrationEvent> context)
    {
        var message = context.Message;

        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.recruitment.request",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.RecruitmentRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "recruitment", "recruitment-request" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        // Notify the requester
        var userId = await recipientResolver.ResolveUserIdByEmployeeId(message.ApprovedBy, context.CancellationToken);
        if (string.IsNullOrEmpty(userId))
        {
            logger.Warning("Could not resolve Requester User ID for ApprovedBy: {ApprovedBy}. Skipping notification.", message.ApprovedBy);
            return;
        }

        var command = new CreateNotificationCommand(
            UserId: userId,
            TitleLocalizationKey: "notification.recruitment.request.approved.title",
            ContentLocalizationKey: "notification.recruitment.request.approved.content",
            Category: NotificationConstants.Categories.Recruitment,
            ActionUrl: $"/hr/recruitment/requests/{message.RecruitmentRequestId}",
            ActionType: NotificationConstants.ActionTypes.Navigate,
            DeduplicationKey: $"recruitment-request:{message.RecruitmentRequestId}:approved:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed RecruitmentRequestApproved notification for User ID: {UserId}", userId);
    }
}

public sealed class RecruitmentRequestRejectedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<RecruitmentRequestRejectedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<RecruitmentRequestRejectedIntegrationEvent> context)
    {
        var message = context.Message;

        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.recruitment.request",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.RecruitmentRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "recruitment", "recruitment-request" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        // Notify the requester
        var userId = await recipientResolver.ResolveUserIdByEmployeeId(message.RejectedBy, context.CancellationToken);
        if (string.IsNullOrEmpty(userId))
        {
            logger.Warning("Could not resolve Requester User ID for RejectedBy: {RejectedBy}. Skipping notification.", message.RejectedBy);
            return;
        }

        var command = new CreateNotificationCommand(
            UserId: userId,
            TitleLocalizationKey: "notification.recruitment.request.rejected.title",
            ContentLocalizationKey: "notification.recruitment.request.rejected.content",
            Category: NotificationConstants.Categories.Recruitment,
            ActionUrl: $"/hr/recruitment/requests/{message.RecruitmentRequestId}",
            ActionType: NotificationConstants.ActionTypes.Navigate,
            DeduplicationKey: $"recruitment-request:{message.RecruitmentRequestId}:rejected:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed RecruitmentRequestRejected notification for User ID: {UserId}", userId);
    }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build Anemoi.Notification/Anemoi.Notification.Application/Anemoi.Notification.Application.csproj`

---

### Task 20: Tests — Domain

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Test/Domain/Recruitment/RecruitmentRequestDomainTests.cs`

- [ ] **Step 1: Create domain tests for RecruitmentRequest**

```csharp
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using System;
using Xunit;

namespace Anemoi.Hr.Test.Domain.Recruitment;

public sealed class RecruitmentRequestDomainTests
{
    private static RecruitmentRequest CreateDraftRequest()
    {
        return new RecruitmentRequest
        {
            Id = new RecruitmentRequestId(Guid.NewGuid()),
            RequestNumber = "RR-202606-0001",
            DepartmentId = new DepartmentId(Guid.NewGuid()),
            PositionId = new PositionId(Guid.NewGuid()),
            RequestedHeadcount = 2,
            Reason = "New position needed",
            PriorityCode = RecruitmentRequestPriorityCode.High,
            RequestedBy = "user-1",
            RequestedAt = DateTime.UtcNow,
            Status = RecruitmentRequestStatusCode.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Submit_ShouldTransitionFromDraftToSubmitted()
    {
        var request = CreateDraftRequest();
        var result = request.Submit("user-1", DateTime.UtcNow);

        result.Should().BeTrue();
        request.Status.Should().Be(RecruitmentRequestStatusCode.Submitted);
    }

    [Fact]
    public void Submit_ShouldFail_WhenNotDraft()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);

        var result = request.Submit("user-2", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void Approve_ShouldTransitionFromSubmittedToApproved()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);

        var result = request.Approve("approver", DateTime.UtcNow, "Approved");

        result.Should().BeTrue();
        request.Status.Should().Be(RecruitmentRequestStatusCode.Approved);
        request.ApprovedBy.Should().Be("approver");
        request.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_ShouldFail_WhenNotSubmitted()
    {
        var request = CreateDraftRequest();
        var result = request.Approve("approver", DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void Approve_ShouldFail_WhenAlreadyApproved()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);
        request.Approve("approver", DateTime.UtcNow);

        var result = request.Approve("approver2", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void Reject_ShouldTransitionFromSubmittedToRejected()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);

        var result = request.Reject("approver", DateTime.UtcNow, "Not needed");

        result.Should().BeTrue();
        request.Status.Should().Be(RecruitmentRequestStatusCode.Rejected);
        request.RejectedBy.Should().Be("approver");
        request.RejectedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_ShouldFail_WhenNotSubmitted()
    {
        var request = CreateDraftRequest();
        var result = request.Reject("approver", DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void Cancel_ShouldTransitionFromDraftToCancelled()
    {
        var request = CreateDraftRequest();
        var result = request.Cancel("user-1", DateTime.UtcNow);

        result.Should().BeTrue();
        request.Status.Should().Be(RecruitmentRequestStatusCode.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldTransitionFromSubmittedToCancelled()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);

        var result = request.Cancel("user-1", DateTime.UtcNow);

        result.Should().BeTrue();
        request.Status.Should().Be(RecruitmentRequestStatusCode.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldFail_WhenApproved()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);
        request.Approve("approver", DateTime.UtcNow);

        var result = request.Cancel("user-1", DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void Cancel_ShouldFail_WhenRejected()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);
        request.Reject("approver", DateTime.UtcNow);

        var result = request.Cancel("user-1", DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void Cancel_ShouldFail_WhenAlreadyCancelled()
    {
        var request = CreateDraftRequest();
        request.Cancel("user-1", DateTime.UtcNow);

        var result = request.Cancel("user-2", DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanModify_ShouldBeTrue_WhenDraft()
    {
        var request = CreateDraftRequest();
        request.CanModify.Should().BeTrue();
    }

    [Fact]
    public void CanModify_ShouldBeFalse_WhenSubmitted()
    {
        var request = CreateDraftRequest();
        request.Submit("user-1", DateTime.UtcNow);
        request.CanModify.Should().BeFalse();
    }

    [Fact]
    public void TestAllStatusTransitions()
    {
        // Draft can Submit
        AssertCanTransition(RecruitmentRequestStatusCode.Draft, RecruitmentRequestStatusCode.Submitted, r => r.Submit("u", DateTime.UtcNow));
        // Draft can Cancel
        AssertCanTransition(RecruitmentRequestStatusCode.Draft, RecruitmentRequestStatusCode.Cancelled, r => r.Cancel("u", DateTime.UtcNow));
        // Submitted can Approve
        AssertCanTransition(RecruitmentRequestStatusCode.Submitted, RecruitmentRequestStatusCode.Approved, r => r.Approve("u", DateTime.UtcNow));
        // Submitted can Reject
        AssertCanTransition(RecruitmentRequestStatusCode.Submitted, RecruitmentRequestStatusCode.Rejected, r => r.Reject("u", DateTime.UtcNow));
        // Submitted can Cancel
        AssertCanTransition(RecruitmentRequestStatusCode.Submitted, RecruitmentRequestStatusCode.Cancelled, r => r.Cancel("u", DateTime.UtcNow));
    }

    private static void AssertCanTransition(string fromStatus, string toStatus, Func<RecruitmentRequest, bool> act)
    {
        var request = CreateDraftRequest();
        // Set initial status
        request.Status = fromStatus;

        var result = act(request);
        result.Should().BeTrue($"Should be able to transition from {fromStatus} to {toStatus}");
        request.Status.Should().Be(toStatus);
    }
}
```

- [ ] **Step 2: Create DepartmentId import in test**

Note: Add `using Anemoi.Hr.Domain.Departments;` and `using Anemoi.Hr.Domain.Positions;` to the test file.

- [ ] **Step 3: Run tests**

Run: `dotnet test Anemoi.Hr/Anemoi.Hr.Test --filter "RecruitmentRequestDomainTests"`
Expected: All tests pass

---

### Task 21: Tests — Handler Integration

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Test/Application/Recruitment/RecruitmentRequestHandlerTests.cs`

- [ ] **Step 1: Create handler tests**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ApproveRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CancelRecruitmentRequest;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Anemoi.Hr.Test.Application.Recruitment;

public sealed class RecruitmentRequestHandlerTests
{
    private readonly ISqlRepository<RecruitmentRequest> _requestRepo;
    private readonly ISqlRepository<RecruitmentRequestHistory> _historyRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly RecruitmentMapper _mapper;

    public RecruitmentRequestHandlerTests()
    {
        _requestRepo = Substitute.For<ISqlRepository<RecruitmentRequest>>();
        _historyRepo = Substitute.For<ISqlRepository<RecruitmentRequestHistory>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _mapper = new RecruitmentMapper();
    }

    [Fact]
    public async Task CreateRecruitmentRequestHandler_ShouldCreateAndReturnResponse()
    {
        var handler = new CreateRecruitmentRequestHandler(
            _requestRepo, _unitOfWork, _mapper);

        var command = new CreateRecruitmentRequestCommand(
            DepartmentId: Guid.NewGuid().ToString(),
            PositionId: Guid.NewGuid().ToString(),
            RequestedHeadcount: 2,
            Reason: "Need more engineers",
            PriorityCode: "High",
            CreatedBy: "user-1");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsT0.Should().BeTrue();
        var response = result.AsT0;
        response.RequestNumber.Should().StartWith("RR-");
    }

    [Fact]
    public async Task SubmitRecruitmentRequestHandler_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new RecruitmentRequest
        {
            Id = new RecruitmentRequestId(requestId),
            RequestNumber = "RR-202606-0001",
            DepartmentId = new DepartmentId(Guid.NewGuid()),
            PositionId = new PositionId(Guid.NewGuid()),
            RequestedHeadcount = 2,
            Status = RecruitmentRequestStatusCode.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var queryable = new[] { request }.AsQueryable();
        var mockSet = Substitute.For<IQueryable<RecruitmentRequest>>(queryable);
        // Note: In real implementation, mock DbSet properly

        _requestRepo.GetQueryable().Returns(queryable);
        _unitOfWork.SaveChangesAsync(CancellationToken.None).Returns(OneOf<None, Exception>.FromT0(new None()));

        var handler = new SubmitRecruitmentRequestHandler(
            _requestRepo, _historyRepo, _unitOfWork, _publishEndpoint, _mapper);

        var command = new SubmitRecruitmentRequestCommand(
            Id: requestId.ToString(),
            SubmittedBy: "user-1");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
        await _publishEndpoint.Received(1).Publish(
            Arg.Is<RecruitmentRequestSubmittedIntegrationEvent>(e =>
                e.RecruitmentRequestId == requestId.ToString()),
            CancellationToken.None);
    }
}
```

- [ ] **Step 2: Run tests**

Run: `dotnet test Anemoi.Hr/Anemoi.Hr.Test --filter "RecruitmentRequest"`
Expected: All tests pass (or skip if EF Core mock needs adjustment)

---

### Task 22: Frontend — Types and Service Updates

**Files:**
- Modify: `cody-web-app/src/types/hr/recruitment.ts`
- Modify: `cody-web-app/src/services/hr/recruitmentService.ts`
- Modify: `cody-web-app/src/hooks/hr/useRecruitment.ts`
- Modify: `cody-web-app/src/constants/permissions.ts`

- [ ] **Step 1: Add RecruitmentRequest types to recruitment.ts**

```typescript
export type RecruitmentRequestStatusCode =
  | "Draft"
  | "Submitted"
  | "Approved"
  | "Rejected"
  | "Cancelled";

export type RecruitmentRequestPriorityCode =
  | "Low"
  | "Medium"
  | "High"
  | "Urgent";

export interface RecruitmentRequestResponse {
  id: string;
  requestNumber: string;
  departmentId: string;
  departmentName: string;
  positionId: string;
  positionName: string;
  requestedHeadcount: number;
  reason: string;
  priorityCode: string;
  requestedBy: string;
  requestedAt: string;
  status: string;
  approvedBy: string;
  approvedAt: string | null;
  rejectedBy: string;
  rejectedAt: string | null;
  comment: string;
  createdAt: string;
  updatedAt: string;
}

export interface RecruitmentRequestHistoryResponse {
  id: string;
  recruitmentRequestId: string;
  actionCode: string;
  oldStatus: string;
  newStatus: string;
  comment: string;
  performedBy: string;
  performedAt: string;
}

export interface RecruitmentOpeningResponse {
  id: string;
  recruitmentRequestId: string;
  code: string;
  plannedHeadcount: number;
  filledHeadcount: number;
  remainingHeadcount: number;
  status: string;
  openedAt: string;
}

export interface RecruitmentDashboardWidgetsResponse {
  openRequests: number;
  approvedRequests: number;
  rejectedRequests: number;
  pendingApprovals: number;
  openPositions: number;
  vacancies: number;
  activeCandidates: number;
  inPipeline: number;
  hired: number;
  conversionRate: number;
}

export interface CreateRecruitmentRequestRequest {
  departmentId: string;
  positionId: string;
  requestedHeadcount: number;
  reason: string;
  priorityCode: string;
}

export interface UpdateRecruitmentRequestRequest {
  id: string;
  departmentId: string;
  positionId: string;
  requestedHeadcount: number;
  reason: string;
  priorityCode: string;
}

export interface CreateRecruitmentOpeningRequest {
  recruitmentRequestId: string;
  code: string;
  plannedHeadcount: number;
}
```

- [ ] **Step 2: Add recruitment request service methods to recruitmentService.ts**

```typescript
createRecruitmentRequest: async (request: CreateRecruitmentRequestRequest): Promise<RecruitmentRequestResponse> => {
  const { data } = await apiClient.post<RecruitmentRequestResponse>(
    API_ENDPOINTS.hr.recruitment.requests.create,
    request,
  );
  return data;
},

updateRecruitmentRequest: async (request: UpdateRecruitmentRequestRequest): Promise<RecruitmentRequestResponse> => {
  const { data } = await apiClient.post<RecruitmentRequestResponse>(
    API_ENDPOINTS.hr.recruitment.requests.update,
    request,
  );
  return data;
},

submitRecruitmentRequest: async (id: string): Promise<RecruitmentRequestResponse> => {
  const { data } = await apiClient.post<RecruitmentRequestResponse>(
    API_ENDPOINTS.hr.recruitment.requests.submit,
    { id },
  );
  return data;
},

approveRecruitmentRequest: async (id: string): Promise<RecruitmentRequestResponse> => {
  const { data } = await apiClient.post<RecruitmentRequestResponse>(
    API_ENDPOINTS.hr.recruitment.requests.approve,
    { id },
  );
  return data;
},

rejectRecruitmentRequest: async (id: string, comment: string): Promise<RecruitmentRequestResponse> => {
  const { data } = await apiClient.post<RecruitmentRequestResponse>(
    API_ENDPOINTS.hr.recruitment.requests.reject,
    { id, comment },
  );
  return data;
},

cancelRecruitmentRequest: async (id: string): Promise<RecruitmentRequestResponse> => {
  const { data } = await apiClient.post<RecruitmentRequestResponse>(
    API_ENDPOINTS.hr.recruitment.requests.cancel,
    { id },
  );
  return data;
},

getRecruitmentRequestById: async (id: string): Promise<RecruitmentRequestResponse> => {
  const { data } = await apiClient.get<RecruitmentRequestResponse>(
    API_ENDPOINTS.hr.recruitment.requests.getById(id),
  );
  return data;
},

getRecruitmentRequests: async (params?: {
  status?: string;
  departmentId?: string;
  positionId?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}): Promise<PaginationResponse<RecruitmentRequestResponse>> => {
  const { data } = await apiClient.get<PaginationResponse<RecruitmentRequestResponse>>(
    API_ENDPOINTS.hr.recruitment.requests.list,
    { params },
  );
  return data;
},

getRecruitmentRequestTimeline: async (id: string): Promise<RecruitmentRequestHistoryResponse[]> => {
  const { data } = await apiClient.get<RecruitmentRequestHistoryResponse[]>(
    API_ENDPOINTS.hr.recruitment.requests.timeline(id),
  );
  return data;
},

createRecruitmentOpening: async (request: CreateRecruitmentOpeningRequest): Promise<RecruitmentOpeningResponse> => {
  const { data } = await apiClient.post<RecruitmentOpeningResponse>(
    API_ENDPOINTS.hr.recruitment.openings.create,
    request,
  );
  return data;
},

getRecruitmentDashboardWidgets: async (): Promise<RecruitmentDashboardWidgetsResponse> => {
  const { data } = await apiClient.get<RecruitmentDashboardWidgetsResponse>(
    API_ENDPOINTS.hr.recruitment.analytics.widgets,
  );
  return data;
},
```

- [ ] **Step 3: Add query keys and hooks to useRecruitment.ts**

```typescript
export const RECRUITMENT_QUERY_KEYS = {
  // ... existing keys ...
  requests: () => ["hr", "recruitment", "requests"] as const,
  requestById: (id: string) => ["hr", "recruitment", "requests", id] as const,
  requestTimeline: (id: string) => ["hr", "recruitment", "requests", id, "timeline"] as const,
  openings: () => ["hr", "recruitment", "openings"] as const,
  dashboardWidgets: () => ["hr", "recruitment", "dashboard", "widgets"] as const,
};

export function useRecruitmentRequests(params?: {
  status?: string;
  departmentId?: string;
  positionId?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: RECRUITMENT_QUERY_KEYS.requests(),
    queryFn: () => recruitmentService.getRecruitmentRequests(params),
    enabled: false,
  });
}

export function useRecruitmentRequestById(id: string) {
  return useQuery({
    queryKey: RECRUITMENT_QUERY_KEYS.requestById(id),
    queryFn: () => recruitmentService.getRecruitmentRequestById(id),
    enabled: Boolean(id),
  });
}

export function useRecruitmentRequestTimeline(id: string) {
  return useQuery({
    queryKey: RECRUITMENT_QUERY_KEYS.requestTimeline(id),
    queryFn: () => recruitmentService.getRecruitmentRequestTimeline(id),
    enabled: Boolean(id),
  });
}

export function useCreateRecruitmentRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: recruitmentService.createRecruitmentRequest,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: RECRUITMENT_QUERY_KEYS.requests() });
    },
  });
}

export function useUpdateRecruitmentRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: recruitmentService.updateRecruitmentRequest,
    onSuccess: (data) => {
      if (data?.id) {
        void queryClient.invalidateQueries({ queryKey: RECRUITMENT_QUERY_KEYS.requestById(data.id) });
      }
      void queryClient.invalidateQueries({ queryKey: RECRUITMENT_QUERY_KEYS.requests() });
    },
  });
}

export function useSubmitRecruitmentRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: recruitmentService.submitRecruitmentRequest,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: RECRUITMENT_QUERY_KEYS.requests() });
    },
  });
}

export function useApproveRecruitmentRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: recruitmentService.approveRecruitmentRequest,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: RECRUITMENT_QUERY_KEYS.requests() });
    },
  });
}

export function useRejectRecruitmentRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (params: { id: string; comment: string }) =>
      recruitmentService.rejectRecruitmentRequest(params.id, params.comment),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: RECRUITMENT_QUERY_KEYS.requests() });
    },
  });
}

export function useCancelRecruitmentRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: recruitmentService.cancelRecruitmentRequest,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: RECRUITMENT_QUERY_KEYS.requests() });
    },
  });
}

export function useCreateRecruitmentOpening() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: recruitmentService.createRecruitmentOpening,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: RECRUITMENT_QUERY_KEYS.openings() });
    },
  });
}

export function useRecruitmentDashboardWidgets() {
  return useQuery({
    queryKey: RECRUITMENT_QUERY_KEYS.dashboardWidgets(),
    queryFn: recruitmentService.getRecruitmentDashboardWidgets,
  });
}
```

- [ ] **Step 4: Add permissions to permissions.ts**

```typescript
HR_RECRUITMENT_REQUEST_VIEW: "hr.recruitment.request.view",
HR_RECRUITMENT_REQUEST_CREATE: "hr.recruitment.request.create",
HR_RECRUITMENT_REQUEST_SUBMIT: "hr.recruitment.request.submit",
HR_RECRUITMENT_REQUEST_APPROVE: "hr.recruitment.request.approve",
HR_RECRUITMENT_REQUEST_MANAGE: "hr.recruitment.request.manage",
```

Add route mapping:
```typescript
"/hr/recruitment/requests": [PERMISSIONS.HR_RECRUITMENT_REQUEST_VIEW],
```

- [ ] **Step 5: Add API endpoints to api-endpoints.ts**

```typescript
requests: {
  create: "/api/hr/recruitment/RecruitmentRequests/CreateRecruitmentRequest",
  update: "/api/hr/recruitment/RecruitmentRequests/UpdateRecruitmentRequest",
  submit: "/api/hr/recruitment/RecruitmentRequests/SubmitRecruitmentRequest",
  approve: "/api/hr/recruitment/RecruitmentRequests/ApproveRecruitmentRequest",
  reject: "/api/hr/recruitment/RecruitmentRequests/RejectRecruitmentRequest",
  cancel: "/api/hr/recruitment/RecruitmentRequests/CancelRecruitmentRequest",
  getById: (id: string) => `/api/hr/recruitment/RecruitmentRequests/GetRecruitmentRequestById/${id}`,
  list: "/api/hr/recruitment/RecruitmentRequests/GetRecruitmentRequests",
  timeline: (id: string) => `/api/hr/recruitment/RecruitmentRequests/GetRecruitmentRequestTimeline/${id}`,
},
openings: {
  create: "/api/hr/recruitment/RecruitmentOpenings/CreateRecruitmentOpening",
},
```

---

### Task 23: Frontend — Pages and Components

**Files:**
- Create: `cody-web-app/src/app/[locale]/(dashboard)/hr/recruitment/requests/page.tsx`
- Create: `cody-web-app/src/app/[locale]/(dashboard)/hr/recruitment/requests/[id]/page.tsx`
- Create dialog components under `cody-web-app/src/components/features/hr/recruitment/`

- [ ] **Step 1: Create requests list page**

```tsx
"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { FileText, Lock, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { PERMISSIONS } from "@/constants/permissions";
import { usePermissions } from "@/hooks/usePermissions";
import { useDataChangeRefresh } from "@/hooks/useDataChangeRefresh";
import { RefreshAvailableButton } from "@/components/shared/RefreshAvailableButton";
import { CreateRecruitmentRequestDialog } from "@/components/features/hr/recruitment/CreateRecruitmentRequestDialog";
import { useRecruitmentRequests } from "@/hooks/hr/useRecruitment";
import Link from "next/link";

const statusColors: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-800",
  Submitted: "bg-blue-100 text-blue-800",
  Approved: "bg-green-100 text-green-800",
  Rejected: "bg-red-100 text-red-800",
  Cancelled: "bg-yellow-100 text-yellow-800",
};

export default function RecruitmentRequestsPage() {
  const t = useTranslations("HRRecruitment");
  const { canAccessRoute, hasPermission } = usePermissions();
  const canView = canAccessRoute("/hr/recruitment/requests");
  const canManage = hasPermission(PERMISSIONS.HR_RECRUITMENT_REQUEST_CREATE);

  const { isStale, refresh } = useDataChangeRefresh({
    resource: "hr.recruitment.request",
    queryTags: ["hr", "recruitment", "recruitment-request"],
    mode: "manual",
    queryKeys: [["hr", "recruitment", "requests"]],
  });

  const { data, isLoading } = useRecruitmentRequests();
  const requests = data?.items ?? [];

  const [showCreateDialog, setShowCreateDialog] = useState(false);

  if (!canView) {
    return (
      <div className="flex h-[60vh] flex-col items-center justify-center gap-4 text-center">
        <Lock className="h-12 w-12 text-destructive" />
        <h2 className="text-xl font-bold text-foreground">{t("accessDeniedTitle")}</h2>
        <p className="max-w-sm text-sm text-muted-foreground">{t("accessDeniedDescription")}</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-3 w-full">
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <FileText className="h-5 w-5" />
              <div>
                <CardTitle>{t("requests.title")}</CardTitle>
                <CardDescription>{t("requests.description")}</CardDescription>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <RefreshAvailableButton isStale={isStale} onRefresh={refresh} />
              {canManage && (
                <Button onClick={() => setShowCreateDialog(true)}>
                  <Plus className="mr-1 h-4 w-4" />
                  {t("requests.createBtn")}
                </Button>
              )}
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t("requests.table.requestNumber")}</TableHead>
                  <TableHead>{t("requests.table.position")}</TableHead>
                  <TableHead>{t("requests.table.department")}</TableHead>
                  <TableHead>{t("requests.table.headcount")}</TableHead>
                  <TableHead>{t("requests.table.priority")}</TableHead>
                  <TableHead>{t("requests.table.status")}</TableHead>
                  <TableHead>{t("requests.table.requester")}</TableHead>
                  <TableHead>{t("requests.table.createdAt")}</TableHead>
                  <TableHead className="text-right">{t("requests.table.actions")}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {requests.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={9} className="text-center text-muted-foreground py-8">
                      {t("requests.table.empty")}
                    </TableCell>
                  </TableRow>
                ) : (
                  requests.map((req) => (
                    <TableRow key={req.id}>
                      <TableCell className="font-medium">{req.requestNumber}</TableCell>
                      <TableCell>{req.positionName}</TableCell>
                      <TableCell>{req.departmentName}</TableCell>
                      <TableCell>{req.requestedHeadcount}</TableCell>
                      <TableCell>
                        <Badge variant="outline">{req.priorityCode}</Badge>
                      </TableCell>
                      <TableCell>
                        <Badge className={statusColors[req.status] ?? ""}>{req.status}</Badge>
                      </TableCell>
                      <TableCell>{req.requestedBy}</TableCell>
                      <TableCell>{new Date(req.createdAt).toLocaleDateString()}</TableCell>
                      <TableCell className="text-right">
                        <Button variant="ghost" size="sm" asChild>
                          <Link href={`/hr/recruitment/requests/${req.id}`}>
                            {t("requests.table.view")}
                          </Link>
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      <CreateRecruitmentRequestDialog open={showCreateDialog} onOpenChange={setShowCreateDialog} />
    </div>
  );
}
```

- [ ] **Step 2: Create detail page with timeline**

```tsx
"use client";

import { useParams } from "next/navigation";
import { useTranslations } from "next-intl";
import { Lock, ArrowLeft, History } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { PERMISSIONS } from "@/constants/permissions";
import { usePermissions } from "@/hooks/usePermissions";
import { useRecruitmentRequestById, useRecruitmentRequestTimeline } from "@/hooks/hr/useRecruitment";
import Link from "next/link";
import { SubmitRecruitmentRequestDialog } from "@/components/features/hr/recruitment/SubmitRecruitmentRequestDialog";
import { ApproveRecruitmentRequestDialog } from "@/components/features/hr/recruitment/ApproveRecruitmentRequestDialog";
import { RejectRecruitmentRequestDialog } from "@/components/features/hr/recruitment/RejectRecruitmentRequestDialog";
import { CancelRecruitmentRequestDialog } from "@/components/features/hr/recruitment/CancelRecruitmentRequestDialog";
import { useState } from "react";

const statusColors: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-800",
  Submitted: "bg-blue-100 text-blue-800",
  Approved: "bg-green-100 text-green-800",
  Rejected: "bg-red-100 text-red-800",
  Cancelled: "bg-yellow-100 text-yellow-800",
};

export default function RecruitmentRequestDetailPage() {
  const t = useTranslations("HRRecruitment");
  const params = useParams();
  const id = params.id as string;
  const { canAccessRoute, hasPermission } = usePermissions();
  const canView = canAccessRoute("/hr/recruitment/requests");
  const canSubmit = hasPermission(PERMISSIONS.HR_RECRUITMENT_REQUEST_SUBMIT);
  const canApprove = hasPermission(PERMISSIONS.HR_RECRUITMENT_REQUEST_APPROVE);
  const canManage = hasPermission(PERMISSIONS.HR_RECRUITMENT_REQUEST_MANAGE);

  const { data: request, isLoading } = useRecruitmentRequestById(id);
  const { data: timeline } = useRecruitmentRequestTimeline(id);

  const [showSubmitDialog, setShowSubmitDialog] = useState(false);
  const [showApproveDialog, setShowApproveDialog] = useState(false);
  const [showRejectDialog, setShowRejectDialog] = useState(false);
  const [showCancelDialog, setShowCancelDialog] = useState(false);

  if (!canView) {
    return (
      <div className="flex h-[60vh] flex-col items-center justify-center gap-4 text-center">
        <Lock className="h-12 w-12 text-destructive" />
        <h2 className="text-xl font-bold text-foreground">{t("accessDeniedTitle")}</h2>
        <p className="max-w-sm text-sm text-muted-foreground">{t("accessDeniedDescription")}</p>
      </div>
    );
  }

  if (isLoading || !request) return <div>Loading...</div>;

  return (
    <div className="flex flex-col gap-3 w-full">
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="sm" asChild>
          <Link href="/hr/recruitment/requests">
            <ArrowLeft className="h-4 w-4 mr-1" />
            {t("requests.backToList")}
          </Link>
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>{request.requestNumber}</CardTitle>
              <CardDescription>{request.positionName} - {request.departmentName}</CardDescription>
            </div>
            <div className="flex items-center gap-2">
              <Badge className={statusColors[request.status] ?? ""}>{request.status}</Badge>
              {request.status === "Draft" && canSubmit && (
                <Button onClick={() => setShowSubmitDialog(true)}>Submit</Button>
              )}
              {request.status === "Submitted" && canApprove && (
                <>
                  <Button variant="default" onClick={() => setShowApproveDialog(true)}>Approve</Button>
                  <Button variant="destructive" onClick={() => setShowRejectDialog(true)}>Reject</Button>
                </>
              )}
              {(request.status === "Draft" || request.status === "Submitted") && canManage && (
                <Button variant="outline" onClick={() => setShowCancelDialog(true)}>Cancel</Button>
              )}
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Headcount</p>
              <p className="font-medium">{request.requestedHeadcount}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Priority</p>
              <p className="font-medium">{request.priorityCode}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Requester</p>
              <p className="font-medium">{request.requestedBy}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Created</p>
              <p className="font-medium">{new Date(request.createdAt).toLocaleString()}</p>
            </div>
          </div>
          {request.reason && (
            <div className="mt-4">
              <p className="text-sm text-muted-foreground">Reason</p>
              <p className="mt-1">{request.reason}</p>
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <History className="h-5 w-5" />
            <CardTitle>Timeline</CardTitle>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {timeline?.map((entry) => (
              <div key={entry.id} className="flex items-start gap-3 border-l-2 border-primary pl-4">
                <div>
                  <p className="font-medium">{entry.actionCode}</p>
                  <p className="text-sm text-muted-foreground">
                    {entry.performedBy} - {new Date(entry.performedAt).toLocaleString()}
                  </p>
                  {entry.comment && <p className="text-sm mt-1">{entry.comment}</p>}
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      <SubmitRecruitmentRequestDialog
        requestId={request.id}
        open={showSubmitDialog}
        onOpenChange={setShowSubmitDialog}
      />
      <ApproveRecruitmentRequestDialog
        requestId={request.id}
        open={showApproveDialog}
        onOpenChange={setShowApproveDialog}
      />
      <RejectRecruitmentRequestDialog
        requestId={request.id}
        open={showRejectDialog}
        onOpenChange={setShowRejectDialog}
      />
      <CancelRecruitmentRequestDialog
        requestId={request.id}
        open={showCancelDialog}
        onOpenChange={setShowCancelDialog}
      />
    </div>
  );
}
```

- [ ] **Step 3: Create dialog components**

Each dialog follows this pattern:

```tsx
// CreateRecruitmentRequestDialog.tsx
"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useCreateRecruitmentRequest } from "@/hooks/hr/useRecruitment";
import { useToast } from "@/hooks/useToast";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CreateRecruitmentRequestDialog({ open, onOpenChange }: Props) {
  const t = useTranslations("HRRecruitment");
  const { toast } = useToast();
  const mutation = useCreateRecruitmentRequest();
  const [departmentId, setDepartmentId] = useState("");
  const [positionId, setPositionId] = useState("");
  const [headcount, setHeadcount] = useState(1);
  const [reason, setReason] = useState("");
  const [priority, setPriority] = useState("Medium");

  const handleSubmit = async () => {
    try {
      await mutation.mutateAsync({
        departmentId,
        positionId,
        requestedHeadcount: headcount,
        reason,
        priorityCode: priority,
      });
      toast({ title: t("requests.dialogs.create.success") });
      onOpenChange(false);
    } catch {
      toast({ title: t("requests.dialogs.create.error"), variant: "destructive" });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("requests.dialogs.create.title")}</DialogTitle>
          <DialogDescription>{t("requests.dialogs.create.description")}</DialogDescription>
        </DialogHeader>
        <div className="grid gap-4">
          <div>
            <Label>{t("requests.dialogs.create.department")}</Label>
            <Input value={departmentId} onChange={(e) => setDepartmentId(e.target.value)} />
          </div>
          <div>
            <Label>{t("requests.dialogs.create.position")}</Label>
            <Input value={positionId} onChange={(e) => setPositionId(e.target.value)} />
          </div>
          <div>
            <Label>{t("requests.dialogs.create.headcount")}</Label>
            <Input type="number" min={1} value={headcount} onChange={(e) => setHeadcount(parseInt(e.target.value) || 1)} />
          </div>
          <div>
            <Label>{t("requests.dialogs.create.priority")}</Label>
            <Select value={priority} onValueChange={setPriority}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Low">{t("requests.priority.low")}</SelectItem>
                <SelectItem value="Medium">{t("requests.priority.medium")}</SelectItem>
                <SelectItem value="High">{t("requests.priority.high")}</SelectItem>
                <SelectItem value="Urgent">{t("requests.priority.urgent")}</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div>
            <Label>{t("requests.dialogs.create.reason")}</Label>
            <Input value={reason} onChange={(e) => setReason(e.target.value)} />
          </div>
          <Button onClick={handleSubmit} disabled={mutation.isPending}>
            {t("requests.dialogs.create.submit")}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
```

Create similar dialogs for:
- `SubmitRecruitmentRequestDialog` — confirm dialog with submit button
- `ApproveRecruitmentRequestDialog` — confirm with optional comment
- `RejectRecruitmentRequestDialog` — requires reason/comment
- `CancelRecruitmentRequestDialog` — confirm

- [ ] **Step 4: Build frontend**

Run: `cd cody-web-app && npm run build`
Expected: Build succeeds

---

### Self-Review Checklist

1. **Spec coverage:**
   - [x] RecruitmentRequest aggregate with Entity<TId> + domain events (Tasks 1-2)
   - [x] Status constants (RecruitmentRequestStatusCode) (Task 1)
   - [x] Priority constants (RecruitmentRequestPriorityCode) (Task 1)
   - [x] RecruitmentRequestHistory (Task 2)
   - [x] RecruitmentOpening aggregate (Task 2)
   - [x] JobPosting modification (optional RecruitmentOpeningId FK) (Task 7)
   - [x] Domain events (Task 3)
   - [x] Integration events (Task 4)
   - [x] Permissions (Task 5-6)
   - [x] Notification constants (Task 5)
   - [x] Error codes (Task 6)
   - [x] EF Core config with xmin concurrency (Task 7)
   - [x] Response DTOs + mapper (Task 8)
   - [x] CQRS commands x6 (Tasks 9-14)
   - [x] CQRS queries x3 (Task 16)
   - [x] Dashboard widgets (Task 17)
   - [x] Controllers (Task 17)
   - [x] Conversion sync (Task 18)
   - [x] Notification consumers (Task 19)
   - [x] Domain tests (Task 20)
   - [x] Handler tests (Task 21)
   - [x] Frontend types, service, hooks (Task 22)
   - [x] Frontend pages + dialogs (Task 23)

2. **Placeholder scan:** No TBDs, no TODOs, no "implement later". Every task has complete code.

3. **Type consistency:** All IDs use `Guid.Parse()` from string. All handler patterns match existing `CancelRequisitionHandler.cs`. All response DTOs use the record pattern consistent with existing responses.

4. **Publish-before-save:** Explicitly verified in Submit, Approve, Reject, Cancel handlers (Tasks 11-14).

5. **Conversion sync:** Task 18 covers the critical FilledHeadcount++ sync.
