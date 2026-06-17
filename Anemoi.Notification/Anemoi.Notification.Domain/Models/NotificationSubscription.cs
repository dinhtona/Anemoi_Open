using System;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationSubscription : Entity<NotificationSubscriptionId>
{
    public UserId UserId { get; init; }
    public string Category { get; init; }
    public bool IsEnabled { get; private set; }
    public DateTime UpdatedTime { get; private set; }

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
        UpdatedTime = DateTime.UtcNow;
    }
}

