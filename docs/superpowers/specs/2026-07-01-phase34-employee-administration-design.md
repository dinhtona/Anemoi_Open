# Phase 34 — Employee Administration & Employee Management Design

**Date:** 2026-07-01
**Status:** Draft
**Phase:** 34
**Previous:** Phase 33 — Employee Lifecycle
**Next:** Phase 35 — Employee Bulk Import & Data Migration (proposed)

---

## 1. Scope

Complete Employee Administration by adding Direct Employee Creation (HR), Edit, Status Actions, Documents, Assets, Notes, and corresponding frontend pages/tabs.

**IN SCOPE:**
- Direct HR Create Employee (alongside existing Recruitment → Employee flow)
- Edit Employee
- Status actions: Activate, Suspend, Resume, Archive
- EmployeeDocument aggregate (metadata-only, no upload yet)
- EmployeeAsset aggregate (with assign/return lifecycle)
- EmployeeNote aggregate (private HR notes, permission-protected)
- History/Timeline integration for all changes
- New permissions: Activate, Suspend, Archive, Document View/Manage, Asset View/Manage, Note View/Manage
- Frontend pages and tabs for all above
- Backend tests
- Browser validation via DevTools MCP

**OUT OF SCOPE (deferred to future phases):**
- Bulk Import (Phase 35)
- Rehire (placeholder only)
- Avatar upload
- S3/real file storage

---

## 2. Architecture Decisions

### 2.1 Domain Extensions

- `Employee` aggregate gains `FirstName`, `LastName`, `DisplayName`, `AvatarUrl` fields
- `FullName` becomes computed: `$"{FirstName} {LastName}"`
- `EmployeeCode` gets unique DB constraint (already unique-applied in practice)
- `EmployeeDocument`, `EmployeeAsset`, `EmployeeNote` are separate aggregates with strongly-typed IDs, own DbSets, own CQRS
- No hard delete; all entities support `IsArchived` flag

### 2.2 History & Timeline

Every mutation that changes employee data must:
1. Call domain method
2. Raise domain event
3. Write to `EmployeeHistory` (single audit log for all employee changes)
4. Update `EmployeeTimeline` query projection

No separate history/value-object entities are created for each change type. The existing `EmployeeHistory` entity (with `EntityType`, `EventType`, `Title`, `Description`, `MetadataJson`) handles all audit entries — department changes, position changes, grade changes, manager changes, contact updates, status actions, document/asset/note events. `EntityType` distinguishes the source: `"Employee"`, `"EmployeeDocument"`, `"EmployeeAsset"`, `"EmployeeNote"`. This keeps the schema simple and queryable via the existing `GetEmployeeTimeline` query.

### 2.3 Status Actions

| Action | Permission | Domain Method | Workflow | History |
|--------|-----------|--------------|----------|---------|
| Activate | `HrEmployeeActivate` | `Employee.Activate()` | No | Yes |
| Suspend | `HrEmployeeSuspend` | `Employee.Suspend()` | No | Yes |
| Resume | `HrEmployeeResume` | `Employee.Resume()` | No | Yes |
| Archive | `HrEmployeeArchive` | `Employee.Archive()` | No | Yes |
| Terminate | (existing) | Via `SubmitSeparation` → Workflow | Yes | Yes |

Terminate is NOT exposed as a direct endpoint — it goes through the existing Separation workflow.

### 2.4 Documents (EmployeeDocument)

```
EmployeeDocumentId (strongly typed)
├── EmployeeId (FK → Employee)
├── DocumentType (DocumentType — value object)
├── DisplayName (string, required)
├── ReferenceNumber (string?, e.g. contract number)
├── IssuedBy (string?)
├── IssuedDate (DateOnly?)
├── ExpiryDate (DateOnly?)
├── StorageKey (string?, nullable — opaque storage reference)
├── FileName (string?)
├── MimeType (string?)
├── FileSize (long?)
├── Notes (string?)
├── IsArchived (bool)
├── CreatedAt
├── UpdatedAt
```

`DocumentType` is a value object (record) wrapping a string code with a known set of static instances: `LaborContract`, `IDCard`, `Passport`, `Visa`, `Certificate`, `Education`, `Resume`, `Other`. This follows the existing `EmploymentStatusCode` / `TransferStatusCode` pattern in the project rather than raw strings.

