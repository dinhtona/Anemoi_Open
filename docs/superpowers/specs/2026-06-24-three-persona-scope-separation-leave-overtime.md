# Three-Persona Scope Separation — Leave & Overtime

**Status:** Draft  
**Date:** 2026-06-24  
**Author:** AI Agent (design review)  
**Version:** 3.0

---

## 1. Problem Statement

Leave and Overtime modules currently mix three concerns in a single HR page:

- Personal data ("My Requests")
- Approval data ("Pending Approvals")
- Organization-wide data ("All Requests")

This causes:

- Confusion for users who see data outside their scope
- Violation of least-privilege visibility
- No clear pattern for future modules

---

## 2. Scope Separation Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                      THREE SCOPES                                 │
├─────────────────┬────────────────────┬───────────────────────────┤
│   EMPLOYEE      │    APPROVAL        │    HR / ADMIN             │
│   (ESS)         │    (MANAGER)       │    (ORGANIZATION)         │
├─────────────────┼────────────────────┼───────────────────────────┤
│ /ess/*          │ /manager/approvals │ /hr/*                     │
│                 │                    │                           │
│ My requests     │ Pending my         │ Organization-wide data    │
│ My balances     │   approval         │ All requests/records      │
│ My history      │ Workflow-routed    │ Full filters              │
│ My status       │   to me            │ Sensitive operations      │
│                 │                    │ Force-approve/cancel      │
│ Self-service    │ Cross-entity       │ Configuration             │
│ only            │   approval center  │                           │
│                 │                    │ Observe workflow          │
│ Approver shown  │ Approve / Reject   │ progress + columns        │
│ (readonly)      │   only             │ (no approve authority)    │
└─────────────────┴────────────────────┴───────────────────────────┘
                           ▲
                           │
        CanObserveWorkflow ≠ CanApproveWorkflow
```

Other modules (Probation, Separation, Recruitment, Transfer) will follow the same three-scope pattern. The Approval Center is a single cross-entity inbox — each module must not create its own approval page.

---

## 3. Route Structure

```
/ess/
  leave/                        → My Leave Requests + My Balances
  overtime/                     → My Overtime Requests

/manager/
  approvals/                    → Approval Center (cross-entity inbox)
    ├─ All (default)            → Aggregate of all entity types (client-side)
    ├─ Leave                    → Pending Leave Approvals
    ├─ Overtime                 → Pending Overtime Approvals
    ├─ Probation                → (future)
    ├─ Separation               → (future)
    ├─ Recruitment              → (future)
    └─ Transfer                 → (future)

/hr/
  leave/                        → All Leave (org-wide + workflow columns)
  overtime/                     → All Overtime (org-wide + workflow columns)
```

The `/manager/approvals` route is the **single** approval inbox. No module may create `/leave/pending`, `/overtime/pending`, etc.

The "All" tab aggregates data client-side from entity-specific endpoints. No dedicated backend endpoint.

---

## 4. Backend API Structure

### Existing (unchanged)

| Method | Path | Purpose | Scope |
|--------|------|---------|-------|
| GET | `/api/hr/ess/leave/requests` | My leave requests + workflow info | ESS |
| GET | `/api/hr/ess/leave/balances` | My leave balances | ESS |
| POST | `/api/hr/ess/leave/submit` | Submit my leave | ESS |
| POST | `/api/hr/ess/leave/cancel/{id}` | Cancel my leave | ESS |
| GET | `/api/hr/ess/overtime/requests` | My overtime requests + workflow info | ESS |
| POST | `/api/hr/ess/overtime/submit` | Submit my overtime | ESS |
| POST | `/api/hr/ess/overtime/cancel/{id}` | Cancel my overtime | ESS |
| GET | `/api/hr/leave/Leave/GetLeaveRequests` | All requests (HR) | HR |
| GET | `/api/hr/overtime/GetPaged` | All overtime (HR) | HR |
| POST | `/api/hr/leave/Leave/SubmitLeaveRequest` | Create for specific employee | HR |
| POST | `/api/hr/leave/Leave/ForceApproveLeaveRequest` | Sensitive approve | HR |
| POST | `/api/hr/leave/Leave/ForceCancelLeaveRequest` | Sensitive cancel | HR |
| POST | `/api/hr/workflow/approve-step` | Approve workflow step | Shared |
| POST | `/api/hr/workflow/reject-step` | Reject workflow step | Shared |

### New — Manager Approval Endpoints

| Method | Path | Purpose | Permission |
|--------|------|---------|------------|
| GET | `/api/hr/manager/approvals/leave` | Pending leave approvals for current approver | `hr.leave.request.approve` |
| GET | `/api/hr/manager/approvals/overtime` | Pending overtime approvals for current approver | `hr.overtime.approve` |

No `/api/hr/manager/approvals/all` endpoint. The "All" tab aggregates client-side.

### New DTOs

**ManagerLeavePendingApprovalResponse:**
```
- RequestId: string
- EmployeeId: string
- EmployeeName: string
- DepartmentName: string
- LeaveTypeCode: string
- StartDate: string
- EndDate: string
- Days: number
- Reason: string
- SubmittedAt: string
- Status: string                          ← entity status ("Pending")
- WorkflowInstanceId: string
- CurrentStepName: string                 ← e.g. "Manager Approval"
- WorkflowStatus: string                  ← e.g. "Pending"
- CurrentApproverName: string             ← resolved from WorkflowInstanceStep
```

**ManagerOvertimePendingApprovalResponse:**
```
- RequestId: string
- EmployeeId: string
- EmployeeName: string
- DepartmentName: string
- OvertimeDate: string
- Hours: number
- Reason: string
- SubmittedAt: string
- Status: string
- WorkflowInstanceId: string
- CurrentStepName: string
- WorkflowStatus: string
- CurrentApproverName: string
```

### ESS DTO Updates

Add workflow fields to existing ESS responses:

**EssLeaveRequestResponse:**
```
Add:
- CurrentApproverName: string?
- CurrentStepName: string?
- WorkflowStatus: string?
```

**EssOvertimeRequestResponse:**
```
Add:
- CurrentApproverName: string?
- CurrentStepName: string?
- WorkflowStatus: string?
```

### HR DTO Updates

Add workflow fields to HR responses so HR pages display workflow observation columns:

**LeaveRequestResponse (HR):**
```
Add:
- CurrentApproverName: string?
- CurrentStepName: string?
- WorkflowStatus: string?
```

**OvertimeRequestResponse (HR):**
```
Add:
- CurrentApproverName: string?
- CurrentStepName: string?
- WorkflowStatus: string?
```

---

## 5. Permission Matrix

### Current Permissions (Reuse Existing)

| Route / Endpoint | Permission | Used By |
|---|---|---|
| `/ess/leave` | `hr.ess.leave.view` | Employees |
| `/ess/overtime` | `hr.ess.overtime.view` | Employees |
| `/manager/approvals` (approve/reject) | `hr.leave.request.approve` or `hr.overtime.approve` | Any user with approval responsibilities (role-agnostic) |
| `/hr/leave` | `hr.leave.request.view` | HR / Admin |
| `/hr/overtime` | `hr.overtime.view` | HR / Admin |
| Force operations | `hr.leave.request.force_approve` / `hr.leave.request.force_cancel` | HR / Admin (sensitive) |
| Balance adjust | `hr.leave.balance.adjust` | HR / Admin (sensitive) |

No new permissions are created. All permissions reuse existing stable constants.

### Future: Combined Approval Permission

Current per-entity permissions (`hr.leave.request.approve` OR `hr.overtime.approve`) scale linearly with each new workflow-enabled module (Recruitment, Transfer, Separation, Probation).

Future direction (no implementation now):

```text
approval.inbox.view
```

or

```text
workflow.approval.view
```

A single permission granting access to the entire `/manager/approvals` approval center. This avoids updating the sidebar permission mapping for every new workflow type.

### CanObserveWorkflow ≠ CanApproveWorkflow

| Capability | Approval Scope | HR Scope |
|---|---|---|
| Approve/reject workflow steps | ✅ | ❌ (unless also assigned as approver) |
| Observe workflow progress | ✅ (own inbox) | ✅ (workflow columns on entity records) |
| Audit approval bottlenecks | ❌ | ✅ (future `hr.workflow.view`) |

HR users who are also workflow approvers use `/manager/approvals` for approval actions. They use `/hr/*` for organization-wide observation and management.

Frontend route-permission mapping:
```typescript
"/ess/leave":             [PERMISSIONS.HR_ESS_LEAVE_VIEW]
"/ess/overtime":          [PERMISSIONS.HR_ESS_OVERTIME_VIEW]
"/manager/approvals":     [PERMISSIONS.HR_LEAVE_REQUEST_APPROVE,    // show if ANY
                           PERMISSIONS.HR_OVERTIME_APPROVE]         // approval permission
"/hr/leave":              [PERMISSIONS.HR_LEAVE_REQUEST_VIEW]
"/hr/overtime":           [PERMISSIONS.HR_OVERTIME_VIEW]
```

---

## 6. Workflow Interaction Diagram

```
Employee submits request
         │
         ▼
  Workflow Engine starts
  (via CreateOvertimeRequestHandler /
   SubmitMyLeaveRequestHandler)
         │
         ▼
  Workflow Step 1: Pending
  ApproverEmployeeId = resolved
  (via IApprovalResolver: DirectManager /
   DepartmentManager / WorkflowRole / Permission / etc.)
         │
         ├── Employee sees CurrentApproverName on ESS page
         │   (via IWorkflowQueryService)
         │
         ▼
  Approver opens /manager/approvals
         │
         ▼
  GET /api/hr/manager/approvals/leave    (or /overtime)
         │
         ├─ Queries WorkflowEngine.GetCurrentApproversAsync()
         │  → returns WorkflowInstanceId[] where current user is approver
         │
         ├─ Joins WorkflowInstance → entity (LeaveRequest / OvertimeRequest)
         │
         └─ Returns business DTOs
                │
                ▼
  Approver clicks Approve / Reject
         │
         ▼
  POST /api/hr/workflow/approve-step  (or reject-step)
  Body: { WorkflowInstanceId, Comment? }
         │
         ▼
  WorkflowEngine.ApproveAsync()
         │
         ├─ Validates current user = current step approver
         ├─ Advances workflow step
         ├─ If more steps → Step N now pending for next approver
         ├─ If final step → raises domain event
         │   └─ WorkflowTargetStatusUpdater updates entity status
         └─ Returns updated WorkflowInstance
```

HR observes workflow state through the same `IWorkflowQueryService` on the `/hr/*` pages.

### Future: Unified Approval API

The entity-specific manager endpoints (`/leave`, `/overtime`) are a stepping stone.

Future migration path:

```text
GET /api/hr/workflow/pending-approvals
```

Response:
```csharp
EntityType          // "LeaveRequest", "OvertimeRequest", ...
EntityId            // GUID of the entity
WorkflowInstanceId  // GUID for approve/reject
DisplayTitle        // Human-readable summary
SubmittedBy         // EmployeeName
SubmittedAt         // DateTime
CurrentStep         // Step name
Status              // Workflow status
```

Do NOT implement this now. Entity-specific endpoints remain.

---

## 7. Employee / Approval / HR Access Matrix

| Data | Employee (ESS) | Approval Center | HR / Admin |
|------|---------------|-----------------|------------|
| **My leave requests** | ✅ Full CRUD (own) | ❌ | ❌ (use /ess) |
| **My leave balances** | ✅ View (own) | ❌ | ✅ View any |
| **My overtime requests** | ✅ Full CRUD (own) | ❌ | ❌ (use /ess) |
| **Current approver / step** | ✅ View (own requests) | ✅ View | ✅ View (on any record) |
| **Pending leave approvals (my responsibility)** | ❌ | ✅ View + Approve/Reject | ❌ (use /manager if approver) |
| **Pending overtime approvals (my responsibility)** | ❌ | ✅ View + Approve/Reject | ❌ (use /manager if approver) |
| **All leave requests (org-wide)** | ❌ | ❌ | ✅ View + Filter |
| **All overtime requests (org-wide)** | ❌ | ❌ | ✅ View + Filter |
| **Workflow progress (any entity)** | ❌ (own only) | ✅ (own inbox) | ✅ Observe (columns on records) |
| **Force operations** | ❌ | ❌ | ✅ (sensitive) |
| **Balance adjustments** | ❌ | ❌ | ✅ (sensitive) |
| **Leave type configuration** | ❌ | ❌ | ✅ Manage |
| **Leave policy configuration** | ❌ | ❌ | ✅ Manage |
| **Overtime rule configuration** | ❌ | ❌ | ✅ Manage |

---

## 8. Frontend Component Architecture

```
/ess/leave/page.tsx                    [UPDATE: add approver columns]
  └─ My leave requests table
  │    ├─ Existing columns: StartDate, EndDate, Days, Reason, Status
  │    └─ New columns: Current Approver, Current Step, Workflow Status
  └─ My leave balance cards
  └─ Create Leave Dialog (auto-resolves, readonly approver preview)

/ess/overtime/page.tsx                 [UPDATE: add approver columns]
  └─ My overtime requests table
  │    ├─ Existing columns: Date, Start/End, Hours, Reason, Status
  │    └─ New columns: Current Approver, Current Step, Workflow Status
  └─ Create Overtime Dialog

/manager/approvals/page.tsx            [NEW]
  └─ Tabs:
       ├─ All tab (default)
       │   ├─ Client-side aggregation from entity-specific endpoints
       │   ├─ Fetches: usePendingLeaveApprovals() + usePendingOvertimeApprovals()
       │   ├─ Merges into unified list sorted by SubmittedAt desc
       │   └─ Columns: Type, Employee, Department, Summary, Submitted At, Actions
       │
       ├─ Leave tab (visible if hr.leave.request.approve)
       │   └─ ManagerApprovalTable (leave-specific)
       │       ├─ Columns: Employee, Department, Leave Type, Date Range, Days, Submitted At
       │       └─ Actions: Approve / Reject → calls workflow API
       │
       └─ Overtime tab (visible if hr.overtime.approve)
           └─ ManagerApprovalTable (overtime-specific)
               ├─ Columns: Employee, Department, Date, Hours, Submitted At
               └─ Actions: Approve / Reject → calls workflow API

/hr/leave/page.tsx                     [CLEANUP + ADD workflow columns]
  └─ Remove: Pending Approvals tab
  └─ Remove: default employee filter (profile-based)
  └─ Keep: All Requests table
  └─ Add: workflow columns (Current Approver, Current Step, Workflow Status)
  └─ Keep: Employee / Department / Approver / Status / Date Range filters
  └─ Keep: Create (for any employee), Force approvals

/hr/overtime/page.tsx                  [CLEANUP + ADD workflow columns]
  └─ Remove: default employee filter
  └─ Add: workflow columns (Current Approver, Current Step, Workflow Status)
  └─ Keep: All Requests table + filters + CRUD
```

---

## 9. Sidebar Changes

Add to `hrWorkspace` group in `Sidebar.tsx`:

```typescript
{
  href: "/manager/approvals",
  label: "hrApprovals",     // i18n key
  icon: CheckCheck,
}
```

### Visibility Logic

Current (acceptable):
```typescript
canAccessRoute("/manager/approvals")
// Route permission: [hr.leave.request.approve, hr.overtime.approve]
```

Future direction (documented, no implementation now):
```typescript
hasAnyApprovalCapability()
// Could check: PendingApprovalCount > 0 OR user owns any approval permission
// Avoid hardcoding every future workflow entity type
```

---

## 10. IWorkflowQueryService — Shared Workflow Read Model

### Problem

Every module (ESS, HR, future) that needs to display workflow information (CurrentApprover, CurrentStep, WorkflowStatus) would duplicate the same join logic:

```
Entity → WorkflowInstance → WorkflowInstanceStep → Employee
```

### Solution: `IWorkflowQueryService`

A shared query service in the Workflow infrastructure that encapsulates the workflow read model.

**Batch query API (avoids N+1):**

```csharp
// Anemoi.Hr.Application/Abstractions/IWorkflowQueryService.cs

public interface IWorkflowQueryService
{
    Task<Dictionary<Guid, WorkflowSummaryResponse>> GetWorkflowSummariesAsync(
        string entityType,
        IReadOnlyCollection<Guid> entityIds,
        CancellationToken ct);
}

public sealed record WorkflowSummaryResponse(
    string? CurrentApproverName,
    string? CurrentStepName,
    string? WorkflowStatus);
```

### Service behavior

```
Input:  entityType = "LeaveRequest",
        entityIds = ["abc-123", "def-456", ...]

Output: {
  "abc-123": { CurrentApproverName: "Nguyen Van A", CurrentStepName: "Manager Approval", WorkflowStatus: "Pending" },
  "def-456": { CurrentApproverName: null, CurrentStepName: null, WorkflowStatus: null },
  ...
}

Return empty dictionary if no active workflows found.
Null/empty values when entity has no active workflow.
```

Single-entity convenience helper (optional, not required):

```csharp
async Task<WorkflowSummaryResponse?> GetWorkflowSummaryAsync(
    string entityType, Guid entityId, CancellationToken ct)
{
    var dict = await GetWorkflowSummariesAsync(entityType, [entityId], ct);
    return dict.GetValueOrDefault(entityId);
}
```

### Implementation (pseudocode — audit Phase 28-30 models before coding)

```csharp
public sealed class WorkflowQueryService(
    ISqlRepository<WorkflowInstance> workflowRepo,
    ISqlRepository<Employee> employeeRepo)
    : IWorkflowQueryService
{
    public async Task<Dictionary<Guid, WorkflowSummaryResponse>> GetWorkflowSummariesAsync(
        string entityType, IReadOnlyCollection<Guid> entityIds, CancellationToken ct)
    {
        // 1. Bulk-load active WorkflowInstances matching entityIds
        // 2. Bulk-load WorkflowInstanceSteps for current step
        // 3. Bulk-load Employee names for ApproverEmployeeId
        // 4. Build dictionary mapping entityId → summary
        // Do NOT iterate with per-entity queries
    }
}
```

**Important:** Before implementing, audit the actual `WorkflowInstance`, `WorkflowInstanceStep`, `CurrentStep`, and `ApproverEmployeeId` structures from Phase 28–30. The pseudocode above is architectural guidance, not a literal template.

### Consumers

| Consumer | Context | API |
|----------|---------|-----|
| ESS Leave query | GetMyLeaveRequests | Batch `GetWorkflowSummariesAsync` |
| ESS Overtime query | GetMyOvertimeRequests | Batch `GetWorkflowSummariesAsync` |
| HR Leave query | GetLeaveRequests | Batch `GetWorkflowSummariesAsync` |
| HR Overtime query | GetOvertimeRequests | Batch `GetWorkflowSummariesAsync` |
| Manager approvals | GetMyPendingLeaveApprovals | Direct WorkflowInstance query (built-in) |
| Future modules | Recruitment, Transfer, ... | Batch `GetWorkflowSummariesAsync` |

### Implementation note

Manager approval handlers bypass `IWorkflowQueryService` because they already load `WorkflowInstance` entities directly as part of the pending approvals resolution. `IWorkflowQueryService` is for ESS and HR read models that need to augment entity records with live workflow state.

---

## 11. Service & Hook Changes

### Backend: Manager Approval Handlers

```csharp
public sealed class GetMyPendingLeaveApprovalsHandler(
    ISender sender,
    ISqlRepository<WorkflowInstance> workflowRepo,
    ISqlRepository<LeaveRequest> leaveRepo,
    ISqlRepository<Employee> employeeRepo)
    : IRequestHandler<GetMyPendingLeaveApprovalsQuery,
        IReadOnlyCollection<ManagerLeavePendingApprovalResponse>>
{
    // 1. Call existing GetPendingApprovalsQuery with UserId
    //    → returns WorkflowInstance[] where current user is approver
    //    at the current step AND EntityType == "LeaveRequest"
    //
    // 2. Extract LeaveRequestId from EntityId
    //
    // 3. Query LeaveRequests by those IDs
    //    Include Employee for name/department
    //
    // 4. Map to ManagerLeavePendingApprovalResponse
    //    (workflow info already available from WorkflowInstance)
    //
    // 5. Return sorted by SubmittedAt desc
}
```

Same pattern for `GetMyPendingOvertimeApprovalsHandler`.

### Backend: ESS + HR Query Updates

Update handlers to use `IWorkflowQueryService` with batch call (no N+1):

```csharp
// Inside GetMyLeaveRequestsHandler / GetLeaveRequestsHandler:
var entityIds = leaveRequests.Select(r => r.Id.Value).ToList();
var summaries = await workflowQueryService.GetWorkflowSummariesAsync(
    "LeaveRequest", entityIds, ct);
// Map: summaries.GetValueOrDefault(r.Id.Value) → attach to response DTO
```

### Frontend Services

```typescript
// services/hr/managerService.ts
export async function getPendingLeaveApprovals(): Promise<ManagerLeavePendingApproval[]> {
  return apiClient.get("/api/hr/manager/approvals/leave");
}

export async function getPendingOvertimeApprovals(): Promise<ManagerOvertimePendingApproval[]> {
  return apiClient.get("/api/hr/manager/approvals/overtime");
}
```

No `getAllPendingApprovals` service. The "All" tab aggregates client-side:

```typescript
// For the "All" tab:
const { data: leaveApprovals } = usePendingLeaveApprovals();
const { data: overtimeApprovals } = usePendingOvertimeApprovals();

const allApprovals = useMemo(() => {
  const leave = (leaveApprovals ?? []).map(r => ({
    type: "Leave" as const,
    entityType: "LeaveRequest",
    id: r.requestId,
    employeeName: r.employeeName,
    departmentName: r.departmentName,
    summary: `${r.leaveTypeCode} (${r.days} days)`,
    submittedAt: r.submittedAt,
    workflowInstanceId: r.workflowInstanceId,
    currentStepName: r.currentStepName,
  }));
  const overtime = (overtimeApprovals ?? []).map(r => ({
    type: "Overtime" as const,
    entityType: "OvertimeRequest",
    id: r.requestId,
    employeeName: r.employeeName,
    departmentName: r.departmentName,
    summary: `${r.hours}h`,
    submittedAt: r.submittedAt,
    workflowInstanceId: r.workflowInstanceId,
    currentStepName: r.currentStepName,
  }));
  return [...leave, ...overtime].sort(
    (a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime()
  );
}, [leaveApprovals, overtimeApprovals]);
```

### Frontend Hooks

```typescript
// hooks/hr/useManagerApprovals.ts
export function usePendingLeaveApprovals() {
  return useQuery({
    queryKey: ["hr", "manager", "approvals", "leave"],
    queryFn: managerService.getPendingLeaveApprovals,
  });
}

export function usePendingOvertimeApprovals() {
  return useQuery({
    queryKey: ["hr", "manager", "approvals", "overtime"],
    queryFn: managerService.getPendingOvertimeApprovals,
  });
}
```

No `useAllPendingApprovals` hook. The "All" tab uses a derived `useMemo` from the two hooks above.

---

## 12. Migration Impact Analysis

### Current State

| Concern | Current Behavior |
|---------|-----------------|
| `/hr/leave` has My Requests + Pending Approvals + All Requests | Mixed in one page with tabs |
| `/hr/leave` defaults to current user's requests | Employee filter defaulted to profile |
| `/hr/leave` shows no workflow columns | Cannot observe current approver/step |
| `/hr/overtime` shows no workflow columns | Cannot observe current approver/step |
| ESS shows no current approver | Employee cannot track who owns their request |
| No approval center | Users navigate to HR page for approvals |
| No `/manager/*` routes | Missing entirely |

### Migration Actions

| Action | Impact | Risk |
|--------|--------|------|
| Add `/manager/approvals` page + backend | Low (new code, no existing code changed) | Low |
| Add "All" tab via client-side aggregation | Low (no backend changes needed) | Low |
| Create `IWorkflowQueryService` | Low (new shared service) | Low |
| Update ESS DTOs with workflow fields | Low (add fields, backward-compatible) | Low |
| Update HR DTOs with workflow fields | Low (add fields, backward-compatible) | Low |
| Update ESS UI with current approver columns | Low (new columns in table) | Low |
| Update HR UI with workflow observation columns | Low (new columns in table) | Low |
| Remove Pending Approvals tab from `/hr/leave` | Medium (users accustomed to it) | Medium |
| Remove default employee filter from `/hr/leave` | Medium (HR page currently shows only selected employee) | Medium |
| Add Manager backend queries | Low (new code) | Low |
| Add sidebar entry for approvals | Low (new nav item) | Low |

### Backward Compatibility

- Existing `hr/leave` URL continues to work — its content changes but route stays
- Existing `ess/leave` and `ess/overtime` URLs unchanged
- Existing backend API endpoints unchanged except DTOs gain new optional fields
- `GET /api/hr/leave/Leave/GetLeaveRequests` still works for HR (with added workflow fields)
- Old `ApproveLeaveRequest`/`RejectLeaveRequest` endpoints remain (marked `[Obsolete]`)

---

## 13. ADRs

### ADR-027: Three-Scope Architecture Separation

Refer to `docs/architecture/ARCHITECTURE_DECISIONS.md` ADR-027 for full text.

Key rules:
1. Every module must define three scopes: Employee, Approval, HR/Admin
2. Approval actions must use workflow commands, never entity-specific commands
3. CanObserveWorkflow ≠ CanApproveWorkflow
4. `IWorkflowQueryService` is the shared read model for workflow observation
5. A feature is not complete until all three scopes are defined and verified

### ADR-028: Approval Inbox Centralization

Refer to `docs/architecture/ARCHITECTURE_DECISIONS.md` ADR-028 for full text.

Key rules:
1. `/manager/approvals` is the single approval inbox
2. Business modules must not create their own approval pages
3. Forbidden: `/leave/pending`, `/overtime/pending`, `/recruitment/pending`, etc.
4. Business modules expose only ESS (my requests) and HR (all records)
5. Future unified endpoint: `GET /api/hr/workflow/pending-approvals` (not yet)
6. Future combined permission: `approval.inbox.view` or `workflow.approval.view` to replace per-entity permission enumeration in sidebar (not yet)

---

## 14. Verification Plan (DevTools MCP)

### Test 1: Employee Scope
1. Login as regular employee
2. Verify `/ess/leave` shows only own requests
3. Verify ESS leave table includes CurrentApproverName, CurrentStepName, WorkflowStatus columns
4. Verify `/ess/overtime` shows only own requests
5. Verify ESS overtime table includes CurrentApproverName column
6. Verify cannot access `/manager/approvals` (if no approval permission)
7. Verify cannot access `/hr/leave` (if no HR permission)

### Test 2: Approval Scope
1. Login as user with approval permission
2. Verify `/manager/approvals` is accessible from sidebar
3. Verify "All" tab shows all pending items from leave + overtime (client-side combined)
4. Verify Leave tab shows only leave requests where user is current step approver
5. Verify Overtime tab shows only overtime requests where user is current step approver
6. Verify Approve/Reject actions work and update status
7. Verify approved/rejected items disappear from the list
8. Verify items stay visible if workflow has multiple steps

### Test 3: HR Scope
1. Login as HR user
2. Verify `/hr/leave` shows ALL requests (no default employee filter)
3. Verify employee filter, department filter, status filter, date range filter work
4. Verify `/hr/leave` table includes CurrentApproverName, CurrentStepName, WorkflowStatus columns
5. Verify no "Pending Approvals" tab exists
6. Verify `/hr/overtime` shows ALL overtime requests
7. Verify `/hr/overtime` table includes workflow observation columns
8. Verify HR cannot approve via HR page (must use `/manager/approvals`)
9. Verify Force operations work

### Test 4: Workflow Integrity
1. Submit leave request as employee
2. Verify employee can see current approver on ESS page
3. Verify request appears in manager's approval list (Leave tab)
4. Verify it appears in the "All" tab
5. Verify HR can see workflow columns on the same request in `/hr/leave`
6. Approve as manager
7. Verify leave request status changes to "Approved"
8. Verify it disappears from approval list
9. Verify employee sees updated status on ESS page

---

## 15. Implementation Order

### Phase 1 — Backend: IWorkflowQueryService
1. Define `IWorkflowQueryService` interface in `Application/Abstractions`
2. Define `WorkflowSummaryResponse` record
3. Implement `WorkflowQueryService` in `Infrastructure/Services`
4. Register in DI

### Phase 2 — Backend: Manager Queries
1. `GetMyPendingLeaveApprovalsQuery` + Handler
2. `GetMyPendingOvertimeApprovalsQuery` + Handler
3. `ManagerApprovalsController` with 2 endpoints
4. Unit tests for handlers

### Phase 3 — Backend: ESS + HR DTO Updates
1. Update ESS Leave/Overtime queries to use `IWorkflowQueryService`
2. Update HR Leave/Overtime queries to use `IWorkflowQueryService`
3. Add workflow fields to ESS DTOs and HR DTOs
4. Unit tests

### Phase 4 — Frontend: Manager Approval Center
1. `managerService.ts` (2 endpoints — leave + overtime)
2. `useManagerApprovals.ts` (2 hooks + "All" aggregation logic)
3. `/manager/approvals/page.tsx` with All + Leave + Overtime tabs
4. Common `ManagerApprovalTable` component
5. Sidebar entry for `/manager/approvals`
6. i18n messages

### Phase 5 — Frontend: ESS + HR Workflow Columns
1. Update ESS leave table: add CurrentApproverName, CurrentStepName, WorkflowStatus
2. Update ESS overtime table: add workflow columns
3. Update HR leave table: add workflow columns
4. Update HR overtime table: add workflow columns
5. i18n messages for new columns

### Phase 6 — Cleanup: HR Pages
1. Remove Pending Approvals tab from `/hr/leave`
2. Remove default employee filter from `/hr/leave`
3. Remove default employee filter from `/hr/overtime`

### Phase 7 — Browser Validation
1. Execute full verification plan via DevTools MCP
2. Fix any issues found
3. Report results
