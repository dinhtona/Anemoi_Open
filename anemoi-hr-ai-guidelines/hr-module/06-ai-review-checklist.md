# AI Review Checklist for HR Implementation

Use this checklist after every Codex output.

## Backend checklist

- [ ] Correct layer placement.
- [ ] No DB query directly in controller.
- [ ] Commands/queries created for every action.
- [ ] Mapperly used instead of AutoMapper.
- [ ] FluentValidation used for input validation.
- [ ] Strongly Typed IDs used for local aggregate IDs.
- [ ] Domain entities are not returned directly from API.
- [ ] Permission constants added.
- [ ] Identity seed updated when permissions are added.
- [ ] User-facing messages localized.
- [ ] Stable error codes used.
- [ ] MassTransit events used for cross-service side effects.
- [ ] Sensitive permissions have risk level and safeguards.
- [ ] Leave balance uses transaction ledger.
- [ ] Monthly leave accrual is idempotent.

## Frontend checklist

- [ ] TypeScript strict compatible.
- [ ] No `any`.
- [ ] Types created before UI.
- [ ] API calls are in services.
- [ ] Business logic is in hooks.
- [ ] Components only render UI.
- [ ] All UI text added to vi/en message files.
- [ ] Axios interceptor errors are not duplicated with extra toast.
- [ ] Async mutateAsync handlers catch handled errors.
- [ ] Sensitive actions show warnings and require reason.
- [ ] Compact layout rules followed.

## HR business checklist

- [ ] Employee history is preserved.
- [ ] Department transfer creates history.
- [ ] Leave request supports selected approver.
- [ ] Approval workflow is reusable.
- [ ] Salary changes are immutable/history-based.
- [ ] Contract and salary permissions are sensitive.
- [ ] Audit log is considered for sensitive actions.