### 2.5 Assets (EmployeeAsset)

```
EmployeeAssetId (strongly typed)
├── EmployeeId (FK → Employee, nullable — null when status is Available)
├── AssetType (AssetType — value object)
├── AssetTag (string, company asset tag number)
├── Name (string, display name)
├── Brand (string?)
├── Model (string?)
├── SerialNumber (string?)
├── AssetStatus (AssetStatus — value object)
├── AssignedDate (DateTime)
├── ReturnedDate (DateTime?)
├── Notes (string?)
├── IsArchived (bool)
├── CreatedAt
├── UpdatedAt
```

`AssetType` is a value object with static instances: `Laptop`, `Phone`, `Card`, `Monitor`, `Equipment`, `Other`.

`AssetStatus` is a value object with static instances: `Available`, `Assigned`, `Returned`, `Lost`, `Damaged`.

- `Available` — asset is in inventory, not assigned to any employee (EmployeeId = null)
- `Assigned` — asset is actively assigned to an employee
- `Returned` — asset was returned by employee, goes back to Available
- `Lost` — asset reported lost
- `Damaged` — asset reported damaged

Transitions: `Available` → `Assigned` → `Returned` → `Available`. From `Assigned` also to `Lost` | `Damaged`. Recovery from Lost/Damaged requires HR admin to move back to `Available` (after recovery/repair).

### 2.6 Notes (EmployeeNote)

```
EmployeeNoteId (strongly typed)
├── EmployeeId (FK → Employee)
├── Content (string, required)
├── NoteCategory (NoteCategory? — value object, optional)
├── CreatedByUserId (string, JWT subject)
├── CreatedAt (DateTime)
├── IsArchived (bool)
```

No denormalized `CreatedByEmployeeName` — only store the user ID. The employee name is resolved at query time via the projection/query handler using the Identity service or a denormalized read model. This keeps the domain entity clean and avoids stale display data.

`NoteCategory` is a value object with static instances: `General`, `Performance`, `Disciplinary`, `Personal`. Optional — if not specified, defaults to `General`.

Notes are append-only. No edit of content — only archive.

---

## 3. API Endpoints

### 3.1 Employee CRUD (EmployeeController — existing, REST-standard routes)

| Method | Route | Command/Query | Permission |
|--------|-------|--------------|------------|
| POST | `/api/hr/employees` | `CreateEmployeeCommand` | `HrEmployeeCreate` |
| GET | `/api/hr/employees/{id}` | `GetEmployeeQuery` | `HrEmployeeView` |
| GET | `/api/hr/employees` | `GetEmployeesQuery` | `HrEmployeeView` |
| PUT | `/api/hr/employees/{id}` | `UpdateEmployeeContactCommand` | `HrEmployeeUpdate` |
| PUT | `/api/hr/employees/{id}/department` | `ChangeEmployeeDepartmentCommand` | `HrEmployeeDepartmentChange` |
| PUT | `/api/hr/employees/{id}/position` | `ChangeEmployeePositionCommand` | `HrEmployeePositionChange` |
| PUT | `/api/hr/employees/{id}/grade` | `ChangeEmployeeGradeCommand` | `HrEmployeeGradeChange` |
| PUT | `/api/hr/employees/{id}/manager` | `ChangeEmployeeManagerCommand` | `HrEmployeeManagerChange` |
| POST | `/api/hr/employees/{id}/activate` | `ActivateEmployeeCommand` | `HrEmployeeActivate` |
| POST | `/api/hr/employees/{id}/suspend` | `SuspendEmployeeCommand` | `HrEmployeeSuspend` |
| POST | `/api/hr/employees/{id}/resume` | `ResumeEmployeeCommand` | `HrEmployeeResume` |
| POST | `/api/hr/employees/{id}/archive` | `ArchiveEmployeeCommand` | `HrEmployeeArchive` |

### 3.2 Documents (DocumentController — new)

