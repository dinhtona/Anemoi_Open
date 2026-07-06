# Phase 31 — HR Settings & Master Data Hub Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a centralized HR Settings Hub at `/hr/settings` with Department, Position, SalaryGrade, and WorkflowRoleAssignment management, plus replace raw GUID inputs in Recruitment forms.

**Architecture:** Frontend-only settings shell routing to nested pages, each calling domain-specific APIs. Backend adds missing CRUD CQRS to existing entities. Department/Position audited and potentially migrated from ValueObject to Entity<TId>. xmin optimistic concurrency added to mutable master data.

**Tech Stack:** .NET 10, CQRS + MediatR, EF Core + PostgreSQL, Mapperly, Next.js 14 App Router, React Query, shadcn/ui, next-intl.

**Design doc:** `docs/superpowers/specs/2026-06-20-phase31-hr-settings-master-data-hub-design.md`

---

## File Map

### Backend — Department CRUD

| File | Action |
|------|--------|
| `Domain/Departments/Department.cs` | Audit + refactor if ValueObject |
| `Domain/Departments/DepartmentTypeCode.cs` | Create |
| `Infrastructure/Configurations/EmployeeOrganizationModelMapping.cs` | Modify (add xmin + unique index) |
| `Application/Cqrs/Commands/DepartmentCommands/CreateDepartment/*` | Create |
| `Application/Cqrs/Commands/DepartmentCommands/UpdateDepartment/*` | Create |
| `Application/Cqrs/Commands/DepartmentCommands/ActivateDepartment/*` | Create |
| `Application/Cqrs/Commands/DepartmentCommands/DeactivateDepartment/*` | Create |
| `Application/Cqrs/Queries/DepartmentQueries/GetDepartmentById/*` | Create |
| `Application/Mappings/EmployeeOrganizationMapper.cs` | Modify (verify Department mapping) |
| `Api/Controllers/Department/DepartmentController.cs` | Modify (add endpoints) |
| `Application/Configurations/HrBusinessErrorCodes.cs` | Modify |
| `Application/Configurations/HrPermissions.cs` | Modify |

### Backend — Position CRUD

Same structure as Department under `Domain/Positions/`, `Application/Cqrs/Commands|Queries/PositionCommands|Queries/`, `Api/Controllers/Position/PositionController.cs`.

### Backend — SalaryGrade missing CQRS

| File | Action |
|------|--------|
| `Application/Cqrs/Queries/SalaryGradeQueries/GetSalaryGrades/*` | Create |
| `Application/Cqrs/Queries/SalaryGradeQueries/GetSalaryGradeById/*` | Create |
| `Application/Cqrs/Commands/SalaryGradeCommands/UpdateSalaryGrade/*` | Create |
| `Application/Cqrs/Commands/SalaryGradeCommands/ActivateSalaryGrade/*` | Create |
| `Application/Cqrs/Commands/SalaryGradeCommands/DeactivateSalaryGrade/*` | Create |
| `Api/Controllers/Compensation/CompensationController.cs` | Modify (add endpoints) |
| `Infrastructure/Configurations/CompensationModelMapping.cs` | Verify xmin |

### Backend — WorkflowRoleAssignment CRUD

| File | Action |
|------|--------|
| `Domain/Workflow/WorkflowRoleAssignment.cs` | Modify (add factory, private setters) |
| `Application/Responses/WorkflowRoleAssignmentResponse.cs` | Create |
| `Application/Mappings/WorkflowRoleAssignmentMapper.cs` | Create |
| `Application/Cqrs/Commands/WorkflowCommands/AssignWorkflowRole/*` | Create |
| `Application/Cqrs/Commands/WorkflowCommands/RemoveWorkflowRoleAssignment/*` | Create |
| `Application/Cqrs/Queries/WorkflowQueries/GetWorkflowRoleAssignments/*` | Create |
| `Application/Cqrs/Queries/WorkflowQueries/GetWorkflowRoleAssignmentById/*` | Create |
| `Infrastructure/Configurations/WorkflowModelMapping.cs` | Modify (unique constraint) |
| `Api/Controllers/Workflow/WorkflowRoleAssignmentController.cs` | Create |

### Frontend

