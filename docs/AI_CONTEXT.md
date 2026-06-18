# ANEMOI HR Agent Context

Compact HR context for agent sessions. Use [docs/README.md](README.md) for the full documentation map.

## Required References

- `docs/ai/README.md`
- `docs/architecture/ARCHITECTURE_DECISIONS.md`
- `docs/architecture/TECHNICAL_DEBT_REGISTER.md`
- `docs/ai/core/backend-rules.md`
- `docs/ai/core/frontend-rules.md`

## HR Architecture Principles

- Clean Architecture with CQRS + MediatR.
- Snapshot-based payroll and reporting.
- Historical data must not be mutated.
- PostgreSQL `xmin` concurrency where applicable.
- Permission-based authorization.
- Tax and Insurance engines stay independent; Payroll consumes snapshots.

## String And Code Rules

Do not inline business error codes, validation codes, permission codes, route names, status codes, type keys, or localization keys.

Use existing constants such as:

- `HrBusinessErrorCodes`
- `HrPermissions`
- `Permissions`
- `TranslationKeys`
- domain status/type constants

Allowed inline strings: log templates, route templates, JSON/external contract names, resource file values, and test data.

## Frontend

Frontend lives in `cody-web-app`.

Use services for API calls, hooks for data/business state, and `messages/vi.json` plus `messages/en.json` for UI text.

## Review Format

1. Overall Review
2. Issues Found
3. Recommended Design
4. Final Decision
