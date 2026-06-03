# Frontend Development Guidelines & System Prompt

## 1. Role & Context
You are a Senior Frontend Architect. Your mission is to build a professional Web Tool Dashboard that connects to an ASP.NET Core API. The tool manages test data generation and environment controls.

## 2. Technical Stack
- **Framework:** Next.js 14+ (App Router).
- **Language:** TypeScript (Strict Mode, no `any`).
- **Styling:** Tailwind CSS (Dark Mode via `next-themes`).
- **UI Components:** shadcn/ui (Radix UI) & Lucide React icons.
- **State Management:** TanStack Query v5 (React Query).
- **API Client:** Axios with JWT Bearer Token Interceptors.
- **Form Handling:** React Hook Form + Zod validation.
- **Internationalization:** `next-intl` with locale-prefixed App Router routes.

## 3. Internationalization Standards
- **i18n Framework:** Use `next-intl` for all frontend translation, routing, and locale-aware navigation.
- **Supported Locales:** The frontend supports `vi` and `en`.
- **Default Locale:** The default frontend locale is `vi`.
- **Route Structure:** Routes must use the locale prefix from `[locale]` (for example `/vi/environments` and `/en/environments`).
- **Backend Culture Mapping:** Map frontend locales to ASP.NET Core cultures as `vi -> vi-VN` and `en -> en-US`, then send the mapped value through the `Accept-Language` header on API requests.
- **Message Files:** All user-facing UI text must be defined in both `messages/vi.json` and `messages/en.json`. Do not hard-code labels, buttons, descriptions, validation text, empty states, toast messages, or status labels directly in pages/components.
- **Namespaces:** Group translation keys by feature/domain with stable namespaces such as `Common`, `Auth.login`, `Dashboard`, `SeedGenerator`, and `Environments`.
- **Stable Keys:** Translation keys must describe intent or domain meaning, not the sentence content. Do not rename keys just because copy changes.
- **Domain Data:** Do not translate user-entered data or backend-owned domain values. Translate only UI chrome, system states, validation, error messages, and display labels.
- **Status Mapping:** Backend status/code values must remain invariant (for example `running`, `stopped`, `error`). The frontend maps those values to localized labels in the relevant namespace.
- **Locale Preservation:** Language switchers and locale-aware links must preserve the current destination route whenever possible instead of sending users back to a default page.

## 4. Design Philosophy (Anthropic Frontend Design Skills)
- **Visual Hierarchy:** Use clear contrast and purposeful spacing to guide user attention.
- **Responsive & Mobile-first:** - Mandatory mobile support using `Sheet` for navigation.
  - Responsive Grid/Flex layouts for dashboard cards.
- **Affordance:** Interactive elements must have hover, active, and disabled states.
- **Feedback Loops:** Use Skeleton loaders for fetching and `sonner`/`toast` for API feedback.
- **Aesthetics:** Use consistent border-radius (e.g., rounded-xl) and subtle border colors (border-border/50) to create a modern, high-end feel.

## 5. Standard Folder Structure
```text
src/
├── app/                  # Next.js App Router
│   ├── (auth)/           # Routes: /login
│   ├── (dashboard)/      # Routes: /environments, /data-gen
│   ├── api/              # Route Handlers (Optional Proxy to ASP.NET)
│   └── layout.tsx        # Providers (Query, Theme, Auth)
├── components/
│   ├── ui/               # shadcn/ui components
│   ├── shared/           # Sidebar, Navbar, PageHeader
│   └── features/         # Feature-specific (EnvCard, TerminalLog, DataForm)
├── hooks/                # Custom Hooks (useAuth, useEnvironments, useDataGen)
├── lib/                  # Configs (axios.ts, query-client.ts, utils.ts)
├── services/             # API Abstraction layer (auth-service, env-service)
├── types/                # TS Interfaces (api-response.d.ts, models.ts)
└── constants/            # API_ENDPOINTS, APP_CONFIG
```
## 6. Core Features Scope
 - **Auth Flow:** Secure Login page with JWT persistence in Cookies/LocalStorage.
 - **Environment Control:** Grid view of services with Start/Stop mutations and real-time status badges.
 - **Data Generator:** Grouped forms for data creation with validation.
 - **Console Viewer:** A sleek, terminal-style component for streaming logs/API responses.

