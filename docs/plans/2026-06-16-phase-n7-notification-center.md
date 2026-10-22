# Phase N7 — Notification Center & User Preferences Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement Notification Center UI, user preferences (global channel toggles), notification hiding/archiving, rich filtering/search, and permission gating.

**Architecture:** Add `NotificationPreference` entity (coexists with existing `NotificationSubscription`), enhance `GetNotifications` with rich filters, add `HideNotification`/`HideAllReadNotifications` commands (soft-hide with `IsHidden`), add preference pipeline check in `CreateNotificationHandler`, and build frontend Notification Center page with infinite scroll and real-time cache mutations.

**Tech Stack:** .NET 10, EF Core, MediatR, MassTransit, SignalR, Next.js 16, React Query, next-intl, shadcn/ui

---

## File Map

### Backend — Contract Layer
| File | Responsibility |
|------|---------------|
| `Anemoi.Contract.Notification/ModelIds/NotificationPreferenceId.cs` | Strongly-typed ID for preference |
| `Anemoi.Contract.Notification/Commands/NotificationPreferenceCommands/CreateOrUpdateNotificationPreference/CreateOrUpdateNotificationPreferenceCommand.cs` | Command to upsert global toggles |
| `Anemoi.Contract.Notification/Commands/NotificationCommands/HideNotification/HideNotificationCommand.cs` | Command to hide single notification |
| `Anemoi.Contract.Notification/Commands/NotificationCommands/HideAllReadNotifications/HideAllReadNotificationsCommand.cs` | Command to hide all read |
| `Anemoi.Contract.Notification/Queries/NotificationQueries/GetNotifications/GetNotificationsQuery.cs` | Enhanced query with filters (replace existing) |
| `Anemoi.Contract.Notification/Queries/NotificationPreferenceQueries/GetMyNotificationPreference/GetMyNotificationPreferenceQuery.cs` | Query for merged preference DTO |
| `Anemoi.Contract.Notification/Responses/NotificationPreferenceResponse.cs` | Merged preference response DTO |
| `Anemoi.Contract.Notification/Errors/NotificationErrorDetail.cs` | Add hide/preference error codes |

### Backend — Domain Layer
| File | Responsibility |
|------|---------------|
| `Anemoi.Notification.Domain/Models/NotificationPreference.cs` | Preference entity |
| `Anemoi.Notification.Domain/Models/NotificationHistory.cs` | Add IsHidden, HiddenAt, Hide() method |

### Backend — Application Layer
| File | Responsibility |
|------|---------------|
| `Anemoi.Notification.Application/Abstractions/INotificationPreferenceProvider.cs` | Interface for preference lookup (enables future caching) |
| `Anemoi.Notification.Application/Mappings/NotificationMapper.cs` | Add preference/hide mappings |
| `Anemoi.Notification.Application/Cqrs/Commands/NotificationPreferenceCommands/CreateOrUpdateNotificationPreference/CreateOrUpdateNotificationPreferenceHandler.cs` | Upsert global toggles handler |
| `Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/HideNotification/HideNotificationHandler.cs` | Hide single + IsHidden check handler |
| `Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/HideAllReadNotifications/HideAllReadNotificationsHandler.cs` | Bulk hide read via ExecuteUpdateAsync |
| `Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/CreateNotification/CreateNotificationHandler.cs` | Add preference gate check |
| `Anemoi.Notification.Application/Cqrs/Queries/NotificationPreferenceQueries/GetMyNotificationPreference/GetMyNotificationPreferenceHandler.cs` | Merge global + subscription data |
| `Anemoi.Notification.Application/Cqrs/Queries/NotificationQueries/GetNotifications/GetNotificationsHandler.cs` | Replace with filter logic |

### Backend — Infrastructure Layer
| File | Responsibility |
|------|---------------|
| `Anemoi.Notification.Infrastructure/DataContext/NotificationDbContext.cs` | Add DbSet<NotificationPreference> |
| `Anemoi.Notification.Infrastructure/DataContext/ModelMapping.cs` | Add NotificationPreference config + update NotificationHistory indexes |

### Backend — API Layer (Centralize)
| File | Responsibility |
|------|---------------|
| `Anemoi.Centralize.Api/Controllers/Notification/NotificationController.cs` | Add GetPreferences, UpdatePreferences, Hide, HideAllRead endpoints |

### Backend — BuildingBlocks (Shared)
| File | Responsibility |
|------|---------------|
| `Anemoi.BuildingBlock.Application/Authorization/Permissions.cs` | Add notification.view, notification.manage, notification.preference.manage |

### Frontend
| File | Responsibility |
|------|---------------|
| `src/app/[locale]/(dashboard)/notifications/page.tsx` | Notification Center page with filters + infinite scroll |
| `src/app/[locale]/(dashboard)/settings/notifications/page.tsx` | Enhance with EnableInApp/EnableEmail toggles |
| `src/hooks/useNotificationsCenter.ts` | React Query hook for notification center |
| `src/hooks/useNotificationSettings.ts` | Enhance with global toggles, merged DTO |
| `src/services/notificationService.ts` | Add hide*, get/updatePreferences methods |
| `src/types/models.ts` | Add NotificationPreferenceResponse |
| `src/constants/api-endpoints.ts` | Add new endpoint constants |
| `src/constants/permissions.ts` | Add notification permissions + route mapping |
| `src/components/shared/NotificationBell.tsx` | Use setQueryData instead of invalidateQueries |
| `src/components/shared/NotificationContext.tsx` | Use setQueryData for real-time cache updates |
| `messages/vi.json` + `messages/en.json` | Add new localization keys |

---

### Task 1: Strongly-Typed ID — NotificationPreferenceId

**Files:**
- Create: `Anemoi.Contract.Notification/ModelIds/NotificationPreferenceId.cs`

- [ ] **Step 1: Create NotificationPreferenceId record**

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Contract.Notification.ModelIds;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<NotificationPreferenceId, Guid>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<NotificationPreferenceId, Guid>))]
public sealed record NotificationPreferenceId(Guid Value) : StronglyTypedId<Guid>(Value);
```

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Contract.Notification/ModelIds/NotificationPreferenceId.cs
git commit -m "feat(n7): add NotificationPreferenceId strongly-typed ID"
```

---

### Task 2: Domain Entity — NotificationPreference

**Files:**
- Create: `Anemoi.Notification.Domain/Models/NotificationPreference.cs`

- [ ] **Step 1: Add Identity contract reference to Domain project**

Add to `Anemoi.Notification.Domain/Anemoi.Notification.Domain.csproj`:
```xml
    <ItemGroup>
        <ProjectReference Include="..\..\Anemoi.Contract\Anemoi.Contract.Notification.ModelIds\Anemoi.Contract.Notification.ModelIds.csproj"/>
        <ProjectReference Include="..\..\Anemoi.BuildingBlocks\Anemoi.BuildingBlock.Domain\Anemoi.BuildingBlock.Domain.csproj"/>
        <!-- Add for strongly-typed UserId -->
        <ProjectReference Include="..\..\Anemoi.Contract\Anemoi.Contract.Identity\Anemoi.Contract.Identity.csproj" />
    </ItemGroup>
```

- [ ] **Step 2: Create NotificationPreference entity**

```csharp
using System;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationPreference : Entity<NotificationPreferenceId>
{
    public UserId UserId { get; set; }
    public bool EnableInApp { get; set; } = true;
    public bool EnableEmail { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Notification.Domain/Models/NotificationPreference.cs
git commit -m "feat(n7): add NotificationPreference domain entity"
```

---

### Task 3: Enhance NotificationHistory — IsHidden + Hide()

**Files:**
- Modify: `Anemoi.Notification.Domain/Models/NotificationHistory.cs`

- [ ] **Step 1: Add IsHidden, HiddenAt, and Hide() method**

Read the current file first, then add after the `Severity` property:

