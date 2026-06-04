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
