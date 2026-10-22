# Phase 31 — HR Settings & Master Data Hub Design

Status: Draft
Date: 2026-06-20

---

## 1. Overview

Build a centralized **HR Settings Hub** at `/hr/settings` as a unified discovery and management entry point for all HR master data configuration. Backend master data entities (Department, Position, SalaryGrade, WorkflowRoleAssignment) get CRUD CQRS where missing. Recruitment Request forms using raw GUID inputs are replaced with proper select lookups.

### Guiding Principles

- Settings Hub is a frontend-only shell — no backend "SettingsModule" created.
- Each settings panel calls the existing domain-specific API.
- No raw GUID text inputs in user-facing forms.
- All mutations go through CQRS commands.
- Permission-driven UI at card, route, and action level.
- Strongly typed IDs at backend, `IdGenerator.NextGuid()` for new IDs.
- xmin optimistic concurrency for mutable master data.

---

## 2. Architecture Decisions

### AD-31-001: Frontend Settings Hub

**Decision**: Hybrid routing — `/hr/settings` hub page with search + category filter + card grid, plus nested routes for major settings items.

**Routes**:
```
/hr/settings                              → Hub (search, categories, cards)
/hr/settings/departments                  → Department CRUD page
/hr/settings/positions                    → Position CRUD page
/hr/settings/salary-grades               → SalaryGrade CRUD page
/hr/settings/workflow-role-assignments    → Workflow Role Assignment CRUD page
```

**URL params**: `?q=department&category=organization` — bookmarkable.

### AD-31-002: Department/Position as Entity<TId>

**Decision**: If currently `ValueObject`, convert to `Entity<TId>`. If already `Entity<TId>`, preserve current architecture.

**⚠️ Implementation note — Audit before refactoring**: Verify actual base class of `Department` and `Position` before any migration. If they are already `Entity<DepartmentId>` / `Entity<PositionId>`, do NOT change them — simply add missing behavior methods. Only migrate from `ValueObject` to `Entity<TId>` if confirmed as `ValueObject`.

**Update pattern**: Fetch tracked entity → domain behavior method → `SaveChangesAsync()`. No `ISqlRepository.Update()` needed because EF Core tracks changes.

### AD-31-003: SalaryGrade reuse

**Decision**: No new `JobGrade` entity. Use existing `SalaryGrade` entity. Add missing queries (list, get-by-id), update command, activate/deactivate commands. `GradeCode` string stays on `Employee` as a reference.

### AD-31-004: WorkflowRoleAssignment Minimal CRUD

**Decision**: No update command (delete+recreate to change). Unique constraint on `(Role, EmployeeId)`. `DELETE` HTTP verb for removal.

---

## 3. Settings Hub Registry

