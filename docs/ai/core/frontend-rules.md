# Frontend Engineering Rules

Target frontend repository: `cody-web-app`.

Normative rules are in `AGENTS.md` section 4. This file defines the frontend structure and validation details. Project-specific patterns for error handling, async UI, and layout are in `cody-web-app/frontend-guidelines.md`.

## Required Stack

- Next.js 16 App Router.
- TypeScript strict mode, no `any`.
- Tailwind CSS.
- shadcn/ui and Radix UI.
- Lucide React.
- TanStack Query v5.
- Axios.
- React Hook Form and Zod.
- next-intl.
- next-themes.

Read `cody-web-app/node_modules/next/dist/docs/` for the installed version before writing code. Do not rely on older Next.js conventions.

## Standard Folder Structure

```text
src/
├── app/
│   └── [locale]/
│       ├── (auth)/
│       └── (dashboard)/
├── components/
│   ├── ui/
│   ├── shared/
│   └── features/
├── hooks/
├── lib/
├── services/
├── types/
└── constants/
```

For HR:

```text
src/components/features/hr/
src/hooks/hr/
src/services/hr/
src/types/hr/
```

## API And Data Rules

- Define TypeScript interfaces before UI.
- API calls belong in `services/`.
- React Query hooks belong in `hooks/`.
- UI components only render data and call hooks.
- Shared HTTP failures belong to the Axios interceptor.
- Feature code must not duplicate global error toasts or console errors.
- Use `handleLocalApiError()` before showing a local domain error.
- Consume rejected `mutateAsync()` promises in form/dialog handlers.

## i18n Rules

- Use next-intl with locale-prefixed routes.
- Frontend locales: `vi`, `en`. Default: `vi`.
- Map `vi -> vi-VN` and `en -> en-US` through `Accept-Language`.
- All UI text belongs in `messages/vi.json` and `messages/en.json`.
- Do not translate backend codes or user-entered domain data.

## Layout Rules

Use the compact dashboard layout:

```tsx
<div className="flex flex-col gap-3 w-full">
```

Avoid large padding, excessive shadows, `gap-6` or larger, and `rounded-3xl`. Use compact card padding (`p-4` or `p-3`). Detailed spacing rules are in `cody-web-app/frontend-guidelines.md`.

## Browser Validation

Browser validation is mandatory for any frontend or full-stack change that affects pages, components, hooks, services, routing, permissions, localization, or user workflows.

Use available browser automation / DevTools MCP. If tooling is unavailable, state that explicitly and do not claim browser validation passed.

Required checks:

- Start the application and open the changed route.
- Verify the page renders without React runtime errors or unhandled promise rejections.
- Verify there are no unexpected failed network requests.
- Verify API data renders correctly and the main flow works.
- For CRUD screens, test create, edit, detail/view, delete/cancel where applicable.
- Verify success and error feedback.
- Verify React Query cache invalidation updates the visible UI.
- Verify permission-based UI visibility.
- Verify `vi` and `en` localization for touched UI.
- Capture console errors and network failures.

Required report:

```text
Browser Validation:
- Page Loaded: PASS/FAIL
- Console Errors: PASS/FAIL
- Network Errors: PASS/FAIL
- Runtime Exceptions: PASS/FAIL
- Main Flow: PASS/FAIL
- CRUD Flow: PASS/FAIL/N/A
- Permission Checks: PASS/FAIL
- Localization: PASS/FAIL
```

For workflow pages, also verify submit/start, pending approval visibility, approve/reject, target status, history, notifications, and visible UI refresh.

Build, TypeScript, lint, and unit tests are not substitutes for browser validation.
