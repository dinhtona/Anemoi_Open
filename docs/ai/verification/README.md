# Verification Index

This directory is the verification map for Anemoi_Open AI engineering work. It connects the core verification standard, change-type checklists, browser UAT packs, and reporting vocabulary into one practical entry point.

Use this file before claiming `PASS`, `VERIFIED`, `COMPLETE`, `PRODUCTION READY`, or `UAT READY`.

## Purpose

Verification proves that a change actually works in the current repository state. Static inspection can support confidence, but it is not execution evidence.

This index helps agents and reviewers:

- choose the correct verification path for a change;
- identify required evidence before reporting status;
- avoid fake or stale verification claims;
- link implementation reports to reproducible checks;
- keep browser, API, database, workflow, event, and security evidence consistent.

The authoritative status vocabulary and minimum evidence rules are defined in [`../core/verification-standard.md`](../core/verification-standard.md).

## When Verification Is Required

Verification is required whenever a change affects behavior, runtime safety, user experience, permissions, persisted data, integration events, workflows, payroll, or sensitive HR data.

Verification is also required for documentation changes that define engineering rules, source-of-truth order, AI execution policy, security expectations, workflow behavior, or UAT procedures. For documentation-only changes, verification normally means repository read/write confirmation and link/path review, not application execution.

Do not report `PASS` when only inspection was performed. Use `REVIEWED` unless the required checks were actually executed in the current session.

## Verification Source-Of-Truth Order

When verification guidance conflicts, use this order:

1. [`../../architecture/ARCHITECTURE_DECISIONS.md`](../../architecture/ARCHITECTURE_DECISIONS.md)
2. [`../../../ArchitectureGuide.md`](../../../ArchitectureGuide.md)
3. [`../core/verification-standard.md`](../core/verification-standard.md)
4. [`../core/backend-rules.md`](../core/backend-rules.md)
5. [`../core/frontend-rules.md`](../core/frontend-rules.md)
6. [`../security/`](../security/)
7. [`../workflows/`](../workflows/)
8. This verification index
9. Relevant UAT packs under [`../uat/`](../uat/)
10. Relevant module guidance under [`../modules/`](../modules/)
11. The current implementation prompt
12. Historical reports, archived notes, or chat summaries

Historical reports can explain context, but they cannot prove the current change works.

## Verification By Change Type

### Documentation-Only

Use when the change only edits documentation, prompts, maps, or guidance files.

Required checks:

- confirm the target files were read from the current branch;
- confirm links point to existing paths where practical;
- confirm there are no placeholder sections, stale phase claims, or fake test results;
- confirm the file was written through GitHub or the local workspace;
- return the real commit SHA if committed.

Allowed final status is normally `REVIEWED`. Use `PASS` only when a documentation-specific automated check was actually executed and passed.

### Backend

Use when the change touches C#, CQRS handlers, domain logic, validators, mappings, controllers, workers, authorization attributes, or service registration.

Required checks:

- `dotnet build Anemoi.sln`;
- relevant unit/integration tests;
- API checks for changed endpoints when behavior is exposed over HTTP;
- logs for workers or background processing when relevant.

### Frontend

Use when the change touches `cody-web-app`, routes, pages, components, hooks, services, schemas, i18n, or permission visibility.

Required checks:

- frontend build and lint/type checks used by the project;
- browser validation through available browser automation / DevTools MCP;
- page load check;
- console error check;
- network error check;
- main user flow check;
- permission and localization checks when affected.

Frontend-impacting work is not complete with TypeScript/build checks alone.

### Full-Stack

Use when a change crosses backend and frontend boundaries.

Required checks:

- backend build and relevant tests;
- frontend build and browser validation;
- API request/response evidence;
- authorization checks for success and expected denial paths;
- database read/write verification when persistence changes;
- logs for relevant services.

### Workflow

Use when the change affects approval routing, `WorkflowDefinition`, workflow instances, submit/start, approve/reject, target status updates, workflow roles, or manager approval views.