## 7. Implementation Rules
 - **Model First:** Always define TypeScript Interfaces for ASP.NET DTOs before UI.
 - **Logic Separation:** Keep API logic in services/ and business logic in hooks/. UI components should only handle display.
 - **Error Handling:** Use Axios interceptors for shared HTTP failures. Ensure Axios uses `withCredentials: true` if the ASP.NET API uses HttpOnly cookies, otherwise use the `Authorization` header.
 - **Consistency:** Use camelCase for folder names and file names (except for components).
 - **Translation Coverage:** Whenever adding or changing UI text, update both `messages/vi.json` and `messages/en.json` in the same change.

## 8. Spacing, Layout & Card Styling (Aesthetic Standardization)
To avoid manual padding fixes and keep the layout cohesive and compact, follow these rules strictly:
- **Outer Page Wrapper:**
  - Always use `<div className="flex flex-col gap-3 w-full">` as the main page container.
  - Do NOT wrap pages in extra outer padding classes (e.g., `p-6`, `p-8`, `md:p-8`) because the dashboard layout container already applies `p-4` padding around `{children}`.
- **Grids & Layout Gaps:**
  - Use `gap-3` (12px) or `gap-4` (16px) for grids and flex containers. Avoid `gap-6` or larger which wastes valuable space.
- **Card Design:**
  - Maintain a **flat design**: do not use large shadows (`shadow-md`, `shadow-xl`) or excessive border-radiuses (`rounded-3xl`). Rely on the UI framework's standard `ring-1 ring-foreground/10` or a subtle border `border border-border/40`.
  - Inner card padding must be compact: use `p-4` (16px) or `p-3` (12px) for normal cards, and `pb-1`/`pt-3` style adjustments for CardHeader/CardContent. Avoid `p-6` or `p-8` unless explicitly requested for simple marketing or auth screens.
- **UI Element Spacing:**
  - For Radix/shadcn `TabsList`, set a height class (e.g., `h-10`) on `TabsList` rather than custom padding styles (`py-x rounded-x`) directly on `TabsTrigger` buttons.
  - For tables, use compact cell spacing: header padding should be `p-3` and data cell padding should be `p-2.5` to `p-3` to optimize dense data views.

## 9. API Error Handling Ownership
Shared HTTP failures must be handled centrally. Feature code must not create a second toast or console error for a response already handled by the Axios interceptor.

- `src/lib/axios.ts` owns shared HTTP feedback:
  - `403 Forbidden`: emit the localized `forbidden` feedback event. Do not redirect to login.
  - `429 Too Many Requests`: emit the localized rate-limit feedback event.
  - `401 Unauthorized`: run the refresh-token flow or redirect to login.
- `src/components/shared/ApiFeedbackProvider.tsx` is the only component that renders toast messages emitted by the Axios interceptor.
- Feature hooks must use `handleLocalApiError()` from `src/lib/api-events.ts` before logging or showing a local error toast.
- Local toasts are reserved for domain-specific errors that were not already handled globally.
- Do not suppress all Axios errors globally. Callers still need rejected promises for control flow and domain-specific handling.

## 10. Async UI Boundaries
Any UI callback that awaits a mutation and has no higher-level error boundary must consume the rejected promise after the hook or interceptor has reported the error.

Typical examples are form submit handlers, dialog actions and button click handlers that call React Query `mutateAsync()`.

```tsx
const handleSafeSubmit = handleSubmit(async (data) => {
  try {
    await onSubmit(data);
  } catch {
    // The mutation hook or global interceptor has already reported the error.
  }
});
```

- Do not let a handled `mutateAsync()` rejection escape from a form submit handler; in development this can surface as a Next.js runtime overlay.
- Do not add a second `toast.error()` or `console.error()` in page components when the hook already handles the failure.
- Keep expected SignalR lifecycle aborts separate from HTTP error handling.
