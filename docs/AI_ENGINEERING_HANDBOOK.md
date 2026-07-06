# AI Engineering Handbook

This handbook is the entry point for every AI agent working in the Anemoi_Open repository.

It defines how an agent must read the repository, choose sources of truth, implement changes, verify execution, and report results. It does not replace the detailed rules under `docs/ai/`; it organizes them into a practical runtime flow for production work.

## 1. Purpose

AI engineering in this repository is implementation work, not brainstorming. Every AI phase must produce reviewable artifacts such as source files, documentation, tests, commits, verification evidence, or a clearly marked BLOCKED report.

An AI agent must:

- Work from repository sources, not memory.
- Respect approved architecture decisions.
- Make small, reviewable changes.
- Commit real files when repository access is available and the task requires implementation.
- Never claim PASS, VERIFIED, COMPLETE, PRODUCTION READY, or UAT READY without execution evidence.
- Report BLOCKED instead of inventing files, test results, or commit SHAs.

## 2. Mandatory Reading Order

Start every new implementation session with this order:

1. `AGENTS.md`
2. `docs/README.md`
3. `docs/ai/README.md`
4. `docs/ai/core/verification-standard.md`
5. `ArchitectureGuide.md`
6. `DEVELOPMENT.md`
7. `docs/architecture/ARCHITECTURE_DECISIONS.md`
8. `docs/architecture/TECHNICAL_DEBT_REGISTER.md`
9. Relevant files under `docs/ai/core/`
10. Relevant files under `docs/ai/security/`
11. Relevant files under `docs/ai/workflows/`
12. Relevant files under `docs/ai/verification/`
13. Relevant UAT packs under `docs/ai/uat/`
14. Relevant module guidance under `docs/ai/modules/`
15. The specific implementation prompt, bugfix prompt, audit prompt, or phase prompt.

For HR-related work, also read all relevant files under:

```text
docs/ai/modules/hr/
docs/ai/RoadmapDocumentation/hr/
docs/ai/prompts/hr/
```

Do not continue when a required source file cannot be read. Report BLOCKED and name the missing file.

## 3. Source Of Truth

When documents disagree, use this precedence:

1. Approved ADRs in `docs/architecture/ARCHITECTURE_DECISIONS.md`
2. `ArchitectureGuide.md`
3. `docs/ai/core/*`
4. `docs/ai/security/*`
5. `docs/ai/workflows/*`
6. `docs/ai/modules/*`
7. Approved roadmap/spec files
8. Current task prompt
9. Historical reports, archived notes, completion summaries, or chat context

Historical reports are useful for context but must not override approved ADRs, architecture rules, verification rules, or current source code.

If a requested change conflicts with an approved ADR, stop and report the conflict. Do not silently implement a conflicting design.

## 4. Repository Access Contract

Before implementation work, verify repository access through the available GitHub connector or local workspace.

A valid implementation report must include:

- Repository name.
- Branch or ref used.
- Files created or modified.
- Commit SHA when a commit was made.
- Verification actually executed.
- Known limitations or unverified areas.

If repository access is unavailable, report:

```text
BLOCKED
Reason: repository access unavailable.
No files were created.
No commit was made.
```

Never claim that files were created or committed unless the write operation actually succeeded.

## 5. Clean Architecture Rules

Anemoi_Open follows Clean Architecture. Keep business policy out of transport and infrastructure layers.

### 5.1 Domain Layer

The Domain layer owns:

- Aggregates.
- Entities.
- Value objects.
- Domain events.
- Invariants.
- Business state transitions.

Rules:

- Do not inject DbContext, HTTP clients, message buses, or external services into domain objects.
- Do not generate new aggregate IDs with `Guid.NewGuid()` in production code. Use `IdGenerator.NextGuid()` where the project standard requires generated GUIDs.
- Keep constructors private or protected when factories enforce invariants.
- Use strongly typed IDs for local aggregate identifiers.
- Record meaningful domain events when business state changes must be observed by other modules.

