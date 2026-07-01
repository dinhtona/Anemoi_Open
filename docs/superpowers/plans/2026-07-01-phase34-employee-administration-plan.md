# Phase 34 — Employee Administration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete Employee Administration with Direct HR Create/Edit, Status Actions, Documents, Assets, Notes, and full frontend.

**Architecture:** Extend existing Employee aggregate (add FirstName, LastName, DisplayName, AvatarStorageKey). Create 3 new aggregates (EmployeeDocument, EmployeeAsset, EmployeeNote) with strongly-typed IDs, own DbSets, own CQRS. Single EmployeeHistory for all audit. RESTful nested endpoints under `/api/hr/employees/{id}/`.

**Pre-flight rules:**
1. Use existing `Employee` factory if one exists — do NOT `new Employee()` directly.
2. WorkEmail update checks: verify if WorkEmail is linked to Identity username/email. If linked, do NOT allow update in Phase 34 (or require Identity sync). Defer to a later Identity-Employee sync phase.
3. **No commit before runtime verification passes.** All backend build, backend tests, frontend build, frontend lint, and browser validation must pass before any commit.

**Optional items (not blocking):**
1. Employee public setters → private/protected with factory methods (encapsulation refactor for a future phase).
2. Asset inventory (creating assets as Available without assignment) → Phase 35+ (IT Asset Management).
3. Name normalization post-migration (e.g., trimming, capitalizing first letters) → post-migration script or Phase 35.

**Tech Stack:** .NET 10, EF Core + PostgreSQL, MediatR + OneOf, Mapperly, FluentValidation, Next.js (cody-web-app)

**Design Doc:** `docs/superpowers/specs/2026-07-01-phase34-employee-administration-design.md`

---

## File Structure Map

### New Files

```
Anemoi.Hr.ModelIds/ModelIds/
  EmployeeDocumentId.cs
  EmployeeAssetId.cs
  EmployeeNoteId.cs

Anemoi.Hr.Domain/Employees/
  ValueObjects/DocumentType.cs
  ValueObjects/AssetType.cs
  ValueObjects/AssetStatus.cs
  ValueObjects/NoteCategory.cs

Anemoi.Hr.Domain/EmployeeDocuments/
  EmployeeDocument.cs

Anemoi.Hr.Domain/EmployeeAssets/
  EmployeeAsset.cs

Anemoi.Hr.Domain/EmployeeNotes/
  EmployeeNote.cs

Anemoi.Hr.Infrastructure/Persistence/Configurations/
  EmployeeDocumentConfiguration.cs
  EmployeeAssetConfiguration.cs
  EmployeeNoteConfiguration.cs

Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/
  CreateEmployee/CreateEmployeeCommand.cs
  CreateEmployee/CreateEmployeeHandler.cs
  CreateEmployee/CreateEmployeeValidator.cs
  UpdateEmployeeContact/UpdateEmployeeContactCommand.cs
  UpdateEmployeeContact/UpdateEmployeeContactHandler.cs
  UpdateEmployeeContact/UpdateEmployeeContactValidator.cs
  ChangeEmployeeDepartment/ChangeEmployeeDepartmentCommand.cs
  ChangeEmployeeDepartment/ChangeEmployeeDepartmentHandler.cs
  ChangeEmployeeDepartment/ChangeEmployeeDepartmentValidator.cs
  ChangeEmployeePosition/ChangeEmployeePositionCommand.cs
  ChangeEmployeePosition/ChangeEmployeePositionHandler.cs
  ChangeEmployeePosition/ChangeEmployeePositionValidator.cs
  ChangeEmployeeGrade/ChangeEmployeeGradeCommand.cs
  ChangeEmployeeGrade/ChangeEmployeeGradeHandler.cs
  ChangeEmployeeGrade/ChangeEmployeeGradeValidator.cs
  ChangeEmployeeManager/ChangeEmployeeManagerCommand.cs
  ChangeEmployeeManager/ChangeEmployeeManagerHandler.cs
  ChangeEmployeeManager/ChangeEmployeeManagerValidator.cs
  ActivateEmployee/ActivateEmployeeCommand.cs
  ActivateEmployee/ActivateEmployeeHandler.cs
  SuspendEmployee/SuspendEmployeeCommand.cs
  SuspendEmployee/SuspendEmployeeHandler.cs
  ResumeEmployee/ResumeEmployeeCommand.cs
  ResumeEmployee/ResumeEmployeeHandler.cs
  ArchiveEmployee/ArchiveEmployeeCommand.cs
  ArchiveEmployee/ArchiveEmployeeHandler.cs

Anemoi.Hr.Application/Cqrs/Commands/EmployeeDocumentCommands/
  CreateDocument/CreateEmployeeDocumentCommand.cs
  CreateDocument/CreateEmployeeDocumentHandler.cs
  CreateDocument/CreateEmployeeDocumentValidator.cs
  UpdateDocument/UpdateEmployeeDocumentCommand.cs
  UpdateDocument/UpdateEmployeeDocumentHandler.cs
  UpdateDocument/UpdateEmployeeDocumentValidator.cs
  ArchiveDocument/ArchiveEmployeeDocumentCommand.cs
  ArchiveDocument/ArchiveEmployeeDocumentHandler.cs

Anemoi.Hr.Application/Cqrs/Commands/EmployeeAssetCommands/
  AssignAsset/AssignEmployeeAssetCommand.cs
  AssignAsset/AssignEmployeeAssetHandler.cs
  AssignAsset/AssignEmployeeAssetValidator.cs
  UpdateAsset/UpdateEmployeeAssetCommand.cs
  UpdateAsset/UpdateEmployeeAssetHandler.cs
  UpdateAsset/UpdateEmployeeAssetValidator.cs
  ReturnAsset/ReturnEmployeeAssetCommand.cs
  ReturnAsset/ReturnEmployeeAssetHandler.cs
  ArchiveAsset/ArchiveEmployeeAssetCommand.cs
  ArchiveAsset/ArchiveEmployeeAssetHandler.cs

Anemoi.Hr.Application/Cqrs/Commands/EmployeeNoteCommands/
  CreateNote/CreateEmployeeNoteCommand.cs
  CreateNote/CreateEmployeeNoteHandler.cs
  CreateNote/CreateEmployeeNoteValidator.cs
  ArchiveNote/ArchiveEmployeeNoteCommand.cs
  ArchiveNote/ArchiveEmployeeNoteHandler.cs

Anemoi.Hr.Application/Cqrs/Queries/DocumentQueries/
  GetEmployeeDocuments/GetEmployeeDocumentsQuery.cs
  GetEmployeeDocuments/GetEmployeeDocumentsHandler.cs
  GetEmployeeDocument/GetEmployeeDocumentQuery.cs
  GetEmployeeDocument/GetEmployeeDocumentHandler.cs

Anemoi.Hr.Application/Cqrs/Queries/AssetQueries/
  GetEmployeeAssets/GetEmployeeAssetsQuery.cs
  GetEmployeeAssets/GetEmployeeAssetsHandler.cs
  GetEmployeeAsset/GetEmployeeAssetQuery.cs
  GetEmployeeAsset/GetEmployeeAssetHandler.cs

Anemoi.Hr.Application/Cqrs/Queries/NoteQueries/
  GetEmployeeNotes/GetEmployeeNotesQuery.cs
  GetEmployeeNotes/GetEmployeeNotesHandler.cs
  GetEmployeeNote/GetEmployeeNoteQuery.cs
  GetEmployeeNote/GetEmployeeNoteHandler.cs

Anemoi.Hr.Application/Responses/
  EmployeeDocumentResponse.cs
  EmployeeAssetResponse.cs
  EmployeeNoteResponse.cs
  CreateEmployeeResponse.cs

Anemoi.Hr.Application/Mappings/
  EmployeeDocumentMapper.cs
  EmployeeAssetMapper.cs
  EmployeeNoteMapper.cs

Anemoi.Hr.Api/Controllers/
  EmployeeDocumentController.cs
  EmployeeAssetController.cs
  EmployeeNoteController.cs

cody-web-app/src/types/hr/
  employeeDocument.ts
  employeeAsset.ts
  employeeNote.ts

cody-web-app/src/services/hr/
  documentService.ts
  assetService.ts
  noteService.ts

cody-web-app/src/hooks/hr/
  useEmployeeDocument.ts
  useEmployeeAsset.ts
  useEmployeeNote.ts

cody-web-app/src/app/[locale]/(dashboard)/hr/employees/create/
  page.tsx
cody-web-app/src/app/[locale]/(dashboard)/hr/employees/[employeeId]/edit/
  page.tsx
```

### Modified Files

