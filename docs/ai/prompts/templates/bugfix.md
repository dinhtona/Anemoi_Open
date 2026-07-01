# Bugfix Prompt Template

Use this prompt when asking an AI agent to reproduce, diagnose, fix, and verify a defect. The fix must be small, evidence-driven, and scoped to the reported bug unless the root cause proves a wider correction is necessary.

Copy the text block below and replace every `<...>` field with concrete information. Do not leave placeholders in the final task prompt.

```text
Repository:
- dinhtona/Anemoi_Open

Target branch:
- dev

Task type:
- Bugfix

Bug summary:
- <one-sentence description of the bug>

Observed behavior:
- <what actually happens>

Expected behavior:
- <what should happen>

Known context:
- Route/API/job/module: <path or name>
- Persona/user/role group: <if relevant>
- Input/test data: <if relevant>
- Error message/log/stack trace: <if available>
- Regression risk area: <if known>

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
5. Do not claim reproduction, files, commits, tests, browser validation, or PASS unless actually performed.

Required approach:
1. Reproduce the bug, or explain exactly why reproduction is BLOCKED.
2. Identify the root cause with file/function references.
3. Check whether the bug violates architecture, workflow, permission, security, database, event, or frontend rules.
4. Make the smallest safe fix.
5. Add or update tests when practical.
6. Re-run the failing path.
7. Verify the adjacent happy path.
8. Verify the relevant permission/negative path when authorization or visibility is involved.
9. Verify persistence, workflow history, logs, or browser behavior when those are part of the risk.

Fix rules:
- Do not mask errors with silent fallbacks unless the fallback is explicitly required.
- Do not bypass domain/application invariants.
- Do not add role-name-based authorization.
- Do not replace workflow-engine behavior with entity-specific shortcuts.
- Do not weaken validation or security checks to make the symptom disappear.
- Do not perform broad refactors unless they are required to fix the root cause safely.

Required verification:
- Backend bug: build and relevant tests.
- API bug: success case and expected failure/authorization case.
- Database bug: schema and read/write persistence check.
- Frontend bug: build plus browser validation through available DevTools MCP.
- Workflow bug: definition, instance, approver visibility, approve/reject, target status, history, and notification evidence.
- Event/notification bug: publish/consume/log/persisted side effect evidence.
- Security bug: allowed and denied authorization evidence.

Final report format:
- Repository:
- Branch:
- Bug summary:
- Reproduction result:
- Root cause:
- Fix summary:
- Files changed:
- Commit SHA:
- Verification actually performed:
- Regression checks:
- Known limitations / unverified areas:
- Status: PASS / FAILED / PARTIAL / REVIEWED / BLOCKED
```

## Usage Notes

- Use `BLOCKED` if the bug cannot be reproduced because the environment, data, or tool access is unavailable.
- Use `PARTIAL` if the fix was made but not all required checks could be executed.
- Use `PASS` only when the bug is reproduced or otherwise proven, fixed, and the required verification passes in the current session.
