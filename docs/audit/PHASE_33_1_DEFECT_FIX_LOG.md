# Phase 33.1 — Defect Fix Log

**Date:** 2026-06-21

---

## GAP-001: Leave Notifications Not Created After Submit

**Severity:** Critical (blocked notification E2E flow)
**Status:** FIXED

### Root Cause
The `LeaveRequestSubmittedConsumer` was correctly implemented and registered via MassTransit auto-discovery. The event was being published by `SubmitMyLeaveRequestHandler`. However, the notification was being created for the **approver** user (Mai Le), not the **requester** (admin). When checking notifications as admin, the new notification wasn't visible.

Additionally, the `GetNotificationsHandler` did not `.Include(x => x.Actions)` so even if notifications existed, actions were never returned in the API response.

### Fix
1. Fixed `GetNotificationsHandler` to include Actions navigation property
2. GAP-001 was partially a testing methodology issue (checking the wrong user's notifications) - notification creation was actually working

### Verification
- Log confirmed: "Successfully created notification ... category Leave"
- API confirmed: notification visible with 3 action buttons
- Browser confirmed: "New Leave Request" notification in notification center

---

## GAP-002: Notifications Don't Have Action Buttons

**Severity:** Critical
**Status:** FIXED

### Root Cause
Three issues:
1. `CreateNotificationCommand` had no way to pass action data or aggregate metadata
2. `CreateNotificationHandler` did not call `notification.AddAction()`
3. Executor resolution used `action.ActionCode` which conflicted with specific action codes (e.g., "ApproveLeaveRequest" vs "Leave")

### Fix
1. Created `CreateNotificationActionInput` DTO
2. Extended `CreateNotificationCommand` with `Actions`, `AggregateType`, `AggregateId`, `WorkflowType`, `WorkflowState`
3. Updated `CreateNotificationHandler` to process actions
4. Updated executor resolution to use `notification.AggregateType` first, fall back to `action.ActionCode`
5. Updated all executors to use aggregate type names (e.g., "LeaveRequest")
6. Updated `LeaveRequestSubmittedConsumer` to pass approve/reject actions

### Files Changed
| File | Change |
|------|--------|
| `.../CreateNotificationActionInput.cs` | New DTO for action data |
| `.../CreateNotificationCommand.cs` | Added Actions, AggregateType, etc. |
| `.../CreateNotificationHandler.cs` | Added action processing |
| `.../ExecuteNotificationActionHandler.cs` | Changed executor resolution |
| `.../WorkflowNotificationExecutors.cs` | Updated executor ActionCodes |
| `.../LeaveRequestConsumers.cs` | Added approve/reject actions |
| `.../GetNotificationsHandler.cs` | Added .Include(x => x.Actions) |

### Verification
- DB confirmed: 3 action records per notification (Approve, Reject, View)
- API confirmed: `actions` array populated in notification response
- API confirmed: approve action executes successfully
- Browser confirmed: Approve/Reject buttons visible in notification center

---

## GAP-003: No Auto-Contract After Candidate Conversion

**Severity:** High (product gap, not a bug)
**Status:** DOCUMENTED — Will not fix in Phase 33.x

### Assessment
`ConvertCandidateToEmployeeHandler` returns `RequiresContractCreation: true` but does not create a contract or fire an integration event. Contract creation requires a separate API call (`POST /api/hr/contract/Contract/CreateContract`).

### Recommendation
Add a `CandidateConvertedIntegrationEvent` in a future phase. Create a consumer that automatically generates a default contract based on employment type and the employee's department/position. This is separate from the notification E2E gap and does not block UAT readiness.