Required checks:

- active `WorkflowDefinition` exists for definition-bound entity types;
- submit/start creates the expected workflow instance;
- pending approval is visible only to the responsible approver;
- approve/reject goes through workflow commands or approved workflow APIs;
- target entity status updates correctly;
- workflow history is append-only and readable;
- notification/event evidence exists when workflow events are expected.

### Permission / Security

Use when the change touches permissions, role groups, authorization attributes, JWT claims, identity mapping, sensitive permission assignment, or UI visibility rules.

Required checks:

- central permission constant and definition are present when a permission is added;
- EN/VI permission metadata exists when permission metadata is changed;
- protected endpoints use permission constants and `[HasPermission]` where applicable;
- positive authorization check succeeds for an allowed user;
- negative authorization check fails for a disallowed user;
- frontend visibility uses `user.permissions`, not display role names;
- audit evidence exists for sensitive HR/security changes where applicable.

### Database / Migration

Use when the change touches EF mappings, migrations, schema, indexes, seed data, concurrency fields, or persisted state.

Required checks:

- build succeeds;
- migration is generated or explicitly not required with reason;
- schema is applied or verified in the target environment used for testing;
- read/write verification proves the changed schema works;
- concurrency behavior such as `xmin` is preserved where used;
- seed data is verified when the change depends on it.

### Notification / Event-Driven

Use when the change touches domain events, integration events, MassTransit consumers, outbox behavior, notification records, SignalR hubs, or worker processing.

Required checks:

- event is published from the intended state transition;
- consumer handles the event and is idempotent where duplicate delivery is possible;
- notification or side effect is persisted;
- logs show processing success or failure details;
- duplicate-event risk is reviewed for all active paths;
- browser/API evidence exists when the notification is user-visible.

### Payroll / Sensitive HR Data

Use when the change touches payroll, payslips, salary, tax, insurance, leave balances, attendance classifications, contracts, employee lifecycle, or other sensitive HR data.

Required checks:

- backend build and relevant tests;
- database verification for persisted sensitive values;
- authorization checks for ESS, Approval, and HR/Admin scopes as applicable;
- audit/log evidence for sensitive writes;
- snapshot behavior is preserved for payroll and payslip records;
- no cross-employee data exposure in ESS;
- browser/API evidence for user-visible sensitive flows.

## Required Evidence By Verification Type

### Build Evidence

Include the command, scope, result, and whether warnings are relevant.

Example:

```text
Command: dotnet build Anemoi.sln
Result: PASS
Evidence: build completed with 0 errors, 0 warnings
```

### Test Evidence

Include the command, number of tests, failures, and whether the tests were run after the change.

Example:

```text
Command: dotnet test Anemoi.sln
Result: PASS
Evidence: 486 passed, 0 failed
```

### Browser Evidence

Include route, user persona, visible result, console errors, network errors, runtime exceptions, and the main flow tested.

Example:

```text
Route: /en/manager/approvals
Persona: direct manager
Result: PASS
Evidence: page loaded, pending leave visible, approval succeeded, no console errors, no failed API requests
```

### API Evidence

Include method, endpoint, persona/auth context, request intent, response status, and key response fields.

Example:

```text
POST /api/hr/workflow/{id}/approve
Persona: valid approver
Result: PASS
Evidence: 200 response, workflow status advanced to next step
```

### Database Evidence

Include the table or read model checked, query intent, and persisted result. Do not expose secrets or unnecessary personal data.

Example:

```text
Table: workflow_instances
Result: PASS
Evidence: instance exists for target entity and status is Pending
```

### Workflow History Evidence

Include workflow instance, action, actor scope, timestamp presence, and resulting target status.

Example:

```text
History: Submit + Approve recorded
Result: PASS
Evidence: append-only history contains expected actions and target entity status changed to Approved
```

### Log Evidence

Include service/worker name, time window, success or failure line, and correlation id when available.

Example:

```text
Service: notification worker
Result: PASS
Evidence: consumer processed WorkflowApprovedIntegrationEvent without error
```

