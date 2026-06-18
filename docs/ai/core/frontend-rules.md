# Frontend Engineering Rules

Target frontend repository: `cody-web-app`.

## Required Stack

Use:

- Next.js 14+ App Router
- TypeScript strict mode
- Tailwind CSS
- shadcn/ui
- Radix UI
- Lucide React
- TanStack Query v5
- Axios
- React Hook Form
- Zod
- next-intl
- next-themes

Do not use `any`.

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

## API Rules

- Define TypeScript interfaces before UI.
- API calls belong in `services/`.
- React Query hooks belong in `hooks/`.
- UI components only render data and call hooks.
- Shared HTTP failures belong to the existing Axios interceptor.
- Feature code must not duplicate global error toasts.

## i18n Rules

- Use `next-intl`.
- Frontend locales: `vi`, `en`.
- Default frontend locale: `vi`.
- Send backend culture through `Accept-Language`:
  - `vi -> vi-VN`
  - `en -> en-US`
- All UI text must be added to:
  - `messages/vi.json`
  - `messages/en.json`

## Layout Rules

Use compact dashboard layout:

```tsx
<div className="flex flex-col gap-3 w-full">
```

Avoid large padding, excessive shadows, and large grid gaps.

## Browser Validation Rules

For any frontend or full-stack task that changes pages, components, hooks, services, routing, permissions, localization, or user workflows, browser validation is mandatory before reporting completion.

Use the available browser automation / DevTools MCP. If browser tooling is unavailable, state that explicitly and do not claim browser validation passed.

Build, TypeScript, lint, and unit tests are not enough for frontend completion. A phase is not complete until the implemented UI is exercised in the browser.

Required checks:

- Start the frontend application.
- Open the implemented route in the browser.
- Verify the page renders without React runtime errors.
- Verify there are no unhandled promise rejections.
- Verify there are no unexpected failed network requests.
- Verify API data renders correctly.
- Execute the main user flow for the task.
- For CRUD screens, test create, edit, detail/view, delete or cancel where applicable.
- Verify primary workflow actions such as submit, approve, reject, publish, send, or complete where applicable.
- Verify success and error feedback behavior.
- Verify React Query cache invalidation or refresh updates visible tables/cards.
- Verify permission-based UI visibility.
- Verify `vi` and `en` localization for touched UI.
- Capture console errors and network failures in the final report.

Required browser validation report:

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

For workflow pages, also validate:

- Create or open the target request.
- Submit/start the workflow.
- Verify workflow instance or approval state is created.
- Open pending approvals.
- Approve or reject the request.
- Verify status, history, notifications, and visible UI refresh.