| Method | Route | Command/Query | Permission |
|--------|-------|--------------|------------|
| GET | `/api/hr/employees/{employeeId}/documents` | `GetEmployeeDocumentsQuery` | `HrEmployeeDocumentView` |
| GET | `/api/hr/employees/{employeeId}/documents/{id}` | `GetEmployeeDocumentQuery` | `HrEmployeeDocumentView` |
| POST | `/api/hr/employees/{employeeId}/documents` | `CreateEmployeeDocumentCommand` | `HrEmployeeDocumentManage` |
| PUT | `/api/hr/employees/{employeeId}/documents/{id}` | `UpdateEmployeeDocumentCommand` | `HrEmployeeDocumentManage` |
| POST | `/api/hr/employees/{employeeId}/documents/{id}/archive` | `ArchiveEmployeeDocumentCommand` | `HrEmployeeDocumentManage` |

### 3.3 Assets (AssetController — new)

| Method | Route | Command/Query | Permission |
|--------|-------|--------------|------------|
| GET | `/api/hr/employees/{employeeId}/assets` | `GetEmployeeAssetsQuery` | `HrEmployeeAssetView` |
| GET | `/api/hr/employees/{employeeId}/assets/{id}` | `GetEmployeeAssetQuery` | `HrEmployeeAssetView` |
| POST | `/api/hr/employees/{employeeId}/assets` | `AssignEmployeeAssetCommand` | `HrEmployeeAssetManage` |
| PUT | `/api/hr/employees/{employeeId}/assets/{id}` | `UpdateEmployeeAssetCommand` | `HrEmployeeAssetManage` |
| POST | `/api/hr/employees/{employeeId}/assets/{id}/return` | `ReturnEmployeeAssetCommand` | `HrEmployeeAssetManage` |
| POST | `/api/hr/employees/{employeeId}/assets/{id}/archive` | `ArchiveEmployeeAssetCommand` | `HrEmployeeAssetManage` |

### 3.4 Notes (NoteController — new)

| Method | Route | Command/Query | Permission |
|--------|-------|--------------|------------|
| GET | `/api/hr/employees/{employeeId}/notes` | `GetEmployeeNotesQuery` | `HrEmployeeNoteView` |
| GET | `/api/hr/employees/{employeeId}/notes/{id}` | `GetEmployeeNoteQuery` | `HrEmployeeNoteView` |
| POST | `/api/hr/employees/{employeeId}/notes` | `CreateEmployeeNoteCommand` | `HrEmployeeNoteManage` |
| POST | `/api/hr/employees/{employeeId}/notes/{id}/archive` | `ArchiveEmployeeNoteCommand` | `HrEmployeeNoteManage` |

---

## 4. New Permissions

### 4.1 Central `Permissions.cs` additions

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

### 4.2 Module `HrPermissions.cs` delegates

All new permissions delegated from `BB.*` constants.

### 4.3 Sensitive Permissions

- `HrEmployeeSuspend` → Sensitive, High
- `HrEmployeeNoteManage` → Sensitive, High (notes contain private HR commentary)
- `HrEmployeeDepartmentChange` → Sensitive, High (org structure change)
- `HrEmployeePositionChange` → Sensitive, High (career impact)
- `HrEmployeeGradeChange` → Sensitive, High (compensation impact)

### 4.4 Localization

EN and VI resource entries for all new `PermissionGroup*` and `PermissionDescription*` keys.

### 4.5 Frontend `permissions.ts`

New constants:
```
HR_EMPLOYEE_ACTIVATE, HR_EMPLOYEE_SUSPEND, HR_EMPLOYEE_RESUME, HR_EMPLOYEE_ARCHIVE
HR_EMPLOYEE_DOCUMENT_VIEW, HR_EMPLOYEE_DOCUMENT_MANAGE
HR_EMPLOYEE_ASSET_VIEW, HR_EMPLOYEE_ASSET_MANAGE
HR_EMPLOYEE_NOTE_VIEW, HR_EMPLOYEE_NOTE_MANAGE
```

---

## 5. Domain Layer

### 5.1 Employee Changes

```csharp
// New properties
public string FirstName { get; set; }
public string LastName { get; set; }
public string? DisplayName { get; set; }
public string? AvatarStorageKey { get; set; }

// Computed
public string FullName => DisplayName ?? $"{FirstName} {LastName}";
```

