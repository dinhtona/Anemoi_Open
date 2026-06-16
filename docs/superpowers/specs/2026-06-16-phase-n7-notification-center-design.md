# Phase N7 — Notification Center & User Preferences Design

## Overview

Implement Notification Center UI, user preferences (global channel toggles), notification deletion, rich filtering/search, and permission gating. Builds on existing N1-N6 notification infrastructure (SignalR, Inbox/Outbox, localization, read tracking).

## Architecture Decision: Coexistence Model

- **NotificationPreference** (new) → global channel toggles per user: `EnableInApp`, `EnableEmail`
- **NotificationSubscription** (existing) → per-category on/off per user
- Both must be satisfied for notification delivery

## Domain Layer

### NotificationPreference Entity

```
NotificationPreference : Entity<NotificationPreferenceId>
  UserId         Guid
  EnableInApp    bool  (default true)
  EnableEmail    bool  (default true)
  CreatedAt      DateTime
  UpdatedAt      DateTime
```

- Unique index on `(UserId)`
- New strongly-typed ID: `NotificationPreferenceId(Guid Value)`

### NotificationHistory Soft-Delete

- Add `IsDeleted` (bool, default false) and `DeletedAt` (DateTime?) to `NotificationHistory`
- All existing queries filter `x.IsDeleted == false`
- Existing indexes unchanged

## Application Layer — Commands

| Command | Type | Notes |
|---------|------|-------|
| `CreateOrUpdateNotificationPreference` | ICommandVoid | Upsert: if exists → update EnableInApp/EnableEmail; if not → create |
| `DeleteNotification` | ICommandVoid | Soft-delete: set IsDeleted=true, DeletedAt=UtcNow. Checks ownership. |
| `DeleteAllReadNotifications` | ICommandVoid | Soft-delete all IsRead==true for user |
| `MarkNotificationAsRead` | ICommandVoid | Already exists |
| `MarkAllNotificationsAsRead` | ICommandVoid | Already exists |
| `UpdateNotificationSettings` | ICommandVoid | Already exists (per-category) |

## Application Layer — Queries

| Query | Type | Notes |
|-------|------|-------|
| `GetNotifications` | IQueryPaged | Enhance with: Status(All/Read/Unread), Category, Severity, Keyword(search title+content), DateFrom, DateTo. Sort: CreatedTime DESC. Response includes UnreadCount. |
| `GetMyNotificationPreference` | IQueryResult | Returns combined: global preference + category subscriptions list. |
| `GetUnreadNotificationCount` | IQueryCounting | Already exists |

### Enhanced GetNotifications

New filter properties on `GetNotificationsQuery`:
- `StatusFilter` (string: "All", "Read", "Unread")
- `Category` (string?)
- `Severity` (string?)
- `Keyword` (string?)
- `DateFrom` (DateTime?)
- `DateTo` (DateTime?)

Response: existing `PaginationResponse<NotificationResponse>` with items + totalCount, plus additional `UnreadCount` field.

## Pipeline: Notification Delivery Gate

In `CreateNotificationHandler`, before creating notification:

```
1. Load NotificationPreference for UserId
2. If Preference exists:
   a. If notification is in-app type → check EnableInApp, skip if false
   b. If notification should be emailed → check EnableEmail, skip if false
3. Load NotificationSubscription for UserId + Category
4. If Subscription exists and IsEnabled == false → skip
5. Otherwise → create notification as before
```

Step 2b is for future email delivery; for now the in-app check (2a) is primary.

## Permissions

```csharp
public const string NotificationView = "notification.view";
public const string NotificationManage = "notification.manage";
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
| DELETE | `Delete/{id}` | notification.manage | Soft-delete, checks ownership |
| DELETE | `DeleteRead` | notification.manage | Soft-delete all read |
| GET | `GetPreferences` | notification.preference.manage | Returns combined global + category |
| PUT | `UpdatePreferences` | notification.preference.manage | Updates both global + category |

## Database Migrations

- New table: `NotificationPreferences`:
  - Columns: Id, UserId (unique), EnableInApp, EnableEmail, CreatedAt, UpdatedAt
  - Unique index on UserId
- Alter `NotificationHistories`:
  - Add `IsDeleted` bool (default false)
  - Add `DeletedAt` DateTime?
- New indexes on `NotificationHistories` (add any missing):
  - `(UserId, IsDeleted)`
  - `(UserId, CreatedAt DESC)` — composite index
  - `(UserId, Category, IsDeleted)`
  - `(UserId, Severity, IsDeleted)`

## Frontend

### Notification Center Page `/notifications`
- Full page with filter bar (Status, Category, Severity dropdowns)
- Search box (debounced keyword search)
- Date range picker
- Paginated list with notification cards
- Per-item: mark read, delete (soft-delete), open detail drawer
- Bulk: Mark All Read, Delete Read
- Real-time updates via SignalR → React Query invalidation
- Empty state, loading state, error state

### Header Bell Enhancement
- Already exists as `NotificationBell` component
- Add React Query cache invalidation on `ReceiveNotification` event
- Ensure unread count badge updates in real-time

### Preferences Page (`/settings/notifications`)
- Enhance existing page with:
  - Global toggles: EnableInApp, EnableEmail (new from `GetPreferences`)
  - Category toggles (existing `UpdateNotificationSettings`)
  - Save both preferences together

### Frontend API Service Updates
- Add methods: `deleteNotification`, `deleteAllRead`, `getPreferences`, `updatePreferences`
- Add frontend types: `NotificationPreferenceResponse`, enhanced `GetNotificationsResponse`

### Localization Updates
- Add keys for: filter labels, search placeholder, date range, delete actions, soft-delete confirmation, EnableInApp, EnableEmail labels
- Update `vi.json` and `en.json`

### Real-Time Data Flow
```
Backend → MassTransit → SignalR Hub → NotificationContext
                                         ↓
                              React Query invalidation
                                         ↓
                              Notification Center re-render
                              Header Bell badge update