### 5.2 Application Layer

The Application layer owns:

- CQRS commands and queries.
- Handlers.
- DTOs and response models.
- Validators.
- Mapping.
- Application services.
- Transaction and orchestration boundaries.

Rules:

- Use MediatR-style CQRS handlers.
- Return `OneOf<TResult, ErrorDetailResponse>` for expected business outcomes where established.
- Use FluentValidation for request validation.
- Use Mapperly, not AutoMapper.
- Keep application logic explicit and testable.
- Do not return EF Core entities directly from APIs.

### 5.3 Infrastructure Layer

The Infrastructure layer owns:

- EF Core persistence.
- PostgreSQL mappings.
- MassTransit integration.
- External service clients.
- Repositories when present.
- Background workers and consumers.

Rules:

- Configure EF mappings centrally and consistently.
- Preserve concurrency behavior such as `xmin` where used.
- Keep integration event consumers idempotent where duplicate delivery is possible.
- Do not bypass domain/application invariants with direct data mutation.

### 5.4 API Layer

The API layer owns:

- Controllers.
- Endpoint routing.
- Authorization attributes.
- Transport-level request/response concerns.

Rules:

- Controllers must delegate to CQRS handlers.
- Controllers must not query DbContext directly.
- Controllers must not contain business logic.
- Protected endpoints must use permission constants and `[HasPermission]`.
- User-facing errors must be localized according to project localization rules.

## 6. CQRS Rules

CQRS is mandatory for business features.

Use commands for state changes:

- Create.
- Update.
- Submit.
- Approve.
- Reject.
- Cancel.
- Return.
- Activate/deactivate.

Use queries for reads:

- Lists.
- Details.
- Search.
- Dashboards.
- Reports.
- Lookup data.

Handlers must:

- Validate input.
- Load only required data.
- Enforce permissions or caller scope when not fully enforced by endpoint policy.
- Apply domain methods for business transitions.
- Persist changes through the established unit of work pattern.
- Publish domain or integration events through the project-standard mechanism.
- Return explicit success or error results.

Do not mix read model shortcuts into write handlers unless the architecture guide or existing module pattern explicitly permits it.

## 7. Workflow Engine Rules

The Workflow Engine is the single source of truth for approvals.

Workflow-enabled business modules must use the engine for approval lifecycle, including:

- LeaveRequest.
- OvertimeRequest.
- PayrollRun.
- RecruitmentRequest.
- EmployeeTransfer.
- EmployeeSeparation.
- ProbationRecord.

Rules:

- Do not create legacy module-specific approval endpoints for workflow-enabled entities.
- Approve/reject must go through workflow commands such as `ApproveWorkflowStepCommand` and `RejectWorkflowStepCommand` or their approved engine-facing APIs.
- Do not update target entity approval status independently from workflow state.
- Target status changes must be handled through workflow target status updaters or approved workflow integration events.
- Keep workflow history append-only and auditable.
- Expose current approver/current step to ESS read models where the user needs visibility.

## 8. Definition-Bound Workflow Architecture

ADR-029 establishes definition-bound workflow architecture.

Required workflow entity types must have active `WorkflowDefinition` records. Production workflow creation must not fall back to hierarchy preview generation.

Required workflow types:

```text
LeaveRequest
OvertimeRequest
PayrollRun
RecruitmentRequest
EmployeeTransfer
EmployeeSeparation
ProbationRecord
```

Rules:

- Any new required workflow type must be added to the required entity type set.
- Any new required workflow type must have a seed definition.
- Changes to `RequiresDefinition`, `IsRequiredEntityType`, or equivalent guard logic require architecture review.
- Hierarchy fallback builders may exist for preview-only behavior, not production submission.

## 9. Permission Model

Permission design separates three concepts:

1. Employee position.
2. System role / role group.
3. Workflow role.