**xmin is NOT a domain property.** It is a persistence concern — configured as an EF Core shadow property (`.IsRowVersion()`) on the Infrastructure entity configuration. The domain entity stays clean of persistence details. Update DTOs may carry the xmin value for concurrency checks at the handler level.

Migration: backfill safely. For all existing rows, set `FirstName = FullName`, `LastName = ""` (empty string), `DisplayName = FullName`, `AvatarStorageKey = null`. Empty string avoids data duplication while preserving the original FullName in DisplayName. HR corrects both via Edit Employee UI post-migration.

EmployeeResponse DTO keeps `FullName` for backward compatibility.

### 5.1.1 Storage Abstraction

All file/avatar references use `StorageKey` (nullable string) — an opaque content reference, not a direct URL. The actual storage resolution (local path, S3 presigned URL, CDN URL) is handled by a future StorageService layer. This prevents coupling to any specific storage provider at the domain level.

| Entity | Old Field | New Field |
|--------|----------|-----------|
| Employee | `AvatarUrl` | `AvatarStorageKey` |
| EmployeeDocument | `FileUrl` | `StorageKey` |

Downstream DTOs may include a computed display URL resolved at query time through an optional resolution service, but the domain entity stores only the opaque key.

### 5.1.2 Business Event–Specific Commands

`UpdateEmployeeCommand` is eliminated. Instead, each business change gets its own command to produce precise domain events and history entries:

| Command | Business Event | History Title |
|---------|--------------|--------------|
| `UpdateEmployeeContactCommand` | Contact info changed | "Contact information updated" |
| `ChangeEmployeeDepartmentCommand` | Department transfer | "Department changed to {name}" |
| `ChangeEmployeePositionCommand` | Position change | "Position changed to {name}" |
| `ChangeEmployeeGradeCommand` | Grade change | "Grade changed to {code}" |
| `ChangeEmployeeManagerCommand` | Manager reassignment | "Manager changed to {name}" |

Each command triggers the existing or analogous domain methods on Employee (e.g. `TransferDepartment`, `ChangePosition`), writes EmployeeHistory with a specific EventType, and updates EmployeeTimeline. This replaces the monolithic UpdateEmployeeCommand approach.

### 5.2 New ModelIds

- `EmployeeDocumentId`
- `EmployeeAssetId`
- `EmployeeNoteId`

### 5.3 New Domain Entities

Each in its own folder under `Anemoi.Hr.Domain/`:
- `EmployeeDocuments/EmployeeDocument.cs`
- `EmployeeAssets/EmployeeAsset.cs`
- `EmployeeNotes/EmployeeNote.cs`

Each with relevant domain events for create/status-change as needed.

---

## 6. Application Layer

### 6.1 New CQRS — Employee Commands

Each command folder follows existing pattern: `Command.cs` + `Handler.cs` + `Validator.cs`:

- `CreateEmployeeCommand` — All employee fields, generates Employee, writes EmployeeHistory("Created"), raises EmployeeCreatedDomainEvent
- `UpdateEmployeeContactCommand` — Updates only contact/personal fields (Email, Phone, Address, etc.). Does NOT update status, EmployeeCode, department, position, grade, manager
- `ChangeEmployeeDepartmentCommand` — Changes employee's primary department. Calls domain method on Employee, writes EmployeeDepartmentHistory, raises EmployeeDepartmentChangedDomainEvent
- `ChangeEmployeePositionCommand` — Changes employee's primary position. Calls domain method, writes EmployeePositionHistory, raises EmployeePositionChangedDomainEvent
- `ChangeEmployeeGradeCommand` — Changes employee's grade. Calls domain method, writes EmployeeGradeHistory, raises EmployeeGradeChangedDomainEvent
- `ChangeEmployeeManagerCommand` — Changes employee's direct manager. Calls domain method, writes EmployeeManagerHistory, raises EmployeeManagerChangedDomainEvent
- `ActivateEmployeeCommand` — Calls Employee.Activate(), writes history
- `SuspendEmployeeCommand` — Calls Employee.Suspend(), writes history
- `ResumeEmployeeCommand` — Calls Employee.Resume(), writes history
- `ArchiveEmployeeCommand` — Calls Employee.Archive(), writes history

