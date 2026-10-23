# Anemoi Open - AI Working Guide

This folder explains how to apply the rules. The mandatory rules live in `AGENTS.md`; do not duplicate them here.

## 1. Read Order

1. `AGENTS.md`
2. `docs/ai/README.md` (this map)
3. `docs/ai/core/verification-standard.md`
4. Task-specific sources:
   - Backend: `docs/ai/core/backend-rules.md`
   - Frontend: `docs/ai/core/frontend-rules.md`, `cody-web-app/frontend-guidelines.md`
   - Security: `docs/ai/security/`
   - Workflow: `docs/ai/workflows/`, ADR-027, ADR-028, ADR-029
   - HR: `docs/ai/modules/hr/`, `docs/roadmap/hr/`
   - Prompts/templates: `docs/ai/prompts/`

## 2. Rule Precedence

One list only: see
[`docs/README.md` § Source-Of-Truth Policy](../README.md#source-of-truth-policy).
Do not restate it here.

If a task-specific document conflicts with `AGENTS.md`, follow `AGENTS.md` and report the conflict.

## 3. Execution Policy

Do not implement a large feature in one pass. Split work into reviewed steps:

```text
Backend: Domain & data -> Application -> API/events/worker
Frontend: Types/services/hooks -> Pages/components -> i18n/permissions -> Browser validation
```

Stop after each step and wait for review. Frontend or full-stack work is not complete until browser validation passes.

## 4. Documentation Map

- `docs/README.md` - full documentation map.
- `docs/AI_ENGINEERING_HANDBOOK.md` - runtime flow and reporting contract.
- `docs/ai/core/` - architecture, backend, frontend, verification, localization, naming.
- `docs/ai/security/` - permission model, sensitive permissions, audit log.
- `docs/ai/workflows/` - workflow domain model and rules.
- `docs/ai/modules/` - module-specific guidance.
- `docs/ai/architecture-guard/` - architecture guard rules, exemptions, and failure triage.
- `docs/ai/verification/` - API, browser, database, and workflow verification guides.
- `docs/ai/uat/` - browser UAT packs.
- `docs/architecture/ARCHITECTURE_DECISIONS.md` - approved ADRs.
