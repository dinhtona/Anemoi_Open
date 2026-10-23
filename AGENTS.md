# AGENTS.md - Anemoi_Open

This is the single source of truth for mandatory rules. Detailed guidance lives under `docs/ai/`; if a document conflicts with this file, this file wins unless an approved ADR explicitly overrides it.

## Project Snapshot

- Modular monolith with independently deployable .NET services: Centralize, Identity, MasterData, Workspace, Notification, and HR. Orchestrator and Secure are placeholder/stub services (planned, not active in docker-compose).
- HR is a bounded context under `Anemoi.Hr/` with Domain, Application, Infrastructure, Api, and ModelIds projects.
- Frontend is `cody-web-app` on Next.js 16 App Router.
- PostgreSQL is the primary database; MassTransit over RabbitMQ handles internal messaging.

## 1. Read Order

Read these before changing code:

1. `AGENTS.md` (this file)
2. `docs/ai/README.md` (documentation map)
3. `docs/ai/core/verification-standard.md` (status and evidence rules)

Then read only what the task needs:

- Backend: `docs/ai/core/backend-rules.md`
- Frontend: `cody-web-app/AGENTS.md`, `cody-web-app/frontend-guidelines.md`, and `docs/ai/core/frontend-rules.md`
- Security: `docs/ai/security/`
- Workflow: `docs/ai/workflows/` and ADR-027, ADR-028, ADR-029
- HR: `docs/ai/modules/hr/` and `docs/roadmap/hr/`
- Decisions: `docs/architecture/ARCHITECTURE_DECISIONS.md`
- Docs map / Start Here: `docs/README.md` (consolidated source-of-truth policy).

`cody-web-app/AGENTS.md` and `cody-web-app/frontend-guidelines.md` live in the separate nested `cody-web-app` repo; they are required reading for frontend work when that repo is checked out alongside this one.

If a required source cannot be read, stop and report `BLOCKED` with the missing path.

## 2. Architecture And Stack

- Clean Architecture, CQRS, event-driven, modular monolith or independently deployable services.
- .NET 10, EF Core + PostgreSQL, MassTransit + RabbitMQ, MediatR + OneOf, Mapperly, FluentValidation, Serilog, Polly, ASP.NET Core localization.
- Do not add libraries, frameworks, or a different .NET version without explicit approval.
- Layer dependencies: Domain depends on nothing above it; Application depends on Domain; Infrastructure depends on Application and Domain; Api/WorkerService depends on Infrastructure.
- Use MassTransit for internal service-to-service work. Do not use HTTP/gRPC between internal services unless explicitly approved.
- External provider calls belong behind Application abstractions and Infrastructure implementations.

## 3. Backend Rules

- Controllers and GraphQL endpoints only dispatch commands/queries. No business logic, no direct DbContext queries.
- Every write needs a Command, Handler, and Validator. Every read needs a Query and Handler.
- Return `OneOf<TResult, ErrorDetailResponse>` for expected business outcomes. Do not throw for normal business validation failures.
- Use strongly typed IDs for local aggregate IDs. Create new IDs in the application layer with `new XxxId(IdGenerator.NextGuid())`. Primitive IDs are allowed only at wire boundaries (DTOs, headers, claims, route values, integration events).
- Use Mapperly partial classes with `[Mapper]`. Do not use AutoMapper.
- Domain must not depend on EF Core, Application, Infrastructure, or Api. Application must not depend on Infrastructure.
- Never return EF Core entities from APIs.
- Use `IQueryable` projections for read-heavy paths.
- Do not inject repositories into other repositories.
- In `Anemoi.Hr.*` production code, centralize business/status/type string literals as constants.
- Name integration events in the past tense (`EmployeeCreatedIntegrationEvent`) and publish async side effects through MassTransit. Use request-response only when the current request cannot complete without the result.
- `Anemoi.Identity` is the only service allowed to create development JWT signing keys. Other services load the configured public key and must not generate keys at startup.

## 4. Frontend Rules

- The frontend repo is `cody-web-app`. The mandatory frontend rulebook is `cody-web-app/AGENTS.md`; detailed patterns and the browser checklist are in `cody-web-app/frontend-guidelines.md`. Read both before changing frontend code.
- Read `cody-web-app/node_modules/next/dist/docs/` for the installed Next.js version before writing code; this version has breaking changes from older Next.js.
- Do not add frontend dependencies, frameworks, or change the stack without explicit approval.
- Browser validation is mandatory before reporting completion for any change that affects runtime UI behavior. Build, TypeScript, lint, and unit tests are not substitutes.

## 5. Permissions And Security

- Employee Position, System Role, and Workflow Role are separate concepts. Position, job title, and department must never grant permissions or JWT claims.
- System permissions come only from Identity role groups and explicit permission assignments. Workflow roles control approval routing only and never grant system access.
- All permissions are defined as `public const` fields plus `Definition` entries in `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/Permissions.cs`. String literals and module-local permission lists are forbidden; module helpers delegate to the central catalog.
- Adding a permission requires: constant + `Definition`, EN/VI resource keys in `SharedResource.en.resx` and `SharedResource.vi.resx`, and a module helper delegate. `Permissions.All` is the authoritative list used by admin role-group seeding.
- Permission metadata localization belongs in API resource files. The frontend renders API-localized group/description values; no client-side permission dictionaries or silent fallbacks. Missing translations must fail tests.
- Protect endpoints with `[HasPermission(...)]` and permission constants. Do not authorize feature code with business role names. `Administrator` is a reserved system role handled by centralized authorization logic.
- Frontend authorization uses `user.permissions`. `user.roles` is display/system-role metadata only.
- Permission codes are stable wire values. Never rename or remove one without an explicit migration and backward-compatibility plan.
- Sensitive permissions are marked with `IsSensitive` and `RiskLevel` in the catalog. Medium risk needs warning, reason, and audit; High adds re-authentication/OTP and notification; Critical adds a second approver and activates only after approval. Do not assign sensitive permissions from a normal role-edit screen.
- Audit logs are append-only and required for sensitive HR actions: personal/bank data, salary, contracts, leave balance adjustments, force approve/cancel, transfers, position/grade changes, promotions, performance reviews, sensitive permission assignment, and user deactivation. Never store secrets or passwords in audit logs.