| File | Action |
|------|--------|
| `src/constants/hr-settings-registry.ts` | Create |
| `src/constants/permissions.ts` | Modify |
| `src/hooks/hr/useSettings.ts` | Create |
| `src/hooks/hr/useDepartments.ts` | Modify (add mutations) |
| `src/hooks/hr/usePositions.ts` | Modify (add mutations) |
| `src/hooks/hr/useSalaryGrades.ts` | Create |
| `src/hooks/hr/useWorkflowRoleAssignments.ts` | Create |
| `src/services/hr/departmentService.ts` | Modify (add CRUD) |
| `src/services/hr/positionService.ts` | Modify (add CRUD) |
| `src/services/hr/salaryGradeService.ts` | Create |
| `src/services/hr/workflowRoleAssignmentService.ts` | Create |
| `src/types/hr/department.ts` | Modify (add request types) |
| `src/types/hr/position.ts` | Modify (add request types) |
| `src/types/hr/salaryGrade.ts` | Create |
| `src/types/hr/workflowRoleAssignment.ts` | Create |
| `src/components/features/hr/shared/DepartmentSelect.tsx` | Create |
| `src/components/features/hr/shared/PositionSelect.tsx` | Create |
| `src/components/features/hr/shared/EmployeeSelect.tsx` | Create |
| `src/components/features/hr/shared/SalaryGradeSelect.tsx` | Create |
| `src/components/features/hr/shared/WorkflowRoleSelect.tsx` | Create |
| `src/components/features/hr/settings/SettingsCard.tsx` | Create |
| `src/components/features/hr/settings/SettingsCardGrid.tsx` | Create |
| `src/components/features/hr/settings/departments/*.tsx` | Create |
| `src/components/features/hr/settings/positions/*.tsx` | Create |
| `src/components/features/hr/settings/salary-grades/*.tsx` | Create |
| `src/components/features/hr/settings/workflow-role-assignments/*.tsx` | Create |
| `src/app/[locale]/(dashboard)/hr/settings/page.tsx` | Create |
| `src/app/[locale]/(dashboard)/hr/settings/departments/page.tsx` | Create |
| `src/app/[locale]/(dashboard)/hr/settings/positions/page.tsx` | Create |
| `src/app/[locale]/(dashboard)/hr/settings/salary-grades/page.tsx` | Create |
| `src/app/[locale]/(dashboard)/hr/settings/workflow-role-assignments/page.tsx` | Create |
| `src/components/features/hr/recruitment/CreateRecruitmentRequestDialog.tsx` | Modify |
| `src/components/features/hr/recruitment/CreateRequisitionDialog.tsx` | Modify |
| `src/components/shared/Sidebar.tsx` | Modify |
| `messages/en.json` | Modify |
| `messages/vi.json` | Modify |

---

## Tasks

### Task 0: Architecture Audit

**Purpose**: Establish current state of all 4 entities before writing any code.

- [ ] **Step 1: Audit Department**

Read the following files and report:
- `Anemoi.Hr.Domain/Departments/Department.cs` — base class (ValueObject vs Entity), properties, access modifiers
- `Anemoi.Hr.Infrastructure/Configurations/EmployeeOrganizationModelMapping.cs` — xmin, indexes, relationships
- `Anemoi.Hr.Application/Cqrs/Queries/DepartmentQueries/GetDepartments/` — existing query structure
- `Anemoi.Hr.Api/Controllers/Department/DepartmentController.cs` — existing endpoints + route template
- `Anemoi.Hr.Application/Mappings/EmployeeOrganizationMapper.cs` — existing mappings
- `Anemoi.Hr.Application/Responses/DepartmentResponse.cs` — existing response DTO

**Report format**:
```
Department:
  Base class: ValueObject / Entity<DepartmentId>
  Has xmin: yes / no
  Existing endpoints: [...]
  Existing CQRS: [...]
  Migration needed: yes / no
  Risk: low / medium / high
```

- [ ] **Step 2: Audit Position**

Same structure as Department audit.

- [ ] **Step 3: Audit SalaryGrade**

Read:
- `Anemoi.Hr.Domain/Compensation/SalaryGrade.cs`
- `Anemoi.Hr.Application/Cqrs/Commands/CompensationCommands/CreateSalaryGrade/` — existing command pattern
- `Anemoi.Hr.Infrastructure/Configurations/CompensationModelMapping.cs` — xmin check
- `Anemoi.Hr.Api/Controllers/Compensation/CompensationController.cs` — existing endpoints

- [ ] **Step 4: Audit WorkflowRoleAssignment**

