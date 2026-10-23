# Anemoi_Open Documentation

This directory is the documentation map for Anemoi_Open. Use it to find the right source of truth instead of copying architecture rules into new files.

## Source-Of-Truth Policy

Use this precedence when documents disagree:

1. `docs/architecture/ARCHITECTURE_DECISIONS.md`
2. `AGENTS.md`
3. `ArchitectureGuide.md`
4. `docs/ai/core/*`
5. `docs/ai/security/*` and `docs/ai/workflows/*`
6. `docs/ai/modules/*`
7. Approved roadmap/spec files
8. Historical specs and prompt templates

Technical debt recommendations do not override approved architecture decisions.

Within `cody-web-app`, `cody-web-app/AGENTS.md` is the frontend source of truth and `cody-web-app/frontend-guidelines.md` holds the detailed patterns. If either conflicts with a platform rule above, the platform rule wins.

## Start Here

- `../AGENTS.md` - single source of truth for mandatory rules
- `AI_ENGINEERING_HANDBOOK.md` - runtime flow, evidence, and reporting contract
- `ai/README.md` - documentation map and task-specific reading
- `ai/AI_CONTEXT.md` - compact context for agent sessions
- `../README.md` - project overview and setup
- `../ArchitectureGuide.md` - practical architecture guide
- `../DEVELOPMENT.md` - development conventions

## Architecture

- `architecture/ARCHITECTURE_DECISIONS.md` - approved architecture decisions (one record per ADR)
- `architecture/TECHNICAL_DEBT_REGISTER.md` - known technical debt and remediation notes

## AI Guidelines

- `ai/core/architecture.md` - core architecture direction
- `ai/core/backend-rules.md` - backend engineering rules
- `../cody-web-app/AGENTS.md` - frontend repo mandatory rules
- `../cody-web-app/frontend-guidelines.md` - frontend patterns and browser checklist
- `ai/core/frontend-rules.md` - frontend rule pointer and platform verification requirements
- `ai/core/verification-standard.md` - execution evidence and status rules for AI reports
- `ai/core/localization.md` - localization rules
- `ai/core/naming-conventions.md` - naming conventions
- `ai/architecture-guard/README.md` - architecture guard rules, exemptions, and failure triage
- `ai/verification/` - browser, API, database, and workflow verification guides
- `ai/uat/` - browser UAT packs for product flows
- `ai/prompts/templates/` - reusable implementation, audit, bugfix, and planning templates
- `ai/security/` - permission, sensitive permission, and audit rules
- `ai/workflows/` - approval workflow rules
- `ai/modules/` - module guidance

## HR Documentation

- `ai/modules/hr/` - HR module guidance
- `roadmap/hr/` - HR roadmap and approved phase descriptions

## Operations

- `operations/PRODUCTION_DEPLOYMENT.md` - production deployment guide
- `operations/BACKUP_RESTORE_AND_ROLLBACK.md` - backup, restore, and rollback procedures
- `operations/OBSERVABILITY_BASELINE.md` - observability baseline
- `operations/production-baseline.md` - production topology and service baseline
- `operations/HTTPS.md` - local HTTPS and Cloudflare Tunnel setup

## Historical Specs

- `specs/` - implemented phase design specs

These files are historical context and do not define platform-wide rules. Delete superseded documents instead of keeping parallel copies; Git history preserves them.

For HR product roadmap phase names, prefer `roadmap/hr/README.md`.

## Maintenance Rules

- Prefer linking to an existing rule instead of copying it.
- Keep README files short and navigational.
- Add design specs to `specs/` only when they remain useful after implementation.
- Delete superseded documents; do not create archive copies.