```
Anemoi.Hr.Domain/Employees/Employee.cs              — add FirstName, LastName, DisplayName, AvatarStorageKey
Anemoi.Hr.ModelIds/IHrModelIdsAssemblyMarker.cs     — no change needed
Anemoi.Hr.Infrastructure/Persistence/HrDbContext.cs  — add 3 DbSets, ignore 3 Ids
Anemoi.Hr.Infrastructure/Persistence/Configurations/EmployeeConfiguration.cs — add new columns
Anemoi.BuildingBlock.Application/Authorization/Permissions.cs — add 14 new permissions
Anemoi.Hr.Application/Configurations/HrPermissions.cs — add delegates
Anemoi.BuildingBlock.Application/Resources/SharedResource.en.resx — add EN localization
Anemoi.BuildingBlock.Application/Resources/SharedResource.vi.resx — add VI localization
Anemoi.Hr.Application/Responses/EmployeeResponse.cs — add FirstName, LastName, DisplayName, AvatarStorageKey, AvatarUrl (resolved from StorageKey)
Anemoi.Hr.Application/Mappings/EmployeeMapper.cs   — map new fields
Anemoi.Hr.Api/Controllers/EmployeeController.cs     — add new endpoints
cody-web-app/src/constants/permissions.ts            — add frontend permission constants
cody-web-app/src/app/[locale]/(dashboard)/hr/employees/[employeeId]/page.tsx — add tabs
cody-web-app/src/app/[locale]/(dashboard)/hr/employees/page.tsx — enhance list
```

---

## Task List

### BACKEND STEP 1: Domain & Data

---

### Task 1: Add Employee ModelIds

**Files:**
- Create: `Anemoi.Hr.ModelIds/ModelIds/EmployeeDocumentId.cs`
- Create: `Anemoi.Hr.ModelIds/ModelIds/EmployeeAssetId.cs`
- Create: `Anemoi.Hr.ModelIds/ModelIds/EmployeeNoteId.cs`

- [ ] **Create EmployeeDocumentId.cs**

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeDocumentId(Guid Value) : IStronglyTypedId
{
    public override string ToString() => Value.ToString();
}
```

- [ ] **Create EmployeeAssetId.cs**

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeAssetId(Guid Value) : IStronglyTypedId
{
    public override string ToString() => Value.ToString();
}
```

- [ ] **Create EmployeeNoteId.cs**

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeNoteId(Guid Value) : IStronglyTypedId
{
    public override string ToString() => Value.ToString();
}
```

---

### Task 2: Add Domain Value Objects

**Files:**
- Create: `Anemoi.Hr.Domain/EmployeeDocuments/DocumentType.cs`
- Create: `Anemoi.Hr.Domain/EmployeeAssets/AssetType.cs`
- Create: `Anemoi.Hr.Domain/EmployeeAssets/AssetStatus.cs`
- Create: `Anemoi.Hr.Domain/EmployeeNotes/NoteCategory.cs`

- [ ] **Create DocumentType.cs**

```csharp
namespace Anemoi.Hr.Domain.EmployeeDocuments;

public sealed record DocumentType(string Value)
{
    public static readonly DocumentType LaborContract = new("labor_contract");
    public static readonly DocumentType IdCard = new("id_card");
    public static readonly DocumentType Passport = new("passport");
    public static readonly DocumentType Visa = new("visa");
    public static readonly DocumentType Certificate = new("certificate");
    public static readonly DocumentType Education = new("education");
    public static readonly DocumentType Resume = new("resume");
    public static readonly DocumentType Other = new("other");

    public static readonly IReadOnlyCollection<DocumentType> All =
    [
        LaborContract, IdCard, Passport, Visa, Certificate, Education, Resume, Other
    ];

    public static DocumentType FromValue(string value) =>
        All.FirstOrDefault(t => t.Value == value) ?? throw new ArgumentException($"Unknown DocumentType: {value}");

    public override string ToString() => Value;
}
```

- [ ] **Create AssetType.cs**

```csharp
namespace Anemoi.Hr.Domain.EmployeeAssets;

public sealed record AssetType(string Value)
{
    public static readonly AssetType Laptop = new("laptop");
    public static readonly AssetType Phone = new("phone");
    public static readonly AssetType Card = new("card");
    public static readonly AssetType Monitor = new("monitor");
    public static readonly AssetType Equipment = new("equipment");
    public static readonly AssetType Other = new("other");

    public static readonly IReadOnlyCollection<AssetType> All =
    [Laptop, Phone, Card, Monitor, Equipment, Other];

    public static AssetType FromValue(string value) =>
        All.FirstOrDefault(t => t.Value == value) ?? throw new ArgumentException($"Unknown AssetType: {value}");

    public override string ToString() => Value;
}
```

- [ ] **Create AssetStatus.cs**

```csharp
namespace Anemoi.Hr.Domain.EmployeeAssets;

public sealed record AssetStatus(string Value)
{
    public static readonly AssetStatus Available = new("available");
    public static readonly AssetStatus Assigned = new("assigned");
    public static readonly AssetStatus Returned = new("returned");
    public static readonly AssetStatus Lost = new("lost");
    public static readonly AssetStatus Damaged = new("damaged");

    public static readonly IReadOnlyCollection<AssetStatus> All =
    [Available, Assigned, Returned, Lost, Damaged];

    public static AssetStatus FromValue(string value) =>
        All.FirstOrDefault(s => s.Value == value) ?? throw new ArgumentException($"Unknown AssetStatus: {value}");

    public bool IsTerminal => this == Lost || this == Damaged;

    public override string ToString() => Value;
}
```

- [ ] **Create NoteCategory.cs**

```csharp
namespace Anemoi.Hr.Domain.EmployeeNotes;

public sealed record NoteCategory(string Value)
{
    public static readonly NoteCategory General = new("general");
    public static readonly NoteCategory Performance = new("performance");
    public static readonly NoteCategory Disciplinary = new("disciplinary");
    public static readonly NoteCategory Personal = new("personal");

    public static readonly IReadOnlyCollection<NoteCategory> All =
    [General, Performance, Disciplinary, Personal];

    public static NoteCategory FromValue(string value) =>
        All.FirstOrDefault(c => c.Value == value) ?? General;

    public override string ToString() => Value;
}
```

---

### Task 3: Create EmployeeDocument Domain Entity

**Files:**
- Create: `Anemoi.Hr.Domain/EmployeeDocuments/EmployeeDocument.cs`

- [ ] **Create the EmployeeDocument aggregate**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.EmployeeDocuments;

public sealed class EmployeeDocument : Entity<EmployeeDocumentId>
{
    public EmployeeId EmployeeId { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string DisplayName { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? IssuedBy { get; private set; }
    public DateOnly? IssuedDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public string? StorageKey { get; private set; }
    public string? FileName { get; private set; }
    public string? MimeType { get; private set; }
    public long? FileSize { get; private set; }
    public string? Notes { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private EmployeeDocument() { }

    public static EmployeeDocument Create(
        EmployeeDocumentId id,
        EmployeeId employeeId,
        DocumentType documentType,
        string displayName,
        string? referenceNumber = null,
        string? issuedBy = null,
        DateOnly? issuedDate = null,
        DateOnly? expiryDate = null,
        string? storageKey = null,
        string? fileName = null,
        string? mimeType = null,
        long? fileSize = null,
        string? notes = null)
    {
        return new EmployeeDocument
        {
            Id = id,
            EmployeeId = employeeId,
            DocumentType = documentType,
            DisplayName = displayName,
            ReferenceNumber = referenceNumber,
            IssuedBy = issuedBy,
            IssuedDate = issuedDate,
            ExpiryDate = expiryDate,
            StorageKey = storageKey,
            FileName = fileName,
            MimeType = mimeType,
            FileSize = fileSize,
            Notes = notes,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateInfo(
        DocumentType documentType,
        string displayName,
        string? referenceNumber = null,
        string? issuedBy = null,
        DateOnly? issuedDate = null,
        DateOnly? expiryDate = null,
        string? storageKey = null,
        string? fileName = null,
        string? mimeType = null,
        long? fileSize = null,
        string? notes = null)
    {
        DocumentType = documentType;
        DisplayName = displayName;
        ReferenceNumber = referenceNumber;
        IssuedBy = issuedBy;
        IssuedDate = issuedDate;
        ExpiryDate = expiryDate;
        StorageKey = storageKey;
        FileName = fileName;
        MimeType = mimeType;
        FileSize = fileSize;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        IsArchived = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

---

### Task 4: Create EmployeeAsset Domain Entity

**Files:**
- Create: `Anemoi.Hr.Domain/EmployeeAssets/EmployeeAsset.cs`

- [ ] **Create the EmployeeAsset aggregate**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.EmployeeAssets;

public sealed class EmployeeAsset : Entity<EmployeeAssetId>
{
    public EmployeeId? EmployeeId { get; private set; }
    public AssetType AssetType { get; private set; }
    public string AssetTag { get; private set; }
    public string Name { get; private set; }
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public string? SerialNumber { get; private set; }
    public AssetStatus AssetStatus { get; private set; }
    public DateTime AssignedDate { get; private set; }
    public DateTime? ReturnedDate { get; private set; }
    public string? Notes { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private EmployeeAsset() { }

    public static EmployeeAsset Create(
        EmployeeAssetId id,
        AssetType assetType,
        string assetTag,
        string name,
        string? brand = null,
        string? model = null,
        string? serialNumber = null,
        string? notes = null)
    {
        return new EmployeeAsset
        {
            Id = id,
            EmployeeId = null,
            AssetType = assetType,
            AssetTag = assetTag,
            Name = name,
            Brand = brand,
            Model = model,
            SerialNumber = serialNumber,
            AssetStatus = AssetStatus.Available,
            AssignedDate = DateTime.UtcNow,
            Notes = notes,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Assign(EmployeeId employeeId)
    {
        if (AssetStatus != AssetStatus.Available && AssetStatus != AssetStatus.Returned)
            throw new InvalidOperationException($"Cannot assign asset in status {AssetStatus}");

        EmployeeId = employeeId;
        AssetStatus = AssetStatus.Assigned;
        AssignedDate = DateTime.UtcNow;
        ReturnedDate = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Return()
    {
        if (AssetStatus != AssetStatus.Assigned)
            throw new InvalidOperationException($"Cannot return asset in status {AssetStatus}");

        EmployeeId = null;
        AssetStatus = AssetStatus.Returned;
        ReturnedDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkLost()
    {
        if (AssetStatus != AssetStatus.Assigned)
            throw new InvalidOperationException($"Cannot mark asset lost in status {AssetStatus}");

        AssetStatus = AssetStatus.Lost;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDamaged()
    {
        if (AssetStatus != AssetStatus.Assigned)
            throw new InvalidOperationException($"Cannot mark asset damaged in status {AssetStatus}");

        AssetStatus = AssetStatus.Damaged;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInfo(
        string name,
        string? brand = null,
        string? model = null,
        string? serialNumber = null,
        string? notes = null)
    {
        Name = name;
        Brand = brand;
        Model = model;
        SerialNumber = serialNumber;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        IsArchived = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

---

### Task 5: Create EmployeeNote Domain Entity

**Files:**
- Create: `Anemoi.Hr.Domain/EmployeeNotes/EmployeeNote.cs`

- [ ] **Create the EmployeeNote aggregate**

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.EmployeeNotes;

public sealed class EmployeeNote : Entity<EmployeeNoteId>
{
    public EmployeeId EmployeeId { get; private set; }
    public string Content { get; private set; }
    public NoteCategory NoteCategory { get; private set; }
    public string CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsArchived { get; private set; }

    private EmployeeNote() { }

    public static EmployeeNote Create(
        EmployeeNoteId id,
        EmployeeId employeeId,
        string content,
        string createdByUserId,
        NoteCategory? category = null)
    {
        return new EmployeeNote
        {
            Id = id,
            EmployeeId = employeeId,
            Content = content,
            NoteCategory = category ?? NoteCategory.General,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            IsArchived = false
        };
    }

    public void Archive()
    {
        IsArchived = true;
    }
}
```

