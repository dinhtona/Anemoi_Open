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
