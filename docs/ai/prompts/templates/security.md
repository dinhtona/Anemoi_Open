# Security Prompt Template

Use this prompt when asking an AI agent to implement, audit, or fix security-sensitive behavior. This includes authorization, permissions, role groups, identity claims, JWT/token behavior, sensitive HR data access, audit logging, and frontend permission visibility.

Copy the text block below and replace every `<...>` field with concrete information. Do not leave placeholders in the final task prompt.

```text
Repository:
- dinhtona/Anemoi_Open

Target branch:
- dev

Task type:
- Security

Security objective:
- <describe the security question, control, bug, or implementation goal>

Affected area:
- Backend module/API: <if relevant>
- Frontend route/component: <if relevant>
- Permission/role group: <if relevant>
- Workflow responsibility: <if relevant>
- Sensitive data type: <if relevant>

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
   - docs/ai/security/permission-model.md
   - docs/ai/security/sensitive-permissions.md
   - docs/ai/security/audit-log.md
   - Relevant ADRs in docs/architecture/ARCHITECTURE_DECISIONS.md
   - Relevant rules under docs/ai/core/, docs/ai/workflows/, docs/ai/modules/, and docs/ai/uat/
4. If any required source file cannot be read, stop and report BLOCKED.
5. Do not claim PASS, VERIFIED, COMPLETE, PRODUCTION READY, or UAT READY without current-session execution evidence.

Security rules:
- Employee Position, System Role, and Workflow Role are separate concepts.
- Position/job title/department must never grant permissions or JWT claims.
- System permissions must come only from explicit Identity role groups/permission assignments.
- Workflow roles route approval responsibility only and must not grant system access.
- Frontend authorization must use `user.permissions`, not display role names.
- Protected endpoints must use central permission constants and `[HasPermission]` where applicable.
- Module-local permission string literals are forbidden unless they delegate to central permission constants.
- Permission metadata must be localized through backend resource files when changed.
- Sensitive HR data must not leak across ESS, Approval, and HR/Admin scopes.
- Sensitive writes must have audit/log evidence where the project requires it.

Required analysis:
1. Identify the trust boundary.
2. Identify the protected resource and sensitive data involved.
3. Identify allowed personas and denied personas.
4. Inspect backend authorization and scope enforcement.
5. Inspect frontend visibility and routing guards when UI is involved.
6. Inspect persistence and audit behavior when sensitive data changes.
7. Inspect token/claim behavior when identity or role group changes are involved.
8. Inspect workflow responsibility behavior when approval visibility is involved.

Required verification:
- Allowed user/persona succeeds.
- Denied user/persona fails with the expected status or hidden UI behavior.
- ESS scope cannot read or mutate another employee's private data.
- Approval scope shows only items the current user is responsible to approve.
- HR/Admin scope requires explicit permissions.
- API response does not include unnecessary sensitive fields.
- Audit/log evidence exists for sensitive writes when applicable.
- Frontend checks use `user.permissions` and are validated in browser when UI is involved.
- Database verification proves permission assignments or sensitive writes persisted correctly when relevant.

Final report format:
- Repository:
- Branch:
- Security objective:
- Files inspected:
- Files changed:
- Commit SHA:
- Allowed-path evidence:
- Denied-path evidence:
- Sensitive data / scope evidence:
- Audit/log evidence:
- Browser evidence, if UI affected:
- Known limitations / unverified areas:
- Status: PASS / FAILED / PARTIAL / REVIEWED / BLOCKED
```

## Usage Notes

- Use this template for security implementation, security audit, and security bugfix work.
- If the work is inspection-only, report `REVIEWED`.
- If either allowed-path or denied-path verification is missing for a security-sensitive runtime change, do not report `PASS`.
