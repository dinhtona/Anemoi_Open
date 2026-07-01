# Implementation Prompt Template

Use this prompt when asking an AI agent to implement a feature, documentation change, backend change, frontend change, workflow change, or full-stack change in Anemoi_Open.

Copy the text block below and replace every `<...>` field with concrete information. Do not leave placeholders in the final task prompt.

```text
Repository:
- dinhtona/Anemoi_Open

Target branch:
- dev

Task type:
- Implementation

Goal:
- <describe the exact feature/change/documentation artifact to implement>

Business context:
- <explain why the change is needed and who uses it>

Scope:
- In scope:
  - <specific files, modules, routes, APIs, workflows, or docs to change>
- Out of scope:
  - <specific exclusions>

Mandatory startup rules:
1. Verify repository access to dinhtona/Anemoi_Open.
2. Confirm the target branch is dev unless this prompt explicitly says otherwise.
3. Read and follow:
   - AGENTS.md
   - docs/AI_ENGINEERING_HANDBOOK.md
   - docs/README.md
   - docs/ai/README.md
   - docs/ai/core/verification-standard.md
   - docs/ai/verification/README.md
   - Relevant ADRs in docs/architecture/ARCHITECTURE_DECISIONS.md
   - Relevant rules under docs/ai/core/, docs/ai/security/, docs/ai/workflows/, docs/ai/modules/, and docs/ai/uat/
4. If any required source file cannot be read, stop and report BLOCKED.
5. Do not claim files were created unless the write operation succeeds.
6. Do not claim commits unless GitHub or git returns a real commit SHA.
7. Do not claim PASS, VERIFIED, COMPLETE, PRODUCTION READY, or UAT READY unless the required verification was executed in the current session.

Architecture rules:
- Respect Clean Architecture boundaries.
- Use CQRS handlers for business behavior.
- Do not put business logic or DbContext queries in controllers.
- Use strongly typed IDs where established.
- Use Mapperly, not AutoMapper.
- Return OneOf<TResult, ErrorDetailResponse> for expected business outcomes where established.
- Do not add role-name-based authorization.
- Use central permission constants and [HasPermission] for protected endpoints.
- Respect the Three-Scope Architecture for HR: ESS, Approval, and HR/Admin scopes must not be mixed.
- If workflow is involved, use the Workflow Engine as the single source of truth.
- Required workflow entity types must be definition-bound and must not use hierarchy fallback in production.
- User-facing text must be localized when code changes introduce new UI/API messages.

Implementation approach:
1. Inspect the current implementation before changing files.
2. Identify the smallest safe change set.
3. Split large work into reviewable steps:
   - Domain and data
   - Application layer
   - API, events, and communication
   - Frontend types/services/hooks
   - Frontend pages/components/i18n
   - Browser validation
4. Prefer updating existing patterns over introducing new abstractions.
5. Add or update tests where practical and relevant.
6. Avoid speculative refactors outside the requested scope.

Required verification:
- Documentation-only: read/write confirmation and link/path review.
- Backend: dotnet build Anemoi.sln and relevant tests.
- API: HTTP success and expected failure/authorization checks.
- Database/migration: schema/migration and read/write verification where practical.
- Frontend: frontend build plus browser validation through available DevTools MCP.
- Workflow: definition, submit/start, approver visibility, approve/reject, target status, history, and notification evidence.
- Security/permission: allowed and denied authorization checks.
- Notification/event-driven: publish/consume/log/persisted side-effect evidence.
- Payroll/sensitive HR data: authorization, audit/log, database, and no cross-employee exposure evidence.

Expected deliverables:
- <list files to create/modify when known>
- <list tests or verification commands expected when known>
- Commit changes directly to dev unless repository policy or task prompt says otherwise.

Final response format:
- Repository:
- Branch:
- Files created/modified:
- Commit SHA:
- Verification actually performed:
- Known limitations / unverified areas:
- Status: PASS / FAILED / PARTIAL / REVIEWED / BLOCKED
```

## Usage Notes

- Use this template for implementation work, not audits or reviews.
- If the work is documentation-only, do not run application builds unless requested or docs tooling exists.
- If verification is not executable in the current environment, report `PARTIAL` or `REVIEWED` honestly instead of inflating the result.
