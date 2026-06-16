# Phase N7 — Notification Center & User Preferences Design

## Overview

Implement Notification Center UI, user preferences (global channel toggles), notification hiding/archiving, rich filtering/search, and permission gating. Builds on existing N1-N6 notification infrastructure (SignalR, Inbox/Outbox, localization, read tracking).

## Architecture Decision: Coexistence Model

- **NotificationPreference** (new) → global channel toggles per user: `EnableInApp`, `EnableEmail`
- **NotificationSubscription** (existing) → per-category on/off per user
- Both must be satisfied for notification delivery

**Note on `EnableEmail`:** This is a global master switch for email notifications. Per-category email channel preferences (e.g., `NotificationChannelSubscription` with `InApp`/`Email` booleans per category) may be introduced later when email delivery is fully implemented. For now, the subscription controls in-app delivery only.

## Domain Layer

### NotificationPreference Entity

```
NotificationPreference : Entity<NotificationPreferenceId>
  UserId         Guid
  EnableInApp    bool  (default true)   // Global master switch for in-app notifications
  EnableEmail    bool  (default true)   // Global master switch for email notifications
                                        // Per-category email preferences may be introduced later
  CreatedAt      DateTime
  UpdatedAt      DateTime
```

- Unique index on `(UserId)`
- New strongly-typed ID: `NotificationPreferenceId(Guid Value)`

### NotificationHistory Hide (Soft-Delete)

- Add `IsHidden` (bool, default false) and `HiddenAt` (DateTime?) to `NotificationHistory`
- All existing queries filter `x.IsHidden == false`
- Domain method named `Hide()` not `Delete()` — semantically this is "hide from user" not "destroy audit data"
- Existing indexes updated to include `IsHidden`

### NotificationHistory xmin (Concurrency)

- Add `xmin` row version column for optimistic concurrency control
- Required because MarkRead, Hide, and SignalR refresh can race concurrently
- EF Core: `builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();`

## Application Layer — Commands

| Command | Type | Notes |
|---------|------|-------|
| `CreateOrUpdateNotificationPreference` | ICommandVoid | Upsert: if exists → update EnableInApp/EnableEmail; if not → create |
| `HideNotification` | ICommandVoid | Set IsHidden=true, HiddenAt=UtcNow. Checks ownership. Domain: `entity.Hide()`. |
| `HideAllReadNotifications` | ICommandVoid | Set IsHidden=true for all IsRead==true for user |
| `MarkNotificationAsRead` | ICommandVoid | Already exists |
| `MarkAllNotificationsAsRead` | ICommandVoid | Already exists |
| `UpdateNotificationSettings` | ICommandVoid | Already exists (per-category) |

## Application Layer — Queries

| Query | Type | Notes |
|-------|------|-------|
| `GetNotifications` | IQueryPaged | Enhance with: Status, Category, Severity (strongly-typed enums), Keyword, DateFrom, DateTo. Sort: CreatedTime DESC. Response includes UnreadCount. |
| `GetMyNotificationPreference` | IQueryResult | Returns merged DTO: global preference + category subscriptions list |
| `GetUnreadNotificationCount` | IQueryCounting | Already exists |

### Enhanced GetNotifications

Strongly-typed filter enums in Application layer (Contract layer may serialize as strings):

```csharp
// NotificationStatusFilter
public enum NotificationStatusFilter { All, Read, Unread }

// Application-layer enums mapped from strings
// NotificationSeverity, NotificationCategory used internally
```

New filter properties on `GetNotificationsQuery`:
- `StatusFilter` (NotificationStatusFilter, default All)
- `Category` (string?)
- `Severity` (string?)
- `Keyword` (string?) — searches Title + Content
- `DateFrom` (DateTime?)
- `DateTo` (DateTime?)

Response: `NotificationPagedResponse` wrapping `PaginationResponse<NotificationResponse>` with additional `UnreadCount`.

### GetMyNotificationPreference Merged Response

```json
{
  "enableInApp": true,
  "enableEmail": false,
  "subscriptions": [
    { "category": "Leave", "isEnabled": true },
    { "category": "Payroll", "isEnabled": false }
  ]
}
```

Single DTO, single request — avoids 2 round trips.

## Pipeline: Notification Delivery Gate

In `CreateNotificationHandler`, before creating notification:

```
1. Load NotificationPreference for UserId
2. If Preference exists:
   a. Check EnableInApp → skip notification if false
   b. Check EnableEmail → (future: skip email dispatch if false)
3. Load NotificationSubscription for UserId + Category
4. If Subscription exists and IsEnabled == false → skip
5. Otherwise → create notification as before
```

**Performance abstraction:** Although N7 does not mandate in-memory caching, the preference lookup should go through an interface `INotificationPreferenceProvider` with a `MemoryCache` implementation prepared for future use. This avoids N+1 query patterns when bulk-sending notifications (e.g., Payroll Finalized → 500 employees → 1000+ queries would otherwise be needed).

