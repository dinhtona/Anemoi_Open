# Anemoi_Open Documentation

This directory is the documentation map for Anemoi_Open. Use it to find the right source of truth instead of copying architecture rules into new files.

## Source-Of-Truth Policy

Use this precedence when documents disagree:

1. `docs/architecture/ARCHITECTURE_DECISIONS.md`
2. `ArchitectureGuide.md`
3. `docs/ai/core/*`
4. `docs/ai/security/*` and `docs/ai/workflows/*`
5. `docs/ai/modules/*`
6. Approved roadmap/spec files
7. Execution plans, completion reports, prompts, and historical notes

Technical debt recommendations do not override approved architecture decisions.

## Start Here

- `AI_ENGINEERING_HANDBOOK.md` - entry point and runtime flow for AI agents
- `../README.md` - project overview and setup
- `../ArchitectureGuide.md` - practical architecture guide
- `../DEVELOPMENT.md` - development conventions
- `../AGENTS.md` - compact project memory for coding agents
- `ai/README.md` - mandatory AI working guide

## Architecture

- `architecture/ARCHITECTURE_DECISIONS.md` - approved architecture decisions
- `architecture/TECHNICAL_DEBT_REGISTER.md` - known technical debt and remediation notes

## AI Guidelines

- `AI_ENGINEERING_HANDBOOK.md` - AI engineering entry point, runtime flow, reporting contract, and definition of done
- `ai/README.md` - reading order, rule precedence, execution policy
- `ai/core/architecture.md` - core architecture direction
- `ai/core/backend-rules.md` - backend engineering rules
- `ai/core/frontend-rules.md` - frontend engineering rules
- `ai/core/verification-standard.md` - execution evidence and status rules for AI reports
- `ai/verification/` - browser, API, database, and workflow verification guides
- `ai/uat/` - browser UAT packs for product flows
- `ai/prompts/templates/` - reusable implementation, audit, and bugfix prompt templates
- `ai/core/localization.md` - localization rules
- `ai/core/naming-conventions.md` - naming conventions
- `ai/security/` - permission, sensitive permission, and audit rules
- `ai/workflows/` - approval workflow rules
- `AI_CONTEXT.md` - compact context for agent sessions

## HR Documentation

- `ai/modules/hr/` - HR module guidance
- `ai/RoadmapDocumentation/hr/` - HR roadmap and approved phase descriptions
- `ai/prompts/hr/` - phase prompts used for guided implementation

## Plans, Specs, And Reports

- `plans/` - current or recent implementation plans
- `superpowers/specs/` - detailed design specs and drafts
- `superpowers/plans/` - execution plans
- `superpowers/reports/` - completion reports
- `archive/superseded/` - preserved old prompts/reports that are no longer canonical

These files are useful historical context, but they should not redefine platform-wide rules already covered by ADRs, architecture guides, or AI core rules.

Some `superpowers` execution files use phase numbers local to an implementation run. For HR product roadmap phase names, prefer `ai/RoadmapDocumentation/hr/README.md`.

## Archived Superseded Files

The following older prompt/report files were moved to `archive/superseded/` because they still contain useful historical implementation detail, but should not be used as current source-of-truth:

- `archive/superseded/prompts/hr-platform-codex-prompts.md`
- `archive/superseded/prompts/CODEX_PROMPT_START_HERE.md`
- `archive/superseded/phase-16-overtime/PHASE16_FINAL_STATUS.md`
- `archive/superseded/phase-16-overtime/PHASE16_IMPLEMENTATION_REPORT.md`
- `archive/superseded/phase-16-overtime/PHASE16_IMPLEMENTATION_SUMMARY.md`
- `archive/superseded/phase-16-overtime/OVERTIME_IMPLEMENTATION.md`
- `archive/superseded/phase-25-recruitment/2026-06-16-phase25-recruitment-design.md`

## Removed Low-Value Files

The following file was removed because it was an outdated folder snapshot and its useful content is covered by the documentation map:

- `docs/ai/FOLDER_STRUCTURE.md`

## Maintenance Rules

- Prefer linking to an existing rule instead of copying it.
- Mark phase documents as draft, approved, implemented, or superseded when possible.
- Move obsolete plans/reports to an archive folder instead of leaving them mixed with active guidance.
- Keep README files short and navigational.
