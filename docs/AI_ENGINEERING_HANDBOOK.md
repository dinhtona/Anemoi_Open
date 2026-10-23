# AI Engineering Handbook

This handbook defines the runtime flow, verification evidence, and reporting contract for AI work. The mandatory rules are in `AGENTS.md`; this document does not repeat them.

## 1. Read Order

1. `AGENTS.md`
2. `docs/ai/README.md`
3. `docs/ai/core/verification-standard.md`
4. Relevant task-specific docs under `docs/ai/`

If a required source cannot be read, stop and report `BLOCKED` with the missing path.

## 2. Source Of Truth

The precedence order is defined once, in
[`docs/README.md` § Source-Of-Truth Policy](README.md#source-of-truth-policy).
Apply it as written; this handbook does not define a second order.

Technical debt recommendations do not override approved architecture decisions.

## 3. Runtime Flow

### Step 1: Access Check

Verify repository access and branch/ref. Report `BLOCKED` if access fails.

### Step 2: Read Sources

Read the mandatory files, then the sources for the task type. Resolve conflicts using the precedence above.

### Step 3: Scope The Change

Identify files to change, verification required, and risk areas such as permissions, workflow, payroll, sensitive HR data, or cross-service side effects.

### Step 4: Implement

Make the smallest coherent change, follow existing patterns, and avoid unrelated refactors. Do not mix multiple phases unless explicitly requested.

### Step 5: Verify

Run every required check available in the environment. If a check cannot run, state the gap; do not replace it with inspection.

### Step 6: Report

Summarize scope, files changed, verification actually executed, evidence, gaps, and final status.

## 4. Verification Evidence

Code review is not verification. Valid evidence includes:

- Build and test output from the current session.
- Browser automation / DevTools MCP evidence.
- API request and response details.
- Database queries showing persisted state.
- Workflow instance, history, and target-status evidence.
- Service or worker logs correlated to the change.
- CI status when available.

Invalid evidence includes:

- "Looks correct" or "should work".
- Static inspection reported as `PASS`.
- Assumed or stale test results.
- Screenshots without checking network/API behavior for data-changing flows.

Use only the status vocabulary in `docs/ai/core/verification-standard.md`.

## 5. Browser And Workflow Evidence

A browser report should include the user account, route, actions performed, expected UI result, network requests checked, console errors checked, and final status.

For workflow changes, verify the full path:

1. Submit the target entity.
2. A workflow instance is created from an active definition.
3. The current step resolves to the expected approver.
4. A non-approver cannot approve.
5. The responsible approver can approve or reject.
6. Multi-step workflow advances correctly.
7. Final approval updates the target entity.
8. Rejection updates the target entity according to business rules.
9. Workflow history records the action.
10. Notification behavior is checked when required.

Partial workflow verification is `PARTIAL`, not `PASS`.

## 6. API And Database Evidence

API verification should include authentication context, endpoint and method, request payload, expected and actual status, important response fields, and authorization results for allowed and denied users.

Database verification is required when persistence behavior matters. Verify records, status transitions, audit/history rows, soft-delete or active flags, and concurrency fields where applicable. Do not mutate production data manually to force a pass.

## 7. Reporting Contract

Every implementation report must include:

- Scope.
- Files changed.
- Commit SHA when a commit was made.
- Verification actually executed.
- Evidence summary.
- Known gaps.
- Final status.

Never claim tests passed, a browser was verified, or a commit exists unless the corresponding operation was executed and returned evidence.

## 8. Definition Of Done

Use the checklist in `AGENTS.md` section 9. Documentation-only changes do not require browser validation unless they change user-facing frontend behavior or executable UAT instructions.

## 9. Anti-Patterns

- Reporting `PASS` after only reading code.
- Creating an approval path outside the Workflow Engine.
- Authorizing with role display names instead of permissions.
- Granting permissions from position or department.
- Querying DbContext from controllers or returning EF entities.
- Hard-coding user-facing text.
- Adding permissions outside `Permissions.cs` or without EN/VI keys.
- Using `Guid.NewGuid()` where `IdGenerator.NextGuid()` is required.
- Mixing ESS, Approval, and HR/Admin scopes.
- Silently swallowing missing translation keys.
- Claiming a commit without a SHA.

## 10. Output Contract

Each phase must produce a concrete artifact: source code, tests, documentation, verification guide, migration, configuration, bugfix, or evidence-backed audit report. Placeholder files, brainstorm-only output, and evidence-free reports are not acceptable.

If the expected artifact cannot be produced, report `BLOCKED` with the reason and stop.

## 11. Maintenance

- Put new normative rules in `AGENTS.md`.
- Put detailed guidance in the relevant `docs/ai/` file.
- Keep this handbook focused on runtime flow, evidence, and reporting.
- Update `docs/README.md` when documents move.