## 6. Workflow Rules

- ADR-027: every HR module defines Employee (ESS), Approval (manager/approvals), and HR/Admin scopes. Do not mix them.
- ADR-028: approval inbox is centralized under `/manager/approvals`.
- ADR-029: all required workflow types are definition-bound. Required types: `LeaveRequest`, `OvertimeRequest`, `PayrollRun`, `RecruitmentRequest`, `EmployeeTransfer`, `EmployeeSeparation`, `ProbationRecord`.
- Hierarchy fallback (`BuildFromHierarchyAsync`) is not allowed for production workflows. Every required type must have an active `WorkflowDefinition` and a seed definition in `HrDevSeedData.cs`.
- Any new workflow-enabled type must be added to `RequiredEntityTypes` and have a seed definition. Changes to `RequiresDefinition` / `IsRequiredEntityType` require architecture review.

## 7. Localization

- Backend cultures: `vi-VN` (default) and `en-US`. Read `Accept-Language`; use `IStringLocalizer<SharedResource>` or service-owned resources.
- Do not hard-code user-facing text in controllers, handlers, validators, middleware, or filters. Business errors use stable, language-agnostic codes.
- Do not localize identifiers, enum/status codes, event names, or machine-readable fields.
- Frontend strings belong in both message files. Do not translate backend codes or user-entered domain data.

## 8. Verification

Use exactly these statuses:

| Status | Meaning |
| --- | --- |
| `PASS` | Executed and passed in the current session. |
| `FAIL` | Executed and failed. |
| `PARTIAL` | Partially executed. |
| `REVIEWED` | Inspected only, not executed. |
| `BLOCKED` | Could not execute; explain why. |
| `N/A` | Not applicable; explain why. |

Minimum evidence:

- Backend: `dotnet build Anemoi.sln` and relevant tests.
- API: success and expected failure/authorization checks.
- Database: migration/schema and read/write verification where practical.
- Frontend: frontend build, lint/type checks, and browser validation through available browser automation / DevTools MCP.
- Workflow: active definition, submit/start, pending approval visibility, approve/reject, target status, history, and notification.
- Localization: touched `vi` and `en` keys verified.
- Security: allowed and denied authorization checks.
- Sensitive HR: authorization, audit trail, database state, and no cross-employee exposure.

Rules:

- Code review is not verification.
- Do not report `PASS`, `VERIFIED`, `COMPLETE`, `PRODUCTION READY`, or `UAT READY` without execution evidence from this session.
- If evidence is missing, report `REVIEWED`, `PARTIAL`, or `BLOCKED` instead.
- Do not mutate production data manually to force a pass.
- Frontend reports must include page load, console errors, network errors, runtime exceptions, main flow, CRUD where applicable, permission checks, and localization.

## 9. Definition Of Done

- [ ] Required sources read; scope is the smallest safe change.
- [ ] Architecture and layer boundaries respected.
- [ ] Backend build/tests executed where applicable.
- [ ] API success and authorization failure checked where applicable.
- [ ] Migration and database state checked where applicable.
- [ ] Frontend build, lint/type checks, and browser validation executed where applicable.
- [ ] Permissions checked for allowed and denied users.
- [ ] Localization checked in `vi` and `en`.
- [ ] Workflow verified end to end where applicable.
- [ ] Sensitive actions audited.
- [ ] Final status matches the evidence actually collected.

## 10. Feature Workflow

For feature work, stop for review after each step:

1. Domain and data.
2. Application layer.
3. API and communication.
4. Browser validation for frontend or full-stack changes.

## 11. Commands

```bash
cp .env.example .env
docker compose up -d --build
dotnet build Anemoi.sln
```

Verification scripts (from repo root):

```powershell
pwsh ./scripts/verify.ps1
pwsh ./scripts/verify.ps1 -IncludeArchitectureGuard
pwsh ./scripts/verify-docs.ps1
pwsh ./scripts/verify-api.ps1 -ApiBaseUrl http://localhost:5000 -Method GET -Path /api/...
pwsh ./scripts/verify-workflow.ps1 -EntityType LeaveRequest
pwsh ./scripts/verify-browser.ps1 -AppBaseUrl http://localhost:3000 -Route /en/...
```

## 12. Detailed References

- `docs/ai/README.md` - documentation map and task-specific reading.
- `docs/ai/core/verification-standard.md` - status and evidence details.
- `docs/ai/core/backend-rules.md` - backend patterns and examples.
- `cody-web-app/AGENTS.md` - frontend repo mandatory rules.
- `cody-web-app/frontend-guidelines.md` - frontend patterns and browser checklist.
- `docs/ai/core/frontend-rules.md` - frontend rule pointer and platform verification requirements.
- `docs/ai/core/localization.md` - localization details.
- `docs/ai/security/permission-model.md` - permission catalog and Identity model.
- `docs/ai/security/sensitive-permissions.md` - risk policy.
- `docs/ai/security/audit-log.md` - audit fields and actions.
- `docs/ai/workflows/approval-workflow.md` - workflow domain model.
- `docs/architecture/ARCHITECTURE_DECISIONS.md` - approved ADRs.