---

### Task 6: Update Employee Entity

**Files:**
- Modify: `Anemoi.Hr.Domain/Employees/Employee.cs`

- [ ] **Update Employee.cs** — add new fields, make FullName computed

```csharp
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees.Events;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using EmpStatus = Anemoi.Hr.Domain.Employees.EmploymentStatusCode;

namespace Anemoi.Hr.Domain.Employees;

public sealed class Employee : Entity<EmployeeId>
{
    public Guid? IdentityUserId { get; set; }
    public string EmployeeCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarStorageKey { get; set; }
    public string FullName => DisplayName ?? $"{FirstName} {LastName}";
    public string WorkEmail { get; set; }
    public string PersonalEmail { get; set; }
    public string PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly JoinDate { get; set; }
    public string EmploymentStatusCode { get; set; }
    public string EmploymentTypeCode { get; set; }
    public string? GradeCode { get; set; }
    public DepartmentId PrimaryDepartmentId { get; set; }
    public PositionId PrimaryPositionId { get; set; }
    public EmployeeId? DirectManagerEmployeeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Department PrimaryDepartment { get; set; }
    public Position PrimaryPosition { get; set; }
    public Employee? DirectManager { get; set; }
    public List<Employee> DirectReports { get; set; } = [];
    // Existing navigation only (legacy from earlier phases).
    // No new history records shall be written to these collections.
    // All new audit events go exclusively to EmployeeHistory.
    public List<EmployeeDepartmentHistory> DepartmentHistories { get; set; } = [];
    public List<EmployeePositionHistory> PositionHistories { get; set; } = [];
    public List<EmployeeGradeHistory> GradeHistories { get; set; } = [];
    public List<EmployeeManagerHistory> ManagerHistories { get; set; } = [];

    private void ChangeStatus(string toStatus, string actor)
    {
        var fromStatus = EmploymentStatusCode;
        if (!EmpStatus.IsValidTransition(fromStatus, toStatus))
            throw new DomainException($"Cannot change status from {fromStatus} to {toStatus}");
        EmploymentStatusCode = toStatus;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new EmployeeStatusChangedDomainEvent(Id, fromStatus, toStatus, actor));
    }

    public void StartOnboarding(string actor) => ChangeStatus(EmpStatus.Onboarding, actor);
    public void Activate(string actor) => ChangeStatus(EmpStatus.Active, actor);
    public void Suspend(string actor) => ChangeStatus(EmpStatus.Suspended, actor);
    public void Resume(string actor) => ChangeStatus(EmpStatus.Active, actor);
    public void Resign(string actor) => ChangeStatus(EmpStatus.Resigned, actor);
    public void Terminate(string actor) => ChangeStatus(EmpStatus.Terminated, actor);
    public void Archive(string actor) => ChangeStatus(EmpStatus.Archived, actor);
}
```

- [ ] **Remove old `FullName` property** (was `public string FullName { get; set; }`) — replaced by computed property.

---

### Task 7: EF Configurations

**Files:**
- Create: `Anemoi.Hr.Infrastructure/Persistence/Configurations/EmployeeDocumentConfiguration.cs`
- Create: `Anemoi.Hr.Infrastructure/Persistence/Configurations/EmployeeAssetConfiguration.cs`
- Create: `Anemoi.Hr.Infrastructure/Persistence/Configurations/EmployeeNoteConfiguration.cs`
- Modify: `Anemoi.Hr.Infrastructure/Persistence/Configurations/EmployeeConfiguration.cs`
- Modify: `Anemoi.Hr.Infrastructure/Persistence/HrDbContext.cs`

- [ ] **Create EmployeeDocumentConfiguration.cs**

```csharp
using Anemoi.Hr.Domain.EmployeeDocuments;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Persistence.Configurations;

public sealed class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("hr.employee_documents");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasConversion(id => id.Value, v => new EmployeeDocumentId(v));
        builder.Property(d => d.EmployeeId).HasConversion(id => id.Value, v => new EmployeeId(v));
        builder.Property(d => d.DocumentType).HasConversion(t => t.Value, v => DocumentType.FromValue(v));
        builder.Property(d => d.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(d => d.ReferenceNumber).HasMaxLength(100);
        builder.Property(d => d.IssuedBy).HasMaxLength(200);
        builder.Property(d => d.StorageKey).HasMaxLength(500);
        builder.Property(d => d.FileName).HasMaxLength(256);
        builder.Property(d => d.MimeType).HasMaxLength(100);
        builder.Property(d => d.Notes).HasMaxLength(2000);
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt).IsRequired();
        builder.HasIndex(d => d.EmployeeId);
        builder.Property<uint>("xmin").IsRowVersion();
    }
}
```

- [ ] **Create EmployeeAssetConfiguration.cs**

```csharp
using Anemoi.Hr.Domain.EmployeeAssets;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Persistence.Configurations;

public sealed class EmployeeAssetConfiguration : IEntityTypeConfiguration<EmployeeAsset>
{
    public void Configure(EntityTypeBuilder<EmployeeAsset> builder)
    {
        builder.ToTable("hr.employee_assets");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasConversion(id => id.Value, v => new EmployeeAssetId(v));
        builder.Property(a => a.EmployeeId).HasConversion(id => id.Value, v => new EmployeeId(v));
        builder.Property(a => a.AssetType).HasConversion(t => t.Value, v => AssetType.FromValue(v));
        builder.Property(a => a.AssetTag).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Name).HasMaxLength(256).IsRequired();
        builder.Property(a => a.Brand).HasMaxLength(100);
        builder.Property(a => a.Model).HasMaxLength(100);
        builder.Property(a => a.SerialNumber).HasMaxLength(100);
        builder.Property(a => a.AssetStatus).HasConversion(s => s.Value, v => AssetStatus.FromValue(v));
        builder.Property(a => a.Notes).HasMaxLength(2000);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();
        builder.HasIndex(a => a.AssetTag).IsUnique();
        builder.HasIndex(a => a.EmployeeId);
        builder.Property<uint>("xmin").IsRowVersion();
    }
}
```

- [ ] **Create EmployeeNoteConfiguration.cs**