```csharp
public interface INotificationPreferenceProvider
{
    Task<NotificationPreferenceDto?> GetPreferenceAsync(Guid userId, CancellationToken ct);
    // Future: Task InvalidateCacheAsync(Guid userId);
}
```

## Permissions

```csharp
public const string NotificationView = "notification.view";
public const string NotificationManage = "notification.manage";
// notification.preference.manage merged into notification.manage
// because preferences are self-service; granular permission kept for consistency
public const string NotificationPreferenceManage = "notification.preference.manage";
```

- Group key: `"PermissionGroupNotifications"`
- Register in `Permissions.cs` Definitions list
- Add localization keys for permission descriptions in `vi-VN` and `en-US`

## API Endpoints

All under `api/notification/Notification/`, authenticated via JWT.

| Method | Route | Permission | Notes |
|--------|-------|------------|-------|
| GET | `GetNotifications` | notification.view | Enhanced with filters as query params |
| GET | `GetUnreadCount` | notification.view | Keep as-is |
| POST | `MarkAsRead/{id}` | notification.manage | Keep as-is |
| POST | `MarkAllAsRead` | notification.manage | Keep as-is |
| POST | `Hide/{id}` | notification.manage | POST (not DELETE) — semantically "hide from user" |
| POST | `HideAllRead` | notification.manage | Hide all read notifications |
| GET | `GetPreferences` | notification.preference.manage | Returns merged DTO |
| PUT | `UpdatePreferences` | notification.preference.manage | Updates global + category preferences |

## Database Migrations

- New table: `NotificationPreferences`:
  - Columns: Id, UserId (unique), EnableInApp, EnableEmail, CreatedAt, UpdatedAt
  - Unique index on UserId
- Alter `NotificationHistories`:
  - Add `IsHidden` bool (default false)
  - Add `HiddenAt` DateTime?
  - Add `xmin` column (row version, auto-managed by PostgreSQL)
- New indexes on `NotificationHistories` (add any missing):
  - `(UserId, IsHidden)`
  - `(UserId, CreatedAt DESC)`
  - `(UserId, Category, IsHidden)`
  - `(UserId, Severity, IsHidden)`

## Frontend

### Notification Center Page `/notifications`

- Full page with filter bar (Status, Category, Severity dropdowns)
- Search box (debounced keyword search)
- Date range picker
- **Infinite scroll** preferred over traditional pagination (Teams/Slack/Facebook pattern)
  - Fall back to pagination if infinite scroll is too complex for N7 timeline
- Per-item: mark read, hide, open detail drawer
- Bulk: Mark All Read, Hide All Read
- Real-time updates via SignalR → direct `queryClient.setQueryData(...)` update (no cache invalidation + re-fetch)
- Empty state, loading state, error state

### Header Bell Enhancement

- Already exists as `NotificationBell` component
- On `ReceiveNotification` SignalR event → use `queryClient.setQueryData()` to **directly append** the new notification into the React Query cache
- This means **zero network calls** for real-time updates — the SignalR payload is the source of truth
- Unread count badge updates in real-time via the same cache mutation

### Preferences Page (`/settings/notifications`)

- Enhance existing page with:
  - Global toggles: EnableInApp, EnableEmail (from `GetPreferences` merged DTO)
  - Category toggles (existing `UpdateNotificationSettings`)
  - Save both preferences via `UpdatePreferences` single call
  - New localization keys for EnableInApp/EnableEmail labels

### Frontend API Service Updates

- Add methods: `hideNotification`, `hideAllRead`, `getPreferences`, `updatePreferences`
- Rename from `delete*` semantics to `hide*` to match backend naming
- Add frontend types: `NotificationPreferenceResponse` (merged DTO)

### Localization Updates

- Add keys for: filter labels, search placeholder, date range, hide actions, confirmation, EnableInApp, EnableEmail labels
- Update `vi.json` and `en.json`

### Real-Time Data Flow

```
Backend → MassTransit → SignalR Hub → NotificationContext
                                         ↓
                              queryClient.setQueryData(...)
                              (direct cache mutation, no HTTP call)
                                         ↓
                              Notification Center re-render
                              Header Bell badge update
```

## Files to Create

