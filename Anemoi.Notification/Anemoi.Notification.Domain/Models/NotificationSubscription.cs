using System;
using System.Collections.Generic;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationSubscription : ValueObject
{
    public NotificationSubscriptionId Id { get; set; }
    public Guid UserId { get; set; }
    public string Category { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime UpdatedTime { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield break;
    }
}
