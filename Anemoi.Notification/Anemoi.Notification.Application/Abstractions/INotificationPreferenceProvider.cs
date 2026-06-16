using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Notification.Application.Abstractions;

/// <summary>
/// Provides notification preference data for the notification creation pipeline.
/// N7 implementation uses direct DB query. Future: add caching layer.
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
