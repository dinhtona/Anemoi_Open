# Phase 28 — Approval Workflow Engine Foundation — Completion Report

**Date:** 2026-06-17
**Status:** Complete
**Branch:** `phase-28-approval-workflow-engine`
**Tag:** `phase-28-workflow-engine`

---

## Build Results

| Target | Result |
|--------|--------|
| `dotnet build Anemoi.sln` | ✅ 0 errors, 7 pre-existing warnings (unchanged) |
| `npm run build` (cody-web-app) | ✅ Compiled successfully |

## Test Results

| Test Project | Passed | Failed | Skipped | Total |
|-------------|--------|--------|---------|-------|
| `Anemoi.Hr.Test` | 173 | 0 | 0 | 173 |
| `Anemoi.BuildingBlock.Test` | 305 | 0 | 0 | 305 |
| **Total** | **478** | **0** | **0** | **478** |

---

## Files Created (30 files)

### Domain — `Anemoi.Hr.Domain/Workflow/` (9 files)
| File | Purpose |
|------|---------|
| `WorkflowDefinition.cs` | Aggregate root — workflow template with ordered steps, activation controls |
| `WorkflowDefinitionStep.cs` | Child entity — step definition (approver type, value, required flag) |
| `WorkflowInstance.cs` | Aggregate root — active workflow tracking state machine |
| `WorkflowInstanceStep.cs` | Child entity — snapshot of step definition at start time |
| `WorkflowHistory.cs` | Entity — audit trail of all actions on an instance |
| `WorkflowTypeCode.cs` | Constants — first type: `Approval` |
| `WorkflowStatusCode.cs` | Constants — `Draft`, `Pending`, `Approved`, `Rejected`, `Cancelled`, `Returned` |
| `WorkflowStepStatusCode.cs` | Constants — `Pending`, `Approved`, `Rejected`, `Skipped`, `Cancelled` |
| `ApproverType.cs` | Constants — `Role`, `Permission`, `DirectManager`, `SpecificUser` |

### ModelIds — `Anemoi.Hr.ModelIds/ModelIds/` (5 files)
| File | Purpose |
|------|---------|
| `WorkflowDefinitionId.cs` | Strongly-typed ID for WorkflowDefinition |
| `WorkflowDefinitionStepId.cs` | Strongly-typed ID for WorkflowDefinitionStep |
| `WorkflowInstanceId.cs` | Strongly-typed ID for WorkflowInstance |
| `WorkflowInstanceStepId.cs` | Strongly-typed ID for WorkflowInstanceStep |
| `WorkflowHistoryId.cs` | Strongly-typed ID for WorkflowHistory |

### Infrastructure — Configurations (2 files)
| File | Purpose |
|------|---------|
| `WorkflowDefinitionModelMapping.cs` | EF config for WorkflowDefinitions + WorkflowDefinitionSteps tables |
| `WorkflowInstanceModelMapping.cs` | EF config for WorkflowInstances + WorkflowInstanceSteps + WorkflowHistories tables |

### Infrastructure — Services (1 file)
| File | Purpose |
|------|---------|
| `Services/UserRolePermissionService.cs` | Stub implementation — will integrate with Identity in Phase 29+ |

### Application — Responses (1 file)
| File | Purpose |
|------|---------|
| `Responses/WorkflowResponses.cs` | 5 response DTOs (Definition, DefinitionStep, Instance, InstanceStep, History) |

### Application — Mappings (1 file)
| File | Purpose |
|------|---------|
| `Mappings/WorkflowMapper.cs` | Manual mapper — entity-to-DTO conversion |

### Application — Abstractions (1 file)
| File | Purpose |
|------|---------|
| `Abstractions/IUserRolePermissionService.cs` | Interface for role/permission checking |

