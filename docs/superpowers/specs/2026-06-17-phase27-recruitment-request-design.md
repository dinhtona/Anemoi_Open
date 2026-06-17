# Phase 27 — Recruitment Request & Recruitment Workflow Completion

**Date:** 2026-06-17
**Status:** Approved
**Last Updated:** 2026-06-17

## Purpose

Complete the recruitment module by implementing the full hiring workflow:
Hiring Need → Recruitment Request → Approval → Recruitment Opening → Job Posting → Candidate Pipeline → Offer → Employee Conversion → Onboarding.

Notification integration via the existing Notification Platform is mandatory.

## Architecture Principles

- Clean Architecture (Domain → Application → Infrastructure → API)
- CQRS + MediatR (no business logic in controllers)
- Strongly Typed IDs via `StronglyTypedId<TValue>` records
- `Entity<TId>` base class for new aggregates (domain events, identity)
- Mapperly for mapping (or manual mapper following existing `RecruitmentMapper` pattern)
- No breaking changes to existing entities
- No generic approval/workflow engine in Phase 27
- Integration events via MassTransit, published **before** `SaveChangesAsync` (per ADR-025)
- Notifications via Notification Platform consumers only — no direct SignalR or notification writes from HR

## Domain Model

### New: RecruitmentRequest (Aggregate)