Read:
- `Anemoi.Hr.Domain/Workflow/WorkflowRoleAssignment.cs` — entity structure
- `Anemoi.Hr.Domain/Workflow/WorkflowRole.cs` — role constants
- `Anemoi.Hr.Application/Services/DefaultWorkflowRoleResolver.cs` — how it's consumed
- `Anemoi.Hr.Infrastructure/Configurations/WorkflowModelMapping.cs` — EF config

- [ ] **Step 5: Check existing permissions**

Run: `grep -n "WorkflowView\|WorkflowManage\|workflow.view\|workflow.manage" Anemoi.Hr.Application/Configurations/HrPermissions.cs`
Purpose: Confirm `hr.workflow.view` / `hr.workflow.manage` exist before deciding to reuse or add.

- [ ] **Step 6: Compile audit report**

Summarize: what needs Create, what needs Modify, what needs no changes. This report determines whether each subsequent task proceeds or requires a decision gate.

### Decision Gate A: Department/Position Migration

**Before proceeding to any Department/Position CRUD code:**

```
IF Department or Position is currently "ValueObject":
  -> Migration impact is HIGH: changes EF mapping, queries, relationships, seed data, tests.
  -> STOP. Report impact and get explicit approval before migrating.
  -> If approved: convert to Entity<TId>, add xmin, run full build + test.

IF Department or Position is already "Entity<TId>":
  -> Low risk. Add missing factory methods + behavior methods only.
  -> Proceed with Task 2+.
```

- [ ] **Step 1: Check Department base class**

Run: `grep -n "class Department :" Anemoi.Hr.Domain/Departments/Department.cs`

- [ ] **Step 2: Check Position base class**

Run: `grep -n "class Position :" Anemoi.Hr.Domain/Positions/Position.cs`

- [ ] **Step 3: Report migration decision**

Document: "Decision: [migrate / no migration needed]. Impact: [description]."

---

### Task 1: TypeCode constants + Domain preparation

Implementation guidance — follow existing project patterns for constants and domain entities.

- [ ] **Step 1: Create DepartmentTypeCode constants**

Create `Anemoi.Hr.Domain/Departments/DepartmentTypeCode.cs`.

**Implementation guidance**:
- Follow the same pattern as existing constants in the Domain layer (e.g., `WorkflowRole` constants).
- Include: `Functional = "functional"`, an `All` array, and an `IsValid(string)` method.
- **Before finalizing**: grep existing seed data and DB migration files for any `DepartmentTypeCode` values beyond "functional". If found, add them to `All`.

- [ ] **Step 2: Create PositionTypeCode constants**

Create `Anemoi.Hr.Domain/Positions/PositionTypeCode.cs`.

**Implementation guidance**:
- Follow the same pattern as `DepartmentTypeCode`.
- Include all values found in `HrDevSeedData.cs`: `"manager"`, `"individual_contributor"`, `"administrator"`.
- Add `IsValid()` for validation.

- [ ] **Step 3: Add domain behavior methods to Department (if Entity<TId>)**

If Department is already `Entity<DepartmentId>`:
- Add `Create(...)` static factory (parameters: Id, Code, Name, DepartmentTypeCode, ParentDepartmentId?, ManagerEmployeeId?)
- Add `UpdateInfo(...)` to mutate code/name/type/parent/manager
- Add `Activate()` / `Deactivate()` methods
- Ensure all setters are `private set`

If Department is still `ValueObject`: **do not proceed without passing Decision Gate A first**.

**Implementation guidance**: Follow the same domain method pattern as entities like `LeaveBalance`, `OvertimeRequest`, or `RecruitmentRequest` — whichever is closest in the existing codebase.

- [ ] **Step 4: Add domain behavior methods to Position (same pattern)**

- [ ] **Step 5: Add xmin to Department + Position EF config**

Modify `EmployeeOrganizationModelMapping.cs`:
- Add `builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();` inside both Department and Position configurations.
- Add `builder.HasIndex(x => x.Code).IsUnique();` for both.

**Implementation guidance**: Follow the exact xmin pattern already used in `LeaveBalanceModelMapping.cs` or `OvertimeRequestModelMapping.cs`.

- [ ] **Step 6: Add unique constraint to WorkflowRoleAssignment**

Modify `WorkflowModelMapping.cs`:
- Add `builder.HasIndex(x => new { x.Role, x.EmployeeId }).IsUnique();`