### Application — Commands (25 files)
| Group | Files | Description |
|-------|-------|-------------|
| `Common/WorkflowDefinitionStepInput.cs` | 1 | Shared step input DTO |
| `CreateWorkflowDefinition` | 3 | Create workflow definition with steps |
| `UpdateWorkflowDefinition` | 3 | Update name, description, replace steps (only if inactive) |
| `ActivateWorkflowDefinition` | 3 | Activate (requires at least 1 step) |
| `DeactivateWorkflowDefinition` | 3 | Deactivate |
| `StartWorkflow` | 3 | Start instance from definition, snapshot steps |
| `ApproveWorkflowStep` | 3 | Approve current step, advance or complete |
| `RejectWorkflowStep` | 3 | Reject (terminal) |
| `CancelWorkflow` | 3 | Cancel pending/returned instances |
| `ReturnWorkflow` | 3 | Return for revision (terminal) |

### Application — Queries (10 files)
| Group | Files | Description |
|-------|-------|-------------|
| `GetWorkflowDefinitions` | 2 | Paginated list, filterable by IsActive, WorkflowTypeCode |
| `GetWorkflowDefinitionById` | 2 | Single definition with steps |
| `GetWorkflowInstances` | 2 | Paginated list, filterable by Status, EntityType, DefinitionId |
| `GetWorkflowInstanceById` | 2 | Single instance with steps and history |
| `GetPendingApprovals` | 2 | Current user's pending approvals (resolved via role/permission/user ID) |

### API — Controllers (2 files)
| File | Endpoints |
|------|-----------|
| `WorkflowDefinitionsController.cs` | GET list, GET by id, POST create, PUT update, POST activate, POST deactivate |
| `WorkflowInstancesController.cs` | POST start, POST approve, POST reject, POST cancel, POST return, GET list, GET by id, GET pending |

### Frontend — New (2 files)
| File | Purpose |
|------|---------|
| `src/types/hr/workflow.ts` | 6 TypeScript interfaces |
| `src/services/hr/workflowService.ts` | 10 API methods |
| `src/app/[locale]/(dashboard)/hr/workflows/page.tsx` | 3-tab page (Definitions, Instances, Pending) |

### Tests (2 files)
| File | Tests |
|------|-------|
| `Domain/Workflow/WorkflowDefinitionTests.cs` | 8 tests |
| `Domain/Workflow/WorkflowInstanceTests.cs` | 25 tests |

---

## Files Modified (8 files)

| File | Change |
|------|--------|
| `Anemoi.BuildingBlocks/.../Permissions.cs` | Added 3 workflow permissions + Definitions |
| `Anemoi.Contract.Notification/.../NotificationConstants.cs` | Added `Workflow` category |
| `Anemoi.Hr/.../HrPermissions.cs` | Added 3 permission references + All array entries |
| `Anemoi.Hr/.../HrBusinessErrorCodes.cs` | Added 16 workflow error codes |
| `Anemoi.Hr/.../HrDbContext.cs` | Added 5 new DbSets + using directive |
| `Anemoi.Hr/.../ServiceInstaller.cs` | Registered WorkflowMapper + UserRolePermissionService |
| `cody-web-app/src/constants/permissions.ts` | Added 3 permissions + route mapping |
| `cody-web-app/src/constants/api-endpoints.ts` | Added 14 workflow endpoint definitions |
| `cody-web-app/src/components/shared/Sidebar.tsx` | Added `hr/workflows` nav item with CheckCheck icon |
| `cody-web-app/messages/en.json` | Added `hrWorkflows` translation |
| `cody-web-app/messages/vi.json` | Added `hrWorkflows` translation |

---

## Architecture Decisions

### 1. WorkflowHistory as Entity (not ValueObject)
WorkflowHistory has its own identity (`WorkflowHistoryId`) and is stored as a separate table. This enables efficient querying, pagination, and independent lifetime management.

### 2. Approver Resolution Strategy
- **SpecificUser / DirectManager:** Resolved at `StartWorkflow` time and stored in `ApproverUserId`. This is safe because the approver is known at start.
- **Role / Permission:** Stored as snapshots (`ApproverTypeSnapshot`, `ApproverValueSnapshot`) with `ApproverUserId = null`. Resolved lazily via `IUserRolePermissionService` at action time. This allows dynamic role/permission changes to take effect.