```
Entity<RecruitmentRequestId>
├── RequestNumber: string (unique)
├── DepartmentId: DepartmentId
├── PositionId: PositionId
├── RequestedHeadcount: int (> 0)
├── Reason: string
├── PriorityCode: string (e.g., "High", "Medium", "Low")
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

### New: RecruitmentRequestHistory

```
Entity<RecruitmentRequestHistoryId>
├── RecruitmentRequestId: RecruitmentRequestId
├── ActionCode: string (e.g., "Submitted", "Approved", "Rejected", "Cancelled")
├── OldStatus: string?
├── NewStatus: string
├── Comment: string?
├── PerformedBy: string (UserId)
├── PerformedAt: DateTime
```

### New: RecruitmentOpening (Aggregate)

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
- `UpdateFilledHeadcount(int filled)` — updates FilledHeadcount, auto-computes RemainingHeadcount

### Existing: JobPosting (modified)

- Add optional `RecruitmentOpeningId: RecruitmentOpeningId?` FK
- Existing records have `null` (no breaking change)
- Navigation property: `RecruitmentOpening`

### Relationship Diagram

```
RecruitmentRequest 1 ──→ * RecruitmentOpening 1 ──→ * JobPosting
```

## Integration Events (Anemoi.Contract.Hr.Events)

New records in the Contract project:

```csharp
public sealed record RecruitmentRequestSubmittedIntegrationEvent(
    string RecruitmentRequestId,
    string RequestNumber,
    string RequestedBy,
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

### CreateRecruitmentRequest
- `DepartmentId`, `PositionId`, `RequestedHeadcount`, `Reason`, `PriorityCode`, `RequestedBy`
- Validates: headcount > 0, department required, position required

### UpdateRecruitmentRequest
- Only allowed in Draft status
- Validates same as create

### SubmitRecruitmentRequest
- Draft → Submitted
- Publishes `RecruitmentRequestSubmittedIntegrationEvent` + `DataChangeOccurredIntegrationEvent`
- Raises `RecruitmentRequestSubmittedDomainEvent`
- Creates history record

### ApproveRecruitmentRequest
- Submitted → Approved
- Publishes `RecruitmentRequestApprovedIntegrationEvent` + `DataChangeOccurredIntegrationEvent`
- Raises `RecruitmentRequestApprovedDomainEvent`
- Creates history record

### RejectRecruitmentRequest
- Submitted → Rejected
- Publishes `RecruitmentRequestRejectedIntegrationEvent` + `DataChangeOccurredIntegrationEvent`
- Raises `RecruitmentRequestRejectedDomainEvent`
- Creates history record

### CancelRecruitmentRequest
- Draft or Submitted → Cancelled
- Publishes `DataChangeOccurredIntegrationEvent`
- Creates history record

### CreateRecruitmentOpening
- Creates opening from approved request
- Validates: request is Approved
- Publishes `RecruitmentOpeningCreatedIntegrationEvent` + `DataChangeOccurredIntegrationEvent`

## CQRS — Queries

- `GetRecruitmentRequestById(RecruitmentRequestId)`
- `GetRecruitmentRequests` — search with filters: Status, DepartmentId, PositionId, DateRange, pagination
- `GetRecruitmentRequestTimeline(RecruitmentRequestId)` — returns history ordered by PerformedAt
- `GetRecruitmentCapacity` — total planned, filled, remaining across all openings
- `GetHiringProgress` — conversion metrics: requests → openings → candidates → hires

## Dashboard Enhancements

Add to existing `RecruitmentAnalyticsController`:
- OpenRequests (Draft + Submitted)
- ApprovedRequests (Approved)
- RejectedRequests (Rejected)
- PendingApprovals (Submitted)
- RecruitmentCapacity (planned/filled/remaining)
- HiringProgress (pipeline metrics)

Response DTOs in `RecruitmentAnalyticsResponses.cs`.

## EF Core Configuration

New table mappings in `RecruitmentModelMapping.cs`:
- `RecruitmentRequests` — unique index on `RequestNumber`, indexes on `Status`, `DepartmentId`, `PositionId`, `RequestedAt`, `xmin`
- `RecruitmentRequestHistories` — index on `RecruitmentRequestId`, `PerformedAt`
- `RecruitmentOpenings` — index on `RecruitmentRequestId`, `xmin`

JobPosting modifications:
- Add `RecruitmentOpeningId` column (nullable)
- Add FK + navigation

## Notification Integration

### New consumers (Anemoi.Notification.Application.Consumers)

Following the exact pattern in `LeaveRequestConsumers.cs`:

1. **RecruitmentRequestSubmittedConsumer** — consumes `RecruitmentRequestSubmittedIntegrationEvent`
   - Publishes `DataChangeOccurredIntegrationEvent` (resource: `"hr.recruitment.request"`)
   - Resolves recipient via `INotificationRecipientResolver`
   - Creates notification with `Category: "Recruitment"`, `Type: "Business"`, `Severity: "Info"`

2. **RecruitmentRequestApprovedConsumer** — consumes `RecruitmentRequestApprovedIntegrationEvent`
   - Same pattern, notifies requester

3. **RecruitmentRequestRejectedConsumer** — consumes `RecruitmentRequestRejectedIntegrationEvent`
   - Same pattern, notifies requester

### New constants

- `NotificationConstants.Categories.Recruitment = "Recruitment"` (add to `AllowedCategories`)

## Permissions

### Backend

```csharp
// Permissions.cs
public const string HrRecruitmentRequestView = "hr.recruitment.request.view";
public const string HrRecruitmentRequestCreate = "hr.recruitment.request.create";
public const string HrRecruitmentRequestSubmit = "hr.recruitment.request.submit";
public const string HrRecruitmentRequestApprove = "hr.recruitment.request.approve";
public const string HrRecruitmentRequestManage = "hr.recruitment.request.manage";

// HrPermissions.cs
// Same references as above
```

### Frontend

```typescript
HR_RECRUITMENT_REQUEST_VIEW: "hr.recruitment.request.view",
HR_RECRUITMENT_REQUEST_CREATE: "hr.recruitment.request.create",
HR_RECRUITMENT_REQUEST_SUBMIT: "hr.recruitment.request.submit",
HR_RECRUITMENT_REQUEST_APPROVE: "hr.recruitment.request.approve",
HR_RECRUITMENT_REQUEST_MANAGE: "hr.recruitment.request.manage",
```

## Localization Keys

### Backend (error codes)

```csharp
// HrBusinessErrorCodes.cs additions
RecruitmentRequestNotFound = "HR_REC_REQUEST_NOT_FOUND"
RecruitmentRequestInvalidStatus = "HR_REC_REQUEST_INVALID_STATUS"
RecruitmentRequestHeadcountInvalid = "HR_REC_REQUEST_HEADCOUNT_INVALID"
RecruitmentRequestAlreadySubmitted = "HR_REC_REQUEST_ALREADY_SUBMITTED"
RecruitmentRequestNotApproved = "HR_REC_REQUEST_NOT_APPROVED"
RecruitmentOpeningNotFound = "HR_REC_OPENING_NOT_FOUND"
RecruitmentOpeningExceedsPlanned = "HR_REC_OPENING_EXCEEDS_PLANNED"
```

### Frontend (en.json / vi.json)

```
HRRecruitment.requests.title, .description, .createBtn
HRRecruitment.requests.table.* (requestNumber, position, department, headcount, priority, status, requester, createdAt, actions)
HRRecruitment.requests.dialogs.* (create, edit, submit, approve, reject, cancel)
HRRecruitment.requests.status.* (draft, submitted, approved, rejected, cancelled)
RecruitmentRequestHistory.action.* (submitted, approved, rejected, cancelled)

notification.recruitment.request.submitted.title
notification.recruitment.request.submitted.content
notification.recruitment.request.approved.title
notification.recruitment.request.approved.content
notification.recruitment.request.rejected.title
notification.recruitment.request.rejected.content
```

## Strongly Typed IDs

```csharp
public sealed record RecruitmentRequestId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record RecruitmentRequestHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record RecruitmentOpeningId(Guid Value) : StronglyTypedId<Guid>(Value);
```

## Audit & History

Every status transition creates a `RecruitmentRequestHistory` record with:
- ActionCode (the transition name)
- OldStatus (previous status)
- NewStatus (target status)
- Comment (if provided)
- PerformedBy (UserId from HttpContext)
- PerformedAt (DateTime.UtcNow)

History is queryable via `GetRecruitmentRequestTimeline`.

## Handler Pattern (All Handlers Must Follow)

```
1. Fetch entity via ISqlRepository<T>.GetQueryable()
2. Validate existence → return error if null
3. Call domain method (submit/approve/reject/cancel)
4. If domain method returns false → return business error
5. Create history record (RecruitmentRequestHistory)
6. Publish integration events via IPublishEndpoint
7. Publish DataChangeOccurredIntegrationEvent via IPublishEndpoint
8. await unitOfWork.SaveChangesAsync()
9. Handle save failure → return concurrency error
10. Map entity to response DTO → return Ok
```

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

public sealed record RecruitmentCapacityResponse(
    int TotalPlanned, int TotalFilled, int TotalRemaining);

public sealed record HiringProgressResponse(
    int OpenPositions, int ActiveCandidates, int InPipeline,
    int Hired, double ConversionRate);
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
- `Anemoi.Hr.Domain/Recruitment/RecruitmentRequest.cs`
- `Anemoi.Hr.Domain/Recruitment/RecruitmentRequestStatusCode.cs`
- `Anemoi.Hr.Domain/Recruitment/RecruitmentRequestHistory.cs`
- `Anemoi.Hr.Domain/Recruitment/RecruitmentOpening.cs`
- `Anemoi.Hr.Domain/Events/RecruitmentRequestSubmittedDomainEvent.cs`
- `Anemoi.Hr.Domain/Events/RecruitmentRequestApprovedDomainEvent.cs`
- `Anemoi.Hr.Domain/Events/RecruitmentRequestRejectedDomainEvent.cs`

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
- `.../GetRecruitmentCapacity/GetRecruitmentCapacityQuery.cs`
- `.../GetRecruitmentCapacity/GetRecruitmentCapacityHandler.cs`
- `.../GetHiringProgress/GetHiringProgressQuery.cs`
- `.../GetHiringProgress/GetHiringProgressHandler.cs`

**Application/Responses:**
- Add to `RecruitmentAnalyticsResponses.cs` (new DTOs)

**Application/Configurations:**
- Update `HrBusinessErrorCodes.cs` (new error codes)
- Update `HrPermissions.cs` (new permissions)

**Infrastructure:**
- Update `RecruitmentModelMapping.cs` (new table configs + JobPosting modification)
- Update `RecruitmentServiceInstaller.cs` (if needed)

**API/Controllers:**
- `RecruitmentRequestsController.cs`
- `RecruitmentOpeningsController.cs`
- Update `RecruitmentAnalyticsController.cs` (dashboard additions)

### Backend (Anemoi.Contract)

- `Anemoi.Contract.Hr/Events/RecruitmentRequestSubmittedIntegrationEvent.cs`
- `Anemoi.Contract.Hr/Events/RecruitmentRequestApprovedIntegrationEvent.cs`
- `Anemoi.Contract.Hr/Events/RecruitmentRequestRejectedIntegrationEvent.cs`
- `Anemoi.Contract.Hr/Events/RecruitmentOpeningCreatedIntegrationEvent.cs`

### Backend (Anemoi.Notification)

- `Anemoi.Notification.Application/Consumers/RecruitmentRequestConsumers.cs`
- Update `NotificationConstants.cs` (add Recruitment category)

### Backend (Anemoi.BuildingBlocks)

- Update `Permissions.cs` (new recruitment request permissions)

### Frontend (cody-web-app)

- `.../hr/recruitment/requests/page.tsx`
- `.../hr/recruitment/requests/[id]/page.tsx`
- Update types, service, hooks
- Create dialog components
- Update permissions.ts
- Update localization files

### Tests

- Domain tests for RecruitmentRequest
- Handler tests
- Existing tests unchanged

## Files to Modify

- `Anemoi.BuildingBlocks/.../Permissions.cs`
- `Anemoi.Hr/.../HrPermissions.cs`
- `Anemoi.Hr/.../HrBusinessErrorCodes.cs`
- `Anemoi.Hr/.../RecruitmentMapper.cs`
- `Anemoi.Hr/.../RecruitmentModelMapping.cs`
- `Anemoi.Hr/.../RecruitmentAnalyticsResponses.cs`
- `Anemoi.Hr.Domain/Recruitment/JobPosting.cs` (add RecruitmentOpeningId)
- `Anemoi.Contract.Notification/.../NotificationConstants.cs`
- Frontend permissions, types, service, hooks, localization

## Test Plan

### Domain Tests
- Status transition matrix (all valid + invalid transitions)
- Headcount validation
- Approval validation (cannot approve non-submitted)
- Rejection validation (cannot reject non-submitted)
- Cancel validation (cannot cancel approved)
- History creation on each transition

### Handler Tests
- Each command handler: success path + error paths
- Integration event publishing
- DataChange event publishing
- History record creation

### Test Commands
- `dotnet test Anemoi.Hr.Test`
- `dotnet build --no-restore` (0 errors, 0 warnings target)

## Deferred Items

- Generic Approval Engine / Workflow Framework (Phase 28/29)
- Full RecruitmentOpening → JobPosting automation
- Advanced recruitment analytics (beyond dashboard basics)
- Multi-level approval chains
- Escalation and delegation
