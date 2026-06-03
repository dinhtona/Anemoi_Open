# Prompt - Phase 4 HR Frontend Core

```text
Read all AI guideline files first.

Task:
Implement HR frontend core screens in cody-web-app.

Scope:
Routes:
- /[locale]/hr/employees
- /[locale]/hr/departments
- /[locale]/hr/positions

Files:
- types/hr
- services/hr
- hooks/hr
- components/features/hr/employees
- components/features/hr/organization
- messages/vi.json
- messages/en.json

Requirements:
- Use Next.js App Router.
- TypeScript strict mode; no any.
- Define DTO/types before components.
- API calls in services.
- React Query hooks in hooks.
- UI uses shadcn/ui and Tailwind.
- Use compact dashboard layout.
- All UI text must be in next-intl messages for vi and en.
- Use permission checks to hide buttons/routes where the project already supports frontend permission mapping.
- Do not rely on frontend permissions for security; backend remains source of truth.

Screens:
- Employee list/search
- Employee detail drawer/page
- Create/update employee form
- Department tree/list
- Create/update department form
- Position list
- Create/update position form

Stop after implementation and summarize files changed.
```