```csharp
    public bool IsHidden { get; set; }
    public DateTime? HiddenAt { get; set; }

    public void Hide()
    {
        IsHidden = true;
        HiddenAt = DateTime.UtcNow;
    }
```

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Notification.Domain/Models/NotificationHistory.cs
git commit -m "feat(n7): add IsHidden, HiddenAt, Hide() to NotificationHistory"
```

---

### Task 4: EF Core Config — NotificationPreference + Updated NotificationHistory Indexes

**Files:**
- Modify: `Anemoi.Notification.Infrastructure/DataContext/ModelMapping.cs`
- Modify: `Anemoi.Notification.Infrastructure/DataContext/NotificationDbContext.cs`

- [ ] **Step 1: Update ModelMapping.cs**

Read the file first. Add `IEntityTypeConfiguration<NotificationPreference>` to the class declaration. Add new Configure method:

```csharp
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new NotificationPreferenceId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId)
            .HasConversion(x => x.Value, id => new UserId(id));
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.Property(x => x.EnableInApp).HasDefaultValue(true);
        builder.Property(x => x.EnableEmail).HasDefaultValue(true);
    }
```

Add using: `using Anemoi.Contract.Identity.ModelIds;` at top.

Update the `NotificationHistory` Configure method to:
- Add `IsHidden` filter to existing indexes or add new composite indexes
- Add index on `(UserId, IsHidden)`

```csharp
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsHidden });
        builder.HasIndex(x => x.WorkspaceId);
        builder.HasIndex(x => x.CreatedTime);
        // ... rest unchanged
```

- [ ] **Step 2: Update NotificationDbContext.cs**

Add DbSet property:
```csharp
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }
```

- [ ] **Step 3: Commit**

```bash
git add Anemoi.Notification.Infrastructure/DataContext/
git commit -m "feat(n7): add NotificationPreference EF config + update indexes"
```

---

### Task 5: Dependency Registration — Assembly Markers

**Files:**
- Check: `Anemoi.Notification.Domain/INotificationDomainAssemblyMarker.cs` (likely exists)
- Check: `Anemoi.Notification.Application/INotificationApplicationAssemblyMarker.cs` (likely exists)
- Check: `Anemoi.Notification.Infrastructure/INotificationInfrastructureAssemblyMarker.cs` (likely exists)

These should already exist from N1-N6. No changes needed — the assembly scanning will pick up the new files automatically. Verify by reading the files.

- [ ] **Step 1: Verify assembly markers exist**

Read each marker file to confirm they exist and are correct.

- [ ] **Step 2: Commit (if any changes needed)**

---

### Task 6: Commands — Contract Layer

**Files:**
- Create: `Anemoi.Contract.Notification/Commands/NotificationPreferenceCommands/CreateOrUpdateNotificationPreference/CreateOrUpdateNotificationPreferenceCommand.cs`
- Create: `Anemoi.Contract.Notification/Commands/NotificationCommands/HideNotification/HideNotificationCommand.cs`
- Create: `Anemoi.Contract.Notification/Commands/NotificationCommands/HideAllReadNotifications/HideAllReadNotificationsCommand.cs`
- Modify: `Anemoi.Contract.Notification/Queries/NotificationQueries/GetNotifications/GetNotificationsQuery.cs`

- [ ] **Step 1: Create CreateOrUpdateNotificationPreferenceCommand**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Commands;

namespace Anemoi.Contract.Notification.Commands.NotificationPreferenceCommands.CreateOrUpdateNotificationPreference;

public sealed record CreateOrUpdateNotificationPreferenceCommand(
    string UserId,
    bool EnableInApp,
    bool EnableEmail) : ICommandVoid;
```

- [ ] **Step 2: Create HideNotificationCommand**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.HideNotification;

public sealed record HideNotificationCommand(NotificationHistoryId Id, string UserId) : ICommandVoid;
```

- [ ] **Step 3: Create HideAllReadNotificationsCommand**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Commands;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.HideAllReadNotifications;

public sealed record HideAllReadNotificationsCommand(string UserId) : ICommandVoid;
```

- [ ] **Step 4: Replace GetNotificationsQuery with enhanced version**

Read the existing file first, then replace with:

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Contract.Notification.Responses;

namespace Anemoi.Contract.Notification.Queries.NotificationQueries.GetNotifications;

