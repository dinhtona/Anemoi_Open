using System;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationSubscription : Entity<NotificationSubscriptionId>
{
    public Guid UserId { get; set; }
    public string Category { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime UpdatedTime { get; set; }
}