- [ ] **Step 7: Build to verify compilation**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Domain/Anemoi.Hr.Domain.csproj`
Expected: 0 errors

---

### Task 2: Department CRUD — Commands + Queries

Create CQRS following existing patterns in the codebase.

- [ ] **Step 1: Add error codes**

In `HrBusinessErrorCodes.cs`:
```csharp
DepartmentCodeExists
DepartmentHasChildren
DepartmentHasActiveEmployees
DepartmentHasActivePositions
ValDepartmentTypeCodeInvalid
```

Follow the existing naming convention (`HR_DEPARTMENT_CODE_EXISTS`, etc.).

- [ ] **Step 2: Add permission constant**

In `HrPermissions.cs`:
```csharp
DepartmentManage = "hr.department.manage"
```
Follow the same format as `DepartmentView` (which already exists).

- [ ] **Step 3: Create Department commands**

**Implementation guidance**: Follow the same structure as the existing `CreateCompensationCommand`, `UpdateCompensationCommand`, or any similar CRUD command in `Anemoi.Hr.Application/Cqrs/Commands/`.

Create commands at `Application/Cqrs/Commands/DepartmentCommands/`:

| Command | Key Fields | Notes |
|---------|-----------|-------|
| `CreateDepartmentCommand` | Code, Name, DepartmentTypeCode, ParentDepartmentId?, ManagerEmployeeId? | Handler: validate TypeCode via `DepartmentTypeCode.IsValid()`, check Code unique, `Department.Create()`, `CreateOneAsync`, `SaveChangesAsync` |
| `UpdateDepartmentCommand` | Id, Code, Name, DepartmentTypeCode, ParentDepartmentId?, ManagerEmployeeId?, **RowVersion** | Handler: fetch tracked entity, `UpdateInfo()`, save. xmin conflict → concurrency error. |
| `ActivateDepartmentCommand` | Id | Handler: fetch, `Activate()`, save |
| `DeactivateDepartmentCommand` | Id | Handler: fetch, guard (no active children/employees/positions), `Deactivate()`, save |

For each command, create:
- `{Name}.cs` — record
- `{Name}Handler.cs` — ICommandHandler
- `{Name}Validator.cs` — FluentValidation (optional for simple toggle commands)

**Validator guidance**: Use `HrBusinessErrorCodes` constants in `.WithMessage()`, not English strings. Follow the same validator pattern as existing commands in the codebase.

- [ ] **Step 4: Create Department queries**

| Query | Notes |
|-------|-------|
| `GetDepartmentByIdQuery(DepartmentId)` | Handler: `GetFirstByConditionAsync`, return oneOf `DepartmentResponse` or `ErrorDetailResponse` (NOT_FOUND) |

**Implementation guidance**: Follow the same pattern as `GetEmployeeQuery` or similar existing GetById query.

- [ ] **Step 5: Add mapper method**

In `EmployeeOrganizationMapper.cs` (or the existing mapper for Department):
- Add `partial DepartmentResponse DepartmentToResponse(Department department)` if not already mapped.
- Follow Mapperly pattern: `[Mapper]` class, `partial` method.

- [ ] **Step 6: Build to verify**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/Anemoi.Hr.Application.csproj`
Expected: 0 errors

---

### Task 3: DepartmentController endpoints

- [ ] **Step 1: Read existing DepartmentController**

Read `DepartmentController.cs` to understand the current route convention, authentication attribute, and OneOf result handling pattern.

- [ ] **Step 2: Add endpoints following REST convention**

**Route convention**: Use `/api/hr/departments` (not `[controller]/[action]`) unless the majority of existing HR controllers use `[action]`.

**Endpoints to add**:
```
GET    /api/hr/departments                           (exists)
GET    /api/hr/departments/{id}                       GetDepartmentById
POST   /api/hr/departments                            CreateDepartment
PUT    /api/hr/departments/{id}                        UpdateDepartment
POST   /api/hr/departments/{id}/activate               ActivateDepartment
POST   /api/hr/departments/{id}/deactivate             DeactivateDepartment
```

**Implementation guidance**:
- Follow the exact `ActionResult` return pattern from existing controllers (e.g., `Ok(result.Value)`, `Problem(...)`, or `BadRequest(...)`)
- Use `[HasPermission(HrPermissions.DepartmentView)]` for GET, `[HasPermission(HrPermissions.DepartmentManage)]` for POST/PUT
- Each endpoint: send command/query via `ISender`, return `result.Match(Ok, Problem)`