### GitHub Actions Evidence

Use when available and relevant. Include workflow name, run URL or run id, commit SHA, status, and conclusion.

Example:

```text
Workflow: CI
Commit: <sha>
Result: PASS
Evidence: GitHub Actions run completed successfully
```

If no GitHub Actions workflow exists or no run is available for the commit, report `N/A` with reason, not `PASS`.

## How To Choose The Right Guide

Use the narrowest guide that covers the highest-risk part of the change:

1. If the change touches workflow approval state, use Workflow verification first.
2. If the change touches authorization, identity, role groups, or sensitive permissions, use Permission / Security verification first.
3. If the change touches payroll, payslips, salary, attendance, leave balances, contracts, or sensitive employee records, use Payroll / Sensitive HR Data verification first.
4. If the change touches schema or seed data, add Database / Migration verification.
5. If the change touches events, consumers, notifications, or hubs, add Notification / Event-Driven verification.
6. If the change touches frontend behavior, add Frontend or Full-Stack browser validation.
7. If the change is documentation-only, use Documentation-Only verification and do not claim runtime execution.

A change can require multiple verification types. The final status must reflect the weakest required area.

## Existing Verification Guides

Canonical verification rules:

- [`../core/verification-standard.md`](../core/verification-standard.md) - status vocabulary and minimum evidence rules
- [`../core/frontend-rules.md`](../core/frontend-rules.md) - frontend completion and browser validation expectations
- [`../core/backend-rules.md`](../core/backend-rules.md) - backend engineering and verification expectations
- [`../workflows/approval-workflow.md`](../workflows/approval-workflow.md) - workflow architecture and approval verification expectations
- [`../security/`](../security/) - permission, audit, and sensitive-access verification expectations

Directory maps:

- [`./`](./) - verification guides and this index
- [`../uat/`](../uat/) - browser UAT packs for product flows

During AI-003, no additional subordinate file under `docs/ai/verification/` was confirmed through the GitHub Connector. Do not add guessed links. Add new links here only after the target file exists on the repository branch.

## Browser UAT Packs

Browser UAT packs live under [`../uat/`](../uat/). Use them for route-level and persona-level validation after frontend or full-stack changes.

When selecting a UAT pack:

- match the product area first, such as ESS, Manager Approval, HR Admin, Recruitment, Onboarding, Payroll, or Workflow;
- match the persona second, such as Employee, Manager, HR, Recruiter, or Administrator;
- execute both positive and negative paths when permissions are involved;
- record console, network, runtime, API, and visible UI evidence.

Do not invent a UAT pack result. If no matching pack is available, run a focused browser validation and report the gap as `PARTIAL` or `REVIEWED`, depending on what was actually executed.

## Status Usage Rules

Use these statuses exactly:

- `PASS`: required checks were executed in the current session and passed.
- `FAILED`: required checks were executed and at least one failed.
- `PARTIAL`: some required checks were executed, but at least one required check was skipped, unavailable, or incomplete.
- `REVIEWED`: static inspection, code review, documentation review, or link review only; no runtime verification for that scope.
- `BLOCKED`: verification or implementation could not proceed; explain the blocking condition.
- `N/A`: the check does not apply; explain why.

Do not use `PASS` for a scope if any required evidence for that scope is missing. Do not use `COMPLETE`, `VERIFIED`, `PRODUCTION READY`, or `UAT READY` unless the required evidence supports that claim.

## Anti-Fake-Verification Rules

Agents must not:

- claim commands were run when they were not run;
- reuse old test output from before the change;
- claim browser validation from screenshots alone for data-changing flows;
- infer API success from frontend rendering alone;
- infer database persistence from API success alone when persistence is the risk;
- report GitHub Actions success without checking the run for the relevant commit;
- claim a commit unless GitHub or git returns a real commit SHA;
- claim a file was created unless the write operation succeeded;
- hide missing checks behind vague wording such as "should work", "looks good", or "validated by review";
- mark workflow changes as `PASS` without workflow history and target status evidence;
- mark security changes as `PASS` without both allowed and denied authorization evidence.

