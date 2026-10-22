# Phase 27 — Recruitment Request & Recruitment Workflow Completion

**Date:** 2026-06-17
**Status:** Approved
**Last Updated:** 2026-06-17 (v2 — incorporated review refinements)

## Purpose

Complete the recruitment module by implementing the full hiring workflow:
Hiring Need → Recruitment Request → Approval → Recruitment Opening → Job Posting → Candidate Pipeline → Offer → Employee Conversion → Onboarding.

Notification integration via the existing Notification Platform is mandatory.

## Architecture Principles

- Clean Architecture (Domain → Application → Infrastructure → API)
- CQRS + MediatR (no business logic in controllers)
- Strongly Typed IDs via `StronglyTypedId<TValue>` records
- `Entity<TId>` base class for new aggregates (domain events, identity)
- Manual mapper following existing `RecruitmentMapper` pattern (consistent with current recruitment code)
- No breaking changes to existing entities
- No generic approval/workflow engine in Phase 27
- Integration events via MassTransir, published **before** `SaveChangesAsync` (per ADR-025, N6 hardening)
- Notifications via Notification Platform consumers only — no direct SignalR or notification writes from HR
- `xmin` concurrency on all mutable aggregates

## Domain Model

### New: RecruitmentRequest (Aggregate)

```
Entity<RecruitmentRequestId>
├── RequestNumber: string (unique, deterministic format: RR-YYYYMM-XXXX)
├── DepartmentId: DepartmentId
├── PositionId: PositionId
├── RequestedHeadcount: int (> 0)
├── Reason: string
├── PriorityCode: string (RecruitmentRequestPriorityCode constants)
├── RequestedBy: string (UserId)
├── RequestedAt: DateTime
├── Status: string (RecruitmentRequestStatusCode)
├── ApprovedBy: string? (UserId)
├── ApprovedAt: DateTime?
├── RejectedBy: string? (UserId)
├── RejectedAt: DateTime?
├── Comment: string?
├── CreatedAt: DateTime
├── UpdatedAt: DateTime
├── xmin (concurrency token)
```

**Statuses** (`RecruitmentRequestStatusCode`):
- `Draft` → `Submitted` → `Approved` | `Rejected`
- `Draft` → `Cancelled`
- `Submitted` → `Cancelled`

**Domain methods:**
- `Submit(actor, now)` — Draft → Submitted
- `Approve(actor, now, comment?)` — Submitted → Approved
- `Reject(actor, now, comment?)` — Submitted → Rejected
- `Cancel(actor, now)` — Draft or Submitted → Cancelled

**Domain events raised:**
- `RecruitmentRequestSubmittedDomainEvent`
- `RecruitmentRequestApprovedDomainEvent`
- `RecruitmentRequestRejectedDomainEvent`

### New: RecruitmentRequestHistory (ValueObject, following existing EmployeeDepartmentHistory/EmployeePositionHistory pattern)

```
ValueObject base class
├── Id: RecruitmentRequestHistoryId
├── RecruitmentRequestId: RecruitmentRequestId
├── ActionCode: string (e.g., "Submitted", "Approved", "Rejected", "Cancelled")
├── OldStatus: string?
├── NewStatus: string
├── Comment: string?
├── PerformedBy: string (UserId)
├── PerformedAt: DateTime
```

### New: RecruitmentOpening (Aggregate)

**Rationale:** JobPosting (existing) does NOT have PositionId, Headcount, or capacity tracking. It is purely a publication entity (title, description, dates, status). RecruitmentOpening fills the capacity-tracking gap: approval-based headcount management with planned/filled/remaining tracking. It is NOT redundant.

```
Entity<RecruitmentOpeningId>
├── RecruitmentRequestId: RecruitmentRequestId (FK)
├── Code: string
├── PlannedHeadcount: int
├── FilledHeadcount: int
├── RemainingHeadcount: int (computed: PlannedHeadcount - FilledHeadcount)
├── Status: string
├── OpenedAt: DateTime
├── ClosedAt: DateTime?
├── CreatedAt, UpdatedAt
├── xmin
```