```csharp
using Anemoi.Hr.Domain.EmployeeNotes;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Persistence.Configurations;

public sealed class EmployeeNoteConfiguration : IEntityTypeConfiguration<EmployeeNote>
{
    public void Configure(EntityTypeBuilder<EmployeeNote> builder)
    {
        builder.ToTable("hr.employee_notes");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasConversion(id => id.Value, v => new EmployeeNoteId(v));
        builder.Property(n => n.EmployeeId).HasConversion(id => id.Value, v => new EmployeeId(v));
        builder.Property(n => n.Content).HasMaxLength(4000).IsRequired();
        builder.Property(n => n.NoteCategory).HasConversion(c => c.Value, v => NoteCategory.FromValue(v));
        builder.Property(n => n.CreatedByUserId).HasMaxLength(100).IsRequired();
        builder.Property(n => n.CreatedAt).IsRequired();
        builder.HasIndex(n => n.EmployeeId);
        builder.Property<uint>("xmin").IsRowVersion();
    }
}
```

- [ ] **Update EmployeeConfiguration.cs** — add FirstName, LastName, DisplayName, AvatarStorageKey columns. Make FullName computed (no column mapping needed).

```csharp
// In EmployeeConfiguration.Configure, add:
builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();
builder.Property(e => e.DisplayName).HasMaxLength(200);
builder.Property(e => e.AvatarStorageKey).HasMaxLength(500);
builder.Ignore(e => e.FullName); // computed property
builder.HasIndex(e => e.EmployeeCode).IsUnique();
```

- [ ] **Update HrDbContext.cs** — add DbSets and ignore new IDs

```csharp
// Add DbSets:
public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
public DbSet<EmployeeAsset> EmployeeAssets { get; set; }
public DbSet<EmployeeNote> EmployeeNotes { get; set; }

// Add ignores in OnModelCreating:
modelBuilder.Ignore<EmployeeDocumentId>();
modelBuilder.Ignore<EmployeeAssetId>();
modelBuilder.Ignore<EmployeeNoteId>();
```

---

### Task 8: Add Permissions (Central + Localization)

**Files:**
- Modify: `Anemoi.BuildingBlock.Application/Authorization/Permissions.cs`
- Modify: `Anemoi.BuildingBlock.Application/Resources/SharedResource.en.resx`
- Modify: `Anemoi.BuildingBlock.Application/Resources/SharedResource.vi.resx`

- [ ] **Add 14 new permission constants to Permissions.cs** (after `HrEmployeeUpdate`)

```csharp
public const string HrEmployeeActivate = "hr.employee.activate";
public const string HrEmployeeSuspend = "hr.employee.suspend";
public const string HrEmployeeResume = "hr.employee.resume";
public const string HrEmployeeArchive = "hr.employee.archive";
public const string HrEmployeeDepartmentChange = "hr.employee.department.change";
public const string HrEmployeePositionChange = "hr.employee.position.change";
public const string HrEmployeeGradeChange = "hr.employee.grade.change";
public const string HrEmployeeManagerChange = "hr.employee.manager.change";
public const string HrEmployeeDocumentView = "hr.employee.document.view";
public const string HrEmployeeDocumentManage = "hr.employee.document.manage";
public const string HrEmployeeAssetView = "hr.employee.asset.view";
public const string HrEmployeeAssetManage = "hr.employee.asset.manage";
public const string HrEmployeeNoteView = "hr.employee.note.view";
public const string HrEmployeeNoteManage = "hr.employee.note.manage";
```

- [ ] **Add Definition entries in Permissions.All** — after HrEmployeeUpdate entry, add:

```csharp
new(HrEmployeeActivate, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeActivate"),
new(HrEmployeeSuspend, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeSuspend"),
new(HrEmployeeResume, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeResume"),
new(HrEmployeeArchive, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeArchive"),
new(HrEmployeeDepartmentChange, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeDepartmentChange"),
new(HrEmployeePositionChange, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeePositionChange"),
new(HrEmployeeGradeChange, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeGradeChange"),
new(HrEmployeeManagerChange, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeManagerChange"),
new(HrEmployeeDocumentView, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeDocumentView"),
new(HrEmployeeDocumentManage, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeDocumentManage"),
new(HrEmployeeAssetView, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeAssetView"),
new(HrEmployeeAssetManage, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeAssetManage"),
new(HrEmployeeNoteView, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeNoteView"),
new(HrEmployeeNoteManage, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeNoteManage",
    true, "High"),
```

- [ ] **Add EN localization to SharedResource.en.resx** — add these entries:

```xml
<data name="PermissionDescriptionHrEmployeeActivate" xml:space="preserve"><value>Activate HR employee accounts.</value></data>
<data name="PermissionDescriptionHrEmployeeSuspend" xml:space="preserve"><value>Suspend HR employee accounts.</value></data>
<data name="PermissionDescriptionHrEmployeeResume" xml:space="preserve"><value>Resume suspended HR employee accounts.</value></data>
<data name="PermissionDescriptionHrEmployeeArchive" xml:space="preserve"><value>Archive HR employee records.</value></data>
<data name="PermissionDescriptionHrEmployeeDepartmentChange" xml:space="preserve"><value>Change employee department assignment.</value></data>
<data name="PermissionDescriptionHrEmployeePositionChange" xml:space="preserve"><value>Change employee position assignment.</value></data>
<data name="PermissionDescriptionHrEmployeeGradeChange" xml:space="preserve"><value>Change employee grade assignment.</value></data>
<data name="PermissionDescriptionHrEmployeeManagerChange" xml:space="preserve"><value>Change employee manager assignment.</value></data>
<data name="PermissionDescriptionHrEmployeeDocumentView" xml:space="preserve"><value>View employee documents.</value></data>
<data name="PermissionDescriptionHrEmployeeDocumentManage" xml:space="preserve"><value>Manage employee documents.</value></data>
<data name="PermissionDescriptionHrEmployeeAssetView" xml:space="preserve"><value>View employee assets.</value></data>
<data name="PermissionDescriptionHrEmployeeAssetManage" xml:space="preserve"><value>Manage employee assets.</value></data>
<data name="PermissionDescriptionHrEmployeeNoteView" xml:space="preserve"><value>View employee notes.</value></data>
<data name="PermissionDescriptionHrEmployeeNoteManage" xml:space="preserve"><value>Manage employee notes (sensitive).</value></data>
```

- [ ] **Add VI localization to SharedResource.vi.resx** — add these entries:

```xml
<data name="PermissionDescriptionHrEmployeeActivate" xml:space="preserve"><value>Kích hoạt tài khoản nhân viên.</value></data>
<data name="PermissionDescriptionHrEmployeeSuspend" xml:space="preserve"><value>Tạm ngưng tài khoản nhân viên.</value></data>
<data name="PermissionDescriptionHrEmployeeResume" xml:space="preserve"><value>Khôi phục tài khoản nhân viên đã tạm ngưng.</value></data>
<data name="PermissionDescriptionHrEmployeeArchive" xml:space="preserve"><value>Lưu trữ hồ sơ nhân viên.</value></data>
<data name="PermissionDescriptionHrEmployeeDepartmentChange" xml:space="preserve"><value>Thay đổi phòng ban của nhân viên.</value></data>
<data name="PermissionDescriptionHrEmployeePositionChange" xml:space="preserve"><value>Thay đổi chức vụ của nhân viên.</value></data>
<data name="PermissionDescriptionHrEmployeeGradeChange" xml:space="preserve"><value>Thay đổi bậc lương của nhân viên.</value></data>
<data name="PermissionDescriptionHrEmployeeManagerChange" xml:space="preserve"><value>Thay đổi quản lý trực tiếp của nhân viên.</value></data>
<data name="PermissionDescriptionHrEmployeeDocumentView" xml:space="preserve"><value>Xem tài liệu nhân viên.</value></data>
<data name="PermissionDescriptionHrEmployeeDocumentManage" xml:space="preserve"><value>Quản lý tài liệu nhân viên.</value></data>
<data name="PermissionDescriptionHrEmployeeAssetView" xml:space="preserve"><value>Xem tài sản nhân viên.</value></data>
<data name="PermissionDescriptionHrEmployeeAssetManage" xml:space="preserve"><value>Quản lý tài sản nhân viên.</value></data>
<data name="PermissionDescriptionHrEmployeeNoteView" xml:space="preserve"><value>Xem ghi chú nhân viên.</value></data>
<data name="PermissionDescriptionHrEmployeeNoteManage" xml:space="preserve"><value>Quản lý ghi chú nhân viên (nhạy cảm).</value></data>
```

- [ ] **Update HrPermissions.cs** — add delegates:

```csharp
public const string EmployeeActivate = BB.HrEmployeeActivate;
public const string EmployeeSuspend = BB.HrEmployeeSuspend;
public const string EmployeeResume = BB.HrEmployeeResume;
public const string EmployeeArchive = BB.HrEmployeeArchive;
public const string EmployeeDepartmentChange = BB.HrEmployeeDepartmentChange;
public const string EmployeePositionChange = BB.HrEmployeePositionChange;
public const string EmployeeGradeChange = BB.HrEmployeeGradeChange;
public const string EmployeeManagerChange = BB.HrEmployeeManagerChange;
public const string EmployeeDocumentView = BB.HrEmployeeDocumentView;
public const string EmployeeDocumentManage = BB.HrEmployeeDocumentManage;
public const string EmployeeAssetView = BB.HrEmployeeAssetView;
public const string EmployeeAssetManage = BB.HrEmployeeAssetManage;
public const string EmployeeNoteView = BB.HrEmployeeNoteView;
public const string EmployeeNoteManage = BB.HrEmployeeNoteManage;
```