### 6.2 New CQRS — Document Commands

- `CreateEmployeeDocumentCommand` + Handler + Validator
- `UpdateEmployeeDocumentCommand` + Handler + Validator
- `ArchiveEmployeeDocumentCommand` + Handler
- `GetEmployeeDocumentsQuery` + Handler
- `GetEmployeeDocumentQuery` + Handler

### 6.3 New CQRS — Asset Commands

- `AssignEmployeeAssetCommand` + Handler + Validator
- `UpdateEmployeeAssetCommand` + Handler + Validator
- `ReturnEmployeeAssetCommand` + Handler
- `ArchiveEmployeeAssetCommand` + Handler
- `GetEmployeeAssetsQuery` + Handler
- `GetEmployeeAssetQuery` + Handler

### 6.4 New CQRS — Note Commands

- `CreateEmployeeNoteCommand` + Handler + Validator
- `ArchiveEmployeeNoteCommand` + Handler
- `GetEmployeeNotesQuery` + Handler
- `GetEmployeeNoteQuery` + Handler

### 6.5 DTOs and Responses

- `EmployeeResponse` — add `FirstName`, `LastName`, `DisplayName`, `AvatarUrl`, keep `FullName`
- `EmployeeDocumentResponse`, `EmployeeAssetResponse`, `EmployeeNoteResponse`
- `CreateEmployeeResponse` — returns `EmployeeId`

### 6.6 Mappers

- Update `EmployeeMapper` for new Employee fields
- `EmployeeDocumentMapper` (new)
- `EmployeeAssetMapper` (new)
- `EmployeeNoteMapper` (new)

---

## 7. Infrastructure Layer

### 7.1 HR DbContext — New DbSets

```csharp
public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
public DbSet<EmployeeAsset> EmployeeAssets { get; set; }
public DbSet<EmployeeNote> EmployeeNotes { get; set; }
```

### 7.2 EF Configurations

New configuration classes:
- `EmployeeDocumentConfiguration` — maps to `hr.employee_documents`
- `EmployeeAssetConfiguration` — maps to `hr.employee_assets`
- `EmployeeNoteConfiguration` — maps to `hr.employee_notes`

Employee entity configuration update: add `FirstName`, `LastName`, `DisplayName`, `AvatarStorageKey` columns; add unique index on `EmployeeCode`.

### 7.3 Migrations

One new migration that:
1. Adds `first_name`, `last_name`, `display_name`, `avatar_storage_key`, `xmin` to `hr.employees`
2. Backfills existing data safely: `first_name = full_name`, `last_name = full_name`, `display_name = full_name`, `avatar_storage_key = null`. No space-splitting — avoids data loss for non-Western names, single-name records, or multi-space names
3. Makes `first_name`/`last_name` non-nullable for new rows (existing rows keep the backfilled values)
4. Creates `hr.employee_documents` (with `storage_key`, `document_type` columns, `xmin` concurrency token), `hr.employee_assets` (with `asset_type`, `asset_status` columns, `xmin`), `hr.employee_notes` (with `note_category` column, `xmin`) tables
5. Adds unique index on `employee_code` if not present

---

## 7.3 Optimistic Concurrency (xmin)

All mutable aggregates use PostgreSQL's `xmin` for optimistic concurrency control, following the project's existing pattern. xmin is configured as an EF Core **shadow property** (`.IsRowVersion()`) in Infrastructure entity configurations — NOT as a property on the domain entity.

Aggregates with xmin: Employee (existing), EmployeeDocument (new), EmployeeAsset (new), EmployeeNote (new).

The frontend should handle 409 Conflict responses by refreshing data and retrying.

### 7.4 Future Refactoring: EmployeeAsset → Asset + AssetAssignment

The current design places `EmployeeAsset` as a child of Employee (FK → EmployeeId). This is acceptable for Phase 34 scope. However, enterprise IT Asset Management typically requires a richer model:

```
Asset (independent aggregate)
  ├── AssetTag, Type, Brand, Model, SerialNumber, Status
  └── AssetAssignment (correlated aggregate, FK → AssetId + EmployeeId)
        ├── AssignedDate, ReturnedDate, Notes
        └── Full assignment history (A → B → C across time)
```

