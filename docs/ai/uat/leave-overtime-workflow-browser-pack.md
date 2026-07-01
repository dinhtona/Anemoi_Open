# Leave & Overtime Browser UAT Pack

Purpose: validate the complete ESS -> Workflow -> Manager Approval -> Status Update flow using browser automation (DevTools MCP when available).

## Personas

- Employee
- Direct Manager
- HR
- Unrelated Manager

## Leave

1. Employee submits a leave request.
2. Verify request appears under My Requests.
3. Verify current workflow status/approver is shown.
4. Verify request appears only in the correct manager approval inbox.
5. Verify unrelated manager cannot see it.
6. Manager approves.
7. Verify employee sees Approved.
8. Verify workflow history exists.
9. Verify notification behavior.
10. Verify HR can see organization-wide request.

## Overtime

Repeat the same flow for overtime.

## Browser Checklist

- Page load
- Console errors
- Network errors
- Runtime exceptions
- API success
- Workflow status updated
- History updated
- Notification behavior
- Permission visibility
- vi/en localization

## Result Format

Report PASS / FAIL / PARTIAL / REVIEWED / BLOCKED for every scenario. Do not report COMPLETE without browser evidence.