- [ ] **Add all new permissions to `All` list and `SensitivePermissions` dictionary in HrPermissions.cs**

---

### Task 9: Generate EF Migration

- [ ] **Run migration command**

```bash
cd Anemoi.Hr/Anemoi.Hr.Api
dotnet ef migrations add AddEmployeeAdminPhase34 --context HrDbContext --output-dir ../Anemoi.Hr.Infrastructure/Persistence/Migrations
```

Verify the migration includes:
- `ALTER TABLE hr.employees ADD COLUMN first_name`, `last_name`, `display_name`, `avatar_storage_key`
- `UPDATE hr.employees SET first_name = full_name, last_name = '', display_name = full_name`
- `CREATE TABLE hr.employee_documents (...)`
- `CREATE TABLE hr.employee_assets (...)`
- `CREATE TABLE hr.employee_notes (...)`
- `CREATE UNIQUE INDEX ix_employees_employee_code ON hr.employees(employee_code)` (if not exists)

---

### BACKEND STEP 2: Application Layer

---

### Task 10: Create Employee Commands — CreateEmployee

**Files:**
- Create: `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/CreateEmployee/CreateEmployeeCommand.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/CreateEmployee/CreateEmployeeHandler.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/CreateEmployee/CreateEmployeeValidator.cs`
- Create: `Anemoi.Hr.Application/Responses/CreateEmployeeResponse.cs`

- [ ] **Create CreateEmployeeCommand.cs**

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.CreateEmployee;

public sealed record CreateEmployeeCommand(
    string EmployeeCode,
    string FirstName,
    string LastName,
    string? DisplayName,
    string? AvatarStorageKey,
    string WorkEmail,
    string? PersonalEmail,
    string PhoneNumber,
    DateOnly? DateOfBirth,
    DateOnly JoinDate,
    string EmploymentTypeCode,
    string? GradeCode,
    DepartmentId PrimaryDepartmentId,
    PositionId PrimaryPositionId,
    EmployeeId? DirectManagerEmployeeId
) : IRequest<OneOf<CreateEmployeeResponse, ErrorDetailResponse>>;

public sealed record CreateEmployeeResponse(EmployeeId Id);
```

- [ ] **Create CreateEmployeeValidator.cs**

```csharp
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.CreateEmployee;

public sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WorkEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.JoinDate).NotEmpty();
        RuleFor(x => x.EmploymentTypeCode).NotEmpty();
        RuleFor(x => x.PrimaryDepartmentId).NotNull();
        RuleFor(x => x.PrimaryPositionId).NotNull();
    }
}
```

- [ ] **Create CreateEmployeeHandler.cs** — uses EmployeeRepository or HrDbContext directly

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Employees.Events;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Anemoi.Hr.Infrastructure.Persistence;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.CreateEmployee;

public sealed class CreateEmployeeHandler(HrDbContext dbContext)
    : IRequestHandler<CreateEmployeeCommand, OneOf<CreateEmployeeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CreateEmployeeResponse, ErrorDetailResponse>> Handle(
        CreateEmployeeCommand request, CancellationToken ct)
    {
        // Check unique employee code
        if (await dbContext.Employees.AnyAsync(e => e.EmployeeCode == request.EmployeeCode, ct))
            return new ErrorDetailResponse("EmployeeCodeAlreadyExists", "Employee code already exists");

        // Check unique work email
        if (await dbContext.Employees.AnyAsync(e => e.WorkEmail == request.WorkEmail, ct))
            return new ErrorDetailResponse("WorkEmailAlreadyExists", "Work email already exists");

        // Validate manager exists if provided
        if (request.DirectManagerEmployeeId != null)
        {
            var managerExists = await dbContext.Employees.AnyAsync(
                e => e.Id == request.DirectManagerEmployeeId, ct);
            if (!managerExists)
                return new ErrorDetailResponse("ManagerNotFound", "Direct manager not found");
        }

        var employeeId = new EmployeeId(IdGenerator.NextGuid());
        // Use existing Employee factory if one exists (e.g., Employee.Create(...)).
        // Only fall back to constructor if aggregate currently has no factory.
        // Check existing codebase for Employee factory pattern before implementing.
        var employee = Employee.Create(
            id: employeeId,
            employeeCode: request.EmployeeCode,
            firstName: request.FirstName,
            lastName: request.LastName,
            displayName: request.DisplayName,
            avatarStorageKey: request.AvatarStorageKey,
            workEmail: request.WorkEmail,
            personalEmail: request.PersonalEmail,
            phoneNumber: request.PhoneNumber,
            dateOfBirth: request.DateOfBirth,
            joinDate: request.JoinDate,
            employmentTypeCode: request.EmploymentTypeCode,
            gradeCode: request.GradeCode,
            primaryDepartmentId: request.PrimaryDepartmentId,
            primaryPositionId: request.PrimaryPositionId,
            directManagerEmployeeId: request.DirectManagerEmployeeId
        );

        dbContext.Employees.Add(employee);

        // Write EmployeeHistory
        var history = new EmployeeHistory.EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = employeeId,
            EntityType = "Employee",
            EventType = "Created",
            Title = "Employee created",
            Description = $"{request.FirstName} {request.LastName} ({request.EmployeeCode})",
            OccurredAt = DateTime.UtcNow,
            ActorUserId = "", // populated from JWT context
        };
        dbContext.EmployeeHistories.Add(history);

        await dbContext.SaveChangesAsync(ct);

        return new CreateEmployeeResponse(employeeId);
    }
}
```

---

### Task 11: Create Employee Commands — Contact Update + Department/Position/Grade/Manager Changes

**Files:**
- Create: `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/UpdateEmployeeContact/UpdateEmployeeContactCommand.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/UpdateEmployeeContact/UpdateEmployeeContactHandler.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/UpdateEmployeeContact/UpdateEmployeeContactValidator.cs`
- (Same pattern for ChangeEmployeeDepartment, ChangeEmployeePosition, ChangeEmployeeGrade, ChangeEmployeeManager)

- [ ] **UpdateEmployeeContact** — updates PersonalEmail, PhoneNumber, DateOfBirth, AvatarStorageKey.
  **WorkEmail:** If the employee's WorkEmail is linked to Identity (i.e., used as login username/email), reject the update with an error. Phase 34 does NOT implement Identity sync. The handler must query IdentityService or check `IdentityUserId != null` — if linked, return `ErrorDetailResponse("WorkEmailLinkedToIdentity", "Cannot update work email linked to Identity").` Only allow WorkEmail update if the employee has NO Identity link.

```csharp
// UpdateEmployeeContactCommand.cs
public sealed record UpdateEmployeeContactCommand(
    EmployeeId EmployeeId,
    string? WorkEmail,
    string? PersonalEmail,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? AvatarStorageKey
) : IRequest<OneOf<Unit, ErrorDetailResponse>>;
```

Handler: loads Employee, updates non-null fields, writes EmployeeHistory with EventType "ContactUpdated".

- [ ] **ChangeEmployeeDepartment** — changes PrimaryDepartmentId

```csharp
// ChangeEmployeeDepartmentCommand.cs
public sealed record ChangeEmployeeDepartmentCommand(
    EmployeeId EmployeeId,
    DepartmentId NewDepartmentId
) : IRequest<OneOf<Unit, ErrorDetailResponse>>;
```

Handler: loads Employee + Department (validate active), calls existing TransferEmployee flow or updates field directly, writes EmployeeHistory.

- [ ] **ChangeEmployeePosition** — changes PrimaryPositionId

```csharp
// ChangeEmployeePositionCommand.cs
public sealed record ChangeEmployeePositionCommand(
    EmployeeId EmployeeId,
    PositionId NewPositionId
) : IRequest<OneOf<Unit, ErrorDetailResponse>>;
```

- [ ] **ChangeEmployeeGrade** — changes GradeCode

```csharp
// ChangeEmployeeGradeCommand.cs
public sealed record ChangeEmployeeGradeCommand(
    EmployeeId EmployeeId,
    string NewGradeCode
) : IRequest<OneOf<Unit, ErrorDetailResponse>>;
```

- [ ] **ChangeEmployeeManager** — changes DirectManagerEmployeeId

```csharp
// ChangeEmployeeManagerCommand.cs
public sealed record ChangeEmployeeManagerCommand(
    EmployeeId EmployeeId,
    EmployeeId? NewManagerEmployeeId
) : IRequest<OneOf<Unit, ErrorDetailResponse>>;
```

Each handler follows the same pattern:
1. Load Employee + validate exists
2. Load and validate referenced entity (Department active, Position active, etc.)
3. Update field
4. Write EmployeeHistory with specific EventType
5. SaveChangesAsync

