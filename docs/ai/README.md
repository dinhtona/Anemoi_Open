# Anemoi Open - AI Working Guide

This folder is the single source of truth for AI agents working in the Anemoi_Open repository.

## 1. Mandatory Reading Order

Before implementing any HR-related feature, read files in this order:

1. `docs/ai/core/architecture.md`
2. `docs/ai/core/backend-rules.md`
3. `docs/ai/core/frontend-rules.md`
4. `docs/ai/core/localization.md`
5. `docs/ai/core/naming-conventions.md`
6. `docs/ai/security/permission-model.md`
7. `docs/ai/security/sensitive-permissions.md`
8. `docs/ai/security/audit-log.md`
9. `docs/ai/workflows/approval-workflow.md`
10. All files under `docs/ai/modules/hr/`
11. The specific phase prompt under `docs/ai/prompts/hr/`

## 2. Rule Precedence

If there is any conflict, follow this order:

```text
Architecture
> Backend / Frontend Rules
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
```

Stop after each step and wait for review.

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

## 5. Recommended Use

Start from:

```text
CODEX_PROMPT_START_HERE.md
```

Then execute one phase prompt at a time from:

```text
docs/ai/prompts/hr/
```