public sealed record GetNotificationsQuery(string UserId) :
    GetManyQuery, IQueryPaged<NotificationResponse>
{
    public string StatusFilter { get; init; } = "All";  // All, Read, Unread
    public string Category { get; init; }
    public string Severity { get; init; }
    public string Keyword { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}
```

- [ ] **Step 5: Commit**

```bash
git add Anemoi.Contract.Notification/Commands/ Anemoi.Contract.Notification/Queries/
git commit -m "feat(n7): add hide/preference commands + enhanced GetNotificationsQuery"
```

---

### Task 7: Queries + Response DTOs — Contract Layer

**Files:**
- Create: `Anemoi.Contract.Notification/Queries/NotificationPreferenceQueries/GetMyNotificationPreference/GetMyNotificationPreferenceQuery.cs`
- Create: `Anemoi.Contract.Notification/Responses/NotificationPreferenceResponse.cs`

- [ ] **Step 1: Create GetMyNotificationPreferenceQuery**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Contract.Notification.Responses;

namespace Anemoi.Contract.Notification.Queries.NotificationPreferenceQueries.GetMyNotificationPreference;

public sealed record GetMyNotificationPreferenceQuery(string UserId) : IQueryResult<NotificationPreferenceResponse>;
```

- [ ] **Step 2: Create NotificationPreferenceResponse (merged DTO)**

```csharp
using System.Collections.Generic;

namespace Anemoi.Contract.Notification.Responses;

public sealed record NotificationPreferenceResponse
{
    public bool EnableInApp { get; init; }
    public bool EnableEmail { get; init; }
    public List<NotificationSubscriptionResponse> Subscriptions { get; init; } = new();
}

public sealed record NotificationSubscriptionResponse
{
    public string Category { get; init; }
    public bool IsEnabled { get; init; }
}
```

- [ ] **Step 3: Commit**

```bash
git add Anemoi.Contract.Notification/Queries/NotificationPreferenceQueries/ Anemoi.Contract.Notification/Responses/NotificationPreferenceResponse.cs
git commit -m "feat(n7): add merged preference query + DTO"
```

---

### Task 8: Application Mapping — Mapperly Updates

**Files:**
- Modify: `Anemoi.Notification.Application/Mappings/NotificationMapper.cs`

- [ ] **Step 1: Add mapping methods to NotificationMapper**

Read the existing mapper first, then add:

```csharp
    public NotificationPreferenceResponse ToPreferenceResponse(
        NotificationPreference preference,
        List<NotificationSubscription> subscriptions)
    {
        return new NotificationPreferenceResponse
        {
            EnableInApp = preference.EnableInApp,
            EnableEmail = preference.EnableEmail,
            Subscriptions = subscriptions.Select(ToSubscriptionResponse).ToList()
        };
    }

    public NotificationPreferenceResponse ToDefaultPreferenceResponse(
        List<NotificationSubscription> subscriptions)
    {
        return new NotificationPreferenceResponse
        {
            EnableInApp = true,
            EnableEmail = true,
            Subscriptions = subscriptions.Select(ToSubscriptionResponse).ToList()
        };
    }

    public NotificationSubscriptionResponse ToSubscriptionResponse(
        NotificationSubscription subscription)
    {
        return new NotificationSubscriptionResponse
        {
            Category = subscription.Category,
            IsEnabled = subscription.IsEnabled
        };
    }
```

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Notification.Application/Mappings/NotificationMapper.cs
git commit -m "feat(n7): add preference mapping methods to mapper"
```

---

### Task 9: INotificationPreferenceProvider Abstraction

**Files:**
- Create: `Anemoi.Notification.Application/Abstractions/INotificationPreferenceProvider.cs`

- [ ] **Step 1: Create interface**

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Notification.Application.Abstractions;

/// <summary>
/// Provides notification preference data for the notification creation pipeline.
/// N7 implementation: DatabaseNotificationPreferenceProvider (direct DB query).
/// Future: CachedNotificationPreferenceProvider wrapping DB provider with memory cache.
/// </summary>
public interface INotificationPreferenceProvider
{
    Task<NotificationPreferenceDto> GetPreferenceAsync(Guid userId, CancellationToken ct);
}

public sealed record NotificationPreferenceDto(
    Guid UserId,
    bool EnableInApp,
    bool EnableEmail);

public sealed record NotificationSubscriptionDto(
    string Category,
    bool IsEnabled);
```

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Notification.Application/Abstractions/INotificationPreferenceProvider.cs
git commit -m "feat(n7): add INotificationPreferenceProvider abstraction"
```

---

### Task 10: CreateOrUpdateNotificationPreference Handler

**Files:**
- Create: `Anemoi.Notification.Application/Cqrs/Commands/NotificationPreferenceCommands/CreateOrUpdateNotificationPreference/CreateOrUpdateNotificationPreferenceHandler.cs`

- [ ] **Step 1: Create handler**

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.Commands.NotificationPreferenceCommands.CreateOrUpdateNotificationPreference;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Notification.Domain.Models;
using MediatR;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationPreferenceCommands.CreateOrUpdateNotificationPreference;

public sealed class CreateOrUpdateNotificationPreferenceHandler(
    ISqlRepository<NotificationPreference> sqlRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : IRequestHandler<CreateOrUpdateNotificationPreferenceCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        CreateOrUpdateNotificationPreferenceCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = new UserId(Guid.Parse(request.UserId));
            var existing = await sqlRepository
                .GetFirstByConditionAsync(x => x.UserId == userId, token: cancellationToken);

            if (existing != null)
            {
                existing.EnableInApp = request.EnableInApp;
                existing.EnableEmail = request.EnableEmail;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var preference = new NotificationPreference
                {
                    Id = new NotificationPreferenceId(IdGenerator.NextGuid()),
                    UserId = userId,
                    EnableInApp = request.EnableInApp,
                    EnableEmail = request.EnableEmail,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var createResult = await sqlRepository.CreateOneAsync(preference, cancellationToken);
                var isFailed = createResult.Match(_ => false, _ => true);
                if (isFailed)
                    return NotificationErrorDetail.PreferenceError.SaveFailed().ToErrorDetailResponse();
            }

            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            return saveResult.Match<OneOf<None, ErrorDetailResponse>>(
                _ => None.Value,
                ex =>
                {
                    logger.Error(ex, "Failed to save notification preference for User: {UserId}", request.UserId);
                    return NotificationErrorDetail.PreferenceError.SaveFailed().ToErrorDetailResponse();
                }
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error in CreateOrUpdateNotificationPreferenceHandler for User: {UserId}", request.UserId);
            return NotificationErrorDetail.PreferenceError.SaveFailed().ToErrorDetailResponse();
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Notification.Application/Cqrs/Commands/NotificationPreferenceCommands/
git commit -m "feat(n7): add CreateOrUpdateNotificationPreference handler"
```

---

### Task 11: HideNotification Handler

**Files:**
- Create: `Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/HideNotification/HideNotificationHandler.cs`

- [ ] **Step 1: Create handler**

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Notification.Commands.NotificationCommands.HideNotification;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Notification.Domain.Models;
using MediatR;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.HideNotification;

public sealed class HideNotificationHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : IRequestHandler<HideNotificationCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        HideNotificationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = await sqlRepository
                .GetFirstByConditionAsync(
                    x => x.Id == request.Id && x.UserId == Guid.Parse(request.UserId),
                    token: cancellationToken);

            if (notification == null)
                return NotificationErrorDetail.NotificationError.NotFound().ToErrorDetailResponse();

            notification.Hide();

            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            return saveResult.Match<OneOf<None, ErrorDetailResponse>>(
                _ => None.Value,
                ex =>
                {
                    logger.Error(ex, "Failed to hide notification {NotificationId}", request.Id);
                    return NotificationErrorDetail.NotificationError.HideFailed().ToErrorDetailResponse();
                }
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error in HideNotificationHandler for {NotificationId}", request.Id);
            return NotificationErrorDetail.NotificationError.HideFailed().ToErrorDetailResponse();
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/HideNotification/
git commit -m "feat(n7): add HideNotification handler"
```

---

### Task 12: HideAllReadNotifications Handler (Bulk ExecuteUpdateAsync)

**Files:**
- Create: `Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/HideAllReadNotifications/HideAllReadNotificationsHandler.cs`

- [ ] **Step 1: Create handler**

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Notification.Commands.NotificationCommands.HideAllReadNotifications;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Notification.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.HideAllReadNotifications;

public sealed class HideAllReadNotificationsHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : IRequestHandler<HideAllReadNotificationsCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        HideAllReadNotificationsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userGuid = Guid.Parse(request.UserId);
            var now = DateTime.UtcNow;

            await sqlRepository.GetQueryable()
                .Where(x => x.UserId == userGuid && x.IsRead && !x.IsHidden)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.IsHidden, true)
                        .SetProperty(x => x.HiddenAt, now),
                    cancellationToken);

            return None.Value;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error in HideAllReadNotificationsHandler for User: {UserId}", request.UserId);
            return NotificationErrorDetail.NotificationError.HideFailed().ToErrorDetailResponse();
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/HideAllReadNotifications/
git commit -m "feat(n7): add HideAllReadNotifications handler with ExecuteUpdateAsync"
```

---

### Task 13: Enhanced GetNotifications Handler

**Files:**
- Replace: `Anemoi.Notification.Application/Cqrs/Queries/NotificationQueries/GetNotifications/GetNotificationsHandler.cs`

- [ ] **Step 1: Replace GetNotificationsHandler with filter logic**

Read the existing file first, then replace content:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.Notification.Queries.NotificationQueries.GetNotifications;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Application.Mappings;
using Anemoi.Notification.Domain.Models;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Queries.NotificationQueries.GetNotifications;

public sealed class GetNotificationsHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    NotificationMapper mapper,
    ILogger logger)
    : EfQueryPaginationHandler<NotificationHistory, GetNotificationsQuery, NotificationResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<NotificationHistory, NotificationResponse> BuildQueryFlow(
        IQueryListFilter<NotificationHistory, NotificationResponse> fromFlow, GetNotificationsQuery query)
    {
        var targetUserGuid = Guid.Parse(query.UserId);

        var filter = BuildFilterExpression(targetUserGuid, query);
        var flow = fromFlow
            .WithFilter(filter)
            .WithSpecialAction(x => x)
            .WithSortFieldWhenNotSet(x => x.CreatedTime)
            .WithSortedDirectionWhenNotSet(SortedDirection.Descending);

        return flow;
    }

    private static Expression<Func<NotificationHistory, bool>> BuildFilterExpression(
        Guid userId, GetNotificationsQuery query)
    {
        // Base: user's own notifications, not hidden
        Expression<Func<NotificationHistory, bool>> filter = x =>
            x.UserId == userId && !x.IsHidden;

        // Status filter
        if (!string.IsNullOrEmpty(query.StatusFilter) &&
            !query.StatusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var isReadFilter = query.StatusFilter.Equals("Read", StringComparison.OrdinalIgnoreCase);
            // Combine with AND
            var param = filter.Parameters[0];
            var body = Expression.AndAlso(filter.Body,
                Expression.Equal(
                    Expression.Property(param, "IsRead"),
                    Expression.Constant(isReadFilter)));
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(body, param);
        }

        // Category filter
        if (!string.IsNullOrEmpty(query.Category))
        {
            var param = filter.Parameters[0];
            var body = Expression.AndAlso(filter.Body,
                Expression.Equal(
                    Expression.Property(param, "Category"),
                    Expression.Constant(query.Category)));
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(body, param);
        }

        // Severity filter
        if (!string.IsNullOrEmpty(query.Severity))
        {
            var param = filter.Parameters[0];
            var body = Expression.AndAlso(filter.Body,
                Expression.Equal(
                    Expression.Property(param, "Severity"),
                    Expression.Constant(query.Severity)));
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(body, param);
        }

        // Keyword search (Title + Content)
        if (!string.IsNullOrEmpty(query.Keyword))
        {
            var keyword = query.Keyword.ToLower();
            var param = filter.Parameters[0];
            var titleContains = Expression.Call(
                Expression.Property(param, "Title"),
                "Contains", null, Expression.Constant(keyword));
            var contentContains = Expression.Call(
                Expression.Property(param, "Content"),
                "Contains", null, Expression.Constant(keyword));
            var orElse = Expression.OrElse(titleContains, contentContains);
            var body = Expression.AndAlso(filter.Body, orElse);
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(body, param);
        }

        // Date range filter
        if (query.DateFrom.HasValue)
        {
            var param = filter.Parameters[0];
            var body = Expression.AndAlso(filter.Body,
                Expression.GreaterThanOrEqual(
                    Expression.Property(param, "CreatedTime"),
                    Expression.Constant(query.DateFrom.Value)));
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(body, param);
        }

        if (query.DateTo.HasValue)
        {
            var param = filter.Parameters[0];
            body = Expression.AndAlso(filter.Body,
                Expression.LessThanOrEqual(
                    Expression.Property(param, "CreatedTime"),
                    Expression.Constant(query.DateTo.Value)));
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(body, param);
        }

        return filter;
    }

    protected override Task<PaginationResponse<NotificationResponse>> MapToResultAsync(
        GetNotificationsQuery query,
        OneOf<List<NotificationHistory>, List<NotificationResponse>> modelsOrResponses,
        long totalRecord)
    {
        return modelsOrResponses.Match(
            models =>
            {
                var list = models.Select(mapper.ToNotificationResponse).ToList();
                return Task.FromResult(new PaginationResponse<NotificationResponse>(list, totalRecord));
            },
            responses => Task.FromResult(new PaginationResponse<NotificationResponse>(responses, totalRecord))
        );
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Notification.Application/Cqrs/Queries/NotificationQueries/GetNotifications/
git commit -m "feat(n7): enhance GetNotificationsHandler with rich filters + IsHidden exclusion"
```

---

### Task 14: GetMyNotificationPreference Handler

**Files:**
- Create: `Anemoi.Notification.Application/Cqrs/Queries/NotificationPreferenceQueries/GetMyNotificationPreference/GetMyNotificationPreferenceHandler.cs`

- [ ] **Step 1: Create handler**

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.Queries.NotificationPreferenceQueries.GetMyNotificationPreference;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Application.Mappings;
using Anemoi.Notification.Domain.Models;
using MediatR;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Queries.NotificationPreferenceQueries.GetMyNotificationPreference;

public sealed class GetMyNotificationPreferenceHandler(
    ISqlRepository<NotificationPreference> preferenceRepository,
    ISqlRepository<NotificationSubscription> subscriptionRepository,
    NotificationMapper mapper,
    ILogger logger)
    : IRequestHandler<GetMyNotificationPreferenceQuery, OneOf<NotificationPreferenceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<NotificationPreferenceResponse, ErrorDetailResponse>> Handle(
        GetMyNotificationPreferenceQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = new UserId(Guid.Parse(request.UserId));
            var userGuid = userId.Value;

            var preference = await preferenceRepository
                .GetFirstByConditionAsync(x => x.UserId == userId, token: cancellationToken);

            var subscriptions = (await subscriptionRepository
                .GetManyByConditionAsync(x => x.UserId == userGuid, token: cancellationToken))
                .ToList();

            if (preference != null)
                return mapper.ToPreferenceResponse(preference, subscriptions);

            return mapper.ToDefaultPreferenceResponse(subscriptions);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error in GetMyNotificationPreferenceHandler for User: {UserId}", request.UserId);
            return new NotificationPreferenceResponse
            {
                EnableInApp = true,
                EnableEmail = true,
                Subscriptions = []
            };
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Notification.Application/Cqrs/Queries/NotificationPreferenceQueries/
git commit -m "feat(n7): add GetMyNotificationPreference handler with merged DTO"
```

---

### Task 15: Update CreateNotificationHandler — Preference Gate

**Files:**
- Modify: `Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/CreateNotification/CreateNotificationHandler.cs`

- [ ] **Step 1: Add preference + subscription gate before dedup check**

Read the existing file. After parsing `userGuid` and before the subscription check, add preference check:

```csharp
            // Check global preference (EnableInApp)
            var preference = await sqlRepository.GetFirstByConditionAsync<NotificationPreference>(
                x => x.UserId == userGuid, token: cancellationToken);
            if (preference != null && !preference.EnableInApp)
            {
                logger.Information(
                    "In-app notifications disabled for User {UserId}. Skipping creation.",
                    request.UserId);
                return new NotificationResponse
                {
                    Id = Guid.Empty.ToString(),
                    UserId = request.UserId,
                    Title = request.Title,
                    Content = request.Content,
                    Category = request.Category,
                    IsRead = true,
                    CreatedTime = DateTime.UtcNow
                };
            }

            // Check per-category subscription (existing logic)
            var subscription = await subscriptionRepository.GetFirstByConditionAsync(
                x => x.UserId == userGuid && x.Category == request.Category,
                token: cancellationToken);

            if (subscription != null && !subscription.IsEnabled)
            {
                logger.Information(
                    "Notification category {Category} is disabled for User {UserId}. Skipping creation.",
                    request.Category, request.UserId);
                return new NotificationResponse
                {
                    Id = Guid.Empty.ToString(),
                    UserId = request.UserId,
                    Title = request.Title,
                    Content = request.Content,
                    Category = request.Category,
                    IsRead = true,
                    CreatedTime = DateTime.UtcNow
                };
            }
```

Note: You need to inject `ISqlRepository<NotificationPreference>` into the constructor and add a using for `Anemoi.Notification.Domain.Models`.

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Notification.Application/Cqrs/Commands/NotificationCommands/CreateNotification/
git commit -m "feat(n7): add preference gate to CreateNotificationHandler"
```

---

### Task 16: Error Codes — NotificationErrorDetail

**Files:**
- Modify: `Anemoi.Contract.Notification/Errors/NotificationErrorDetail.cs`

- [ ] **Step 1: Read existing file and add preference/hide error codes**

```csharp
    public static class PreferenceError
    {
        public static ErrorDetail SaveFailed() => new("NotificationPreference.SaveFailed",
            "Failed to save notification preferences.");
    }

    public static class NotificationError
    {
        // ... existing errors ...

        public static ErrorDetail NotFound() => new("Notification.NotFound",
            "Notification not found.");

        public static ErrorDetail HideFailed() => new("Notification.HideFailed",
            "Failed to hide notification.");
    }
```

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Contract.Notification/Errors/NotificationErrorDetail.cs
git commit -m "feat(n7): add preference and hide error codes"
```

---

### Task 17: Permissions — Register Notification Permissions

**Files:**
- Modify: `Anemoi.BuildingBlock.Application/Authorization/Permissions.cs`

- [ ] **Step 1: Add permission constants**

```csharp
    public const string NotificationView = "notification.view";
    public const string NotificationManage = "notification.manage";
    public const string NotificationPreferenceManage = "notification.preference.manage";
```

- [ ] **Step 2: Add to Definitions list**

```csharp
        new(NotificationView, "PermissionGroupNotifications", "PermissionDescriptionNotificationView"),
        new(NotificationManage, "PermissionGroupNotifications", "PermissionDescriptionNotificationManage"),
        new(NotificationPreferenceManage, "PermissionGroupNotifications", "PermissionDescriptionNotificationPreferenceManage"),
```

- [ ] **Step 3: Commit**

```bash
git add Anemoi.BuildingBlock.Application/Authorization/Permissions.cs
git commit -m "feat(n7): add notification permissions"
```

---

### Task 18: API Controller — New Endpoints

**Files:**
- Modify: `Anemoi.Centralize.Api/Controllers/Notification/NotificationController.cs`

- [ ] **Step 1: Add new endpoints to controller**

Read the existing controller first, then add:

```csharp
    [HttpPost("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Hide(Guid id, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new HideNotificationCommand(new NotificationHistoryId(id), userId);
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HideAllRead(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new HideAllReadNotificationsCommand(userId);
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpGet]
    [ProducesResponseType(typeof(NotificationPreferenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var query = new GetMyNotificationPreferenceQuery(userId);
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] CreateOrUpdateNotificationPreferenceCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var cmd = command with { UserId = userId };
        var res = await sender.Send(cmd, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }
```

Add necessary using imports at the top:
```csharp
using Anemoi.Contract.Notification.Commands.NotificationCommands.HideNotification;
using Anemoi.Contract.Notification.Commands.NotificationCommands.HideAllReadNotifications;
using Anemoi.Contract.Notification.Commands.NotificationPreferenceCommands.CreateOrUpdateNotificationPreference;
using Anemoi.Contract.Notification.Queries.NotificationPreferenceQueries.GetMyNotificationPreference;
using Anemoi.Contract.Notification.Responses;
```

- [ ] **Step 2: Commit**

```bash
git add Anemoi.Centralize.Api/Controllers/Notification/
git commit -m "feat(n7): add hide, preferences API endpoints"
```

---

### Task 19: Database Migration

**Files:**
- Run: `dotnet ef migrations add` in Anemoi.Notification.Infrastructure

- [ ] **Step 1: Create migration**

```bash
cd Anemoi.Notification/Anemoi.Notification.Infrastructure
dotnet ef migrations add PhaseN7_AddNotificationPreferenceAndHide --startup-project ../Anemoi.Notification.WorkerService
```

- [ ] **Step 2: Review generated migration**

Verify the migration contains:
- CreateTable for NotificationPreferences (columns: Id, UserId, EnableInApp, EnableEmail, CreatedAt, UpdatedAt)
- Unique index on UserId
- AddColumn for IsHidden (bool, default false) on NotificationHistories
- AddColumn for HiddenAt (DateTime?) on NotificationHistories

- [ ] **Step 3: Build and verify**

```bash
dotnet build Anemoi.Notification.Infrastructure
```

- [ ] **Step 4: Generate SQL script (optional verification)**

```bash
dotnet ef migrations script -o ../../migration_n7.sql --startup-project ../Anemoi.Notification.WorkerService
```

- [ ] **Step 5: Commit**

```bash
git add Anemoi.Notification.Infrastructure/DataContext/Migrations/
git commit -m "feat(n7): add migration for NotificationPreference + IsHidden"
```

---

### Task 20: Localization — Permission Descriptions

**Files:**
- Locate: Localization resource files for permission descriptions

- [ ] **Step 1: Find localization files for permissions**

Search for existing permission localization keys like `PermissionDescriptionUserRead` in resource files (.resx or similar).

- [ ] **Step 2: Add notification permission descriptions**

Add entries for:
- `PermissionGroupNotifications` = "Notifications"
- `PermissionDescriptionNotificationView` = "View notifications" / "Xem thông báo"
- `PermissionDescriptionNotificationManage` = "Manage notifications" / "Quản lý thông báo"
- `PermissionDescriptionNotificationPreferenceManage` = "Manage notification preferences" / "Quản lý tùy chọn thông báo"

- [ ] **Step 3: Commit**

```bash
git add [localization files]
git commit -m "feat(n7): add notification permission localization"
```

---

### Task 21: Build Verification

- [ ] **Step 1: Build the solution**

```bash
dotnet build Anemoi.sln
```

Fix any compilation errors.

- [ ] **Step 2: Run backend tests**

```bash
dotnet test Anemoi.sln --filter "FullyQualifiedName~Notification"
```

---

### Task 22: Frontend — Types + API Endpoints

**Files:**
- Modify: `cody-web-app/src/types/models.ts`
- Modify: `cody-web-app/src/constants/api-endpoints.ts`
- Modify: `cody-web-app/src/constants/permissions.ts`

- [ ] **Step 1: Add NotificationPreferenceResponse type**

Add to `cody-web-app/src/types/models.ts`:

```typescript
export interface NotificationPreferenceResponse {
  enableInApp: boolean;
  enableEmail: boolean;
  subscriptions: NotificationSubscriptionResponse[];
}

export interface NotificationSubscriptionResponse {
  category: string;
  isEnabled: boolean;
}
```

- [ ] **Step 2: Add API endpoint constants**

Add to `cody-web-app/src/constants/api-endpoints.ts`:

```typescript
  notification: {
    // ... existing ...
    hideNotification: (id: string) => `/api/notification/Notification/Hide/${id}`,
    hideAllRead: "/api/notification/Notification/HideAllRead",
    getPreferences: "/api/notification/Notification/GetPreferences",
    updatePreferences: "/api/notification/Notification/UpdatePreferences",
  },
```

- [ ] **Step 3: Add frontend permissions and route mapping**

Add to `cody-web-app/src/constants/permissions.ts`:

```typescript
  NOTIFICATION_VIEW: "notification.view",
  NOTIFICATION_MANAGE: "notification.manage",
  NOTIFICATION_PREFERENCE_MANAGE: "notification.preference.manage",
```

Add route mapping:
```typescript
  "/notifications": [PERMISSIONS.NOTIFICATION_VIEW],
  "/settings/notifications": [PERMISSIONS.NOTIFICATION_PREFERENCE_MANAGE],
```

- [ ] **Step 4: Commit**

```bash
git add cody-web-app/src/types/models.ts cody-web-app/src/constants/api-endpoints.ts cody-web-app/src/constants/permissions.ts \
  cody-web-app/src/types/models.ts cody-web-app/src/constants/api-endpoints.ts cody-web-app/src/constants/permissions.ts
git commit -m "feat(n7): add frontend types, endpoints, permissions"
```

---

### Task 23: Frontend — Notification Service

**Files:**
- Modify: `cody-web-app/src/services/notificationService.ts`

- [ ] **Step 1: Add new API methods**

```typescript
  hideNotification: async (id: string): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.notification.hideNotification(id));
  },

  hideAllRead: async (): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.notification.hideAllRead);
  },

  getPreferences: async (): Promise<NotificationPreferenceResponse> => {
    const { data } = await apiClient.get<NotificationPreferenceResponse>(
      API_ENDPOINTS.notification.getPreferences
    );
    return data;
  },

  updatePreferences: async (enableInApp: boolean, enableEmail: boolean): Promise<void> => {
    await apiClient.put(API_ENDPOINTS.notification.updatePreferences, {
      userId: "",
      enableInApp,
      enableEmail,
    });
  },
```

Add the import:
```typescript
import type { NotificationPreferenceResponse } from "@/types/models";
```

- [ ] **Step 2: Commit**

```bash
git add cody-web-app/src/services/notificationService.ts
git commit -m "feat(n7): add hide/preferences API methods to notification service"
```

---

### Task 24: Frontend — Notification Context (setQueryData for real-time)

**Files:**
- Modify: `cody-web-app/src/components/shared/NotificationContext.tsx`

- [ ] **Step 1: Update `ReceiveNotification` handler to use setQueryData**

Read the existing file. In the SignalR `ReceiveNotification` handler, replace the current logic with:

```typescript
          notificationHubManager.on("ReceiveNotification", (notification: NotificationResponse) => {
            // Skip duplicates by ID
            if (notificationIdsRef.current.has(notification.id)) return;

            notificationIdsRef.current.add(notification.id);

            // Update unread count
            setUnreadCount((prev) => prev + 1);

            // Update Notification Bell cache directly (prepend)
            queryClient.setQueryData<NotificationResponse[]>(
              ["notifications", "bell"],
              (old) => old ? [notification, ...old] : [notification]
            );

            // Update Notification Center cache directly (prepend)
            queryClient.setQueryData<PaginatedResponse<NotificationResponse>>(
              ["notifications", "center"],
              (old) => {
                if (!old) return { items: [notification], totalCount: 1 };
                // Check duplicate
                if (old.items.some((n) => n.id === notification.id)) return old;
                return {
                  ...old,
                  items: [notification, ...old.items],
                  totalCount: old.totalCount + 1,
                };
              }
            );

            // Show toast
            // ... existing toast logic ...
          });
```

- [ ] **Step 2: Commit**

```bash
git add cody-web-app/src/components/shared/NotificationContext.tsx
git commit -m "feat(n7): use queryClient.setQueryData for real-time notification updates"
```

---

### Task 25: Frontend — Notification Bell (setQueryData)

**Files:**
- Modify: `cody-web-app/src/components/shared/NotificationBell.tsx`

- [ ] **Step 1: Update NotificationBell to use React Query cache**

The NotificationBell already reads from NotificationContext. Add a query key for the bell preview:

```typescript
const { data: bellNotifications } = useQuery({
  queryKey: ["notifications", "bell"],
  queryFn: () => notificationService.getNotifications(1, 5),
  select: (res) => res.items,
  staleTime: 30_000,
});
```

Or keep it as-is using context (the context already gets updates via setQueryData). The bell reads from `notifications` and `unreadCount` which are already React state in the context. No changes needed if the context already updates state correctly from SignalR.

- [ ] **Step 2: Verify real-time flow**

Ensure that when `ReceiveNotification` fires:
1. Context adds to local `notifications` state (already done)
2. Context updates `unreadCount` (already done)
3. Bell component re-renders via context (already done)

- [ ] **Step 3: Commit** (minimal or skip if no changes needed)

---

### Task 26: Frontend — Notification Center Page

**Files:**
- Create: `cody-web-app/src/app/[locale]/(dashboard)/notifications/page.tsx`
- Create: `cody-web-app/src/hooks/useNotificationsCenter.ts`

- [ ] **Step 1: Create useNotificationsCenter hook**

```typescript
"use client";

import { useInfiniteQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { notificationService } from "@/services/notificationService";
import { useNotifications } from "@/components/shared/NotificationContext";
import type { NotificationResponse } from "@/types/models";

interface UseNotificationsCenterProps {
  statusFilter?: string;
  category?: string;
  severity?: string;
  keyword?: string;
  dateFrom?: string;
  dateTo?: string;
}

export function useNotificationsCenter(filters: UseNotificationsCenterProps = {}) {
  const qc = useQueryClient();
  const { unreadCount, markAsRead, markAllAsRead } = useNotifications();

  const buildQueryParams = (page: number) => {
    const params = new URLSearchParams();
    params.set("page", String(page));
    params.set("size", "20");
    if (filters.statusFilter && filters.statusFilter !== "All") params.set("StatusFilter", filters.statusFilter);
    if (filters.category) params.set("Category", filters.category);
    if (filters.severity) params.set("Severity", filters.severity);
    if (filters.keyword) params.set("Keyword", filters.keyword);
    if (filters.dateFrom) params.set("DateFrom", filters.dateFrom);
    if (filters.dateTo) params.set("DateTo", filters.dateTo);
    return params.toString();
  };

  const query = useInfiniteQuery({
    queryKey: ["notifications", "center", filters],
    queryFn: async ({ pageParam = 1 }) => {
      const params = buildQueryParams(pageParam);
      const res = await notificationService.getNotifications(pageParam, 20);
      return { items: res.items, totalCount: res.totalCount, page: pageParam };
    },
    getNextPageParam: (lastPage, allPages) => {
      const totalFetched = allPages.reduce((sum, p) => sum + p.items.length, 0);
      return totalFetched < lastPage.totalCount ? lastPage.page + 1 : undefined;
    },
    initialPageParam: 1,
    staleTime: 30_000,
  });

  const hideMutation = useMutation({
    mutationFn: (id: string) => notificationService.hideNotification(id),
    onSuccess: (_, id) => {
      qc.setQueryData(["notifications", "center"], (old: any) => {
        if (!old) return old;
        return {
          ...old,
          pages: old.pages.map((page: any) => ({
            ...page,
            items: page.items.filter((n: NotificationResponse) => n.id !== id),
          })),
        };
      });
    },
  });

  const hideAllReadMutation = useMutation({
    mutationFn: () => notificationService.hideAllRead(),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["notifications", "center"] });
    },
  });

  const notifications = query.data?.pages.flatMap((p) => p.items) ?? [];
  const totalCount = query.data?.pages[0]?.totalCount ?? 0;

  return {
    notifications,
    totalCount,
    unreadCount,
    isLoading: query.isLoading,
    isFetchingNextPage: query.isFetchingNextPage,
    hasNextPage: query.hasNextPage,
    fetchNextPage: query.fetchNextPage,
    markAsRead,
    markAllAsRead,
    hideNotification: hideMutation.mutate,
    hideAllRead: hideAllReadMutation.mutate,
    isHidingAllRead: hideAllReadMutation.isPending,
    refetch: query.refetch,
  };
}
```

- [ ] **Step 2: Create Notification Center page**

```tsx
"use client";

import { useState, useEffect, useCallback, useRef } from "react";
import { useTranslations } from "next-intl";
import { usePermissions } from "@/hooks/usePermissions";
import { useNotificationsCenter } from "@/hooks/useNotificationsCenter";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Bell,
  CheckCheck,
  EyeOff,
  Search,
  Loader2,
  Filter,
  Clock,
  Inbox,
  Circle,
} from "lucide-react";
import { cn } from "@/lib/utils";

const STATUS_OPTIONS = ["All", "Read", "Unread"] as const;
const CATEGORY_OPTIONS = ["", "system", "workspace", "task", "environment", "leave", "overtime", "payroll"] as const;
const SEVERITY_OPTIONS = ["", "Info", "Warning", "Error", "Critical"] as const;

export default function NotificationsPage() {
  const t = useTranslations("Notifications");
  const { canAccessRoute } = usePermissions();

  const [statusFilter, setStatusFilter] = useState("All");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [severityFilter, setSeverityFilter] = useState("");
  const [keyword, setKeyword] = useState("");
  const [debouncedKeyword, setDebouncedKeyword] = useState("");

  // Debounce keyword search
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedKeyword(keyword), 300);
    return () => clearTimeout(timer);
  }, [keyword]);

  const {
    notifications,
    totalCount,
    unreadCount,
    isLoading,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
    markAsRead,
    markAllAsRead,
    hideNotification,
    hideAllRead,
    isHidingAllRead,
  } = useNotificationsCenter({
    statusFilter: statusFilter !== "All" ? statusFilter : undefined,
    category: categoryFilter || undefined,
    severity: severityFilter || undefined,
    keyword: debouncedKeyword || undefined,
  });

  // Permission guard
  if (!canAccessRoute("/notifications")) {
    return (
      <div className="flex h-[50vh] items-center justify-center">
        <div className="text-center">
          <Bell className="mx-auto h-12 w-12 text-slate-300" />
          <p className="mt-2 text-sm text-slate-500">{t("noAccess")}</p>
        </div>
      </div>
    );
  }

  const getCategoryIcon = (cat: string) => {
    switch (cat.toLowerCase()) {
      case "system": return <Bell className="h-4 w-4 text-blue-500" />;
      case "workspace": return <Bell className="h-4 w-4 text-emerald-500" />;
      case "task": return <Bell className="h-4 w-4 text-amber-500" />;
      case "environment": return <Bell className="h-4 w-4 text-indigo-500" />;
      case "leave": return <Bell className="h-4 w-4 text-purple-500" />;
      case "overtime": return <Bell className="h-4 w-4 text-orange-500" />;
      case "payroll": return <Bell className="h-4 w-4 text-rose-500" />;
      default: return <Bell className="h-4 w-4 text-slate-500" />;
    }
  };

  const formatTime = (timeString: string) => {
    try {
      const date = new Date(timeString);
      const now = new Date();
      const diffMs = now.getTime() - date.getTime();
      const diffMins = Math.floor(diffMs / 60000);
      if (diffMins < 1) return t("justNow");
      if (diffMins < 60) return t("minutesAgo", { count: diffMins });
      const diffHrs = Math.floor(diffMins / 60);
      if (diffHrs < 24) return t("hoursAgo", { count: diffHrs });
      const diffDays = Math.floor(diffHrs / 24);
      if (diffDays < 7) return t("daysAgo", { count: diffDays });
      return date.toLocaleDateString();
    } catch {
      return "";
    }
  };

  return (
    <div className="flex flex-col gap-3 w-full">
      {/* Header */}
      <div className="flex items-start justify-between border-b border-border/40 pb-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t("title")}</h1>
          <p className="text-sm text-slate-500 mt-1">
            {unreadCount > 0
              ? t("unreadCount", { count: unreadCount })
              : t("allRead")}
          </p>
        </div>
        <div className="flex items-center gap-2">
          {unreadCount > 0 && (
            <>
              <Button
                variant="outline"
                size="sm"
                onClick={() => markAllAsRead()}
                className="flex items-center gap-1.5"
              >
                <CheckCheck className="h-4 w-4" />
                {t("markAllAsRead")}
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => hideAllRead()}
                disabled={isHidingAllRead}
                className="flex items-center gap-1.5"
              >
                {isHidingAllRead ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <EyeOff className="h-4 w-4" />
                )}
                {t("hideAllRead")}
              </Button>
            </>
          )}
        </div>
      </div>

      {/* Filters */}
      <Card className="border border-border/40">
        <CardContent className="p-4">
          <div className="flex flex-wrap items-center gap-3">
            <div className="relative flex-1 min-w-[200px]">
              <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-slate-400" />
              <Input
                placeholder={t("searchPlaceholder")}
                value={keyword}
                onChange={(e) => setKeyword(e.target.value)}
                className="pl-8"
              />
            </div>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-[130px]">
                <SelectValue placeholder={t("status")} />
              </SelectTrigger>
              <SelectContent>
                {STATUS_OPTIONS.map((opt) => (
                  <SelectItem key={opt} value={opt}>
                    {t(`status.${opt.toLowerCase()}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={categoryFilter} onValueChange={setCategoryFilter}>
              <SelectTrigger className="w-[160px]">
                <SelectValue placeholder={t("category")} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="">{t("allCategories")}</SelectItem>
                {CATEGORY_OPTIONS.filter(Boolean).map((cat) => (
                  <SelectItem key={cat} value={cat}>
                    {t(`categories.${cat}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={severityFilter} onValueChange={setSeverityFilter}>
              <SelectTrigger className="w-[130px]">
                <SelectValue placeholder={t("severity")} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="">{t("allSeverities")}</SelectItem>
                {SEVERITY_OPTIONS.filter(Boolean).map((sev) => (
                  <SelectItem key={sev} value={sev}>
                    {t(`severities.${sev.toLowerCase()}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Notification List */}
      <div className="flex flex-col gap-2">
        {isLoading ? (
          <div className="flex h-[40vh] items-center justify-center">
            <Loader2 className="h-8 w-8 animate-spin text-slate-400" />
          </div>
        ) : notifications.length === 0 ? (
          <div className="flex h-[40vh] flex-col items-center justify-center text-slate-400">
            <Inbox className="h-12 w-12 mb-3 opacity-30" />
            <p className="text-sm">{t("emptyState")}</p>
          </div>
        ) : (
          <>
            {notifications.map((n) => (
              <Card
                key={n.id}
                className={cn(
                  "border border-border/40 transition-colors duration-150 hover:bg-slate-50/50 dark:hover:bg-slate-900/10",
                  !n.isRead && "border-l-2 border-l-blue-500 bg-blue-50/20 dark:bg-blue-950/5"
                )}
              >
                <CardContent className="p-4">
                  <div className="flex items-start gap-3">
                    <div className="flex-shrink-0 mt-0.5 p-2 rounded-full bg-slate-100 dark:bg-slate-800">
                      {getCategoryIcon(n.category)}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center justify-between gap-2">
                        <div className="flex items-center gap-2">
                          <span className={cn(
                            "text-sm",
                            !n.isRead && "font-semibold"
                          )}>
                            {n.title}
                          </span>
                          {!n.isRead && (
                            <Circle className="h-2 w-2 fill-blue-600 text-blue-600" />
                          )}
                        </div>
                        <div className="flex items-center gap-1.5">
                          <Badge
                            variant="outline"
                            className="text-[10px] px-1.5 py-0 h-5"
                          >
                            {t(`categories.${n.category.toLowerCase()}`)}
                          </Badge>
                        </div>
                      </div>
                      <p className="text-xs text-slate-600 dark:text-slate-400 mt-1 leading-relaxed">
                        {n.content}
                      </p>
                      <div className="flex items-center justify-between mt-2">
                        <div className="flex items-center gap-1 text-[10px] text-slate-400">
                          <Clock className="h-3 w-3" />
                          {formatTime(n.createdTime)}
                        </div>
                        <div className="flex items-center gap-1">
                          {!n.isRead && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => markAsRead(n.id)}
                              className="h-7 text-xs px-2"
                            >
                              <CheckCheck className="h-3 w-3 mr-1" />
                              {t("markRead")}
                            </Button>
                          )}
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => hideNotification(n.id)}
                            className="h-7 text-xs px-2 text-slate-400 hover:text-red-500"
                          >
                            <EyeOff className="h-3 w-3 mr-1" />
                            {t("hide")}
                          </Button>
                        </div>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}

            {/* Infinite scroll trigger */}
            {hasNextPage && (
              <div className="flex justify-center py-4">
                <Button
                  variant="ghost"
                  onClick={() => fetchNextPage()}
                  disabled={isFetchingNextPage}
                  className="text-sm text-slate-500"
                >
                  {isFetchingNextPage ? (
                    <Loader2 className="h-4 w-4 animate-spin mr-2" />
                  ) : null}
                  {t("loadMore")}
                </Button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}
```

- [ ] **Step 3: Add route to app router**

Create the directory `cody-web-app/src/app/[locale]/(dashboard)/notifications/` and add `page.tsx`.

- [ ] **Step 4: Commit**

```bash
git add cody-web-app/src/app/[locale]/(dashboard)/notifications/ cody-web-app/src/hooks/useNotificationsCenter.ts
git commit -m "feat(n7): add Notification Center page with infinite scroll"
```

---

### Task 27: Frontend — Preferences Page (EnableInApp/EnableEmail)

**Files:**
- Modify: `cody-web-app/src/app/[locale]/(dashboard)/settings/notifications/page.tsx`
- Modify: `cody-web-app/src/hooks/useNotificationSettings.ts`

- [ ] **Step 1: Update useNotificationSettings hook with global toggles**

Replace the hook with:

```typescript
"use client";

import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { toast } from "sonner";
import { notificationService } from "@/services/notificationService";
import type { NotificationPreferenceResponse, NotificationSettingResponse } from "@/types/models";
import { handleLocalApiError } from "@/lib/api-events";

export function useNotificationSettings() {
  const t = useTranslations("Notifications");
  const [preferences, setPreferences] = useState<NotificationPreferenceResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    void notificationService.getPreferences()
      .then((response) => setPreferences(response))
      .catch((error: unknown) => {
        handleLocalApiError(error, () => {
          console.error("Failed to load notification preferences", error);
          toast.error(t("failedLoadSettings"));
        });
      })
      .finally(() => setIsLoading(false));
  }, [t]);

  const toggleGlobal = (key: "enableInApp" | "enableEmail", value: boolean) => {
    setPreferences((prev) => prev ? { ...prev, [key]: value } : prev);
  };

  const toggleCategory = (category: string, checked: boolean) => {
    setPreferences((prev) => {
      if (!prev) return prev;
      return {
        ...prev,
        subscriptions: prev.subscriptions.map((s) =>
          s.category.toLowerCase() === category.toLowerCase()
            ? { ...s, isEnabled: checked }
            : s
        ),
      };
    });
  };

  const save = async () => {
    if (!preferences) return;
    setIsSaving(true);
    try {
      await notificationService.updatePreferences(
        preferences.enableInApp,
        preferences.enableEmail
      );
      await notificationService.updateSettings(
        preferences.subscriptions.map((s) => ({
          category: s.category,
          isEnabled: s.isEnabled,
        }))
      );
      toast.success(t("settingsSaved"));
    } catch (error) {
      handleLocalApiError(error, () => {
        console.error("Failed to save notification preferences", error);
        toast.error(t("failedSaveSettings"));
      });
    } finally {
      setIsSaving(false);
    }
  };

  return {
    preferences,
    isLoading,
    isSaving,
    toggleGlobal,
    toggleCategory,
    save,
  };
}
```

- [ ] **Step 2: Update preferences page**

Add global toggles above the category list:

```tsx
      {/* Global toggles section */}
      <Card className="border border-slate-200 dark:border-slate-800 dark:bg-slate-950/40 backdrop-blur-sm shadow-sm rounded-lg overflow-hidden">
        <CardHeader className="border-b border-slate-100 dark:border-slate-900">
          <CardTitle className="text-lg flex items-center gap-2">
            <Bell className="h-5 w-5 text-blue-600 dark:text-blue-400" />
            {t("globalPreferences")}
          </CardTitle>
          <CardDescription>
            {t("globalPreferencesDesc")}
          </CardDescription>
        </CardHeader>
        <CardContent className="divide-y divide-slate-100 dark:divide-slate-900 p-0">
          <div className="flex items-center justify-between p-4.5 hover:bg-slate-50/50 dark:hover:bg-slate-900/10 transition-colors">
            <div className="flex items-start gap-3">
              <Bell className="h-5 w-5 text-blue-500 mt-0.5" />
              <div>
                <h4 className="font-medium text-sm">{t("enableInApp")}</h4>
                <p className="text-xs text-slate-500">{t("enableInAppDesc")}</p>
              </div>
            </div>
            <Switch
              checked={preferences?.enableInApp ?? true}
              onCheckedChange={(checked) => handleToggleGlobal("enableInApp", checked)}
            />
          </div>
          <div className="flex items-center justify-between p-4.5 hover:bg-slate-50/50 dark:hover:bg-slate-900/10 transition-colors">
            <div className="flex items-start gap-3">
              <Bell className="h-5 w-5 text-emerald-500 mt-0.5" />
              <div>
                <h4 className="font-medium text-sm">{t("enableEmail")}</h4>
                <p className="text-xs text-slate-500">{t("enableEmailDesc")}</p>
              </div>
            </div>
            <Switch
              checked={preferences?.enableEmail ?? true}
              onCheckedChange={(checked) => handleToggleGlobal("enableEmail", checked)}
            />
          </div>
        </CardContent>
      </Card>
```

You'll need to add `Switch` component from shadcn/ui if not already present: `cody-web-app/src/components/ui/switch.tsx`.

- [ ] **Step 3: Commit**

```bash
git add cody-web-app/src/app/[locale]/(dashboard)/settings/notifications/ cody-web-app/src/hooks/useNotificationSettings.ts
git commit -m "feat(n7): add global toggles to notification preferences page"
```

---

### Task 28: Frontend — Localization

**Files:**
- Modify: `cody-web-app/messages/vi.json`
- Modify: `cody-web-app/messages/en.json`

- [ ] **Step 1: Add new localization keys to en.json**

Add to the `Notifications` namespace:

```json
    "allRead": "All notifications read.",
    "hide": "Hide",
    "hideAllRead": "Hide read",
    "searchPlaceholder": "Search notifications...",
    "status": "Status",
    "category": "Category",
    "severity": "Severity",
    "allCategories": "All categories",
    "allSeverities": "All severities",
    "globalPreferences": "Global Preferences",
    "globalPreferencesDesc": "Master switches for notification channels.",
    "enableInApp": "In-App Notifications",
    "enableInAppDesc": "Show notifications within the application.",
    "enableEmail": "Email Notifications",
    "enableEmailDesc": "Send notifications via email.",
    "status": {
      "all": "All",
      "read": "Read",
      "unread": "Unread"
    },
    "severities": {
      "info": "Info",
      "warning": "Warning",
      "error": "Error",
      "critical": "Critical"
    },
    "noAccess": "You do not have permission to view notifications."
```

- [ ] **Step 2: Add same keys to vi.json**

```json
    "allRead": "Tất cả thông báo đã đọc.",
    "hide": "Ẩn",
    "hideAllRead": "Ẩn đã đọc",
    "searchPlaceholder": "Tìm kiếm thông báo...",
    "status": "Trạng thái",
    "category": "Danh mục",
    "severity": "Mức độ",
    "allCategories": "Tất cả danh mục",
    "allSeverities": "Tất cả mức độ",
    "globalPreferences": "Tùy chọn chung",
    "globalPreferencesDesc": "Công tắc tổng cho các kênh thông báo.",
    "enableInApp": "Thông báo trong ứng dụng",
    "enableInAppDesc": "Hiển thị thông báo trong ứng dụng.",
    "enableEmail": "Thông báo qua email",
    "enableEmailDesc": "Gửi thông báo qua email.",
    "status": {
      "all": "Tất cả",
      "read": "Đã đọc",
      "unread": "Chưa đọc"
    },
    "severities": {
      "info": "Thông tin",
      "warning": "Cảnh báo",
      "error": "Lỗi",
      "critical": "Nghiêm trọng"
    },
    "noAccess": "Bạn không có quyền xem thông báo."
```

- [ ] **Step 3: Commit**

```bash
git add cody-web-app/messages/
git commit -m "feat(n7): add notification center localization keys"
```

---

### Task 29: Frontend — Notification Bell / Context Integration

- [ ] **Step 1: Verify sidebar has Notifications link**

Check `cody-web-app/src/components/shared/Sidebar.tsx` for a notifications link. If not present, add one pointing to `/notifications` with the `Bell` icon.

- [ ] **Step 2: Commit**

```bash
git add cody-web-app/src/components/shared/Sidebar.tsx
git commit -m "feat(n7): add notifications link to sidebar"
```

---

### Task 30: Final Build & Test

- [ ] **Step 1: Backend build**

```bash
cd /Users/dinhtona/SourceCode/Anemoi_Open && dotnet build Anemoi.sln
```

Fix any compilation errors.

- [ ] **Step 2: Frontend build**

```bash
cd /Users/dinhtona/SourceCode/Anemoi_Open/cody-web-app && npx tsc --noEmit
```

Fix any TypeScript errors.

- [ ] **Step 3: Run backend tests**

```bash
cd /Users/dinhtona/SourceCode/Anemoi_Open && dotnet test Anemoi.sln
```

- [ ] **Step 4: Final commit**

```bash
git add -A && git commit -m "Phase N7 - Notification Center & User Preferences"
```