**TODO / Future Phase:** When IT Asset Management is implemented, `EmployeeAsset` should be refactored into a standalone `Asset` aggregate with a separate `AssetAssignment` aggregate. The Phase 34 design keeps `EmployeeAsset` as-is to avoid scope bloat, but AssetTag uniqueness across all employees and clean status transitions are maintained with this extraction path in mind.

---

## 8. Frontend

### 8.1 New/Modified Routes

Frontend routes follow REST conventions:

```
/hr/employees                       → EmployeeListPage (enhance existing)
/hr/employees/create                → CreateEmployeePage (new)
/hr/employees/[employeeId]          → EmployeeDetailPage with tabs (enhance existing)
/hr/employees/[employeeId]/edit     → EditEmployeePage (new)
```

### 8.2 Detail Tabs

The existing `[employeeId]/page.tsx` gets tab navigation:

| Tab | Route | Source |
|-----|-------|--------|
| Overview | `/hr/employees/{id}?tab=overview` | Enhanced existing detail |
| Employment | `/hr/employees/{id}?tab=employment` | New (dept, position, grade, manager) |
| Compensation | `/hr/employees/{id}?tab=compensation` | New (salary, allowances, contracts) |
| History | `/hr/employees/{id}?tab=history` | Existing EmployeeHistory |
| Timeline | `/hr/employees/{id}?tab=timeline` | Existing EmployeeTimeline |
| Documents | `/hr/employees/{id}?tab=documents` | New (EmployeeDocument list) |
| Assets | `/hr/employees/{id}?tab=assets` | New (EmployeeAsset list) |
| Notes | `/hr/employees/{id}?tab=notes` | New (EmployeeNote list) |

### 8.3 New Services

- `documentService.ts` — CRUD for EmployeeDocument
- `assetService.ts` — CRUD + Return for EmployeeAsset
- `noteService.ts` — Create + List + Archive for EmployeeNote

### 8.4 New Types

- `employeeDocument.ts` — EmployeeDocument, DocumentTypeCode enum
- `employeeAsset.ts` — EmployeeAsset, AssetTypeCode, AssetStatusCode enums
- `employeeNote.ts` — EmployeeNote, NoteCategory enum

### 8.5 New Hooks

- `useEmployeeDocument.ts` — query/mutation hooks
- `useEmployeeAsset.ts` — query/mutation hooks
- `useEmployeeNote.ts` — query/mutation hooks

### 8.6 Permission Gating

Each tab and action button gated by `user.permissions` using the new frontend permission constants. Non-permitted tabs are hidden, not shown as disabled.

---

## 9. Validation Rules

### 9.1 Create Employee

| Field | Rule |
|-------|------|
| EmployeeCode | Required, unique across all employees (not just active) |
| FirstName | Required, max 100 |
| LastName | Required, max 100 |
| WorkEmail | Required, unique, valid email format |
| PersonalEmail | Optional, valid email format |
| PhoneNumber | Required |
| DateOfBirth | Optional, must be past date |
| PrimaryDepartmentId | Required, department must exist and be active |
| PrimaryPositionId | Required, position must exist and be active |
| GradeCode | Optional, grade must exist |
| DirectManagerEmployeeId | Optional, employee must exist and be active |
| JoinDate | Required, must be today or past |
| EmploymentTypeCode | Required, valid code |

### 9.2 Update Employee

Same as Create but fields optional (partial update). `EmployeeCode` not updatable. Status not updatable via Update.

### 9.3 Status Actions

Valid transitions enforced by `EmploymentStatusCode.IsValidTransition()`:
- `Draft` → `PendingOnboarding` → `Onboarding` → `Active` ↔ `Suspended` → `Resigned`|`Terminated` → `Archived`
- Only `Active` can be Suspended; only `Suspended` can be Resumed.
- `Archived` is terminal.

---

## 10. Tests

### 10.1 Unit Tests

- Employee domain method tests (Activate, Suspend, Resume, Archive) — valid/invalid transitions
- EmployeeDocument domain tests
- EmployeeAsset domain tests (assign, return, status transitions)
- EmployeeNote domain tests

### 10.2 Application Tests

