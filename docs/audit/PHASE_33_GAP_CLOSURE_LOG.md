# Phase 33 — Gap Closure Log

**Date:** 2026-06-21

---

## Critical Gaps (Block UAT Readiness)

### GAP-001: Leave Notifications Not Created After Leave Submit
**Status:** Unresolved

**Description:** After submitting a leave request via ESS, no notification is created in the notification center. 23 notifications exist in the system (Overtime + Environment only) but 0 are Leave category.

**Evidence:**
- Leave submit API succeeds (id returned)
- Workflow instance created (2 instances exist)
- Notification center shows 23 items, 0 with Leave category
- `LeaveRequestSubmittedConsumer` exists in Notification module but may not be processing events

**Root Cause (suspected):** The `LeaveRequestSubmittedIntegrationEvent` may not be published by the ESS leave submit handler, or the consumer may not be processing it correctly.

**Recommended Fix:**
1. Verify `LeaveRequestSubmittedIntegrationEvent` is published in `SubmitMyLeaveRequestHandler`
2. Check consumer registration in Notification module DI
3. Check message broker routing (MassTransit/RabbitMQ)
4. Add logging to consumer to verify message reception

---

### GAP-002: Notification Actions Not Populated
**Status:** Documented

**Description:** No notification carries action buttons (Approve/Reject). The `Actions` collection on `NotificationHistory` is never populated during notification creation.

**Evidence:**
- 23 notifications examined, 0 have action buttons
- Backend machinery is fully built: `INotificationActionExecutor`, `WorkflowNotificationExecutors`, `NotificationActionCommandConsumer`
- The gap is in `LeaveRequestSubmittedConsumer` which only sets `ActionUrl: "/hr/leave"` (string) instead of calling `notification.AddAction(...)` with proper `NotificationAction` records

**Recommended Fix:**
1. Update `CreateNotificationCommand` to accept action metadata (AggregateType, AggregateId, WorkflowType)
2. Update consumers to attach `NotificationAction` records with approve/reject action codes
3. Wire through `NotificationWorkflowConstants.ActionCodes` mapping

---

### GAP-003: No Automated Contract Creation After Candidate Conversion
**Status:** Documented

**Description:** When a candidate is converted to an employee, no contract is created. The response includes `RequiresContractCreation: true` but no integration event or automated process handles it.

**Evidence:**
- `ConvertCandidateToEmployeeHandler.cs` does not create a contract
- No domain event or integration event fired after conversion
- `EmployeeContract` is a ValueObject, not an aggregate root

**Recommended Fix:**
1. Publish `CandidateConvertedIntegrationEvent` from the handler
2. Create a consumer that generates a default contract based on employment type
3. Or, add a UI flow that steps from conversion directly to contract creation

---

### GAP-004: PaidLeaveDays Hardcoded to 0 in Payslip
**Status:** Documented

**Description:** When generating a payslip from a finalized payroll run, `PaidLeaveDays` is hardcoded to `0` instead of being sourced from the PayrollRun or AttendanceSummary data.

**Location:** `GeneratePayslipsForPayrollRunHandler.cs:75`

**Evidence:** Browser UI shows "Paid Leave Days: 0" in the payroll run detail for all runs, even when attendance data has leave days.

**Recommended Fix:**
```csharp
// Change from:
PaidLeaveDays = 0,
// To:
PaidLeaveDays = run.PaidLeaveDays,
```

---

## Medium Gaps

### GAP-005: GradeCode Hardcoded to "G1" on Conversion
**Status:** Documented

**Evidence:** `ConvertCandidateToEmployeeHandler.cs` sets `GradeCode = "G1"` regardless of position or department.

### GAP-006: Conversion Doesn't Set Manager
**Status:** Documented

**Evidence:** `ConvertCandidateToEmployeeHandler` doesn't accept or set `DirectManagerEmployeeId`.

### GAP-007: Payroll Recalculate ignores Tax/Insurance/Overtime
**Status:** Documented

**Evidence:** `RecalculatePayrollRunHandler.cs` doesn't fetch TaxCalculationSnapshot, InsuranceCalculationSnapshot, or OvertimeSnapshotProvider.

### GAP-008: Overtime Notifications Have Null Title
**Status:** Documented

**Evidence:** 8 Overtime notifications exist with `title: null`. These may still display correctly in the UI but could cause issues.

---

## Closure Summary

| Gap ID | Severity | Status | Requires New Phase? |
|--------|----------|--------|:---:|
| GAP-001 | Critical | Unresolved | Yes (Phase 34) |
| GAP-002 | Critical | Unresolved | Yes (Phase 34) |
| GAP-003 | High | Documented | Yes (Phase 34) |
| GAP-004 | Medium | Documented | Yes (Phase 34) |
| GAP-005 | Medium | Documented | Phase 34+ |
| GAP-006 | Medium | Documented | Phase 34+ |
| GAP-007 | Low | Documented | Phase 34+ |
| GAP-008 | Low | Documented | Phase 34+ |
