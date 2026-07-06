using System;

namespace Anemoi.Contract.Identity.Events;

public sealed record UserPermissionsChangedIntegrationEvent
{
    public Guid UserId { get; init; }
    public DateTime ChangedAt { get; init; }
}