If evidence is incomplete, say exactly what was and was not verified.

## Minimum Evidence Matrix

| Change type | Build | Tests | Browser | API | Database | Workflow history | Logs | GitHub Actions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Documentation-only | N/A unless docs tooling exists | N/A unless docs tests exist | N/A | N/A | N/A | N/A | N/A | Optional if configured |
| Backend | Required | Required | N/A unless user-visible | Required for endpoint changes | Required for persistence changes | Required for workflow changes | Required for workers/events | Optional if configured |
| Frontend | Required | Recommended if available | Required | Required for data flows | N/A unless persistence risk exists | Required for workflow UI | Required when diagnosing failures | Optional if configured |
| Full-stack | Required | Required | Required | Required | Required for persistence changes | Required for workflow changes | Required for workers/events | Optional if configured |
| Workflow | Required | Required | Required if UI touched | Required | Required for target state | Required | Required for events/notifications | Optional if configured |
| Permission/security | Required | Required where available | Required if UI touched | Required allowed + denied | Required for assignment persistence | Required if workflow responsibility changes | Required for sensitive changes | Optional if configured |
| Database/migration | Required | Required where available | N/A unless UI affected | Required for read/write path | Required | Required if workflow state affected | Required for migration/runtime errors | Optional if configured |
| Notification/event-driven | Required | Required where available | Required if user-visible | Required if API-triggered | Required for persisted notification | Required if workflow event | Required | Optional if configured |
| Payroll/sensitive HR data | Required | Required | Required if UI touched | Required | Required | Required if approval-gated | Required for sensitive writes | Optional if configured |

`Required` means the check must be executed before reporting `PASS` for that scope. `Optional if configured` means check it when available; otherwise report `N/A` with a reason.

## Example Verification Report Format

Use this format in implementation reports:

```text
Repository: dinhtona/Anemoi_Open
Branch: dev
Commit: <real commit SHA or N/A>

Files changed:
- <path>

Change type:
- <documentation-only | backend | frontend | full-stack | workflow | permission/security | database/migration | notification/event-driven | payroll/sensitive HR data>

Verification performed:
- Build: <PASS | FAILED | PARTIAL | REVIEWED | BLOCKED | N/A> - <command/evidence/reason>
- Tests: <PASS | FAILED | PARTIAL | REVIEWED | BLOCKED | N/A> - <command/evidence/reason>
- Browser: <PASS | FAILED | PARTIAL | REVIEWED | BLOCKED | N/A> - <route/persona/evidence/reason>
- API: <PASS | FAILED | PARTIAL | REVIEWED | BLOCKED | N/A> - <endpoint/evidence/reason>
- Database: <PASS | FAILED | PARTIAL | REVIEWED | BLOCKED | N/A> - <table/query intent/evidence/reason>
- Workflow history: <PASS | FAILED | PARTIAL | REVIEWED | BLOCKED | N/A> - <history evidence/reason>
- Logs: <PASS | FAILED | PARTIAL | REVIEWED | BLOCKED | N/A> - <service/evidence/reason>
- GitHub Actions: <PASS | FAILED | PARTIAL | REVIEWED | BLOCKED | N/A> - <run evidence/reason>

Known limitations:
- <anything not verified>

Final status: <PASS | FAILED | PARTIAL | REVIEWED | BLOCKED>
```

Final status must be the strictest honest summary of the evidence. For example, if build passed but browser validation was required and not run, the final status is `PARTIAL`, not `PASS`.

## Maintenance Rules

- Keep this file navigational and evidence-focused.
- Link only to files or directories that exist on the repository branch.
- Do not duplicate large rule sections from core, security, workflow, or module guides unless the duplication is necessary for verification execution.
- Update this index whenever a new verification guide or UAT pack becomes canonical.
- Move obsolete verification reports to an archive instead of presenting them as current proof.
