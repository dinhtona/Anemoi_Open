# Prompt - Phase 5 Leave Frontend

```text
Read all AI guideline files first.

Task:
Implement Leave Management frontend screens in cody-web-app.

Scope:
Routes:
- /[locale]/hr/leave/my-requests
- /[locale]/hr/leave/new
- /[locale]/hr/leave/balance
- /[locale]/hr/leave/approvals
- /[locale]/hr/leave/policies

Files:
- types/hr/leave.ts
- services/hr/leave-service.ts
- hooks/hr/useLeave*.ts
- components/features/hr/leave
- messages/vi.json
- messages/en.json

Requirements:
- TypeScript strict mode; no any.
- Use React Hook Form + Zod for forms.
- Use TanStack Query for queries/mutations.
- Use Axios service abstraction.
- Use compact layout and shadcn/ui.
- All UI text must be localized vi/en.
- Avoid duplicate toast errors already handled by Axios interceptor.
- Submit handlers must catch handled mutateAsync errors.

Screens:
- My leave balance card
- My leave requests table
- Create leave request form
- Select leave type
- Select date range / half-day if supported
- Select approver from manager/department manager list
- Leave request detail
- Approval inbox
- Approve/reject dialog with comment
- Leave policy management if permission exists

Sensitive UI:
- Leave balance adjustment must show sensitive action warning.
- Force approve/cancel must show warning and reason requirement.

Stop after implementation and summarize files changed.
```