---

### Task 12: Create Employee Commands — Status Actions

**Files:**
- Create: `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/ActivateEmployee/ActivateEmployeeCommand.cs`
- Create: `Anemoi.Hr.Application/Cqrs/Commands/EmployeeCommands/ActivateEmployee/ActivateEmployeeHandler.cs`
- (Same for SuspendEmployee, ResumeEmployee, ArchiveEmployee)

- [ ] **ActivateEmployeeCommand**

```csharp
public sealed record ActivateEmployeeCommand(
    EmployeeId EmployeeId
) : IRequest<OneOf<Unit, ErrorDetailResponse>>;
```

```csharp
// Handler
public async Task<OneOf<Unit, ErrorDetailResponse>> Handle(
    ActivateEmployeeCommand request, CancellationToken ct)
{
    var employee = await dbContext.Employees.FindAsync(new object[] { request.EmployeeId }, ct);
    if (employee == null)
        return new ErrorDetailResponse("EmployeeNotFound", "Employee not found");

    try { employee.Activate(""); } // actor from JWT
    catch (DomainException ex)
        return new ErrorDetailResponse("InvalidStatusTransition", ex.Message);

    WriteHistory(employee.Id, "Activated", "Employee activated");

    await dbContext.SaveChangesAsync(ct);
    return Unit.Value;
}
```

- [ ] **SuspendEmployeeCommand** — same pattern, calls `employee.Suspend()`
- [ ] **ResumeEmployeeCommand** — same pattern, calls `employee.Resume()`
- [ ] **ArchiveEmployeeCommand** — same pattern, calls `employee.Archive()`

---

### Task 13: Create Document CQRS

**Files:**
- Create: 8 files (Command, Handler, Validator for Create, Update, Archive + Query, Handler for GetList, GetById)
- Create: `Anemoi.Hr.Application/Responses/EmployeeDocumentResponse.cs`
- Create: `Anemoi.Hr.Application/Mappings/EmployeeDocumentMapper.cs`

- [ ] **CreateEmployeeDocumentCommand** — accepts employeeId, documentType, displayName, optional fields. Creates EmployeeDocument via factory, writes EmployeeHistory.
- [ ] **UpdateEmployeeDocumentCommand** — loads document, calls UpdateInfo, writes history.
- [ ] **ArchiveEmployeeDocumentCommand** — loads document, calls Archive, writes history.
- [ ] **GetEmployeeDocumentsQuery** — returns list for an employeeId, excludes archived unless requested.
- [ ] **GetEmployeeDocumentQuery** — returns single document by ID.
- [ ] **EmployeeDocumentResponse** — DTO with all fields.
- [ ] **EmployeeDocumentMapper** — Mapperly mapper (partial class with `[Mapper]` attribute).

---

### Task 14: Create Asset CQRS

**Files:**
- Create: 10 files (Command, Handler, Validator for Assign, Update, Return, Archive + Query, Handler for GetList, GetById)
- Create: `Anemoi.Hr.Application/Responses/EmployeeAssetResponse.cs`
- Create: `Anemoi.Hr.Application/Mappings/EmployeeAssetMapper.cs`

- [ ] **AssignEmployeeAssetCommand** — creates a new asset and immediately assigns to an employee (assign-on-create MVP). Asset is created with `AssetStatus.Assigned` directly. A separate "add to inventory" flow (creating asset as Available without assigning) is deferred to Phase 35+ when IT Asset Management is implemented.
- [ ] **UpdateEmployeeAssetCommand** — updates asset info fields.
- [ ] **ReturnEmployeeAssetCommand** — calls asset.Return(), writes history.
- [ ] **ArchiveEmployeeAssetCommand** — calls asset.Archive().
- [ ] **GetEmployeeAssetsQuery** — list for employeeId or all assets (based on filter).
- [ ] **GetEmployeeAssetQuery** — single asset.

---

### Task 15: Create Note CQRS

**Files:**
- Create: 6 files (Command, Handler, Validator for Create, Archive + Query, Handler for GetList, GetById)
- Create: `Anemoi.Hr.Application/Responses/EmployeeNoteResponse.cs`
- Create: `Anemoi.Hr.Application/Mappings/EmployeeNoteMapper.cs`
- Create: `Anemoi.Hr.Application/Mappings/EmployeeNoteMapper.cs`

- [ ] **CreateEmployeeNoteCommand** — accepts employeeId, content, category (optional), sets CreatedByUserId from JWT claims.
- [ ] **ArchiveEmployeeNoteCommand** — calls note.Archive().
- [ ] **GetEmployeeNotesQuery** — list notes for employee, archived excluded by default.
- [ ] **GetEmployeeNoteQuery** — single note.

Note: No update allowed — notes are append-only.

---

### BACKEND STEP 3: API Layer

---

### Task 16: Update EmployeeController

**Files:**
- Modify: `Anemoi.Hr.Api/Controllers/EmployeeController.cs`

- [ ] **Add new endpoints to EmployeeController.cs**

```csharp
[HasPermission(HrPermissions.EmployeeCreate)]
[HttpPost]
public async Task<IActionResult> CreateEmployee(
    CreateEmployeeCommand command, CancellationToken ct)
{
    var result = await Mediator.Send(command, ct);
    return result.Match(
        response => CreatedAtAction(nameof(GetEmployeeById), new { id = response.Id.Value }, response),
        error => BadRequest(error));
}

[HasPermission(HrPermissions.EmployeeUpdate)]
[HttpPut("{id}")]
public async Task<IActionResult> UpdateEmployeeContact(
    Guid id, UpdateEmployeeContactCommand command, CancellationToken ct)
{
    if (id != command.EmployeeId.Value) return BadRequest("Id mismatch");
    var result = await Mediator.Send(command, ct);
    return result.Match(_ => NoContent(), error => BadRequest(error));
}

[HasPermission(HrPermissions.EmployeeDepartmentChange)]
[HttpPut("{id}/department")]
public async Task<IActionResult> ChangeDepartment(
    Guid id, ChangeEmployeeDepartmentCommand command, CancellationToken ct)
{
    if (id != command.EmployeeId.Value) return BadRequest("Id mismatch");
    var result = await Mediator.Send(command, ct);
    return result.Match(_ => NoContent(), error => BadRequest(error));
}

[HasPermission(HrPermissions.EmployeePositionChange)]
[HttpPut("{id}/position")]
public async Task<IActionResult> ChangePosition(
    Guid id, ChangeEmployeePositionCommand command, CancellationToken ct)
{
    if (id != command.EmployeeId.Value) return BadRequest("Id mismatch");
    var result = await Mediator.Send(command, ct);
    return result.Match(_ => NoContent(), error => BadRequest(error));
}

[HasPermission(HrPermissions.EmployeeGradeChange)]
[HttpPut("{id}/grade")]
public async Task<IActionResult> ChangeGrade(
    Guid id, ChangeEmployeeGradeCommand command, CancellationToken ct)
{
    if (id != command.EmployeeId.Value) return BadRequest("Id mismatch");
    var result = await Mediator.Send(command, ct);
    return result.Match(_ => NoContent(), error => BadRequest(error));
}

[HasPermission(HrPermissions.EmployeeManagerChange)]
[HttpPut("{id}/manager")]
public async Task<IActionResult> ChangeManager(
    Guid id, ChangeEmployeeManagerCommand command, CancellationToken ct)
{
    if (id != command.EmployeeId.Value) return BadRequest("Id mismatch");
    var result = await Mediator.Send(command, ct);
    return result.Match(_ => NoContent(), error => BadRequest(error));
}

[HasPermission(HrPermissions.EmployeeActivate)]
[HttpPost("{id}/activate")]
public async Task<IActionResult> ActivateEmployee(
    Guid id, CancellationToken ct)
{
    var result = await Mediator.Send(new ActivateEmployeeCommand(new EmployeeId(id)), ct);
    return result.Match(_ => NoContent(), error => BadRequest(error));
}

[HasPermission(HrPermissions.EmployeeSuspend)]
[HttpPost("{id}/suspend")]
public async Task<IActionResult> SuspendEmployee(
    Guid id, CancellationToken ct) { /* similar */ }

[HasPermission(HrPermissions.EmployeeResume)]
[HttpPost("{id}/resume")]
public async Task<IActionResult> ResumeEmployee(
    Guid id, CancellationToken ct) { /* similar */ }

[HasPermission(HrPermissions.EmployeeArchive)]
[HttpPost("{id}/archive")]
public async Task<IActionResult> ArchiveEmployee(
    Guid id, CancellationToken ct) { /* similar */ }
```

- [ ] **Update EmployeeController route prefix** to `api/hr/employees` (from `api/hr/employee/Employee`). **Backward compat check:** The existing frontend in `cody-web-app/src/constants/api-endpoints.ts` uses API endpoint paths. Verify the existing route definitions and update them to match the new RESTful paths. Do NOT keep the old route — it's a breaking change but the frontend is the only consumer and can be updated atomically in this phase.

---

