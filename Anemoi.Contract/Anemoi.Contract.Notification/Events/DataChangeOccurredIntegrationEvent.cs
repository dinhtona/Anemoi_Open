using System;
using System.Collections.Generic;

namespace Anemoi.Contract.Notification.Events;

public sealed record DataChangeOccurredIntegrationEvent
{
    public string Resource { get; init; }
    public string Action { get; init; }
    public string EntityId { get; init; }
    public string? WorkspaceId { get; init; }
    public string Sensitivity { get; init; }
    public IReadOnlyList<string> QueryTags { get; init; } = Array.Empty<string>();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