- [ ] **Step 3: Build to verify**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Api/Anemoi.Hr.Api.csproj`
Expected: 0 errors

---

### Task 4: Position CRUD (same pattern as Department)

Follow the exact same steps as Tasks 2-3 but for Position.

- [ ] **Step 1: Add error codes + permission**

Error codes: `PositionCodeExists`, `PositionHasActiveEmployees`, `ValPositionTypeCodeInvalid`.
Permission: `PositionManage = "hr.position.manage"`.

- [ ] **Step 2: Create Position CQRS**

Commands: `CreatePositionCommand(Code, Name, PositionTypeCode, DepartmentId)`, `UpdatePositionCommand(...)`, `ActivatePositionCommand`, `DeactivatePositionCommand`.
Queries: `GetPositionByIdQuery(PositionId)`.

**Deactivate guard**: Block if any active Employee has this PositionId.

- [ ] **Step 3: Add PositionController endpoints**

Same REST pattern as Department. Route: `/api/hr/positions`.

- [ ] **Step 4: Build to verify**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Api/Anemoi.Hr.Api.csproj`
Expected: 0 errors

---

### Task 5: SalaryGrade missing CQRS

- [ ] **Step 1: Verify xmin on SalaryGrade**

Run: `grep -n "xmin" Anemoi.Hr.Infrastructure/Configurations/CompensationModelMapping.cs`
If missing, add xmin shadow property following existing pattern.

- [ ] **Step 2: Create SalaryGrade queries**

| Query | Notes |
|-------|-------|
| `GetSalaryGradesQuery(string? SearchKey, bool? IsActive)` | Follow same paginated pattern as `GetDepartmentsQuery`. Filter by GradeCode/Name + IsActive. |
| `GetSalaryGradeByIdQuery(SalaryGradeId)` | Standard get-by-id pattern. |

- [ ] **Step 3: Create SalaryGrade commands**

| Command | Notes |
|---------|-------|
| `UpdateSalaryGradeCommand(Id, GradeCode, Name, Description, RowVersion)` | Fetch tracked entity, mutate, save. xmin conflict → concurrency error. GradeCode unique check. |
| `ActivateSalaryGradeCommand(Id)` | Simple toggle |
| `DeactivateSalaryGradeCommand(Id)` | Simple toggle. No deactivate guard — old data still references GradeCode string. Only new forms hide inactive grades. |

- [ ] **Step 4: Add controller endpoints**

Add to `CompensationController` or create a separate controller at `/api/hr/salary-grades`.

Endpoints:
```
GET    /api/hr/salary-grades
GET    /api/hr/salary-grades/{id}
POST   /api/hr/salary-grades              (exists — verify)
PUT    /api/hr/salary-grades/{id}
POST   /api/hr/salary-grades/{id}/activate
POST   /api/hr/salary-grades/{id}/deactivate
```

- [ ] **Step 5: Build to verify**

Run: `dotnet build Anemoi.Hr/Anemoi.Hr.Api/Anemoi.Hr.Api.csproj`
Expected: 0 errors

---

### Task 6: WorkflowRoleAssignment CRUD

- [ ] **Step 1: Add factory method + private setters to entity**

Modify `WorkflowRoleAssignment.cs`:
- Add `private WorkflowRoleAssignment() { }` for EF Core
- Change all setters to `private set`
- Add `static Create(WorkflowRoleAssignmentId id, string role, EmployeeId employeeId)` factory

- [ ] **Step 2: Create response DTO**

`WorkflowRoleAssignmentResponse(Id, Role, EmployeeId, EmployeeName, EmployeeCode, DepartmentName, PositionName, CreatedAt)`

- [ ] **Step 3: Create Mapperly mapper**

`WorkflowRoleAssignmentMapper.cs` — `partial ToResponse(WorkflowRoleAssignment)`.

- [ ] **Step 4: Create commands**

| Command | Notes |
|---------|-------|
| `AssignWorkflowRoleCommand(Role, EmployeeId)` | Validate role from constants (`WorkflowRole.HrManager`), check employee exists, check no duplicate (Role+EmployeeId). Create + save. |
| `RemoveWorkflowRoleAssignmentCommand(WorkflowRoleAssignmentId)` | Fetch, `RemoveOneAsync`, save. Return simple success or not-found. |

- [ ] **Step 5: Create queries**

- `GetWorkflowRoleAssignmentsQuery(Role?, EmployeeId?, paginated)`
- `GetWorkflowRoleAssignmentByIdQuery(Id)`