### Task 17: Create DocumentController

**Files:**
- Create: `Anemoi.Hr.Api/Controllers/EmployeeDocumentController.cs`

- [ ] **Create EmployeeDocumentController**

```csharp
[Route("api/hr/employees/{employeeId}/documents")]
[Authorize]
public sealed class EmployeeDocumentController(ISender mediator) : ControllerBase
{
    [HasPermission(HrPermissions.EmployeeDocumentView)]
    [HttpGet]
    public async Task<IActionResult> GetDocuments(
        Guid employeeId, CancellationToken ct)
    {
        var query = new GetEmployeeDocumentsQuery(new EmployeeId(employeeId));
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    [HasPermission(HrPermissions.EmployeeDocumentView)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(
        Guid employeeId, Guid id, CancellationToken ct)
    {
        var query = new GetEmployeeDocumentQuery(new EmployeeDocumentId(id));
        var result = await mediator.Send(query, ct);
        return result.Match(Ok, error => NotFound(error));
    }

    [HasPermission(HrPermissions.EmployeeDocumentManage)]
    [HttpPost]
    public async Task<IActionResult> CreateDocument(
        Guid employeeId, CreateEmployeeDocumentCommand command, CancellationToken ct)
    {
        if (employeeId != command.EmployeeId.Value) return BadRequest("Id mismatch");
        var result = await mediator.Send(command, ct);
        return result.Match(
            response => CreatedAtAction(nameof(GetDocument), new { employeeId, id = response.Id.Value }, response),
            error => BadRequest(error));
    }

    [HasPermission(HrPermissions.EmployeeDocumentManage)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDocument(
        Guid employeeId, Guid id, UpdateEmployeeDocumentCommand command, CancellationToken ct)
    {
        if (id != command.Id.Value) return BadRequest("Id mismatch");
        var result = await mediator.Send(command, ct);
        return result.Match(_ => NoContent(), error => BadRequest(error));
    }

    [HasPermission(HrPermissions.EmployeeDocumentManage)]
    [HttpPost("{id}/archive")]
    public async Task<IActionResult> ArchiveDocument(
        Guid employeeId, Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new ArchiveEmployeeDocumentCommand(new EmployeeDocumentId(id)), ct);
        return result.Match(_ => NoContent(), error => BadRequest(error));
    }
}
```

---

### Task 18: Create AssetController

**Files:**
- Create: `Anemoi.Hr.Api/Controllers/EmployeeAssetController.cs`

- [ ] **Create EmployeeAssetController** — same pattern as DocumentController but with:

```
GET    /api/hr/employees/{employeeId}/assets
GET    /api/hr/employees/{employeeId}/assets/{id}
POST   /api/hr/employees/{employeeId}/assets
PUT    /api/hr/employees/{employeeId}/assets/{id}
POST   /api/hr/employees/{employeeId}/assets/{id}/return
POST   /api/hr/employees/{employeeId}/assets/{id}/archive
```

---

### Task 19: Create NoteController

**Files:**
- Create: `Anemoi.Hr.Api/Controllers/EmployeeNoteController.cs`

- [ ] **Create EmployeeNoteController** — same pattern:

```
GET    /api/hr/employees/{employeeId}/notes
GET    /api/hr/employees/{employeeId}/notes/{id}
POST   /api/hr/employees/{employeeId}/notes
POST   /api/hr/employees/{employeeId}/notes/{id}/archive
```

---

### Task 20: Build and Verify Backend

- [ ] **Build the solution**

```bash
dotnet build Anemoi.sln
```

Expected: Build succeeded with 0 warnings. Fix any compilation errors.

- [ ] **Run existing tests**

```bash
dotnet test
```

Expected: All existing tests pass. Fix any regressions from Employee entity changes.

---

### FRONTEND

---

### Task 21: Update Frontend Permissions

**Files:**
- Modify: `cody-web-app/src/constants/permissions.ts`
- Modify: `cody-web-app/src/constants/api-endpoints.ts`

- [ ] **Add new permission constants to permissions.ts**

```typescript
HR_EMPLOYEE_ACTIVATE: "hr.employee.activate",
HR_EMPLOYEE_SUSPEND: "hr.employee.suspend",
HR_EMPLOYEE_RESUME: "hr.employee.resume",
HR_EMPLOYEE_ARCHIVE: "hr.employee.archive",
HR_EMPLOYEE_DEPARTMENT_CHANGE: "hr.employee.department.change",
HR_EMPLOYEE_POSITION_CHANGE: "hr.employee.position.change",
HR_EMPLOYEE_GRADE_CHANGE: "hr.employee.grade.change",
HR_EMPLOYEE_MANAGER_CHANGE: "hr.employee.manager.change",
HR_EMPLOYEE_DOCUMENT_VIEW: "hr.employee.document.view",
HR_EMPLOYEE_DOCUMENT_MANAGE: "hr.employee.document.manage",
HR_EMPLOYEE_ASSET_VIEW: "hr.employee.asset.view",
HR_EMPLOYEE_ASSET_MANAGE: "hr.employee.asset.manage",
HR_EMPLOYEE_NOTE_VIEW: "hr.employee.note.view",
HR_EMPLOYEE_NOTE_MANAGE: "hr.employee.note.manage",
```

- [ ] **Add new API endpoints to api-endpoints.ts**

```typescript
employees: {
  create: "/api/hr/employees",
  updateContact: (id: string) => `/api/hr/employees/${id}`,
  changeDepartment: (id: string) => `/api/hr/employees/${id}/department`,
  changePosition: (id: string) => `/api/hr/employees/${id}/position`,
  changeGrade: (id: string) => `/api/hr/employees/${id}/grade`,
  changeManager: (id: string) => `/api/hr/employees/${id}/manager`,
  activate: (id: string) => `/api/hr/employees/${id}/activate`,
  suspend: (id: string) => `/api/hr/employees/${id}/suspend`,
  resume: (id: string) => `/api/hr/employees/${id}/resume`,
  archive: (id: string) => `/api/hr/employees/${id}/archive`,
  documents: {
    list: (employeeId: string) => `/api/hr/employees/${employeeId}/documents`,
    get: (employeeId: string, id: string) => `/api/hr/employees/${employeeId}/documents/${id}`,
    create: (employeeId: string) => `/api/hr/employees/${employeeId}/documents`,
    update: (employeeId: string, id: string) => `/api/hr/employees/${employeeId}/documents/${id}`,
    archive: (employeeId: string, id: string) => `/api/hr/employees/${employeeId}/documents/${id}/archive`,
  },
  assets: { /* similar */ },
  notes: { /* similar */ },
},
```

---

### Task 22: Create Frontend Types

**Files:**
- Create: `cody-web-app/src/types/hr/employeeDocument.ts`
- Create: `cody-web-app/src/types/hr/employeeAsset.ts`
- Create: `cody-web-app/src/types/hr/employeeNote.ts`
- Modify: `cody-web-app/src/types/hr/employee.ts` — add FirstName, LastName, DisplayName, AvatarUrl (resolved URL from backend response)

- [ ] **employeeDocument.ts**

```typescript
export type DocumentType = "labor_contract" | "id_card" | "passport" | "visa" | "certificate" | "education" | "resume" | "other";

export interface EmployeeDocument {
  id: string;
  employeeId: string;
  documentType: DocumentType;
  displayName: string;
  referenceNumber?: string;
  issuedBy?: string;
  issuedDate?: string;
  expiryDate?: string;
  storageKey?: string;
  fileName?: string;
  mimeType?: string;
  fileSize?: number;
  notes?: string;
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateDocumentRequest {
  employeeId: string;
  documentType: DocumentType;
  displayName: string;
  referenceNumber?: string;
  issuedBy?: string;
  issuedDate?: string;
  expiryDate?: string;
  storageKey?: string;
  fileName?: string;
  mimeType?: string;
  fileSize?: number;
  notes?: string;
}
```

- [ ] **employeeAsset.ts**

```typescript
export type AssetType = "laptop" | "phone" | "card" | "monitor" | "equipment" | "other";
export type AssetStatus = "available" | "assigned" | "returned" | "lost" | "damaged";

export interface EmployeeAsset {
  id: string;
  employeeId?: string;
  assetType: AssetType;
  assetTag: string;
  name: string;
  brand?: string;
  model?: string;
  serialNumber?: string;
  assetStatus: AssetStatus;
  assignedDate: string;
  returnedDate?: string;
  notes?: string;
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface AssignAssetRequest {
  employeeId: string;
  assetType: AssetType;
  assetTag: string;
  name: string;
  brand?: string;
  model?: string;
  serialNumber?: string;
  notes?: string;
}
```

- [ ] **employeeNote.ts**

```typescript
export type NoteCategory = "general" | "performance" | "disciplinary" | "personal";

export interface EmployeeNote {
  id: string;
  employeeId: string;
  content: string;
  noteCategory: NoteCategory;
  createdByUserId: string;
  createdAt: string;
  isArchived: boolean;
}

export interface CreateNoteRequest {
  employeeId: string;
  content: string;
  noteCategory?: NoteCategory;
}
```

