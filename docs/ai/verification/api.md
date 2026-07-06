# API Verification

Use this when a task changes controllers, CQRS handlers exposed through endpoints, request/response DTOs, authorization, validation, or integration behavior.

## Required Evidence

- Endpoint URL and HTTP method.
- Test user or token context.
- Success case result.
- Expected validation failure result when applicable.
- Expected authorization failure result when applicable.
- Response shape checked against UI/client expectations.

## Standard Report

```text
API Verification:
- Endpoint:
- User/token:
- Success case: PASS/FAIL
- Validation case: PASS/FAIL/N/A
- Authorization case: PASS/FAIL/N/A
- Response shape: PASS/FAIL
- Evidence:
```

Do not report API verification as PASS from handler tests only. Handler tests are useful, but endpoint evidence is required for API changes.
