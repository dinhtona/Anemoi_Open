# Browser Verification

Use this when a task changes frontend pages, components, routes, hooks, services, permissions, localization, or a full-stack user flow. Frontend rules live in `cody-web-app/AGENTS.md` and `cody-web-app/frontend-guidelines.md`.

## Required Evidence

- Route opened in a real browser.
- Test user and persona/role recorded.
- Page renders without runtime exception.
- Console errors captured.
- Network failures captured with URL and HTTP status.
- Main user flow executed.
- CRUD flow executed where applicable.
- Permission visibility checked.
- `vi` and `en` checked for touched UI.

## Standard Report

```text
Browser Validation:
- Routes opened:
- Test users:
- Page Loaded: PASS/FAIL
- Console Errors: PASS/FAIL
- Network Errors: PASS/FAIL
- Runtime Exceptions: PASS/FAIL
- Main Flow: PASS/FAIL
- CRUD Flow: PASS/FAIL/N/A
- Permission Checks: PASS/FAIL
- Localization: PASS/FAIL
- Evidence:
```

If browser tooling is unavailable, report `BLOCKED` or `PARTIAL` and state what was not verified. Do not claim browser validation passed.