**Domain helper:**
- `MarkFilled(int count)` — increments FilledHeadcount by count, recomputes RemainingHeadcount

### Existing: JobPosting (modified)

- Add optional `RecruitmentOpeningId: RecruitmentOpeningId?` FK
- Existing records have `null` (no breaking change)
- Navigation property: `RecruitmentOpening`

### Relationship Diagram

```
RecruitmentRequest 1 ──→ * RecruitmentOpening 1 ──→ * JobPosting
```

## RequestNumber Generation Strategy

Format: `RR-YYYYMM-XXXX` (e.g., `RR-202606-0001`)

Strategy:
1. Seed: `RR-{year}{month:D2}-`
2. Sequence: auto-increment per month, reset monthly
3. Implementation: query max existing number for current month, increment by 1, pad to 4 digits
4. Deterministic, human-readable, sortable by month
5. No GUIDs, no random strings

## PriorityCode

Introduce `RecruitmentRequestPriorityCode` constant class in `Anemoi.Hr.Domain.Recruitment` (consistent pattern with `RequisitionStatusCode`, `LeaveRequestStatusCode`, `PayrollRunStatusCode`, etc.):

```csharp
public static class RecruitmentRequestPriorityCode
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Urgent = "Urgent";
}
```

No magic strings. All priority comparisons use constants.

## Integration Events (Anemoi.Contract.Hr.Events)

Scope strictly limited to Phase 27 request/opening events. No CandidateApplied, no OfferAccepted — those belong to the existing Candidate Pipeline (Phase 25).

```csharp
public sealed record RecruitmentRequestSubmittedIntegrationEvent(
    string RecruitmentRequestId,
    string RequestNumber,
    string RequestedByUser,
    string ApproverUserId,
    string DepartmentId,
    string PositionId,
    int Headcount);

public sealed record RecruitmentRequestApprovedIntegrationEvent(
    string RecruitmentRequestId,
    string RequestNumber,
    string ApprovedBy);

public sealed record RecruitmentRequestRejectedIntegrationEvent(
    string RecruitmentRequestId,
    string RequestNumber,
    string RejectedBy,
    string Reason);

public sealed record RecruitmentOpeningCreatedIntegrationEvent(
    string RecruitmentOpeningId,
    string RecruitmentRequestId,
    string Code,
    int PlannedHeadcount);
```

## CQRS — Commands

All handlers follow this mandatory pattern (ADR-025, N6):

```
1. Fetch entity via ISqlRepository<T>.GetQueryable()
2. Validate existence → return error if null
3. Call domain method (submit/approve/reject/cancel)
4. If domain method returns false → return business error
5. Create history record (RecruitmentRequestHistory)
6. Publish integration events via IPublishEndpoint (BEFORE SaveChanges)
7. Publish DataChangeOccurredIntegrationEvent via IPublishEndpoint (BEFORE SaveChanges)
8. await unitOfWork.SaveChangesAsync()
9. Handle save failure → return concurrency error
10. Map entity to response DTO → return Ok
```

### CreateRecruitmentRequest
- `DepartmentId`, `PositionId`, `RequestedHeadcount`, `Reason`, `PriorityCode`, `RequestedBy`
- Generates RequestNumber deterministically (RR-YYYYMM-XXXX)
- Validates: headcount > 0, department required, position required, priority required
- No integration events (Draft is not yet in workflow)

### UpdateRecruitmentRequest
- Only allowed in Draft status
- Validates same as create

### SubmitRecruitmentRequest
- Draft → Submitted
- Publishes `RecruitmentRequestSubmittedIntegrationEvent` (BEFORE SaveChanges)
- Publishes `DataChangeOccurredIntegrationEvent` (BEFORE SaveChanges)
- Raises `RecruitmentRequestSubmittedDomainEvent`
- Creates history record

