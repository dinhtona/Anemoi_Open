using System;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationAction : Entity<NotificationActionId>
{
    public NotificationHistoryId NotificationId { get; init; }
    public string ActionCode { get; init; }
    public string ActionLabel { get; init; }
    public string ActionUrl { get; init; }
    public string ActionType { get; init; }
    public bool RequiresConfirmation { get; init; }
    public int SortOrder { get; init; }
    public DateTime CreatedAt { get; init; }

    public NotificationHistory Notification { get; set; }
}
