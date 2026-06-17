using System;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationPreference : Entity<NotificationPreferenceId>
{
    public UserId UserId { get; init; }
    public bool EnableInApp { get; private set; } = true;
    public bool EnableEmail { get; private set; } = true;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; private set; }

    public void Update(bool enableInApp, bool enableEmail)
    {
        EnableInApp = enableInApp;
        EnableEmail = enableEmail;
        UpdatedAt = DateTime.UtcNow;
    }
}