### ApproveRecruitmentRequest
- Submitted → Approved
- Publishes `RecruitmentRequestApprovedIntegrationEvent` (BEFORE SaveChanges)
- Publishes `DataChangeOccurredIntegrationEvent` (BEFORE SaveChanges)
- Raises `RecruitmentRequestApprovedDomainEvent`
- Creates history record

### RejectRecruitmentRequest
- Submitted → Rejected
- Publishes `RecruitmentRequestRejectedIntegrationEvent` (BEFORE SaveChanges)
- Publishes `DataChangeOccurredIntegrationEvent` (BEFORE SaveChanges)
- Raises `RecruitmentRequestRejectedDomainEvent`
- Creates history record

### CancelRecruitmentRequest
- Draft or Submitted → Cancelled
- Publishes `DataChangeOccurredIntegrationEvent` (BEFORE SaveChanges)
- Creates history record

### CreateRecruitmentOpening
- Creates opening from approved request
- Validates: request is Approved, headcount > 0
- Publishes `RecruitmentOpeningCreatedIntegrationEvent` (BEFORE SaveChanges)
- Publishes `DataChangeOccurredIntegrationEvent` (BEFORE SaveChanges)

## Conversion Sync (Critical)

When `ConvertCandidateToEmployeeHandler` converts a candidate:
1. Look up the candidate's application → find associated JobPosting → if JobPosting has `RecruitmentOpeningId`, load the RecruitmentOpening
2. Call `opening.MarkFilled(1)` to increment FilledHeadcount and recompute RemainingHeadcount
3. No additional integration event needed for the opening update itself (SaveChanges is sufficient)
4. EmployeeConverted notification: via a new `EmployeeConvertedIntegrationEvent` published from the handler

This ensures RemainingHeadcount is always accurate after every conversion.

**Files modified:** `ConvertCandidateToEmployeeHandler.cs` (add `ISqlRepository<RecruitmentOpening>` + sync logic)

## CQRS — Queries

- `GetRecruitmentRequestById(RecruitmentRequestId)` — single request response
- `GetRecruitmentRequests` — search with: Status, DepartmentId, PositionId, DateRange, pagination
- `GetRecruitmentRequestTimeline(RecruitmentRequestId)` — history ordered by PerformedAt

Dashboard widgets (added to existing `RecruitmentAnalyticsController`, not a separate analytics layer):
- `GetDashboardWidgets` — returns:
  - OpenRequests (Draft + Submitted count)
  - ApprovedRequests (Approved count)
  - RejectedRequests (Rejected count)
  - PendingApprovals (Submitted count)
  - OpenPositions (open RecruitmentOpenings count)
  - Vacancies (sum of RemainingHeadcount across all openings)
  - HiringProgress (active candidates + pipeline count + hired count + conversion rate)

These are simple aggregate queries using the existing `ISqlRepository` pattern — no separate analytics framework.

## EF Core Configuration

New table mappings in `RecruitmentModelMapping.cs`:
- `RecruitmentRequests` — unique index on `RequestNumber`, indexes on `Status`, `DepartmentId`, `PositionId`, `RequestedAt`, `xmin`
- `RecruitmentRequestHistories` — index on `RecruitmentRequestId`, `PerformedAt`
- `RecruitmentOpenings` — index on `RecruitmentRequestId`, `xmin`

JobPosting modifications:
- Add `RecruitmentOpeningId` column (nullable)
- Add FK to `RecruitmentOpenings` table
- Navigation property: `public RecruitmentOpening RecruitmentOpening { get; set; }`

## Notification Integration

### New consumers (Anemoi.Notification.Application.Consumers)

Following the exact pattern in `LeaveRequestConsumers.cs`:

1. **RecruitmentRequestSubmittedConsumer** — consumes `RecruitmentRequestSubmittedIntegrationEvent`
   - Publishes `DataChangeOccurredIntegrationEvent` (resource: `"hr.recruitment.request"`, action: `Create`)
   - Resolves recipient via `INotificationRecipientResolver` (approver user)
   - Creates notification with:
     - `Category: NotificationConstants.Categories.Recruitment`
     - `TargetService: NotificationWorkflowConstants.TargetServices.Recruitment`
     - `ActionCode: NotificationWorkflowConstants.ActionCodes.ViewRecruitmentRequest`
     - `ActionUrl: "/hr/recruitment/requests/{id}"`
     - `TitleLocalizationKey: "notification.recruitment.request.submitted.title"`
     - `ContentLocalizationKey: "notification.recruitment.request.submitted.content"`
     - `DeduplicationKey: $"recruitment-request:{id}:submitted:{userId}"`
     - `Type: NotificationConstants.Types.Business`
     - `Severity: NotificationConstants.Severities.Info`

2. **RecruitmentRequestApprovedConsumer** — consumes `RecruitmentRequestApprovedIntegrationEvent`
   - Publishes `DataChangeOccurredIntegrationEvent` (resource: `"hr.recruitment.request"`, action: `Update`)
   - Notifies the original requester
   - Same notification pattern with appropriate localization keys

3. **RecruitmentRequestRejectedConsumer** — consumes `RecruitmentRequestRejectedIntegrationEvent`
   - Publishes `DataChangeOccurredIntegrationEvent` (resource: `"hr.recruitment.request"`, action: `Update`)
   - Notifies the original requester
   - Same notification pattern with appropriate localization keys

### New constants

**NotificationConstants:**
- `Categories.Recruitment = "Recruitment"` (add to `AllowedCategories`)

**NotificationWorkflowConstants.ActionCodes:**
- `ViewRecruitmentRequest = "ViewRecruitmentRequest"`

**NotificationWorkflowConstants.TargetServices:**
- `Recruitment = "Recruitment"` — already exists, verified.

### DataChange resource names

- `"hr.recruitment.request"` for RecruitmentRequest CRUD
- `"hr.recruitment"` for general recruitment data changes

## Permissions

Additive — no existing permissions changed.

### Backend — Permissions.cs

```csharp
public const string HrRecruitmentRequestView = "hr.recruitment.request.view";
public const string HrRecruitmentRequestCreate = "hr.recruitment.request.create";
public const string HrRecruitmentRequestSubmit = "hr.recruitment.request.submit";
public const string HrRecruitmentRequestApprove = "hr.recruitment.request.approve";
public const string HrRecruitmentRequestManage = "hr.recruitment.request.manage";
```

### Backend — HrPermissions.cs

```csharp
public const string RecruitmentRequestView = Permissions.HrRecruitmentRequestView;
public const string RecruitmentRequestCreate = Permissions.HrRecruitmentRequestCreate;
public const string RecruitmentRequestSubmit = Permissions.HrRecruitmentRequestSubmit;
public const string RecruitmentRequestApprove = Permissions.HrRecruitmentRequestApprove;
public const string RecruitmentRequestManage = Permissions.HrRecruitmentRequestManage;
```

### Frontend — permissions.ts

```typescript
export const PERMISSIONS = {
  // ... existing ...
  HR_RECRUITMENT_REQUEST_VIEW: "hr.recruitment.request.view",
  HR_RECRUITMENT_REQUEST_CREATE: "hr.recruitment.request.create",
  HR_RECRUITMENT_REQUEST_SUBMIT: "hr.recruitment.request.submit",
  HR_RECRUITMENT_REQUEST_APPROVE: "hr.recruitment.request.approve",
  HR_RECRUITMENT_REQUEST_MANAGE: "hr.recruitment.request.manage",
};
```

Route mapping: `/hr/recruitment/requests` → `[HR_RECRUITMENT_REQUEST_VIEW]`

## Localization Keys

### Backend error codes (HrBusinessErrorCodes.cs)

