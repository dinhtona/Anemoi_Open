# Anemoi Open - AI Working Guide

This folder is the single source of truth for AI agents working in the Anemoi_Open repository.

## 1. Mandatory Reading Order

Before implementing any HR-related feature, read files in this order:

1. `docs/ai/core/architecture.md`
2. `docs/ai/core/backend-rules.md`
3. `docs/ai/core/frontend-rules.md`
4. `docs/ai/core/verification-standard.md`
5. `docs/ai/core/localization.md`
6. `docs/ai/core/naming-conventions.md`
7. `docs/ai/security/permission-model.md`
8. `docs/ai/security/sensitive-permissions.md`
9. `docs/ai/security/audit-log.md`
10. `docs/ai/workflows/approval-workflow.md`
11. All files under `docs/ai/modules/hr/`
12. The specific phase prompt under `docs/ai/prompts/hr/`

## 2. Rule Precedence

If there is any conflict, follow this order:

```text
Architecture
> Backend / Frontend / Verification Rules
> Security Rules
> Workflow Rules
> HR Module Rules
> Phase Prompt
> User Ad-hoc Request
```

## 3. Execution Policy

Do not implement a large feature in one pass.

Backend work must be split into:

```text
Step 1: Domain & Data
Step 2: Application Layer - CQRS, DTOs, Mappers, Validators
Step 3: API, Events, Worker, MassTransit wiring
```

Frontend work must be split into:

```text
Step 1: Types, services, hooks, schemas
Step 2: Pages and components
Step 3: i18n, permission visibility, loading/error polish
Step 4: Browser validation through available browser automation / DevTools MCP
```

Stop after each step and wait for review.

Frontend or full-stack phases are not complete until browser validation passes. Do not rely only on `npm run build`, TypeScript, lint, or unit tests. Follow `docs/ai/core/frontend-rules.md` and `docs/ai/core/verification-standard.md` for the required validation report.

## 4. Forbidden Shortcuts

Do not:

- Put business logic in Controllers.
- Query DbContext directly from Controllers.
- Hard-code role names such as Manager, Staff, HR, DepartmentManager.
- Hard-code user-facing messages.
- Create protected endpoints without permissions.
- Add arbitrary external libraries.
- Return EF Core entities directly from APIs.
- Update sensitive HR data without audit logs.
- Assign sensitive permissions without explicit confirmation workflow.
- Implement multiple phases at once unless explicitly requested.
- Report frontend completion without browser validation when browser tooling is available.
- Report PASS, VERIFIED, COMPLETE, PRODUCTION READY, or UAT READY without the evidence required by `docs/ai/core/verification-standard.md`.

## 5. Recommended Use

Start from the documentation map:

```text
docs/README.md
```

Then execute one phase prompt at a time from:

```text
docs/ai/prompts/hr/
```

# ANEMOI HR

## Mandatory Reading Before Making Changes

All contributors, reviewers, and AI agents must read:

- docs/architecture/ARCHITECTURE_DECISIONS.md
- docs/architecture/TECHNICAL_DEBT_REGISTER.md

These documents contain approved architectural decisions and known technical debt.

Implementations that violate ARCHITECTURE_DECISIONS.md should be considered incorrect unless an explicit architecture review supersedes the decision.
