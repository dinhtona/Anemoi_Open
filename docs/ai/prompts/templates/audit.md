# Audit Prompt Template

Use this prompt when asking an AI agent to audit an area of the system.

```text
Read and follow:
- AGENTS.md
- docs/README.md
- docs/ai/README.md
- docs/ai/core/verification-standard.md
- docs/ai/verification/browser.md
- docs/ai/verification/api.md
- docs/ai/verification/database.md
- docs/ai/verification/workflow.md
- Relevant ADRs in docs/architecture/ARCHITECTURE_DECISIONS.md

Audit scope:
<describe module, route, workflow, or permission area>

Audit requirements:
- Separate code review findings from executed verification findings.
- Mark unexecuted findings as REVIEWED, not PASS.
- For frontend/full-stack behavior, use browser automation / DevTools MCP when available.
- For workflow behavior, prove definition, instance, approver visibility, approve/reject, target status, history, and notification.
- For permissions, verify both allowed and denied cases.

Final report:
- Executive summary
- Findings by severity
- Evidence table
- PASS / PARTIAL / REVIEWED / BLOCKED result
- Recommended fixes
```
