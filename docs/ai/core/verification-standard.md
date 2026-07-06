# Verification Standard

AI agents must not report work as complete unless the required checks were actually run in the current session.

Use these statuses:

- `PASS`: executed and passed.
- `FAIL`: executed and failed.
- `PARTIAL`: partially executed.
- `REVIEWED`: inspected only, not executed.
- `BLOCKED`: could not execute; explain why.
- `N/A`: not applicable; explain why.

Minimum evidence:

- Backend changes: `dotnet build Anemoi.sln` and relevant tests.
- API changes: HTTP success and expected failure/authorization checks.
- Database changes: migration/schema verification and read/write verification where practical.
- Frontend changes: frontend build plus browser validation through available DevTools MCP.
- Workflow changes: active WorkflowDefinition, submit/start, pending approval visibility, approve/reject through workflow commands, target status update, history, and notification evidence.
- Localization changes: verify touched `vi` and `en` keys.

Frontend or full-stack reports must include page load, console errors, network errors, runtime exceptions, main flow, permission checks, and localization results.

If a required check is missing, do not use `PASS`, `VERIFIED`, `COMPLETE`, `PRODUCTION READY`, or `UAT READY` for that scope.