Rules:

- Position, job title, and department must never grant permissions or JWT claims.
- System permissions come only from explicit Identity role groups and permission assignments.
- Workflow roles route approval responsibility only; they do not grant system access.
- Frontend authorization must use `user.permissions`.
- Frontend must not use display roles as authorization source.
- User Management and Settings Profile UI should display role names, not raw permission strings.

All permissions must be defined centrally in:

```text
Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/Permissions.cs
```

Adding a permission requires all of these:

1. Constant and `Definition` entry in central `Permissions.cs`.
2. Localization keys in `SharedResource.en.resx`.
3. Localization keys in `SharedResource.vi.resx`.
4. Delegate constant in the module helper class if one exists.
5. Tests or verification that the permission appears in the expected catalog and admin seeding path.

Module-local permission string literals are forbidden.

## 10. Three-Scope Architecture

Every HR module must respect the Three-Scope Architecture.

### 10.1 Employee Scope

Route pattern:

```text
/ess/*
```

Purpose:

- The authenticated employee sees and manages only their own data.

Examples:

- My profile.
- My leave requests.
- My overtime requests.
- My attendance.
- My payslips.
- My onboarding tasks.

Rules:

- Do not call company-wide employee search APIs from ESS pages unless explicitly allowed and scoped.
- Do not expose other employees' private HR data.
- ESS lists should show workflow status and current approver when relevant, but approval actions do not belong here.

### 10.2 Approval Scope

Route pattern:

```text
/manager/*
/manager/approvals
```

Purpose:

- The user sees only items they are responsible to approve.

Rules:

- Scope by workflow responsibility, not role names.
- Use workflow query services or approved approval inbox APIs.
- Centralize approval UX under `/manager/approvals` where applicable.
- Do not show approvals belonging to another manager unless the current user is a valid approver for that workflow step.

### 10.3 HR/Admin Scope

Route pattern:

```text
/hr/*
```

Purpose:

- HR or administrators manage organization-wide records according to permissions.

Rules:

- HR pages are not "my requests" pages.
- HR pages must not default to the current user or current manager unless the feature explicitly says so.
- HR pages may provide filters for employee, department, status, date range, or workflow state.
- HR actions must be protected by explicit permissions and audited where sensitive.

## 11. Frontend Rules

The frontend lives under:

```text
cody-web-app
```

Rules:

- Use the established Next.js structure.
- Use existing service, hook, schema, and component patterns.
- Use `user.permissions` for authorization checks.
- Do not hard-code permission strings in UI components when constants or shared permission helpers exist.
- Do not hide missing translations with silent fallback sanitizers.
- Keep loading, empty, and error states explicit.
- Browser validation is required for frontend-impacting changes when browser tooling is available.

Frontend work should be split into:

1. Types, services, hooks, schemas.
2. Pages and components.
3. i18n, permission visibility, loading/error polish.
4. Browser validation.

## 12. Localization Rules

User-facing messages must be localized.

Rules:

- Backend permission metadata localization belongs in API resource files.
- Frontend should render API-localized permission group and description metadata directly.
- Do not create client-side permission translation dictionaries.
- Missing permission translation keys must fail tests or verification.
- Do not convert missing keys into readable names silently.

## 13. Verification Rules

Code review is not verification.

Valid evidence includes:

- Build output.
- Test execution output.
- Browser automation / DevTools MCP evidence.
- API calls with request and response details.
- Database queries showing persisted state.
- Workflow history records.
- Logs from the relevant service or worker.
- GitHub Actions status when available and applicable.

Invalid evidence includes:

- "Looks correct".
- "Should work".
- Static inspection only, when reporting PASS.
- Assumed test results.
- Old test output from before the change.
- Browser screenshots without checking network/API behavior for data-changing flows.

Use the status vocabulary from `docs/ai/core/verification-standard.md`.

If only code review was performed, say REVIEWED, not PASS.