```typescript
export type HrSettingCategoryKey =
  | "organization" | "workforce" | "payroll"
  | "time" | "recruitment" | "workflow";

export interface HrSettingCategory {
  key: HrSettingCategoryKey;
  order: number;
  icon: LucideIcon;
}

export interface HrSettingItem {
  key: string;
  category: HrSettingCategoryKey;
  titleKey: string;
  descriptionKey: string;
  permission?: string;
  route?: string;
  comingSoon?: boolean;
  tags: string[];
}

const settingsCategories: HrSettingCategory[] = [
  { key: "organization", order: 1, icon: Building2 },
  { key: "workforce",    order: 2, icon: Users },
  { key: "payroll",      order: 3, icon: Coins },
  { key: "time",         order: 4, icon: Clock },
  { key: "recruitment",  order: 5, icon: Users },
  { key: "workflow",     order: 6, icon: CheckCheck },
];

const settingsRegistry: HrSettingItem[] = [
  // Organization
  { key: "departments",        category: "organization", permission: "hr.department.view", route: "/hr/settings/departments",        tags: ["department", "phòng ban", "org"] },
  { key: "positions",          category: "organization", permission: "hr.position.view",   route: "/hr/settings/positions",          tags: ["position", "vị trí", "job"] },
  { key: "reporting-lines",    category: "organization", comingSoon: true,                 tags: ["reporting", "hierarchy", "quản lý"] },

  // Payroll & Compensation
  { key: "salary-grades",      category: "payroll",      permission: "hr.salary-grade.view", route: "/hr/settings/salary-grades",    tags: ["grade", "level", "ngạch lương"] },
  { key: "allowance-types",    category: "payroll",      comingSoon: true,                   tags: ["allowance", "phụ cấp"] },
  { key: "tax-rules",          category: "payroll",      comingSoon: true,                   tags: ["tax", "thuế"] },
  { key: "insurance-rules",    category: "payroll",      comingSoon: true,                   tags: ["insurance", "bảo hiểm"] },

  // Time & Attendance
  { key: "leave-types",        category: "time",         comingSoon: true,                   tags: ["leave", "nghỉ phép"] },
  { key: "leave-policies",     category: "time",         comingSoon: true,                   tags: ["leave", "policy", "chính sách"] },
  { key: "holidays",           category: "time",         comingSoon: true,                   tags: ["holiday", "lễ"] },
  { key: "overtime-rules",     category: "time",         comingSoon: true,                   tags: ["overtime", "tăng ca"] },

  // Recruitment & Onboarding
  { key: "onboarding-templates", category: "recruitment", comingSoon: true,                  tags: ["onboarding", "template"] },

  // Workflow
  { key: "workflow-role-assignments", category: "workflow", permission: "hr.workflow.manage", route: "/hr/settings/workflow-role-assignments", tags: ["workflow", "role", "approver", "phân quyền"] },
  { key: "workflow-definitions", category: "workflow",    comingSoon: true,                  tags: ["workflow", "definition", "luồng phê duyệt"] },
];
```

---

## 4. Backend Changes

### 4.1 Department CRUD

- **Entity**: Convert to `Entity<DepartmentId>`. Add domain factory + behavior methods.
- **TypeCode constants**: `DepartmentTypeCode.cs` — `Functional = "functional"` + `IsValid()`.
- **xmin**: Shadow property in EF config. Unique index on `Code`.
- **Update token**: Frontend sends `RowVersion` (uint) in update command.
- **Deactivate guard**: Block if active children/employees/positions exist.

**Commands**:
| Command | Handler Logic |
|---------|--------------|
| `CreateDepartmentCommand(Code, Name, DepartmentTypeCode, ParentDepartmentId?, ManagerEmployeeId?)` | Validate TypeCode, check Code unique, `Department.Create(...)`, save |
| `UpdateDepartmentCommand(Id, Code, Name, DepartmentTypeCode, ParentDepartmentId?, ManagerEmployeeId?, RowVersion)` | Fetch tracked entity, `department.UpdateInfo(...)`, save — xmin conflict = concurrency error |
| `ActivateDepartmentCommand(Id)` | Fetch, `department.Activate()`, save |
| `DeactivateDepartmentCommand(Id)` | Fetch, guard active children/employees/positions, `department.Deactivate()`, save |

**Queries**: GetDepartments (exists), GetDepartmentById (new).

**API**:
```
GET    /api/hr/departments
GET    /api/hr/departments/{id}
POST   /api/hr/departments
PUT    /api/hr/departments/{id}
POST   /api/hr/departments/{id}/activate
POST   /api/hr/departments/{id}/deactivate
```

**Permissions**: `hr.department.view` (exists), `hr.department.manage` (new).

**Error codes**: `HR_DEPARTMENT_NOT_FOUND` (exists), `HR_DEPARTMENT_CODE_EXISTS`, `HR_DEPARTMENT_HAS_CHILDREN`, `HR_DEPARTMENT_HAS_ACTIVE_EMPLOYEES`, `HR_DEPARTMENT_HAS_ACTIVE_POSITIONS`, `VAL_DEPARTMENT_DEPARTMENT_TYPE_CODE_INVALID`.

### 4.2 Position CRUD

Same pattern as Department. Differences:

- `DepartmentId` is required. Deactivate blocked if active employees exist.
- TypeCode constants: `PositionTypeCode.cs` — `Manager`, `IndividualContributor`, `Administrator` + `IsValid()`.
- Code is **global unique**.
- No hard DELETE — use `POST /{id}/deactivate`.

