using System;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationActionAudit : Entity<NotificationActionAuditId>
{
    public NotificationHistoryId NotificationId { get; init; }
    public NotificationActionId ActionId { get; init; }
    public UserId ExecutedBy { get; init; }
    public DateTime ExecutedAt { get; init; }
    public bool Success { get; init; }
    public string Result { get; init; }
    public string ClientIp { get; init; }
    public string UserAgent { get; init; }
}
