using System;

namespace Anemoi.Contract.Identity.Events;

public sealed record UserTokenRevokedIntegrationEvent
{
    public Guid UserId { get; init; }
    public DateTime RevokedAt { get; init; }
}
