# Bugfix Prompt Template

Use this prompt when asking an AI agent to fix a bug.

```text
Read and follow:
- AGENTS.md
- docs/README.md
- docs/ai/README.md
- docs/ai/core/verification-standard.md
- Relevant verification guide under docs/ai/verification/

Bug:
<describe actual behavior, expected behavior, route/API, user, and data if known>

Required approach:
1. Reproduce or explain why reproduction is BLOCKED.
2. Identify root cause with file/function references.
3. Make the smallest safe fix.
4. Add or update tests where practical.
5. Re-run the failing path.
6. Verify no regression for adjacent happy path and permission path.

Final report:
- Root cause
- Fix summary
- Files changed
- Verification evidence
- Result: PASS / PARTIAL / REVIEWED / BLOCKED
```
