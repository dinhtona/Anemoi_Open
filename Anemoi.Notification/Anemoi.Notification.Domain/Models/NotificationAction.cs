using System;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationAction : Entity<NotificationActionId>
{
    public NotificationHistoryId NotificationId { get; set; }
    public string ActionCode { get; set; }
    public string ActionLabel { get; set; }
    public string ActionUrl { get; set; }
    public string ActionType { get; set; }
    public bool RequiresConfirmation { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    public NotificationHistory Notification { get; set; }
}