```

## Files to Create

### Backend
1. `Anemoi.Contract.Notification/ModelIds/NotificationPreferenceId.cs`
2. `Anemoi.Contract.Notification/Commands/NotificationPreferenceCommands/CreateOrUpdateNotificationPreference/CreateOrUpdateNotificationPreferenceCommand.cs`
3. `Anemoi.Contract.Notification/Commands/NotificationCommands/DeleteNotification/DeleteNotificationCommand.cs`
4. `Anemoi.Contract.Notification/Commands/NotificationCommands/DeleteAllReadNotifications/DeleteAllReadNotificationsCommand.cs`
5. `Anemoi.Contract.Notification/Queries/NotificationQueries/GetNotifications/GetNotificationsQuery.cs` (replace)
6. `Anemoi.Contract.Notification/Queries/NotificationPreferenceQueries/GetMyNotificationPreference/GetMyNotificationPreferenceQuery.cs`
7. `Anemoi.Contract.Notification/Responses/NotificationPreferenceResponse.cs`
8. `Anemoi.Notification.Domain/Models/NotificationPreference.cs`
9. `Anemoi.Notification.Application/Cqrs/Commands/NotificationPreferenceCommands/CreateOrUpdateNotificationPreference/CreateOrUpdateNotificationPreferenceHandler.cs`
10. `Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/DeleteNotification/DeleteNotificationHandler.cs`
11. `Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/DeleteAllReadNotifications/DeleteAllReadNotificationsHandler.cs`
12. `Anemoi.Notification.Application/Cqrs/Queries/NotificationPreferenceQueries/GetMyNotificationPreference/GetMyNotificationPreferenceHandler.cs`
13. `Anemoi.Notification.Application/Cqrs/Queries/NotificationQueries/GetNotifications/GetNotificationsHandler.cs` (replace)
14. `Anemoi.Contract.Notification/Errors/NotificationErrorDetail.cs` (add new error codes)

### Modified (Backend)
1. `Anemoi.BuildingBlocks/.../Permissions.cs` — add notification permissions
2. `Anemoi.Notification.Domain/Models/NotificationHistory.cs` — add IsDeleted, DeletedAt
3. `Anemoi.Notification.Infrastructure/DataContext/ModelMapping.cs` — add NotificationPreference config, NotificationHistory indexes
4. `Anemoi.Notification.Infrastructure/DataContext/NotificationDbContext.cs` — add DbSet<NotificationPreference>
5. `Anemoi.Notification.Application/Mappings/NotificationMapper.cs` — add preference mapping
6. `Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/CreateNotification/CreateNotificationHandler.cs` — add preference check
7. `Anemoi.Centralize.Api/Controllers/Notification/NotificationController.cs` — add new endpoints
8. Application assembly marker (if needed for MediatR scan)

### Frontend (Create)
1. `src/app/[locale]/(dashboard)/notifications/page.tsx` — Notification Center
2. `src/hooks/useNotificationsCenter.ts` — React Query hook for notification center

### Frontend (Modified)
1. `src/services/notificationService.ts` — add new API methods
2. `src/types/models.ts` — add new types
3. `src/constants/api-endpoints.ts` — add new endpoints
4. `src/constants/permissions.ts` — add new permissions + route mapping
5. `src/components/shared/NotificationBell.tsx` — enhance with cache invalidation
6. `src/components/shared/NotificationContext.tsx` — enhance SignalR event handling
7. `src/hooks/useNotificationSettings.ts` — enhance with global toggles
8. `src/app/[locale]/(dashboard)/settings/notifications/page.tsx` — add EnableInApp/EnableEmail toggles
9. `messages/vi.json` + `messages/en.json` — add new keys

## Test Requirements

- Domain: NotificationPreference entity tests
- Application: Create/Update preference handler tests
- Application: DeleteNotification handler tests (soft-delete)
- Application: DeleteAllReadNotifications handler tests
- Application: Enhanced GetNotifications filter tests
- Application: CreateNotificationHandler preference gate tests
- Integration: Full preference + subscription pipeline
- Integration: Permission enforcement for new endpoints
- Frontend: Component rendering tests (if available)
- Build verification: `dotnet build` + TypeScript compilation

## Acceptance Criteria

1. User can view their notifications with rich filters
2. User can search notifications by keyword
3. User can mark individual notification as read
4. User can mark all as read
5. User can delete individual notification (soft-delete)
6. User can delete all read notifications
7. User can enable/disable in-app notifications globally
8. User can enable/disable email notifications globally
9. User can enable/disable per-category subscriptions
10. Unread count updates in real-time via SignalR
11. Notification Center reflects real-time notifications
12. Preferences API returns combined global + category settings
13. Permissions enforced: notification.view, notification.manage, notification.preference.manage
14. Soft-deleted notifications hidden from all queries
15. Build passes, no warnings