```csharp
// Recruitment Request error codes
RecruitmentRequestNotFound = "HR_REC_REQUEST_NOT_FOUND"
RecruitmentRequestInvalidStatus = "HR_REC_REQUEST_INVALID_STATUS"
RecruitmentRequestHeadcountInvalid = "HR_REC_REQUEST_HEADCOUNT_INVALID"
RecruitmentRequestAlreadySubmitted = "HR_REC_REQUEST_ALREADY_SUBMITTED"
RecruitmentRequestNotApproved = "HR_REC_REQUEST_NOT_APPROVED"
RecruitmentOpeningNotFound = "HR_REC_OPENING_NOT_FOUND"
RecruitmentOpeningExceedsPlanned = "HR_REC_OPENING_EXCEEDS_PLANNED"

// Validation error codes (Recruitment Request)
ValRecruitmentRequestIdRequired = "VAL_RECRUITMENT_REQUEST_ID_REQUIRED"
ValRecruitmentRequestDepartmentRequired = "VAL_RECRUITMENT_REQUEST_DEPARTMENT_REQUIRED"
ValRecruitmentRequestPositionRequired = "VAL_RECRUITMENT_REQUEST_POSITION_REQUIRED"
ValRecruitmentRequestHeadcountPositive = "VAL_RECRUITMENT_REQUEST_HEADCOUNT_POSITIVE"
ValRecruitmentRequestPriorityRequired = "VAL_RECRUITMENT_REQUEST_PRIORITY_REQUIRED"
```

### Frontend (en.json / vi.json)

**HRRecruitment module:**
```
HRRecruitment.requests.title = "Recruitment Requests"
HRRecruitment.requests.description = "Manage hiring requests and approvals"
HRRecruitment.requests.createBtn = "New Request"

HRRecruitment.requests.table.requestNumber = "Request #"
HRRecruitment.requests.table.position = "Position"
HRRecruitment.requests.table.department = "Department"
HRRecruitment.requests.table.headcount = "Headcount"
HRRecruitment.requests.table.priority = "Priority"
HRRecruitment.requests.table.status = "Status"
HRRecruitment.requests.table.requester = "Requester"
HRRecruitment.requests.table.createdAt = "Created"
HRRecruitment.requests.table.actions = "Actions"

HRRecruitment.requests.status.draft = "Draft"
HRRecruitment.requests.status.submitted = "Submitted"
HRRecruitment.requests.status.approved = "Approved"
HRRecruitment.requests.status.rejected = "Rejected"
HRRecruitment.requests.status.cancelled = "Cancelled"

HRRecruitment.requests.dialogs.create.title = "Create Recruitment Request"
HRRecruitment.requests.dialogs.edit.title = "Edit Recruitment Request"
HRRecruitment.requests.dialogs.submit.title = "Submit Request"
HRRecruitment.requests.dialogs.submit.confirm = "Are you sure you want to submit this request?"
HRRecruitment.requests.dialogs.approve.title = "Approve Request"
HRRecruitment.requests.dialogs.approve.confirm = "Approve this recruitment request?"
HRRecruitment.requests.dialogs.reject.title = "Reject Request"
HRRecruitment.requests.dialogs.reject.reason = "Rejection reason"
HRRecruitment.requests.dialogs.cancel.title = "Cancel Request"
HRRecruitment.requests.dialogs.cancel.confirm = "Cancel this request?"

HRRecruitment.requests.priority.low = "Low"
HRRecruitment.requests.priority.medium = "Medium"
HRRecruitment.requests.priority.high = "High"
HRRecruitment.requests.priority.urgent = "Urgent"
```

**Notifications:**
```
notification.recruitment.request.submitted.title = "Recruitment Request Submitted"
notification.recruitment.request.submitted.content = "A new recruitment request {requestNumber} has been submitted for your approval"
notification.recruitment.request.approved.title = "Recruitment Request Approved"
notification.recruitment.request.approved.content = "Your recruitment request {requestNumber} has been approved"
notification.recruitment.request.rejected.title = "Recruitment Request Rejected"
notification.recruitment.request.rejected.content = "Your recruitment request {requestNumber} has been rejected"
```

