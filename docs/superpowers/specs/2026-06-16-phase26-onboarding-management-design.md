# Phase 26 — Onboarding Management Design

Status: Approved

Date: 2026-06-16

---

## Objective

Manage onboarding activities for new employees after hiring. Provide reusable onboarding plan templates, snapshot-based instance generation, task assignment (role-based with user resolution), and task-driven completion workflow.

---

## Architecture Decisions

### ADR-26-00 — Concurrency: Aggregate Root Only

Only aggregate roots (OnboardingPlanTemplate, OnboardingInstance) carry PostgreSQL xmin concurrency tokens. Child entities (OnboardingTaskTemplate, OnboardingTask) are owned/aggregate-managed and do NOT have xmin.

### ADR-26-01 — Template → Instance Snapshot Pattern

OnboardingPlanTemplate serves as a reusable template. When StartOnboarding is called, all OnboardingTaskTemplate records are snapshotted into OnboardingTask records on the OnboardingInstance. The instance does NOT maintain a live reference to the template after creation — historical reproducibility is preserved.

### ADR-26-02 — Role-Based Template + User-Specific Instance (Phase 26 Scope)

- **Template level**: Tasks define `AssigneeType` = Role only, with `AssigneeRoleCode` (HR/Manager/IT/Employee).
- **Instance level**: When starting onboarding, role-to-user mappings are provided (e.g., Manager = UserA, IT = UserB). Tasks are resolved to specific `AssignedUserId`. After resolution, HR can manually reassign tasks.
- **Future** (Phase 30+): Support `AssigneeType` = User for direct user assignment in templates.

### ADR-26-03 — Auto-Complete with Manual Override

OnboardingInstance auto-completes when all tasks are Completed or Skipped. Additional manual operations: Cancel, Reopen, Force Complete (with audit).

### ADR-26-04 — OffsetDay for Due Date Calculation

Each OnboardingTaskTemplate has an `OffsetDays` field (integer). When the instance is created with a `StartDate`, each task's `DueDate` is calculated as `StartDate.AddDays(OffsetDays)`. Negative values represent pre-hire tasks.

### ADR-26-05 — Future Notification Integration Point

Phase 26 does NOT implement notification delivery. However, the domain model must publish domain events for future Notification Phase N2 consumption:

- `OnboardingTaskAssignedEvent` — task assigned/reassigned to user
- `OnboardingTaskOverdueEvent` — task past due date
- `OnboardingCompletedEvent` — instance completed (auto/manual)
- `OnboardingCancelledEvent` — instance cancelled

Events carry: UserId, TaskId/InstanceId, OccurredAt, TenantId (future).

### ADR-26-06 — Historical Snapshot Includes User Display Name

`OnboardingTask` stores `AssignedUserDisplayNameSnapshot` in addition to `AssignedUserId`. If a user is renamed later, audit trails preserve the original assignee name. Follows PayrollItem DepartmentNameSnapshot/PositionNameSnapshot pattern (Phase 24).

---

## Domain Model

### OnboardingPlanTemplate (Aggregate Root)

| Field | Type | Notes |
|-------|------|-------|
| Id | OnboardingPlanTemplateId | StronglyTypedId<Guid> |
| Name | string | Required, max 200 |
| Description | string? | Max 1000 |
| Status | string | Active / Inactive (constant class) |
| CreatedAt | DateTime | |
| CreatedBy | string | |
| UpdatedAt | DateTime | |
| UpdatedBy | string | |
| Version | int | Incremented on each UpdateTemplate |
| Tasks | List<OnboardingTaskTemplate> | Child collection |

### OnboardingTaskTemplate (Child Entity)

| Field | Type | Notes |
|-------|------|-------|
| Id | OnboardingTaskTemplateId | StronglyTypedId<Guid> |
| TemplateId | OnboardingPlanTemplateId | FK |
| Title | string | Required, max 200 |
| Description | string? | Max 500 |
| AssigneeType | string | Role only (Phase 26) |
| AssigneeRoleCode | string? | HR / Manager / IT / Employee |
| OffsetDays | int | Can be negative (pre-hire) |
| SortOrder | int | Display ordering |
| IsRequired | bool | Default true |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |

### OnboardingInstance (Aggregate Root)

