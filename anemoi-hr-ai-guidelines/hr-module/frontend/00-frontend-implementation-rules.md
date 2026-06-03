# HR Frontend Implementation Rules

## Mandatory rules

Before coding, read:

```text
shared/01-frontend-guidelines.md
hr-module/00-hr-platform-overview.md
hr-module/03-permission-sensitive-guide.md
hr-module/04-approval-workflow-guide.md
```

## Stack

Use:

```text
Next.js 14+ App Router
TypeScript strict mode
Tailwind CSS
shadcn/ui
Lucide React
TanStack Query v5
Axios
React Hook Form
Zod
next-intl
```

## Folder structure for HR

```text
src/
├── app/[locale]/(dashboard)/hr/
│   ├── employees/
│   ├── departments/
│   ├── leave/
│   ├── approvals/
│   ├── contracts/
│   ├── salary/
│   └── skills/
├── components/features/hr/
│   ├── employees/
│   ├── leave/
│   ├── approvals/
│   ├── permissions/
│   └── shared/
├── hooks/hr/
├── services/hr/
├── types/hr/
└── constants/hr/
```

## UI rules

- Model first: define TypeScript interfaces before UI.
- Services own API calls.
- Hooks own business/query/mutation logic.
- Components only render UI.
- Use compact dashboard layout: `flex flex-col gap-3 w-full`.
- Use Skeleton for loading.
- Use toast only through established error-handling pattern.
- Do not duplicate Axios interceptor errors.

## i18n

Add all HR UI text to:

```text
messages/vi.json
messages/en.json
```

Recommended namespaces:

```text
HR.employee
HR.department
HR.leave
HR.approval
HR.permission
HR.salary
HR.contract
HR.skill
```

## Sensitive permission UI

When assigning sensitive permissions:

- Show warning badge.
- Show risk level.
- Require reason.
- Require confirmation step.
- For critical permissions, show approval-required state.
- Do not silently assign sensitive permission.
