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
8. Execution plans, completion reports, prompts, and historical notes

Technical debt recommendations do not override approved architecture decisions.

## Start Here

- `../AGENTS.md` - single source of truth for mandatory rules
- `AI_ENGINEERING_HANDBOOK.md` - runtime flow, evidence, and reporting contract
- `ai/README.md` - documentation map and task-specific reading
- `ai/AI_CONTEXT.md` - compact context for agent sessions
- `../README.md` - project overview and setup
- `../ArchitectureGuide.md` - practical architecture guide
- `../DEVELOPMENT.md` - development conventions

## Architecture

- `architecture/ARCHITECTURE_DECISIONS.md` - approved architecture decisions
- `architecture/RELEASE_1_ARCHITECTURE_FREEZE.md` - Release 1 architecture freeze
- `architecture/adr/` - individual architecture decision records
- `architecture/TECHNICAL_DEBT_REGISTER.md` - known technical debt and remediation notes

## AI Guidelines

- `AI_ENGINEERING_HANDBOOK.md` - runtime flow, evidence, and reporting contract
- `ai/README.md` - documentation map, reading order, and execution policy
- `ai/core/architecture.md` - core architecture direction
- `ai/core/backend-rules.md` - backend engineering rules
- `ai/core/frontend-rules.md` - frontend engineering rules
- `ai/core/verification-standard.md` - execution evidence and status rules for AI reports
- `ai/core/localization.md` - localization rules
- `ai/core/naming-conventions.md` - naming conventions
- `ai/verification/` - browser, API, database, and workflow verification guides
- `ai/uat/` - browser UAT packs for product flows
- `ai/prompts/templates/` - reusable implementation, audit, bugfix, and planning templates
- `ai/security/` - permission, sensitive permission, and audit rules
- `ai/workflows/` - approval workflow rules
- `ai/modules/` - module guidance

## HR Documentation

- `ai/modules/hr/` - HR module guidance
- `roadmap/hr/` - HR roadmap and approved phase descriptions
- `ai/prompts/hr/` - phase prompts used for guided implementation

## Operations

- `operations/PRODUCTION_DEPLOYMENT.md` - production deployment guide
- `operations/BACKUP_RESTORE_AND_ROLLBACK.md` - backup, restore, and rollback procedures
- `operations/OBSERVABILITY_BASELINE.md` - observability baseline
- `operations/production-baseline.md` - production topology and service baseline
- `operations/HTTPS.md` - local HTTPS and Cloudflare Tunnel setup

## Plans, Specs, And Reports

- `plans/` - current or recent implementation plans
- `specs/` - detailed design specs and drafts
- `reports/` - completion and verification reports
- `audit/` - phase audit evidence and UAT reports
- `archive/superseded/` - preserved old prompts and reports that are no longer canonical

These files are useful historical context, but they should not redefine platform-wide rules already covered by ADRs, architecture guides, or AI core rules. Completed plans, specs, and reports are historical even when they remain useful as implementation detail.

Some execution plans use phase numbers local to an implementation run. For HR product roadmap phase names, prefer `roadmap/hr/README.md`.

## Archived Superseded Files

The following older prompt/report files live in `archive/superseded/` because they still contain useful historical implementation detail, but should not be used as current source-of-truth:

- `archive/superseded/prompts/hr-platform-codex-prompts.md`
- `archive/superseded/prompts/CODEX_PROMPT_START_HERE.md`
- `archive/superseded/phase-16-overtime/PHASE16_FINAL_STATUS.md`
- `archive/superseded/phase-16-overtime/PHASE16_IMPLEMENTATION_REPORT.md`
- `archive/superseded/phase-16-overtime/PHASE16_IMPLEMENTATION_SUMMARY.md`
- `archive/superseded/phase-16-overtime/OVERTIME_IMPLEMENTATION.md`
- `archive/superseded/phase-25-recruitment/2026-06-16-phase25-recruitment-design.md`

## Maintenance Rules

- Prefer linking to an existing rule instead of copying it.
- Mark phase documents as draft, approved, implemented, or superseded when possible.
- Promote a document by lifecycle: new plans to `plans/`, design specs to `specs/`, completion reports to `reports/`, audit evidence to `audit/`, and obsolete documents to `archive/superseded/`.
- Keep README files short and navigational.
- Update this map when adding, moving, or archiving documentation.