**⚠️ Query performance guidance**:
- **Avoid N+1**: Do NOT use `.Include(x => x.Employee).ThenInclude(x => x.Department)` which loads entire aggregate graphs.
- **Use projection**: Write a LINQ query that projects directly to `WorkflowRoleAssignmentResponse`:
  ```csharp
  _queryService.GetQueryable<WorkflowRoleAssignment>()
      .Where(...)
      .Select(x => new WorkflowRoleAssignmentResponse(
          x.Id, x.Role, x.EmployeeId,
          x.Employee.FullName, x.Employee.EmployeeCode,
          x.Employee.PrimaryDepartment.Name,
          x.Employee.PrimaryPosition.Name,
          x.CreatedAt
      ))
  ```
- Follow the existing projection query pattern used by other read-heavy handlers in the codebase.

- [ ] **Step 6: Add error codes**

```csharp
WorkflowRoleAssignmentNotFound
WorkflowRoleAssignmentDuplicate
ValWorkflowRoleInvalid
```

- [ ] **Step 7: Create controller**

**Implementation guidance**: Follow the exact same controller structure as `WorkflowDefinitionsController` (same area, same route prefix `/api/hr/workflow/` or direct `/api/hr/workflow-role-assignments`).

Permissions: `hr.workflow.view` for GET, `hr.workflow.manage` for POST/DELETE.

- [ ] **Step 8: Build + test**

Run: `dotnet build Anemoi.sln && dotnet test`
Expected: 0 errors, all tests pass

---

### Task 7: Frontend — Shared Lookup Components

**Implementation guidance**: Follow existing React component patterns in `cody-web-app/src/components/features/hr/`. Use the same import conventions, Radix UI Select wrapper, and Tailwind classes.

- [ ] **Step 1: Create DepartmentSelect**

`src/components/features/hr/shared/DepartmentSelect.tsx`
- Props: `value?: string`, `onValueChange: (v: string) => void`, `disabled?: boolean`, `placeholder?: string`
- Uses `useDepartments({ isActive: true })` to load active departments
- Renders existing `Select` UI component (Radix wrapper)
- Shows `{code} - {name}` in dropdown items
- Loading state: disabled while loading
- No `any` types

- [ ] **Step 2: Create PositionSelect**

Same pattern as DepartmentSelect.
- Additional optional prop: `departmentId?: string` — when set, filters positions by department
- Uses `usePositions` with optional departmentId filter

- [ ] **Step 3: Create EmployeeSelect**

Reuse the existing `EmployeeLookup` component at `src/components/features/hr/employee/EmployeeLookup.tsx`. Wrap it as a Select-like interface:
- Dialog-based search with debounced input
- When employee selected, show name + code in trigger
- Props: `value`, `onValueChange`, `disabled`, `placeholder`

- [ ] **Step 4: Create SalaryGradeSelect**

Same pattern as DepartmentSelect.
- Loads active salary grades only
- Shows `{gradeCode} - {name}`

- [ ] **Step 5: Create WorkflowRoleSelect**

Static dropdown component.
- Options sourced from a constant array: `["HrManager"]`
- Props: `value`, `onValueChange`, `disabled`, `placeholder`

---

### Task 8: Frontend — Settings Hub Shell

- [ ] **Step 1: Create settings registry**

`src/constants/hr-settings-registry.ts`:

```typescript
export type HrSettingCategoryKey = "organization" | "workforce" | "payroll" | "time" | "recruitment" | "workflow";
export interface HrSettingCategory { key: HrSettingCategoryKey; order: number; icon: LucideIcon; }
export interface HrSettingItem { key: string; category: HrSettingCategoryKey; titleKey: string; descriptionKey: string; permission?: string; route?: string; comingSoon?: boolean; tags: string[]; }
```

Export `settingsCategories: HrSettingCategory[]` (6 categories with order + icon) and `settingsRegistry: HrSettingItem[]` (all items including comingSoon).

**Implementation guidance**: Separate data from rendering. Registry is pure data — no imports of React components. Category icons are `LucideIcon` type references.

- [ ] **Step 2: Create useSettings hook**

`src/hooks/hr/useSettings.ts`:
- Import registry + categories
- Accept `search?: string`, `category?: HrSettingCategoryKey`
- Filter by permission: `canAccessRoute(item.route)`
- Filter by search: match `titleKey`, `descriptionKey`, `tags`
- Filter by category
- Return filtered items + categories

