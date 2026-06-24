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

## HR Scope Architecture

All HR business modules must separate three perspectives and must not mix them:

- Employee / Self-Service: only the current employee's own data, such as my leave requests, overtime requests, payslips, attendance, and profile.
- Manager / Approval Scope: only data under the manager's approval or management scope, such as requests needing my approval, employees I manage, and onboarding/probation employees I am responsible for. Managers do not automatically have HR rights.
- HR/Admin / Organization Scope: company-wide data, such as all leave requests, overtime requests, payslips, attendance data, and employee records.

UI routes must follow `/ess/*`, `/manager/*`, and `/hr/*`. Permission conventions are `hr.ess.*` for ESS, `*.approve` for manager approval actions, and `hr.*.view` / `hr.*.manage` for HR/Admin.

Every new HR feature must answer: what does Employee see, what does Manager see, and what does HR see? If those three answers are not clear, the feature is incomplete.

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

Frontend-impacting work requires browser validation through available browser automation / DevTools MCP before completion. Verify render, console, network, main flow, permission gating, localization, and React Query refresh after mutations.

## Review Format

1. Overall Review
2. Issues Found
3. Recommended Design
4. Final Decision