### 3. Returned is Terminal
`ReturnForRevision` sets status to `Returned` and `CompletedAt`. No auto-restart. The module owner (e.g., Recruitment Request in Phase 29) will create a new workflow instance after revision. This avoids complex state management for restarted workflows.

### 4. StartWorkflow Restricted
`hr.workflow.manage` permission required (dev/admin only). No frontend action in Phase 28. This prevents orphaned workflow instances before module integrations exist.

### 5. Approver Validation in Handler
`ApproveWorkflowStep` and `RejectWorkflowStep` validate that the current user matches the step's approver before allowing the action. This prevents unauthorized approvals even if `hr.workflow.execute` permission is granted.

### 6. `IUserRolePermissionService` as Abstraction
The domain entity `WorkflowInstance.IsCurrentStepApprover()` accepts `Func<string, bool>` delegates for role/permission checking. The handlers inject `IUserRolePermissionService` to provide these delegates. This keeps the domain pure (no service dependencies) while enabling testability.

### 7. No Integration Events
Domain events and MassTransit integration events are deferred to Phase 31 (Notification Workflow Automation). Phase 28 is pure engine — no side effects beyond persistence.

---

## Deferred Items

| Item | Target Phase | Rationale |
|------|-------------|-----------|
| Recruitment Request integration | Phase 29 | First consumer of the workflow engine |
| Leave Request integration | Phase 30+ | Second consumer |
| Overtime Request integration | Phase 30+ | Third consumer |
| Notification business flow | Phase 31 | Link workflow status changes to notifications |
| DirectManager resolution | Phase 29+ | Stub returns null; needs employee hierarchy data |
| Role/Permission resolution | Phase 29+ | Stub returns false; needs Identity integration |
| Escalation | Phase 32+ | Auto-escalate unapproved steps after timeout |
| Delegation | Phase 32+ | Allow approvers to delegate to substitutes |
| Parallel approval | Phase 33+ | Multiple concurrent approvers at same step |
| BPMN support | Not planned | Out of current roadmap scope |

---

## Known Limitations

1. **DirectManager resolution is a stub** — `ResolveDirectManager()` in `StartWorkflowHandler` returns `null`. Will be implemented when employee hierarchy data is accessible (Phase 29+).
2. **Role/Permission checking is a stub** — `UserRolePermissionService` returns `false` for all queries. Will be integrated with the Identity service in Phase 29+.
3. **No auto-restart after ReturnForRevision** — Module owners must create new instances manually. This is intentional for Phase 28.
4. **No concurrency optimization** — `xmin` is configured but optimistic concurrency retry logic is not implemented in handlers. A `SaveChangesFailed` simply returns an error.
5. **No integration events** — Status transitions do not publish any events. Phase 31 will add this.
6. **No migration created** — EF Core migrations are not included. A migration must be generated before deploying to any database.
7. **Frontend page is basic** — The 3-tab page provides minimal UI (tables with status badges). Action dialogs (create definition, approve/reject) are not fully implemented — this is acceptable for Phase 28 as the engine is dev/admin facing.

---

## Architecture Review Checklist

- [x] **Domain:** WorkflowDefinition is aggregate root
- [x] **Domain:** WorkflowInstance is aggregate root
- [x] **Domain:** Child collections use backing field (`_steps`, `_histories`) — not exposed as mutable `List<T>`
- [x] **Infrastructure:** `xmin` concurrency token on all aggregate roots
- [x] **Infrastructure:** StronglyTypedId conversion configured on all ID properties
- [x] **Application:** Handlers do not call controllers or UI services directly
- [x] **Application:** No business logic in controllers — all delegated to handlers
- [x] **Application:** Validators check input structure only — no complex business rules
- [x] **Security:** Approve/Reject validate correct approver via `IsCurrentStepApprover()`
- [x] **Security:** `StartedBy` resolved from `HttpContext.GetUserId()` — never from client
- [x] **Security:** Permission mapping correct — `hr.workflow.view`, `hr.workflow.manage`, `hr.workflow.execute`
- [x] **Frontend:** No hardcoded endpoint URLs — all through `API_ENDPOINTS`
- [x] **Frontend:** No hardcoded permission strings — all through `PERMISSIONS` constants
