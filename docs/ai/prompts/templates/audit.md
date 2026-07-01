# Audit Prompt Template

Use this prompt when asking an AI agent to audit an area of the system. An audit may include static review and executed verification, but the report must clearly separate inspected findings from executed evidence.

Copy the text block below and replace every `<...>` field with concrete information. Do not leave placeholders in the final task prompt.

```text
Repository:
- dinhtona/Anemoi_Open

Target branch:
- dev

Task type:
- Audit

Audit scope:
- <describe module, route, API, workflow, permission area, database area, or documentation area>

Audit objective:
- <describe what risk or readiness question the audit must answer>

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
5. Do not claim PASS, VERIFIED, COMPLETE, PRODUCTION READY, or UAT READY without current-session execution evidence.

Audit requirements:
- Identify source-of-truth documents and code paths used for the audit.
- Separate static review findings from executed verification findings.
- Mark unexecuted checks as REVIEWED, PARTIAL, BLOCKED, or N/A as appropriate.
- For frontend/full-stack behavior, use browser automation / DevTools MCP when available.
- For API behavior, verify both expected success and expected failure/authorization paths.
- For database behavior, verify persisted state when persistence is a risk.
- For workflow behavior, prove definition, instance, approver visibility, approve/reject path, target status, history, and notification evidence.
- For permission/security behavior, verify both allowed and denied cases.
- For payroll or sensitive HR data, verify authorization scope, audit/log behavior, and absence of cross-employee exposure.

Severity definitions:
- Critical: security breach, data leak, broken approval/payroll/sensitive HR behavior, or production-blocking build/runtime failure.
- High: feature is unusable, authorization is incomplete, workflow state is inconsistent, or data integrity is at risk.
- Medium: important behavior is incorrect but has a workaround or limited blast radius.
- Low: documentation, naming, UX polish, or maintainability concern with low runtime risk.
- Info: observation without a required fix.

Recommended audit flow:
1. Read source-of-truth documents.
2. Map expected behavior.
3. Inspect implementation against expected behavior.
4. Execute verification where the environment and tools allow.
5. Record evidence for every claim.
6. Provide fix recommendations without implementing them unless explicitly requested.

Final report format:
- Repository:
- Branch/ref:
- Audit scope:
- Source files/documents inspected:
- Verification executed:
- Findings by severity:
  - Critical:
  - High:
  - Medium:
  - Low:
  - Info:
- Evidence table:
  - Area | Expected | Actual | Evidence | Status
- Recommended fixes:
- Known limitations / unverified areas:
- Final status: PASS / FAILED / PARTIAL / REVIEWED / BLOCKED
```

## Usage Notes

- Use `REVIEWED` when the audit is inspection-only.
- Use `PARTIAL` when some required verification ran but one or more required checks were unavailable or skipped.
- Use `PASS` only when all required checks for the declared audit scope were executed and passed.
- Do not mix historical completion reports with current execution evidence.