## Strongly Typed IDs

```csharp
public sealed record RecruitmentRequestId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record RecruitmentRequestHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record RecruitmentOpeningId(Guid Value) : StronglyTypedId<Guid>(Value);
```

## Audit & History

Every status transition creates a `RecruitmentRequestHistory` record with:
- `Id = new RecruitmentRequestHistoryId(IdGenerator.NextGuid())`
- `ActionCode` (the transition name, e.g., "Submitted")
- `OldStatus` (previous status value)
- `NewStatus` (target status value)
- `Comment` (if provided)
- `PerformedBy = HttpContext.GetUserId()`
- `PerformedAt = DateTime.UtcNow`

History is queryable via `GetRecruitmentRequestTimeline`.

Follows existing history pattern used by `EmployeeDepartmentHistory`, `EmployeePositionHistory`, `EmployeeGradeHistory`.

## Response DTOs

```csharp
public sealed record RecruitmentRequestResponse(
    string Id, string RequestNumber, string DepartmentId, string DepartmentName,
    string PositionId, string PositionName, int RequestedHeadcount,
    string Reason, string PriorityCode, string RequestedBy, DateTime RequestedAt,
    string Status, string ApprovedBy, DateTime? ApprovedAt,
    string RejectedBy, DateTime? RejectedAt, string Comment,
    DateTime CreatedAt, DateTime UpdatedAt);

public sealed record RecruitmentRequestHistoryResponse(
    string Id, string RecruitmentRequestId, string ActionCode,
    string OldStatus, string NewStatus, string Comment,
    string PerformedBy, DateTime PerformedAt);

public sealed record RecruitmentOpeningResponse(
    string Id, string RecruitmentRequestId, string Code,
    int PlannedHeadcount, int FilledHeadcount, int RemainingHeadcount,
    string Status, DateTime OpenedAt);

// Dashboard widget DTOs (added to RecruitmentAnalyticsResponses.cs)
public sealed record RecruitmentDashboardWidgetsResponse(
    int OpenRequests, int ApprovedRequests, int RejectedRequests,
    int PendingApprovals, int OpenPositions, int Vacancies,
    int ActiveCandidates, int InPipeline, int Hired, double ConversionRate);
```

## Mapper

Add to existing `RecruitmentMapper.cs`:
- `ToResponse(RecruitmentRequest)` → `RecruitmentRequestResponse`
- `ToResponses(IEnumerable<RecruitmentRequest>)` → `IReadOnlyCollection<RecruitmentRequestResponse>`
- `ToResponse(RecruitmentRequestHistory)` → `RecruitmentRequestHistoryResponse`
- `ToResponses(IEnumerable<RecruitmentRequestHistory>)` → `IReadOnlyCollection<RecruitmentRequestHistoryResponse>`
- `ToResponse(RecruitmentOpening)` → `RecruitmentOpeningResponse`
- `ToResponses(IEnumerable<RecruitmentOpening>)` → `IReadOnlyCollection<RecruitmentOpeningResponse>`

## Files to Create

### Backend (Anemoi.Hr)

**Domain:**
- `Anemoi.Hr.Domain/Recruitment/RecruitmentRequest.cs` (Entity<RecruitmentRequestId>, aggregate)
- `Anemoi.Hr.Domain/Recruitment/RecruitmentRequestStatusCode.cs` (constants)
- `Anemoi.Hr.Domain/Recruitment/RecruitmentRequestPriorityCode.cs` (constants)
- `Anemoi.Hr.Domain/Recruitment/RecruitmentRequestHistory.cs` (ValueObject)
- `Anemoi.Hr.Domain/Recruitment/RecruitmentOpening.cs` (Entity<RecruitmentOpeningId>, aggregate)
- `Anemoi.Hr.Domain/Events/RecruitmentRequestSubmittedDomainEvent.cs` (DomainEvent record)
- `Anemoi.Hr.Domain/Events/RecruitmentRequestApprovedDomainEvent.cs` (DomainEvent record)
- `Anemoi.Hr.Domain/Events/RecruitmentRequestRejectedDomainEvent.cs` (DomainEvent record)