- [ ] **Step 3: Create SettingsCard component**

`src/components/features/hr/settings/SettingsCard.tsx`:
- Props: `item: HrSettingItem` (with translated title/description resolved)
- If `comingSoon`: render disabled card with "Coming Soon" badge, no link
- If `route` + has permission: render clickable card as `Link`
- Use `Card` shadcn component, consistent with existing UI

- [ ] **Step 4: Create SettingsCardGrid component**

`src/components/features/hr/settings/SettingsCardGrid.tsx`:
- Props: `items: HrSettingItem[]`, `activeCategory: HrSettingCategoryKey`
- Renders grid of SettingsCard
- Empty state when no items match

- [ ] **Step 5: Create hub page**

`src/app/[locale]/(dashboard)/hr/settings/page.tsx`:
- Search bar (debounced, updates `?q=` URL param)
- Category tabs with icons (horizontal, clickable, updates `?category=` URL param)
- Card grid
- All translations from `hr.settings.*` namespace
- Responsive layout

- [ ] **Step 6: Verify build**

Run: `cd cody-web-app && npm run build`
Expected: 0 TypeScript errors

---

### Task 9: Frontend — Settings Pages (Department, Position, SalaryGrade, WorkflowRole)

- [ ] **Step 1: Create types + services + hooks**

For each of the 4 entities:

**Types** (`src/types/hr/`):
- Add create/update request interfaces (e.g., `CreateDepartmentRequest`, `UpdateDepartmentRequest`)
- Add response interfaces (reuse existing or extend)

**Services** (`src/services/hr/`):
- Add API call functions: `get{Entity}s(params)`, `get{Entity}ById(id)`, `create{Entity}(data)`, `update{Entity}(id, data)`, `activate{Entity}(id)`, `deactivate{Entity}(id)`

**Hooks** (`src/hooks/hr/`):
- Add `use{Entity}List(params)` — useQuery
- Add `useCreate{Entity}()` — useMutation + invalidate list
- Add `useUpdate{Entity}()` — useMutation + invalidate list
- Add `useActivate{Entity}()` / `useDeactivate{Entity}()` — useMutation + invalidate list

**Implementation guidance**: Follow exactly the same React Query pattern used by existing hooks like `useDepartments()`, `usePositions()`, etc. Use the same query key naming convention, stale time, and invalidation pattern.

- [ ] **Step 2: Create page + table + dialogs for Departments**

**Page**: `/hr/settings/departments/page.tsx`
- Renders PageHeader + DepartmentTable

**Table**: `DepartmentTable.tsx`
- Columns: Code, Name, Type, Parent Dept, Manager, Status (Active/Inactive badge), Actions
- Search input filters list
- Create button → CreateDepartmentDialog
- Row actions: Edit, Activate/Deactivate buttons
- Loading skeleton state
- Empty state

**CreateDialog**: `CreateDepartmentDialog.tsx`
- Form: Code, Name, DepartmentTypeSelect, DepartmentSelect (parent), EmployeeSelect (manager)
- Validation: required fields, TypeCode validity
- Submit → createMutation → on success → dialog close + table refresh
- Error handling via `translateApiError`

**EditDialog**: `EditDepartmentDialog.tsx`
- Same form + hidden RowVersion field
- Submit → updateMutation

- [ ] **Step 3: Create pages for Positions, SalaryGrades, WorkflowRoleAssignments**

Same pattern as Departments, with entity-specific columns and form fields:

**Positions**: Code, Name, Type, Department (DepartmentSelect), Status
**SalaryGrades**: GradeCode, Name, Description, Status (no ranges editing in this phase)
**WorkflowRoleAssignments**: Role, Employee Name, Code, Department, Created At (read-only table + create dialog)

- [ ] **Step 4: Verify build**

Run: `cd cody-web-app && npm run build`
Expected: 0 TypeScript errors

---

### Task 10: Frontend — Recruitment DepartmentId/PositionId fix

- [ ] **Step 1: Read CreateRecruitmentRequestDialog**

Read `CreateRecruitmentRequestDialog.tsx` to understand current form structure.

- [ ] **Step 2: Replace DepartmentId input**

```tsx
// Remove:
<Input value={departmentId} onChange={...} placeholder="Department ID" />

// Add:
<DepartmentSelect value={departmentId} onValueChange={setDepartmentId} />
```