### Backend
1. `Anemoi.Contract.Notification/ModelIds/NotificationPreferenceId.cs`
2. `Anemoi.Contract.Notification/Commands/NotificationPreferenceCommands/CreateOrUpdateNotificationPreference/CreateOrUpdateNotificationPreferenceCommand.cs`
3. `Anemoi.Contract.Notification/Commands/NotificationCommands/HideNotification/HideNotificationCommand.cs`
4. `Anemoi.Contract.Notification/Commands/NotificationCommands/HideAllReadNotifications/HideAllReadNotificationsCommand.cs`
5. `Anemoi.Contract.Notification/Queries/NotificationQueries/GetNotifications/GetNotificationsQuery.cs` (replace)
6. `Anemoi.Contract.Notification/Queries/NotificationPreferenceQueries/GetMyNotificationPreference/GetMyNotificationPreferenceQuery.cs`
7. `Anemoi.Contract.Notification/Responses/NotificationPreferenceResponse.cs`
8. `Anemoi.Notification.Domain/Models/NotificationPreference.cs`
9. `Anemoi.Notification.Application/Abstractions/INotificationPreferenceProvider.cs`
10. `Anemoi.Notification.Application/Cqrs/Commands/NotificationPreferenceCommands/CreateOrUpdateNotificationPreference/CreateOrUpdateNotificationPreferenceHandler.cs`
11. `Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/HideNotification/HideNotificationHandler.cs`
12. `Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/HideAllReadNotifications/HideAllReadNotificationsHandler.cs`
13. `Anemoi.Notification.Application/Cqrs/Queries/NotificationPreferenceQueries/GetMyNotificationPreference/GetMyNotificationPreferenceHandler.cs`
14. `Anemoi.Notification.Application/Cqrs/Queries/NotificationQueries/GetNotifications/GetNotificationsHandler.cs` (replace)
15. `Anemoi.Contract.Notification/Errors/NotificationErrorDetail.cs` (add new error codes)

### Modified (Backend)
1. `Anemoi.BuildingBlocks/.../Authorization/Permissions.cs` — add notification permissions
2. `Anemoi.Notification.Domain/Models/NotificationHistory.cs` — add IsHidden, HiddenAt
3. `Anemoi.Notification.Infrastructure/DataContext/ModelMapping.cs` — add NotificationPreference config, NotificationHistory indexes + xmin
4. `Anemoi.Notification.Infrastructure/DataContext/NotificationDbContext.cs` — add DbSet<NotificationPreference>
5. `Anemoi.Notification.Application/Mappings/NotificationMapper.cs` — add preference + hide mapping
6. `Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/CreateNotification/CreateNotificationHandler.cs` — add preference check through INotificationPreferenceProvider
7. `Anemoi.Centralize.Api/Controllers/Notification/NotificationController.cs` — add new endpoints
8. Application assembly marker (if needed for MediatR scan)

### Frontend (Create)
1. `src/app/[locale]/(dashboard)/notifications/page.tsx` — Notification Center
2. `src/hooks/useNotificationsCenter.ts` — React Query hook for notification center

### Frontend (Modified)
1. `src/services/notificationService.ts` — add new API methods (hide* naming)
2. `src/types/models.ts` — add NotificationPreferenceResponse type
3. `src/constants/api-endpoints.ts` — add new endpoints
4. `src/constants/permissions.ts` — add new permissions + route mapping
5. `src/components/shared/NotificationBell.tsx` — use setQueryData instead of invalidateQueries
6. `src/components/shared/NotificationContext.tsx` — use setQueryData for real-time cache updates
7. `src/hooks/useNotificationSettings.ts` — enhance with global toggles, use merged DTO
8. `src/app/[locale]/(dashboard)/settings/notifications/page.tsx` — add EnableInApp/EnableEmail toggles
9. `messages/vi.json` + `messages/en.json` — add new keys

## Test Requirements

- Domain: NotificationPreference entity tests
- Domain: NotificationHistory.Hide() method tests
- Application: Create/Update preference handler tests
- Application: HideNotification handler tests (checks ownership, sets IsHidden)
- Application: HideAllReadNotifications handler tests
- Application: Enhanced GetNotifications filter tests
- Application: CreateNotificationHandler preference gate tests
- Integration: Full preference + subscription pipeline
- Integration: Permission enforcement for new endpoints
- Integration: Concurrency (xmin row version) handling tests
- Frontend: Component rendering tests (if available)
- Build verification: `dotnet build` + TypeScript compilation

## Acceptance Criteria

1. User can view their notifications with rich filters (Status, Category, Severity, Keyword, Date Range)
2. User can search notifications by keyword
3. User can mark individual notification as read
4. User can mark all as read
5. User can hide individual notification (soft-hide, audit preserved)
6. User can hide all read notifications
7. User can enable/disable in-app notifications globally
8. User can enable/disable email notifications globally (master switch)
9. User can enable/disable per-category subscriptions
10. Unread count updates in real-time via SignalR + direct cache mutation
11. Notification Center reflects real-time notifications (no HTTP re-fetch)
12. Preferences API returns combined global + category settings (single DTO)
13. Permissions enforced: notification.view, notification.manage, notification.preference.manage
14. Hidden notifications excluded from all queries (IsHidden == false)
15. Optimistic concurrency via xmin for race-prone operations
16. Build passes, no warnings