| Field | Type | Notes |
|-------|------|-------|
| Id | OnboardingInstanceId | StronglyTypedId<Guid> |
| EmployeeId | EmployeeId | FK to Employee |
| TemplateId | OnboardingPlanTemplateId? | Nullable — which template was used (audit) |
| TemplateName | string | Snapshot for history |
| TemplateVersion | int? | Template version at snapshot time |
| Status | string | Draft → InProgress → Completed / Cancelled |
| StartDate | DateTime | Hire date / effective date |
| CompletedAt | DateTime? | |
| CompletedBy | string? | |
| CancelledAt | DateTime? | |
| CancelledBy | string? | |
| ForceCompletedAt | DateTime? | |
| ForceCompletedBy | string? | |
| ForceCompleteReason | string? | |
| CreatedAt | DateTime | |
| CreatedBy | string | |
| UpdatedAt | DateTime | |
| UpdatedBy | string | |
| xmin | uint | PostgreSQL concurrency |
| Tasks | List<OnboardingTask> | Child collection |

### OnboardingTask (Child Entity, Snapshot — no xmin, aggregate-managed)

| Field | Type | Notes |
|-------|------|-------|
| Id | OnboardingTaskId | StronglyTypedId<Guid> |
| InstanceId | OnboardingInstanceId | FK |
| Title | string | Snapshot from template |
| Description | string? | Snapshot from template |
| AssigneeType | string | Snapshot from template (always "Role" in Phase 26) |
| AssigneeRoleCode | string? | Snapshot from template |
| AssignedUserId | string? | Resolved at start or reassigned |
| AssignedUserDisplayNameSnapshot | string? | Historical snapshot of user's display name |
| AssignedAt | DateTime? | When task was first assigned |
| AssignedBy | string? | Who assigned the task |
| ReassignedAt | DateTime? | When task was last reassigned |
| ReassignedBy | string? | Who reassigned the task |
| DueDate | DateTime | StartDate + OffsetDays |
| SortOrder | int | |
| IsRequired | bool | |
| Status | string | Pending / Completed / Skipped |
| CompletedAt | DateTime? | |
| CompletedBy | string? | |
| CompletedNotes | string? | |
| SkippedAt | DateTime? | |
| SkippedBy | string? | |
| ReopenedAt | DateTime? | |
| ReopenedBy | string? | |
| ReopenedReason | string? | |

### Status Flow

```
OnboardingPlanTemplate:
  Active ←→ Inactive

OnboardingInstance:
  Draft → InProgress → Completed (auto/manual)
                    ↘ Cancelled
  Completed → InProgress (Reopen)

OnboardingTask:
  Pending → Completed
  Pending → Skipped
  Completed → Pending (Reopen)
  Skipped → Pending (Reopen)
```

### Business Rule Definitions

**Overdue Task**: Status = Pending AND DueDate < Today (UTC).

**Auto-Completion**: When all tasks in an instance are Completed or Skipped, the instance status automatically transitions to Completed.

### Database Constraints

**Filtered Unique Index — Active Onboarding Per Employee**:
```sql
CREATE UNIQUE INDEX IX_OnboardingInstances_EmployeeId_Active
ON "Hr"."OnboardingInstances" ("EmployeeId")
WHERE "Status" IN ('Draft', 'InProgress');
```
Prevents race conditions where StartOnboarding could create duplicate active instances for the same employee.

---

## Permissions

| Key | Description |
|-----|-------------|
| `hr.onboarding.view` | View templates, instances, tasks |
| `hr.onboarding.manage` | CRUD templates, start/cancel/reopen/force-complete onboarding |
| `hr.onboarding.task.complete` | Complete/skip own assigned tasks |
| `hr.onboarding.task.manage` | Reassign/reopen any task |

---

## Commands

| Command | Description |
|---------|-------------|
| CreateOnboardingPlanTemplateCommand | Create template with task templates |
| UpdateOnboardingPlanTemplateCommand | Update template (title, description, tasks) |
| ActivateOnboardingPlanTemplateCommand | Activate template |
| DeactivateOnboardingPlanTemplateCommand | Deactivate template |
| StartOnboardingCommand | Create instance from template, snapshot tasks, resolve roles |
| CompleteOnboardingTaskCommand | Mark task as completed |
| ReopenOnboardingTaskCommand | Reopen completed/skipped task |
| SkipOnboardingTaskCommand | Skip a task |
| ReassignOnboardingTaskCommand | Change assigned user for a task |
| CancelOnboardingCommand | Cancel instance |
| ReopenOnboardingCommand | Reopen completed instance |
| ForceCompleteOnboardingCommand | Force complete instance (with reason) |