**API**:
```
GET    /api/hr/positions
GET    /api/hr/positions/{id}
POST   /api/hr/positions
PUT    /api/hr/positions/{id}
POST   /api/hr/positions/{id}/activate
POST   /api/hr/positions/{id}/deactivate
```

**Permissions**: `hr.position.view` (exists), `hr.position.manage` (new).

### 4.3 SalaryGrade — Missing CQRS

Already `Entity<SalaryGradeId>` with Create command. Add:

- `GetSalaryGradesQuery` — search, is-active filter, paginated
- `GetSalaryGradeByIdQuery`
- `UpdateSalaryGradeCommand` — GradeCode, Name, Description, RowVersion
- `ActivateSalaryGradeCommand` / `DeactivateSalaryGradeCommand`
- No deactivate guard, but warning logged if in use (optional). Employee/Recruitment keep old GradeCode string. New forms show only active grades.

**API**:
```
GET    /api/hr/salary-grades
GET    /api/hr/salary-grades/{id}
POST   /api/hr/salary-grades (exists)
PUT    /api/hr/salary-grades/{id}
POST   /api/hr/salary-grades/{id}/activate
POST   /api/hr/salary-grades/{id}/deactivate
```

**Permissions**: `hr.salary-grade.view` (exists), `hr.salary-grade.manage` (exists).

### 4.4 WorkflowRoleAssignment CRUD

- Entity exists: `WorkflowRoleAssignment : Entity<WorkflowRoleAssignmentId>`.
- Add unique constraint on `(Role, EmployeeId)`.
- Role from constants: `WorkflowRole.HrManager = "HrManager"`.
- No update command.

**Commands**:
| Command | Notes |
|---------|-------|
| `AssignWorkflowRoleCommand(Role, EmployeeId)` | Validate role from constants, check employee exists, check no duplicate |
| `RemoveWorkflowRoleAssignmentCommand(Id)` | Hard DELETE, simple removal |

**Queries**: `GetWorkflowRoleAssignmentsQuery` (paginated, with employee name/code/department/position), `GetWorkflowRoleAssignmentByIdQuery`.

**Response DTO**:
```csharp
WorkflowRoleAssignmentResponse(Id, Role, EmployeeId, EmployeeName, EmployeeCode, DepartmentName, PositionName, CreatedAt)
```

**API**:
```
GET    /api/hr/workflow-role-assignments
GET    /api/hr/workflow-role-assignments/{id}
POST   /api/hr/workflow-role-assignments
DELETE /api/hr/workflow-role-assignments/{id}
```

**Permissions**: `hr.workflow.view` / `hr.workflow.manage` (reuse existing Workflow permissions).

---

## 5. Frontend Changes

### 5.1 Settings Hub Page

New files:

| File | Role |
|------|------|
| `src/app/[locale]/(dashboard)/hr/settings/page.tsx` | Hub page |
| `src/hooks/hr/useSettings.ts` | Registry + search/filter logic |
| `src/components/features/hr/settings/SettingsCard.tsx` | Individual card |
| `src/components/features/hr/settings/SettingsCardGrid.tsx` | Card grid with filtering |
| `src/constants/hr-settings-registry.ts` | Registry data |

Hub page flow:
1. Load registry → filter by permissions → render cards
2. Search filters cards client-side by titleKey, descriptionKey, tags
3. Category tabs filter by `category`
4. URL params `?q=&category=` maintained via `useSearchParams`
5. Empty state when no results match search
6. "Coming soon" cards: disabled style, no link, no runtime error

### 5.2 Settings Pages (nested routes)

Each page follows pattern:
```
PageHeader → DataTable → CreateDialog / EditDialog
```

Shared behavior:
- `useQuery()` for list data (React Query)
- `useMutation()` for create/update/activate/deactivate
- `useHasPermission()` for action visibility
- Centralized error translation via existing `translateApiError()`
- Loading and empty states

#### Departments (`/hr/settings/departments`)

- DataTable: Code, Name, Type, Parent Dept, Manager, Status
- Create dialog: Code, Name, DepartmentTypeSelect, DepartmentSelect (parent), EmployeeSelect (manager)
- Edit dialog: same + RowVersion hidden
- Row actions: Edit, Activate/Deactivate

