# Pull Request

## Summary

- What changed:
- Why:
- Scope:

## Architecture / ADR Check

- [ ] I checked `AGENTS.md`.
- [ ] I checked `docs/ai/README.md`.
- [ ] I checked `docs/ai/core/verification-standard.md`.
- [ ] I checked relevant ADRs in `docs/architecture/ARCHITECTURE_DECISIONS.md`.
- [ ] No ADR impact.
- [ ] ADR impact is described below.

ADR impact:

```text
N/A
```

## HR Scope / Permission Check

- [ ] ESS scope only sees self data.
- [ ] Manager approval scope only sees assigned approvals.
- [ ] HR/Admin scope is company-wide and permission-protected.
- [ ] No role-name-based authorization was added.
- [ ] New permissions, if any, are defined in central `Permissions.cs`, localized, and delegated from module helpers.
- [ ] N/A - not an HR permission/scope change.

## Workflow Check

Required for Leave, Overtime, Payroll, RecruitmentRequest, EmployeeTransfer, EmployeeSeparation, ProbationRecord, or any new workflow-enabled type.

- [ ] Active WorkflowDefinition exists or is seeded.
- [ ] Submit/start workflow was verified.
- [ ] Pending approval is visible to the correct approver.
- [ ] Pending approval is not visible to unrelated users/managers.
- [ ] Approve/reject uses workflow engine endpoints/commands.
- [ ] Target entity status is updated.
- [ ] Workflow history is created.
- [ ] Notification behavior is verified or explicitly marked partial.
- [ ] N/A - not a workflow change.

Workflow evidence:

```text
N/A
```

## Verification Evidence

Follow `docs/ai/core/verification-standard.md`. Do not mark a row PASS unless it was executed in this PR/current run.

| Check | Status | Evidence |
| --- | --- | --- |
| Backend build | REVIEWED / PASS / FAIL / BLOCKED / N/A | |
| Backend tests | REVIEWED / PASS / FAIL / BLOCKED / N/A | |
| Database / migration | REVIEWED / PASS / FAIL / BLOCKED / N/A | |
| API / AuthZ | REVIEWED / PASS / FAIL / BLOCKED / N/A | |
| Frontend build | REVIEWED / PASS / FAIL / BLOCKED / N/A | |
| Browser validation | REVIEWED / PASS / FAIL / BLOCKED / N/A | |
| Localization vi/en | REVIEWED / PASS / FAIL / BLOCKED / N/A | |
| Workflow validation | REVIEWED / PASS / FAIL / BLOCKED / N/A | |

Commands run:

```bash
# paste exact commands and results here
```

## Browser Validation

Required for frontend or full-stack changes. Use DevTools MCP/browser automation when available.

- Routes opened:
- Test users / roles:
- Page loaded: REVIEWED / PASS / FAIL / BLOCKED / N/A
- Console errors: REVIEWED / PASS / FAIL / BLOCKED / N/A
- Network errors: REVIEWED / PASS / FAIL / BLOCKED / N/A
- Runtime exceptions: REVIEWED / PASS / FAIL / BLOCKED / N/A
- Main flow: REVIEWED / PASS / FAIL / BLOCKED / N/A
- CRUD flow: REVIEWED / PASS / FAIL / BLOCKED / N/A
- Permission checks: REVIEWED / PASS / FAIL / BLOCKED / N/A
- Localization: REVIEWED / PASS / FAIL / BLOCKED / N/A
- Screenshots / captures:

## Risk And Follow-up

- Remaining risks:
- Known gaps:
- Follow-up tasks:

## Final Status

Choose one:

- [ ] PASS - all required checks for this PR were executed and passed.
- [ ] PARTIAL - some required checks are missing or inconclusive, listed above.
- [ ] REVIEWED - inspected only; behavior was not executed.
- [ ] BLOCKED - cannot complete verification due to listed blocker.
