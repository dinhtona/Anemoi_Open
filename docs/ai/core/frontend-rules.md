# Frontend Engineering Rules

Target frontend repository: `cody-web-app`.

The frontend repo's mandatory rulebook is `cody-web-app/AGENTS.md`. Detailed patterns and the browser validation checklist are in `cody-web-app/frontend-guidelines.md`. Read both before frontend work.

## Platform Requirements

These platform requirements also apply to frontend work; the frontend rulebook adds repo-specific detail:

- Browser validation is mandatory for any change that affects runtime UI behavior. Use available browser automation / DevTools MCP. If tooling is unavailable, report `BLOCKED` or `PARTIAL`; do not claim browser validation passed.
- Build, TypeScript, lint, and unit tests are not substitutes for browser validation.
- Use the status vocabulary and evidence rules in `docs/ai/core/verification-standard.md`.
- Frontend authorization uses `user.permissions`; `user.roles` is display/system-role metadata only. See `docs/ai/security/permission-model.md`.
- Localization follows `docs/ai/core/localization.md`: `vi` default, `en`, and both message files updated together.
- Verification evidence for frontend work: `docs/ai/verification/README.md` and `docs/ai/verification/browser.md`.

Do not add new frontend rules here. Update `cody-web-app/AGENTS.md` or `cody-web-app/frontend-guidelines.md` instead, then update this pointer if the location changes.
