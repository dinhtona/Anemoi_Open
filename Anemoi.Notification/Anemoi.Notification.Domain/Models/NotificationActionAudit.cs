using System;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationActionAudit : Entity<NotificationActionAuditId>
{
    public NotificationHistoryId NotificationId { get; set; }
    public NotificationActionId ActionId { get; set; }
    public Guid ExecutedBy { get; set; }
    public DateTime ExecutedAt { get; set; }
    public bool Success { get; set; }
    public string Result { get; set; }
    public string ClientIp { get; set; }
    public string UserAgent { get; set; }
}