**ModelIds:**
- `Anemoi.Hr.ModelIds/ModelIds/RecruitmentRequestId.cs`
- `Anemoi.Hr.ModelIds/ModelIds/RecruitmentRequestHistoryId.cs`
- `Anemoi.Hr.ModelIds/ModelIds/RecruitmentOpeningId.cs`

**Application/Commands:**
- `.../CreateRecruitmentRequest/CreateRecruitmentRequestCommand.cs`
- `.../CreateRecruitmentRequest/CreateRecruitmentRequestHandler.cs`
- `.../CreateRecruitmentRequest/CreateRecruitmentRequestValidator.cs`
- `.../UpdateRecruitmentRequest/UpdateRecruitmentRequestCommand.cs`
- `.../UpdateRecruitmentRequest/UpdateRecruitmentRequestHandler.cs`
- `.../UpdateRecruitmentRequest/UpdateRecruitmentRequestValidator.cs`
- `.../SubmitRecruitmentRequest/SubmitRecruitmentRequestCommand.cs`
- `.../SubmitRecruitmentRequest/SubmitRecruitmentRequestHandler.cs`
- `.../SubmitRecruitmentRequest/SubmitRecruitmentRequestValidator.cs`
- `.../ApproveRecruitmentRequest/ApproveRecruitmentRequestCommand.cs`
- `.../ApproveRecruitmentRequest/ApproveRecruitmentRequestHandler.cs`
- `.../ApproveRecruitmentRequest/ApproveRecruitmentRequestValidator.cs`
- `.../RejectRecruitmentRequest/RejectRecruitmentRequestCommand.cs`
- `.../RejectRecruitmentRequest/RejectRecruitmentRequestHandler.cs`
- `.../RejectRecruitmentRequest/RejectRecruitmentRequestValidator.cs`
- `.../CancelRecruitmentRequest/CancelRecruitmentRequestCommand.cs`
- `.../CancelRecruitmentRequest/CancelRecruitmentRequestHandler.cs`
- `.../CancelRecruitmentRequest/CancelRecruitmentRequestValidator.cs`
- `.../CreateRecruitmentOpening/CreateRecruitmentOpeningCommand.cs`
- `.../CreateRecruitmentOpening/CreateRecruitmentOpeningHandler.cs`
- `.../CreateRecruitmentOpening/CreateRecruitmentOpeningValidator.cs`

**Application/Queries:**
- `.../GetRecruitmentRequestById/GetRecruitmentRequestByIdQuery.cs`
- `.../GetRecruitmentRequestById/GetRecruitmentRequestByIdHandler.cs`
- `.../GetRecruitmentRequests/GetRecruitmentRequestsQuery.cs`
- `.../GetRecruitmentRequests/GetRecruitmentRequestsHandler.cs`
- `.../GetRecruitmentRequestTimeline/GetRecruitmentRequestTimelineQuery.cs`
- `.../GetRecruitmentRequestTimeline/GetRecruitmentRequestTimelineHandler.cs`

**Application/Responses:**
- Add `RecruitmentDashboardWidgetsResponse` to `RecruitmentAnalyticsResponses.cs`

**Application/Configurations:**
- Update `HrBusinessErrorCodes.cs` (new error codes)
- Update `HrPermissions.cs` (new permissions)

**Infrastructure:**
- Update `RecruitmentModelMapping.cs` (3 new table configs + JobPosting modification)
- Update `RecruitmentServiceInstaller.cs` (if new scoped services needed)

**API/Controllers:**
- `RecruitmentRequestsController.cs`
- `RecruitmentOpeningsController.cs`
- Update `RecruitmentAnalyticsController.cs` (add dashboard widget endpoint)

### Backend (Anemoi.Contract)