#### Positions (`/hr/settings/positions`)

- DataTable: Code, Name, Type, Department, Status
- Create dialog: Code, Name, PositionTypeSelect, DepartmentSelect
- Edit dialog: same + RowVersion
- Row actions: Edit, Activate/Deactivate. DepartmentSelect disabled on edit (or allowed).

#### Salary Grades (`/hr/settings/salary-grades`)

- DataTable: GradeCode, Name, Description, Ranges count, Status
- Create dialog: GradeCode, Name, Description (IsActive managed via activate/deactivate actions)
- Edit dialog: GradeCode (disabled on edit?), Name, Description, RowVersion
- Row actions: Edit, Activate/Deactivate

#### Workflow Role Assignments (`/hr/settings/workflow-role-assignments`)

- DataTable: Role, Employee Name, Employee Code, Department, Created At
- Create dialog: Role (WorkflowRoleSelect — static dropdown, currently "HrManager"), Employee (EmployeeSelect)
- Row actions: Remove (with confirmation dialog)

### 5.3 Shared Lookup Components

| Component | Props | Behavior |
|-----------|-------|----------|
| `DepartmentSelect` | value, onValueChange, disabled, placeholder | Loads active departments, shows "Code - Name", filterable |
| `PositionSelect` | value, onValueChange, departmentId?, disabled, placeholder | Loads active positions, optional departmentId filter |
| `EmployeeSelect` | value, onValueChange, disabled, placeholder | Dialog-based search via existing `SearchEmployees` API, reuses `EmployeeLookup` pattern |
| `SalaryGradeSelect` | value, onValueChange, disabled, placeholder | Loads active grades, shows "GradeCode - Name" |
| `WorkflowRoleSelect` | value, onValueChange, disabled, placeholder | Static dropdown from constants (e.g., "HrManager") |

All components:
- Typed props (no `any`)
- Loading state (disabled while loading)
- Error state (show error message, disabled)
- Translation placeholders

### 5.4 Recruitment Request Fix

**Files to modify**:
- `src/components/features/hr/recruitment/CreateRecruitmentRequestDialog.tsx`
- `src/components/features/hr/recruitment/CreateRequisitionDialog.tsx`

Replace:
```tsx
<Input value={departmentId} ... placeholder="Department ID" />
<Input value={positionId} ... placeholder="Position ID" />
```

With:
```tsx
<DepartmentSelect value={departmentId} onValueChange={setDepartmentId} />
<PositionSelect value={positionId} onValueChange={setPositionId}
  departmentId={departmentId || undefined} />
```

- Validation: both required, error codes mapped via `translateApiError`
- API contract unchanged (still submits GUID string internally)

### 5.5 Sidebar

Add to `Sidebar.tsx` navGroups, group `hrWorkspace`:
```typescript
{ href: "/hr/settings", label: "hrSettings", icon: Settings },
```
Position: last item in HR group. Visibility: OR of all settings permissions.

### 5.6 Route Permissions

Add to `ROUTE_PERMISSIONS`:
```typescript
"/hr/settings": [HR_DEPARTMENT_VIEW, HR_POSITION_VIEW, HR_SALARY_GRADE_VIEW, HR_WORKFLOW_VIEW],
"/hr/settings/departments": [HR_DEPARTMENT_VIEW],
"/hr/settings/positions": [HR_POSITION_VIEW],
"/hr/settings/salary-grades": [HR_SALARY_GRADE_VIEW],
"/hr/settings/workflow-role-assignments": [HR_WORKFLOW_VIEW],
```

**Note**: `HR_WORKFLOW_VIEW` is the existing workflow view permission. If it does not exist, add it as `hr.workflow.view`. The Workflow Role Assignment page uses the same permission as Workflow Definitions — they share the `hr.workflow.*` namespace.

---

## 6. Permissions

### New Backend Permissions

| Constant | Values | Sensitivity | Used In |
|----------|--------|-------------|---------|
| `HrPermissions.DepartmentManage` | `hr.department.manage` | Normal | DepartmentController (create/update/activate/deactivate) |
| `HrPermissions.PositionManage` | `hr.position.manage` | Normal | PositionController (create/update/activate/deactivate) |
| — | `hr.workflow.manage` (reuse existing) | Normal | WorkflowRoleAssignmentController (POST/DELETE) |

