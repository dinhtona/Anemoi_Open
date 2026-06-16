using System;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationPreference : Entity<NotificationPreferenceId>
{
    public UserId UserId { get; set; }
    public bool EnableInApp { get; set; } = true;
    public bool EnableEmail { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
