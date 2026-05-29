using System;
using System.Collections.Generic;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationHistory : ValueObject
{
    public NotificationHistoryId Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Category { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime? ReadTime { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield break;
    }
}