- [ ] **Update employee.ts** — add FirstName, LastName, DisplayName, AvatarUrl (resolved URL from backend `avatarUrl`) to Employee interface

---

### Task 23: Create Frontend Services

**Files:**
- Create: `cody-web-app/src/services/hr/documentService.ts`
- Create: `cody-web-app/src/services/hr/assetService.ts`
- Create: `cody-web-app/src/services/hr/noteService.ts`

- [ ] **documentService.ts**

```typescript
import api from "@/lib/api";
import { API_ENDPOINTS } from "@/constants/api-endpoints";
import { EmployeeDocument, CreateDocumentRequest } from "@/types/hr/employeeDocument";

export const documentService = {
  list: (employeeId: string) =>
    api.get<EmployeeDocument[]>(API_ENDPOINTS.HR.EMPLOYEES.DOCUMENTS.LIST(employeeId)),

  get: (employeeId: string, id: string) =>
    api.get<EmployeeDocument>(API_ENDPOINTS.HR.EMPLOYEES.DOCUMENTS.GET(employeeId, id)),

  create: (employeeId: string, data: CreateDocumentRequest) =>
    api.post<EmployeeDocument>(API_ENDPOINTS.HR.EMPLOYEES.DOCUMENTS.CREATE(employeeId), data),

  update: (employeeId: string, id: string, data: Partial<CreateDocumentRequest>) =>
    api.put<void>(API_ENDPOINTS.HR.EMPLOYEES.DOCUMENTS.UPDATE(employeeId, id), data),

  archive: (employeeId: string, id: string) =>
    api.post<void>(API_ENDPOINTS.HR.EMPLOYEES.DOCUMENTS.ARCHIVE(employeeId, id)),
};
```

- [ ] **assetService.ts** — similar pattern with list, get, assign, update, return, archive
- [ ] **noteService.ts** — similar pattern with list, get, create, archive (no update)

---

### Task 24: Create Frontend Hooks

**Files:**
- Create: `cody-web-app/src/hooks/hr/useEmployeeDocument.ts`
- Create: `cody-web-app/src/hooks/hr/useEmployeeAsset.ts`
- Create: `cody-web-app/src/hooks/hr/useEmployeeNote.ts`

- [ ] **useEmployeeDocument.ts** — React Query hooks

```typescript
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { documentService } from "@/services/hr/documentService";
import { CreateDocumentRequest } from "@/types/hr/employeeDocument";

export const useEmployeeDocuments = (employeeId: string) =>
  useQuery({
    queryKey: ["employee-documents", employeeId],
    queryFn: () => documentService.list(employeeId),
  });

export const useEmployeeDocument = (employeeId: string, id: string) =>
  useQuery({
    queryKey: ["employee-document", id],
    queryFn: () => documentService.get(employeeId, id),
    enabled: !!id,
  });

export const useCreateDocument = (employeeId: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateDocumentRequest) => documentService.create(employeeId, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["employee-documents", employeeId] }),
  });
};

export const useArchiveDocument = (employeeId: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => documentService.archive(employeeId, id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["employee-documents", employeeId] }),
  });
};
```

- [ ] **useEmployeeAsset.ts** — similar with assign, update, return, archive mutations
- [ ] **useEmployeeNote.ts** — similar with create, archive mutations (no update)

---

### Task 25: Create Frontend Pages

**Files:**
- Create: `cody-web-app/src/app/[locale]/(dashboard)/hr/employees/create/page.tsx`
- Create: `cody-web-app/src/app/[locale]/(dashboard)/hr/employees/[employeeId]/edit/page.tsx`
- Modify: `cody-web-app/src/app/[locale]/(dashboard)/hr/employees/[employeeId]/page.tsx`

- [ ] **Create Employee page** — form with EmployeeCode, FirstName, LastName, DisplayName, WorkEmail, PhoneNumber, DateOfBirth, JoinDate, EmploymentType, Grade, Department dropdown, Position dropdown, Manager dropdown. Submit calls createEmployee API.
- [ ] **Edit Employee page** — loads existing employee, shows editable fields (contact info only — status, code not editable). Submit calls updateEmployeeContact API.
- [ ] **Employee Detail tabs** — modify the existing `[employeeId]/page.tsx` to add tab navigation:

```
Tab: Overview | Employment | Compensation | History | Timeline | Documents | Assets | Notes
```

Each tab renders a component. Existing tabs (History, Timeline) remain unchanged. **Compensation tab is read-only projection** — salary changes remain in the Compensation module. Do NOT implement salary editing in this phase. New tabs:
- Documents — list of employee documents with add/archive actions
- Assets — list of assigned assets with assign/return actions
- Notes — list of HR notes with add/archive actions

---

### Task 26: Frontend Build & Polish

- [ ] **Build frontend**

```bash
cd cody-web-app
npm run build
```

Expected: Build succeeds with 0 errors. Fix any TypeScript/compilation issues.

- [ ] **Lint frontend**

```bash
cd cody-web-app
npm run lint
```

Expected: Lint passes with 0 errors.

- [ ] **Add i18n keys** for all new UI text (tab labels, form labels, button text, status labels, toast messages)

---

### VERIFICATION

---

### Task 27: Backend Tests

- [ ] **Write unit tests for EmployeeDocument domain** — test Create, UpdateInfo, Archive methods
- [ ] **Write unit tests for EmployeeAsset domain** — test Assign, Return, MarkLost, MarkDamaged, Archive, invalid transitions
- [ ] **Write unit tests for EmployeeNote domain** — test Create, Archive
- [ ] **Write unit tests for Employee status transitions** — test Activate, Suspend, Resume, Archive — valid and invalid transitions
- [ ] **Write handler tests** — test CreateEmployeeCommand handler (success, duplicate code, duplicate email, invalid manager)
- [ ] **Write permission tests** — verify each new endpoint requires correct permission (user with permission = 200, without = 403)
- [ ] **Write history tests** — verify each mutation produces EmployeeHistory entry with correct EventType

- [ ] **Run all tests**

```bash
dotnet test
```

Expected: All tests pass.

---

### Task 28: Browser Validation (DevTools MCP)

- [ ] **Verify Employee list page** — navigate to `/hr/employees`, verify list renders with search/filter
- [ ] **Verify Create Employee flow** — fill form, submit, verify redirect to employee detail
- [ ] **Verify Employee Detail with tabs** — navigate to employee detail, verify all tabs render
- [ ] **Verify Edit Employee** — change contact info, submit, verify update
- [ ] **Verify Status actions** — activate, suspend, resume, archive — verify each updates status badge
- [ ] **Verify Documents tab** — create document, view document, archive document
- [ ] **Verify Assets tab** — assign asset, return asset, archive asset
- [ ] **Verify Notes tab** — create note, view note, archive note
- [ ] **Verify Permissions** — verify unauthorized users get 403 on restricted actions
- [ ] **Verify Console** — no console errors
- [ ] **Verify Network** — no failed API calls (all 2xx/4xx expected)

---

### Task 29: Final Verification

### Runtime Verification (required before any commit)

- [ ] **Apply EF migration**

```bash
cd Anemoi.Hr/Anemoi.Hr.Api
dotnet ef database update --context HrDbContext
```

- [ ] **Verify database schema** — confirm all new tables and columns exist:

```
hr.employees: first_name, last_name, display_name, avatar_storage_key
hr.employee_documents: employee_id, document_type, display_name, storage_key, ...
hr.employee_assets: employee_id, asset_type, asset_tag, name, asset_status, ...
hr.employee_notes: employee_id, content, note_category, created_by_user_id, ...
```

- [ ] **Verify PostgreSQL data after Create/Edit** — run a test employee creation, then query DB:

```sql
SELECT id, employee_code, first_name, last_name FROM hr.employees WHERE employee_code = 'TEST001';
SELECT * FROM hr.employee_histories WHERE entity_type = 'Employee' ORDER BY occurred_at DESC LIMIT 5;
```

- [ ] **Verify EmployeeHistory records** — confirm each mutation (Create, Activate, Suspend, Resume, Archive, Document/Asset/Note changes) writes a corresponding EmployeeHistory row with correct EventType.

- [ ] **Verify no EF mapping/runtime exceptions** — the application starts without `InvalidOperationException` from EF configuration.

- [ ] **Full backend build**

```bash
dotnet build Anemoi.sln
```

- [ ] **Full backend tests**

```bash
dotnet test
```

- [ ] **Full frontend build**

```bash
cd cody-web-app && npm run build
```

- [ ] **Full frontend lint**

```bash
cd cody-web-app && npm run lint
```

- [ ] **Smoke test API** — run verify-api.ps1 for new endpoints

```powershell
pwsh ./scripts/verify-api.ps1 -ApiBaseUrl http://localhost:5000 -Method GET -Path /api/hr/employees
pwsh ./scripts/verify-api.ps1 -ApiBaseUrl http://localhost:5000 -Method POST -Path /api/hr/employees
```

**Commit is allowed only after all runtime verification passes.**

---