## 14. Browser Validation

Browser validation through DevTools MCP or equivalent browser automation is required for frontend-impacting work when tooling is available.

A browser validation report should include:

- User account used.
- Route visited.
- Actions performed.
- Expected UI result.
- Network requests checked.
- Console errors checked.
- Final status.

For workflow-related browser validation, also verify:

- Submission created a workflow instance.
- Pending approval appears only for the responsible approver.
- Approve/reject action goes through workflow endpoint or approved workflow command path.
- Target entity status changes only after workflow state changes.
- Workflow history records the action.

Do not report browser PASS if only the page rendered but the workflow/API behavior was not checked.

## 15. API Verification

API verification should include:

- Authentication context.
- Endpoint and HTTP method.
- Request payload.
- Expected status code.
- Actual status code.
- Important response fields.
- Authorization result for allowed and denied users when relevant.

For secured endpoints, verify both:

- A user with permission can access.
- A user without permission is denied.

For ESS endpoints, verify the endpoint cannot access another employee's data.

For manager approval endpoints, verify the endpoint only returns items the current user is responsible to approve.

For HR endpoints, verify organization-wide access is permission-based and not accidentally scoped to the current user.

## 16. Database Verification

Database verification is required when persistence behavior matters.

Verify:

- Records are created in the expected tables.
- Status fields match the business transition.
- Audit/history rows are written where required.
- Workflow instance and workflow history records exist for workflow-enabled actions.
- Soft delete, active/inactive flags, and concurrency fields behave according to the module standard.

Do not mutate production data manually to force a PASS. Use supported APIs or test fixtures unless the task explicitly requires a migration or data repair.

## 17. Workflow Verification

For any approval-related change, verify the full path:

1. Submit target entity.
2. Workflow instance is created from active definition.
3. Current step resolves to the expected approver.
4. Non-approver cannot approve.
5. Responsible approver can approve or reject.
6. Multi-step workflow advances correctly.
7. Final approval updates target entity.
8. Rejection updates target entity according to business rules.
9. Workflow history is append-only and contains action metadata.
10. Notification behavior is checked when the feature requires it.

Partial workflow verification must be reported as partial, not PASS.

## 18. Reporting Rules

Every implementation report must include:

- Scope.
- Files changed.
- Commit SHA.
- Verification executed.
- Evidence summary.
- Known gaps.
- Final status.

Use honest labels:

- PASS: required verification executed and passed.
- REVIEWED: code or documentation reviewed, but execution evidence is missing.
- PARTIAL: some required verification passed, but important paths remain unchecked.
- BLOCKED: work cannot continue due to missing access, dependency, environment, or required information.
- FAILED: verification executed and found defects.

Never write:

- "All tests pass" unless tests were run.
- "Browser verified" unless browser automation was executed.
- "Committed" unless the commit operation returned a real SHA.
- "No issues" unless the verification scope was defined and executed.

## 19. Definition Of Done

A phase is done only when all applicable conditions are true:

- Required source-of-truth documents were read.
- Implementation is complete for the agreed scope.
- Architecture rules are followed.
- Permissions are explicit and centralized.
- Workflow-enabled actions use the Workflow Engine.
- Three-Scope Architecture is preserved.
- User-facing text is localized.
- Tests/build/browser/API/database verification were executed as required by the change type.
- Evidence is included in the report.
- Files are committed when repository access and task scope require a commit.
- Commit SHA is reported.
- Known gaps are stated honestly.

Documentation-only phases do not require browser validation unless they change user-facing frontend behavior or UAT instructions that must be executed.

## 20. AI Runtime Flow

Use this runtime flow for every task:

### Step 1: Access Check

- Verify repository access.
- Confirm branch/ref.
- If access fails, report BLOCKED.

### Step 2: Read Sources

- Read mandatory files.
- Read relevant module and verification files.
- Resolve conflicts using source-of-truth precedence.

