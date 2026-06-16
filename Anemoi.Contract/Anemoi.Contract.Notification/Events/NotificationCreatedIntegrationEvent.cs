using System;
using System.Collections.Generic;

namespace Anemoi.Contract.Notification.Events;

public sealed record NotificationCreatedIntegrationEvent
{
    public string Id { get; init; }
    public string UserId { get; init; }
    public string WorkspaceId { get; init; }
    public string Title { get; init; }
    public string Content { get; init; }
    public string Category { get; init; }
    public DateTime CreatedTime { get; init; }
    public string TitleLocalizationKey { get; init; }
    public List<string>? TitleLocalizationArgs { get; init; }
    public string ContentLocalizationKey { get; init; }
    public List<string>? ContentLocalizationArgs { get; init; }
    public string ActionUrl { get; init; }
    public string ActionType { get; init; }
    public string DeduplicationKey { get; init; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Type { get; init; }
    public string Severity { get; init; }
}