Keep the same `departmentId` state variable (string). The Select returns the ID string, same as what the text input produced. API contract unchanged.

- [ ] **Step 3: Replace PositionId input**

```tsx
<PositionSelect value={positionId} onValueChange={setPositionId}
  departmentId={departmentId || undefined} />
```

- [ ] **Step 4: Same fix for CreateRequisitionDialog**

Apply the same two replacements.

- [ ] **Step 5: Verify build**

Run: `cd cody-web-app && npm run build`
Expected: 0 TypeScript errors

---

### Task 11: Frontend — Sidebar, Permissions, Translation

- [ ] **Step 1: Add permission constants**

In `src/constants/permissions.ts`:
```typescript
HR_DEPARTMENT_MANAGE = "hr.department.manage",
HR_POSITION_MANAGE = "hr.position.manage",
```

- [ ] **Step 2: Add route permissions**

In `ROUTE_PERMISSIONS`:
```typescript
"/hr/settings": [HR_DEPARTMENT_VIEW, HR_POSITION_VIEW, HR_SALARY_GRADE_VIEW, HR_WORKFLOW_VIEW],
"/hr/settings/departments": [HR_DEPARTMENT_VIEW],
"/hr/settings/positions": [HR_POSITION_VIEW],
"/hr/settings/salary-grades": [HR_SALARY_GRADE_VIEW],
"/hr/settings/workflow-role-assignments": [HR_WORKFLOW_VIEW],
```

- [ ] **Step 3: Add sidebar entry**

In `Sidebar.tsx`, add to `hrWorkspace` group (last item):
```typescript
{ href: "/hr/settings", label: "hrSettings", icon: Settings },
```

- [ ] **Step 4: Add translation keys**

In both `en.json` and `vi.json`:

**Sidebar** (under `Dashboard`):
```json
"hrSettings": "HR Settings"
```

**Hub page** (namespace `hr.settings`):
```json
"hr": {
  "settings": {
    "title": "...",
    "description": "...",
    "searchPlaceholder": "...",
    "noResults": "...",
    "categories": { "organization": "...", ... },
    "items": { "departments": { "title": "...", "description": "..." }, ... },
    "comingSoon": "..."
  }
}
```

**Per-page namespaces** (`hr.departments`, `hr.positions`, `hr.salaryGrades`, `hr.workflowRoleAssignments`):
```json
"hr": {
  "departments": {
    "title": "Departments",
    "description": "Manage organizational departments",
    "fields": { "code": "Code", "name": "Name", "type": "Type" },
    "actions": { "create": "Create Department", "edit": "Edit", "activate": "Activate", "deactivate": "Deactivate" },
    "validation": { "codeRequired": "Code is required" },
    "empty": { "title": "No departments", "description": "Create your first department" }
  }
}
```

- [ ] **Step 5: Verify build + lint**

Run:
```bash
cd cody-web-app
npm run build
npm run lint
```
Expected: 0 TypeScript errors, 0 new lint errors

---

### Task 12: Backend build + test verification

- [ ] **Step 1: Build entire solution**

Run: `dotnet build Anemoi.sln`
Expected: 0 errors. Document any pre-existing warnings.

- [ ] **Step 2: Run all tests**

Run: `dotnet test`
Expected: All tests pass. Document any pre-existing failures.

---

### Task 13: Browser E2E verification

Follow the checklist from the design doc. Use the available MCP browser-rendering tool.

- [ ] **Step 1: Navigate `/en/hr/settings`**
  - Page loads without console errors — PASS/FAIL

- [ ] **Step 2: Verify settings cards render**
  - Cards with correct translations appear — PASS/FAIL

- [ ] **Step 3: Search "department"**
  - Filters correctly — PASS/FAIL

- [ ] **Step 4: Switch category "Organization"**
  - Only org items shown — PASS/FAIL

- [ ] **Step 5: Open Departments panel**
  - List renders, create department dialog works — PASS/FAIL

- [ ] **Step 6: Open Positions panel**
  - Position form uses DepartmentSelect, not raw text input — PASS/FAIL

- [ ] **Step 7: Open Recruitment Request create dialog**
  - Department field is Select, Position field is Select — PASS/FAIL

- [ ] **Step 8: Vietnamese route `/vi/hr/settings`**
  - Translations render correctly — PASS/FAIL

- [ ] **Step 9: No hydration errors, no network 404s caused by new routes**
  — PASS/FAIL