### Step 3: Scope The Change

- Identify files to create or modify.
- Identify required verification.
- Identify risk areas such as permissions, workflow, payroll, or sensitive HR data.

### Step 4: Implement

- Make the smallest coherent change.
- Follow existing patterns.
- Avoid unrelated cleanup.
- Do not mix multiple phases unless explicitly requested.

### Step 5: Verify

- Run the required checks available in the environment.
- For documentation-only changes, verify file creation and content consistency.
- For backend changes, run build/tests or explain why unavailable.
- For frontend changes, run browser validation when tooling is available.
- For workflow changes, verify workflow instance/history/target status.

### Step 6: Commit

- Commit only real changes.
- Use a clear commit message.
- Capture the returned commit SHA.

### Step 7: Report

- Summarize files changed.
- Provide commit SHA.
- State exactly what verification was executed.
- State gaps.
- Use the correct final status.

## 21. Common Anti-Patterns

Avoid these patterns:

- Reporting PASS after only reading code.
- Creating a new approval path outside the Workflow Engine.
- Adding frontend authorization based on role display names.
- Granting permissions from employee position or department.
- Querying DbContext from controllers.
- Returning EF entities from APIs.
- Hard-coding user-facing strings.
- Adding permissions outside central `Permissions.cs`.
- Adding a permission without EN/VI localization.
- Using `Guid.NewGuid()` where `IdGenerator.NextGuid()` is required.
- Letting HR pages behave like ESS pages.
- Letting manager pages show company-wide HR records.
- Silently swallowing missing translation keys.
- Creating placeholder documentation that says "TBD" or "to be added".
- Claiming a commit was made without a commit SHA.
- Using old test output as evidence for new changes.

## 22. Release Process

A release candidate must have:

- Clean build.
- Relevant automated tests executed.
- Browser validation for changed frontend flows.
- API verification for changed endpoints.
- Database verification for changed persistence behavior.
- Workflow verification for changed approval flows.
- Migration review when schema changes exist.
- Permission review when authorization changes exist.
- Localization review when user-facing text changes.
- Known issues documented.

Before release, confirm:

- No architecture decision is violated.
- No critical technical debt item is newly introduced.
- No sensitive permission is granted accidentally.
- No Three-Scope boundary is broken.
- No workflow-enabled module bypasses Workflow Engine.
- No completion report overstates verification.

## 23. Documentation Maintenance

Keep this handbook stable and practical.

When adding new AI rules:

- Prefer updating the detailed rule file under `docs/ai/`.
- Keep `docs/README.md` navigational.
- Keep `AGENTS.md` compact.
- Update this handbook only when the runtime flow or platform-wide AI engineering contract changes.
- Archive superseded reports and prompts instead of leaving them mixed with active guidance.

## 24. Quick Start Checklist

Before coding:

```text
[ ] Repository access verified
[ ] AGENTS.md read
[ ] docs/README.md read
[ ] docs/ai/README.md read
[ ] Verification standard read
[ ] Relevant architecture/security/workflow/module docs read
[ ] Scope confirmed
[ ] Required verification identified
```

Before reporting completion:

```text
[ ] Files actually created/updated
[ ] Required checks executed or gap stated
[ ] Commit created if required
[ ] Commit SHA captured
[ ] Status label matches evidence
[ ] No fake PASS / COMPLETE / UAT READY claim
```

## 25. AI-Phase Output Contract

Each AI phase must produce a concrete output.

Acceptable outputs:

- Source code.
- Tests.
- Documentation.
- Verification guide.
- UAT pack.
- Migration.
- Configuration change.
- Bugfix.
- Audit report backed by executed evidence.

Unacceptable outputs:

- Brainstorm only.
- Placeholder files.
- Uncommitted claims.
- Reports with no evidence.
- Fake commit SHAs.

If the phase cannot create the expected artifact, report BLOCKED with the reason and stop.
