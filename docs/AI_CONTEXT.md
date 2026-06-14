ANEMOI HR MASTER CONTEXT

Before doing anything, read:

- docs/ai/README.md
- docs/AI_CONTEXT.md
- docs/architecture/ARCHITECTURE_DECISIONS.md
- docs/architecture/TECHNICAL_DEBT_REGISTER.md

ERROR CODE RULE

Never use inline string literals for business error codes,
validation codes, permission codes, route names,
status codes, or localization keys.

Always reference the appropriate constant from:
- HrBusinessErrorCodes
- HrPermissions
- Permissions
- TranslationKeys
- Domain constants

If a constant does not exist:
1. Create it.
2. Use the constant.
3. Do not use raw string literals.

String literals are allowed only for:
- UI labels in resource files
- Logging text
- Test data


## ===============


Architecture:

- Clean Architecture
- CQRS + MediatR
- PostgreSQL
- Next.js
- React Query
- shadcn/ui

Principles:

- Snapshot-based architecture
- Historical preservation
- PostgreSQL xmin concurrency
- Permission-based authorization
- Payroll consumes snapshots
- Tax Engine independent
- Insurance Engine independent

Frontend:

./cody-web-app

Review Format:

1. Overall Review
2. Issues Found
3. Recommended Design
4. Final Decision