- CreateEmployeeCommand handler tests
- UpdateEmployeeCommand handler tests
- Status action handler tests
- Document/Asset/Note handler tests
- Permission enforcement tests (user with/without permission)

### 10.3 History Tests

- CreateEmployee produces EmployeeHistory
- Status changes produce EmployeeHistory + EmployeeTimeline updates
- Document/Asset/Note mutations produce history entries as appropriate

### 10.4 Workflow Tests

- Verify that Terminate endpoint does NOT exist
- Verify SubmitSeparation → Workflow → Employee.Terminate() path still works

### 10.5 Build & Integration

- `dotnet build Anemoi.sln` passes
- `dotnet test` passes
- Frontend build passes (`npm run build` in cody-web-app)
- Frontend lint passes (`npm run lint` in cody-web-app)

---

## 11. Implementation Order

Per the project's 3-step backend workflow + 4-step frontend workflow:

**Backend Step 1:** Domain & Data
1. Add ModelIds (EmployeeDocumentId, EmployeeAssetId, EmployeeNoteId)
2. Update Employee entity (FirstName, LastName, DisplayName, AvatarUrl)
3. Create EmployeeDocument domain entity
4. Create EmployeeAsset domain entity
5. Create EmployeeNote domain entity
6. Add DbSets to HrDbContext
7. Create EF configurations
8. Generate migration
9. Add new permissions to central Permissions.cs

**Backend Step 2:** Application Layer
1. Create EmployeeDocuments CQRS (command, handler, validator, query, mapper, DTOs)
2. Create EmployeeAssets CQRS (same pattern)
3. Create EmployeeNotes CQRS (same pattern)
4. Create CreateEmployeeCommand + handler + validator
5. Create UpdateEmployeeContactCommand + handler + validator
6. Create ChangeEmployeeDepartmentCommand + handler + validator
7. Create ChangeEmployeePositionCommand + handler + validator
8. Create ChangeEmployeeGradeCommand + handler + validator
9. Create ChangeEmployeeManagerCommand + handler + validator
10. Create status action commands (Activate, Suspend, Resume, Archive) + handlers + validators
11. Update EmployeeMapper, EmployeeResponse DTO
12. Add HrPermissions delegates

**Backend Step 3:** API & Communication
1. Create DocumentController
2. Create AssetController
3. Create NoteController
4. Add new endpoints to EmployeeController
5. Register new DbSets and services in DI
6. Build + test pass

**Frontend Step 1:** Types, Services, Hooks
1. Add frontend permission constants
2. Create document.types.ts, asset.types.ts, note.types.ts
3. Create documentService.ts, assetService.ts, noteService.ts
4. Create useEmployeeDocument.ts, useEmployeeAsset.ts, useEmployeeNote.ts

**Frontend Step 2:** Pages & Components
1. CreateEmployeePage
2. EditEmployeePage
3. DocumentTab component
4. AssetTab component
5. NoteTab component
6. Tab navigation for employee detail page
7. Status action buttons on employee detail

**Frontend Step 3:** i18n, Permissions, Polish
1. Add localization keys for new UI text
2. Permission-gate tabs and actions
3. Loading/empty/error states
4. Confirm dialogs for status changes

**Frontend Step 4:** Browser Validation via DevTools MCP

---

## 12. Risks

| Risk | Mitigation |
|------|-----------|
| Migration backfill of FullName → FirstName/LastName may produce duplicate full names | Safer approach: `FirstName = FullName`, `LastName = ""` avoids data duplication while preserving original in DisplayName. HR corrects via Edit UI |
| EmployeeCode uniqueness enforcement at DB level may fail on existing duplicates | Check existing duplicates in migration; add unique index with cleanup |
| Document/Asset/Note aggregates increase DB schema complexity | Each is lightweight; follows established aggregate pattern |
| Frontend tab navigation regression on existing detail page | Existing tabs (History, Timeline) remain unchanged; new tabs added beside them |
| Permission bloat from 8 new permissions | All follow existing naming pattern; no role-name-based checks |
| Separate business commands increase handler count | Each is small, follows the same CQRS template, and produces precise history events |
| Value objects for type codes require EF conversion | Follow existing pattern (e.g., EmploymentStatusCode, TransferStatusCode) — EF value conversion with known static instances |