- `Anemoi.Contract.Hr/Events/RecruitmentRequestSubmittedIntegrationEvent.cs`
- `Anemoi.Contract.Hr/Events/RecruitmentRequestApprovedIntegrationEvent.cs`
- `Anemoi.Contract.Hr/Events/RecruitmentRequestRejectedIntegrationEvent.cs`
- `Anemoi.Contract.Hr/Events/RecruitmentOpeningCreatedIntegrationEvent.cs`
- `Anemoi.Contract.Hr/Events/EmployeeConvertedIntegrationEvent.cs`

### Backend (Anemoi.Notification)

- `Anemoi.Notification.Application/Consumers/RecruitmentRequestConsumers.cs`
- Update `NotificationConstants.cs` (add Recruitment category)

### Backend (Anemoi.BuildingBlocks)

- Update `Permissions.cs` (new recruitment request permissions)
- Update `NotificationWorkflowConstants.ActionCodes` (add `ViewRecruitmentRequest`)

### Frontend (cody-web-app)

- `.../hr/recruitment/requests/page.tsx`
- `.../hr/recruitment/requests/[id]/page.tsx`
- Dialog components: Create, Edit, Submit, Approve, Reject, Cancel
- Update `types/hr/recruitment.ts`
- Update `services/hr/recruitmentService.ts`
- Update `hooks/hr/useRecruitment.ts`
- Update `constants/permissions.ts`
- Update localization files (en.json, vi.json)

### Tests

- Domain tests for RecruitmentRequest (status transitions, validation)
- Handler tests (command flows, event publishing, history creation)
- Existing tests untouched

## Files to Modify

- `Anemoi.BuildingBlocks/.../Permissions.cs` — add 5 new constants + Definitions
- `Anemoi.BuildingBlocks/.../NotificationWorkflowConstants.cs` — add `ViewRecruitmentRequest`
- `Anemoi.Contract.Notification/.../NotificationConstants.cs` — add `Categories.Recruitment`
- `Anemoi.Hr/.../HrPermissions.cs` — add 5 new references
- `Anemoi.Hr/.../HrBusinessErrorCodes.cs` — add error codes
- `Anemoi.Hr/.../RecruitmentMapper.cs` — add 5 new mapping methods
- `Anemoi.Hr/.../RecruitmentModelMapping.cs` — add 3 entity configs + JobPosting update
- `Anemoi.Hr/.../RecruitmentAnalyticsResponses.cs` — add `RecruitmentDashboardWidgetsResponse`
- `Anemoi.Hr/.../ConvertCandidateToEmployeeHandler.cs` — add RecruitmentOpening sync (FilledHeadcount++)
- `Anemoi.Hr.Domain/Recruitment/JobPosting.cs` — add `RecruitmentOpeningId` + navigation
- Frontend: permissions, types, service, hooks, localization

## Test Plan

### Domain Tests
- Status transition matrix (all valid + invalid transitions: Draft→Submitted→Approved, Draft→Cancelled, Submitted→Rejected, Approved cannot be cancelled, etc.)
- Headcount validation (must be > 0)
- PriorityCode validation
- RequestNumber format validation
- History creation on each transition

### Handler Tests
- Each command handler: success path + all error paths
- Integration event publishing (verify correct event type and payload)
- DataChange event publishing (verify resource, action, query tags)
- History record creation (verify fields on each transition)
- Publish-before-save ordering (verify events are created before SaveChanges)

### Integration Tests
- API endpoints respond correctly
- Validation errors return proper error codes
- Authorization checks work (permission-gated endpoints)

### Build
- `dotnet build Anemoi.sln` — 0 errors, 0 new warnings target

### Test Command
- `dotnet test Anemoi.Hr.Test`

## Deferred Items

- Generic Approval Engine / Workflow Framework (recommended for Phase 28/29)
- Multi-level approval chains (beyond single-step approval)
- Escalation and delegation for recruitment approvals
- Advanced recruitment analytics beyond basic dashboard widgets
- Auto-creation of JobPosting from RecruitmentOpening (manual in Phase 27)
