# Verification Scripts

This directory contains helper scripts for AI engineering verification. The scripts are designed to produce honest evidence, not to replace product-specific UAT or runtime validation.

## Scripts

| Script | Purpose |
| --- | --- |
| `verify.ps1` | Default verification entry point for backend, frontend, optional workflow, browser, and architecture guard checks. |
| `verify-docs.ps1` | Lightweight documentation verification for required AI docs and common fake-verification wording. |
| `verify-api.ps1` | Focused HTTP endpoint check with expected status support. Run once for allowed paths and once for denied paths when security is involved. |
| `verify-workflow.ps1` | Workflow verification checklist plus optional workflow definition endpoint check. |
| `verify-browser.ps1` | Browser validation checklist plus optional route reachability check. DevTools MCP is still required for true frontend PASS. |
| `architecture-guard.ps1` | PowerShell + Roslyn architecture guard for common architecture/security violations. |

## Common Commands

Run the default build/test-oriented checks:

```powershell
pwsh ./scripts/verify.ps1
```

Run documentation-only checks:

```powershell
pwsh ./scripts/verify-docs.ps1
```

Run with Architecture Guard:

```powershell
pwsh ./scripts/verify.ps1 -IncludeArchitectureGuard
```

Check one API endpoint:

```powershell
pwsh ./scripts/verify-api.ps1 -ApiBaseUrl http://localhost:5000 -Method GET -Path /api/hr/ess/profile -ExpectedStatus 401
```

Generate workflow evidence checklist:

```powershell
pwsh ./scripts/verify-workflow.ps1 -EntityType LeaveRequest
```

Check route reachability while still requiring DevTools MCP for full browser validation:

```powershell
pwsh ./scripts/verify-browser.ps1 -AppBaseUrl http://localhost:3000 -Route /en/ess/leave -Persona Employee
```

## Reporting Rules

- Use `PASS` only for checks actually executed and passed.
- Use `PARTIAL` when a helper performs only part of the required verification.
- Use `REVIEWED` when a script only produced a checklist or inspection result.
- Use `N/A` when a script does not apply to the change type.
- Do not claim browser PASS from route reachability alone.
- Do not claim workflow PASS from definition endpoint evidence alone.
- Do not claim security PASS without allowed and denied checks.

## Relationship To AI Verification Docs

Use these scripts with:

```text
docs/ai/verification/README.md
```

That document remains the source of truth for evidence requirements and status vocabulary.
