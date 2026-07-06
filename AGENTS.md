# AGENTS.md - Anemoi_Open

Compact project memory for coding agents. Keep this file short; detailed rules live under `docs/`.

## Read First

1. `docs/AI_ENGINEERING_HANDBOOK.md`
2. `docs/README.md`
3. `docs/ai/README.md`
4. `docs/ai/core/verification-standard.md`
5. `docs/ai/architecture-guard/README.md`
6. `scripts/README.md`
7. `ArchitectureGuide.md`
8. `DEVELOPMENT.md`
9. `docs/architecture/ARCHITECTURE_DECISIONS.md`
10. `docs/architecture/TECHNICAL_DEBT_REGISTER.md`

If documents conflict, follow approved ADRs first, then architecture/development rules, then module or phase documents.

## Snapshot

- Platform: .NET 10 HR management platform, modular monolith / independently deployable services.
- Architecture: Clean Architecture, CQRS, Event-Driven Architecture, Saga orchestration.
- Backend stack: EF Core + PostgreSQL, MassTransit + RabbitMQ, MediatR + OneOf, Mapperly, FluentValidation, Serilog, Polly.
- Frontend: Next.js app in `cody-web-app`.

## Non-Negotiables

- Use CQRS handlers; no business logic or DbContext queries in controllers.
- Use strongly typed IDs for local aggregate IDs and `IdGenerator.NextGuid()` for new IDs.
- Use Mapperly, not AutoMapper.
- Return `OneOf<TResult, ErrorDetailResponse>` for expected business outcomes.
- Use MassTransit for internal messaging; avoid HTTP/gRPC between internal services unless explicitly approved.
- Localize user-facing messages.
- Protect features with permission constants and `[HasPermission]`.
- Permission model rule: Employee Position, System Role, and Workflow Role are separate concepts. Position/job title/department must never grant permissions or JWT claims. System permissions come only from explicit Identity role groups/permission assignments. Workflow roles are approval-routing responsibilities only and must not grant system access. Frontend authorization must use `user.permissions`; `user.roles` is display/system-role metadata only.
- Permission translation rule: Permission metadata localization belongs in API resource files (SharedResource.*.resx). Frontend must render API-localized group/description directly — no client-side permission translation dictionaries, no sanitizer/fallback that hides missing keys. Missing translations must cause test failure, not be silently converted to readable names. Tests scanning the full `Permissions.cs` catalog must assert every `PermissionGroup*` and `PermissionDescription*` key has non-empty EN/VI values.
- Permission single-source-of-truth rule: ALL permissions MUST be defined as `public const` fields in `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/Permissions.cs` with corresponding `Definition` entries. Module-local permission constants (e.g., `HrPermissions.cs`) MUST delegate to the central `Permissions` class — string literals are forbidden. Adding a new permission requires three coordinated changes: (1) constant + Definition in `Permissions.cs`, (2) localization keys in `SharedResource.en.resx` and `SharedResource.vi.resx`, (3) delegate constant in the module helper class. The `Permissions.All` collection is the authoritative list consumed by Admin role_group seeding — any permission missing from `Permissions.All` is invisible to administrators.
- In `Anemoi.Hr.*` production code, centralize business/status/type string literals as constants and follow the HR scope rules in `docs/AI_CONTEXT.md`.
- Follow the Three-Scope Architecture (ADR-027): every module must define Employee (ESS), Approval (manager/approvals), and HR/Admin (organization) scopes. Approval inbox must be centralized under `/manager/approvals` (ADR-028).
- Workflow definition-bound rule (ADR-029): All required business workflow types (LeaveRequest, OvertimeRequest, PayrollRun, RecruitmentRequest, EmployeeTransfer, EmployeeSeparation, ProbationRecord) are definition-bound. Hierarchy fallback (BuildFromHierarchyAsync) is NOT allowed for production — every type must have an active WorkflowDefinition. Any new workflow-enabled type must be added to both RequiredEntityTypes set AND have a seed definition. Changes to the RequiresDefinition/IsRequiredEntityType logic require architecture review.
- For frontend-impacting work, run browser validation through available browser automation / DevTools MCP before reporting completion.

## Verification Requirements

- Code review is NOT verification.
- Do not report PASS, VERIFIED, COMPLETE, or UAT READY without execution evidence.
- Browser validation is required for frontend-impacting work.
- Evidence must come from browser, API, database, test execution, or build output.
- If evidence is missing, report REVIEWED instead of PASS.
- Detailed status vocabulary and evidence matrix live in `docs/ai/core/verification-standard.md`.

## Feature Workflow

When the repository workflow applies, stop for review after each step:

1. Domain and data.
2. Application layer.
3. API and communication.
4. Browser validation for frontend or full-stack changes.

## Commands

```bash
cp .env.example .env
docker compose up -d --build
dotnet build Anemoi.sln
```

Verification scripts (run from repo root):

```powershell
pwsh ./scripts/verify.ps1
pwsh ./scripts/verify.ps1 -IncludeArchitectureGuard
pwsh ./scripts/verify-docs.ps1
pwsh ./scripts/verify-api.ps1 -ApiBaseUrl http://localhost:5000 -Method GET -Path /api/...
pwsh ./scripts/verify-workflow.ps1 -EntityType LeaveRequest
pwsh ./scripts/verify-browser.ps1 -AppBaseUrl http://localhost:3000 -Route /en/...
```
