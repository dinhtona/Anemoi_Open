using System;

namespace Anemoi.Contract.Identity.Events;

public sealed record RoleGroupPermissionChangedIntegrationEvent
{
    public string RoleGroupCode { get; init; }
    public DateTime ChangedAt { get; init; }
}