## Queries

| Query | Description |
|-------|-------------|
| GetOnboardingPlanTemplatesQuery | Paginated list of templates (filter: status) |
| GetOnboardingPlanTemplateByIdQuery | Template detail with task templates |
| GetOnboardingInstancesQuery | Paginated list of instances (filter: employee, status, date) |
| GetOnboardingInstanceByIdQuery | Instance detail with tasks, progress % |
| GetEmployeeOnboardingQuery | Current onboarding for a specific employee |
| GetMyOnboardingTasksQuery | Tasks assigned to current user |
| GetPendingOnboardingTasksQuery | All pending/overdue tasks (HR view) |
| GetOnboardingDashboardQuery | Stats: completion rate, overdue count, upcoming tasks |

---

## API Endpoints

### Templates
```
GET    /api/hr/onboarding/templates
POST   /api/hr/onboarding/templates
GET    /api/hr/onboarding/templates/{id}
PUT    /api/hr/onboarding/templates/{id}
POST   /api/hr/onboarding/templates/{id}/activate
POST   /api/hr/onboarding/templates/{id}/deactivate
```

### Instances
```
POST   /api/hr/onboarding/instances/start
GET    /api/hr/onboarding/instances
GET    /api/hr/onboarding/instances/{id}
POST   /api/hr/onboarding/instances/{id}/cancel
POST   /api/hr/onboarding/instances/{id}/reopen
POST   /api/hr/onboarding/instances/{id}/force-complete
GET    /api/hr/employees/{employeeId}/onboarding
```

### Tasks
```
POST   /api/hr/onboarding/tasks/{id}/complete
POST   /api/hr/onboarding/tasks/{id}/reopen
POST   /api/hr/onboarding/tasks/{id}/skip
POST   /api/hr/onboarding/tasks/{id}/reassign
```

### Dashboard / My Views
```
GET    /api/hr/onboarding/my-tasks
GET    /api/hr/onboarding/pending-tasks
GET    /api/hr/onboarding/dashboard
```

---

## Domain Events (Future Notification Integration)

Events published by Phase 26 aggregate roots for future Phase N2 notification consumption:

| Event | Trigger | Payload |
|-------|---------|---------|
| OnboardingTaskAssignedDomainEvent | Task created or reassigned | TaskId, InstanceId, AssignedUserId, AssignedBy, DueDate |
| OnboardingTaskCompletedDomainEvent | Task completed | TaskId, InstanceId, CompletedBy, CompletedAt |
| OnboardingTaskSkippedDomainEvent | Task skipped | TaskId, InstanceId, SkippedBy, SkippedAt |
| OnboardingTaskOverdueDomainEvent | Task overdue (scheduled job, future) | TaskId, InstanceId, AssignedUserId, DueDate |
| OnboardingInstanceCompletedDomainEvent | Instance auto-completed or force-completed | InstanceId, EmployeeId, CompletedBy, CompletionType (Auto/Manual/Force) |
| OnboardingInstanceCancelledDomainEvent | Instance cancelled | InstanceId, EmployeeId, CancelledBy, CancelledAt |

Phase 26 publishes events but does NOT implement notification consumers. Consumers are added in a dedicated notification phase.

---

## Error Codes

All onboarding error codes follow the `HR_ONB_*` pattern:

| Code | Description |
|------|-------------|
| HR_ONB_TEMPLATE_NOT_FOUND | Template not found |
| HR_ONB_TEMPLATE_INACTIVE | Template is inactive, cannot use |
| HR_ONB_TEMPLATE_HAS_TASKS | Cannot deactivate template with active instances |
| HR_ONB_INSTANCE_NOT_FOUND | Instance not found |
| HR_ONB_INSTANCE_INVALID_STATUS | Invalid status transition |
| HR_ONB_INSTANCE_ALREADY_COMPLETED | Instance already completed |
| HR_ONB_INSTANCE_ALREADY_CANCELLED | Instance already cancelled |
| HR_ONB_INSTANCE_NOT_IN_PROGRESS | Action requires InProgress status |
| HR_ONB_INSTANCE_FORCE_COMPLETE_REQUIRES_REASON | Reason is required for force complete |
| HR_ONB_TASK_NOT_FOUND | Task not found |
| HR_ONB_TASK_INVALID_STATUS | Invalid task status transition |
| HR_ONB_TASK_ALREADY_COMPLETED | Task already completed |
| HR_ONB_TASK_ALREADY_SKIPPED | Task already skipped |
| HR_ONB_EMPLOYEE_ALREADY_ONBOARDING | Employee already has active onboarding |
| HR_ONB_EMPLOYEE_NOT_FOUND | Employee not found |
| HR_ONB_RESOLVE_ROLE_MISSING | Required role mapping missing |
| HR_ONB_TEMPLATE_HAS_NO_TASKS | Template has no tasks, cannot start onboarding |
| HR_ONB_TASK_ALREADY_REOPENED | Task not completed or skipped — already in Pending status |
| VAL_NAME_REQUIRED | Name is required |
| VAL_TITLE_REQUIRED | Title is required |
| VAL_TEMPLATE_ID_REQUIRED | TemplateId is required |
| VAL_EMPLOYEE_ID_REQUIRED | EmployeeId is required |
| VAL_START_DATE_REQUIRED | StartDate is required |
| VAL_TASK_ID_REQUIRED | TaskId is required |
| VAL_OFFSET_DAYS_REQUIRED | OffsetDays is required |
| VAL_SORT_ORDER_REQUIRED | SortOrder is required |
| VAL_INSTANCE_ID_REQUIRED | InstanceId is required |
| VAL_REASON_REQUIRED | Reason is required |
| VAL_ASSIGNEE_ROLE_REQUIRED | AssigneeRoleCode is required when AssigneeType is Role |
| VAL_ROLE_MAPPINGS_REQUIRED | RoleMappings are required when template has role tasks |
| VAL_DUPLICATE_TASK_TITLE | Task title already exists in template |

---

## Frontend Pages

### HR Pages (under /hr/onboarding/)

1. **Templates** (`/hr/onboarding/templates`)
   - Table of templates with status badge
   - Create/Edit template dialog (with dynamic task template rows)
   - Activate/Deactivate actions
   - SortOrder reorder for task templates

2. **Instances** (`/hr/onboarding/instances`)
   - Table of all instances filterable by status, employee, date
   - Quick status badges + progress bar
   - Click → detail page

3. **Instance Detail** (`/hr/onboarding/instances/[id]`)
   - Employee info + dates + progress %
   - Task list with status, assignee, due date
   - Complete/Skip/Reopen/Reassign task actions (permission-gated)
   - Cancel/Reopen/Force Complete instance actions

4. **Task Board** (`/hr/onboarding/tasks`)
   - All pending + overdue tasks across all instances
   - Filters: assignee, status, date range
   - Bulk operations if feasible

### ESS Pages (under /ess/)

5. **My Onboarding** (`/ess/onboarding`)
   - Current employee's onboarding checklist
   - Progress bar
   - Task list with status and due dates
   - Complete task action (if assigned)

---

## Testing Strategy

### Domain Tests
- Template status transitions (activate/deactivate)
- Instance status transitions (start/complete/cancel/reopen)
- Task status transitions (complete/skip/reopen)
- Auto-completion when all tasks done
- OffsetDays → DueDate calculation
- Role resolution from mappings

### Application Tests
- CreateTemplateHandler: validation, duplicate name
- StartOnboardingHandler: snapshot correctness, role resolution, employee check
- CompleteTaskHandler: status validation, auto-complete instance

### Integration Tests
- Full flow: create template → start onboarding → complete tasks → auto-complete
- Concurrency: xmin conflict handling

---

## Success Criteria

- [x] Design approved
- [ ] All domain entities implemented
- [ ] All commands/queries implemented
- [ ] All API endpoints working
- [ ] Permissions enforced
- [ ] Frontend pages rendered
- [ ] Localization complete (en/vi)
- [ ] Tests passing
- [ ] Build passes
