# Implementation Prompt Template

Use this prompt when asking an AI agent to implement a feature or change.

```text
Read and follow:
- AGENTS.md
- docs/README.md
- docs/ai/README.md
- docs/ai/core/verification-standard.md
- Relevant ADRs in docs/architecture/ARCHITECTURE_DECISIONS.md

Task:
<describe the requested change>

Rules:
- Do not violate Clean Architecture or CQRS boundaries.
- Do not add role-name-based authorization.
- Respect the Three-Scope Architecture for HR screens.
- If workflow is involved, use the Workflow Engine as the single source of truth.
- Split large work into Domain/Data, Application, API, Frontend, and Browser Verification steps.

Required verification:
- Backend build/tests when backend changes.
- API verification when endpoints change.
- Database verification when persistence changes.
- Browser verification when frontend/full-stack behavior changes.
- Workflow verification when approval flow changes.

Final report must use PASS / FAIL / PARTIAL / REVIEWED / BLOCKED only, with evidence.
```