**Note**: `WorkflowRoleAssignment` uses existing `hr.workflow.manage` permission — no new permission created for it. Audit whether `WorkflowView` / `WorkflowManage` exists; if `hr.workflow.manage` is not present, add it as a single permission shared between Workflow Definitions and Workflow Role Assignments.

### New Frontend Permission Constants

```typescript
HR_DEPARTMENT_MANAGE = "hr.department.manage",
HR_POSITION_MANAGE = "hr.position.manage",
```

---

## 7. Translation Keys

### Namespace: `hr.settings.*`

```json
{
  "hr": {
    "settings": {
      "title": "HR Settings",
      "description": "Manage organization, workforce, payroll, time, recruitment and workflow settings.",
      "searchPlaceholder": "Search settings...",
      "noResults": "No settings found matching \"{query}\"",
      "categories": {
        "organization": "Organization",
        "workforce": "Workforce",
        "payroll": "Payroll & Compensation",
        "time": "Time & Attendance",
        "recruitment": "Recruitment & Onboarding",
        "workflow": "Workflow"
      },
      "items": {
        "departments": { "title": "Departments", "description": "Manage organizational departments and structure" },
        "positions": { "title": "Positions", "description": "Manage job positions within departments" },
        "salaryGrades": { "title": "Salary Grades", "description": "Manage grade levels and salary ranges" },
        "workflowRoleAssignments": { "title": "Workflow Role Assignments", "description": "Assign HR roles for workflow approval routing" }
      },
      "comingSoon": "Coming soon"
    }
  }
}
```

### Per-page namespaces: `hr.departments.*`, `hr.positions.*`, `hr.salaryGrades.*`, `hr.workflowRoleAssignments.*`

Each contains `title`, `description`, `fields.*`, `actions.*`, `validation.*`, `empty.*` sub-keys.

Example:
```json
{
  "hr": {
    "departments": {
      "title": "Departments",
      "description": "Manage organizational departments",
      "fields": { "code": "Code", "name": "Name", "type": "Type" },
      "actions": { "create": "Create Department", "edit": "Edit", "activate": "Activate", "deactivate": "Deactivate" },
      "validation": { "codeRequired": "Code is required" }
    }
  }
}
```

### Sidebar key: `hrSettings` under `Dashboard`

---

## 8. Security & Audit

- All new backend endpoints require permission attributes.
- No anonymous access.
- UI hides cards/actions without permission via `hasPermission()`.
- Backend enforces permissions regardless of FE.
- No raw GUID editing in user-facing forms.
- Employee lookup returns minimal info (name, code, department, position).
- SalaryGrade.Mange is High sensitivity per existing classification.

---

## 9. Browser E2E Verification

Required checks:
1. Navigate `/en/hr/settings` — page loads without console errors
2. Settings cards render with correct translations
3. Search "department" filters correctly
4. Switch category "Organization"
5. Open Departments panel — list renders
6. Create department dialog opens, form validates
7. Open Positions panel — position form uses DepartmentSelect, not raw text input
8. Open Recruitment Request create dialog — Department field is Select, Position field is Select
9. Vietnamese route `/vi/hr/settings` — translations render
10. No hydration errors, no network 404s

---

## 10. Future Considerations (Deferred)

- Dynamic Workflow Role management (add/edit roles in DB)
- Full Leave Type / Policy management pages
- Tax & Insurance rule management pages
- Skill / Competency / Career Framework (Phase 35+)
- Job Family & Job Grade separation (Phase 35+)
- Employee create/edit full form (note: deferred, current phase only replaces lookup in existing forms)

---

## 11. Out of Scope

- Employee Create/Edit full form (only lookup fix for existing Recruitment forms)
- Leave/Policy/Tax/Insurance settings pages (Coming Soon status only)
- Backend refactoring beyond Department, Position, SalaryGrade, WorkflowRoleAssignment
- Shared approval workflow configuration (Workflow Definitions card is Coming Soon)
- Frontend testing infrastructure (TD-003)
- Existing build warnings reduction (TD-002)
