# Browser Verification

Use this when a task changes frontend pages, components, routes, hooks, services, permissions, localization, or a full-stack user flow.

## Required Evidence

- Route opened in a real browser.
- Test user and role recorded.
- Page renders without runtime exception.
- Console errors captured.
- Network failures captured with URL and HTTP status.
- Main user flow executed.
- Permission visibility checked.
- `vi` and `en` checked for touched UI.

## Standard Report

```text
Browser Verification:
- Routes opened:
- Test users:
- Page load: PASS/FAIL
- Console errors: PASS/FAIL
- Network errors: PASS/FAIL
- Runtime exceptions: PASS/FAIL
- Main flow: PASS/FAIL
- Permission visibility: PASS/FAIL/N/A
- Localization vi/en: PASS/FAIL/N/A
- Evidence:
```

If browser tooling is unavailable, report `BLOCKED` and do not claim browser verification passed.
