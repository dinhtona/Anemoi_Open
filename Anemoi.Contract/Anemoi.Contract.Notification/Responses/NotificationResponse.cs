using System;
using System.Collections.Generic;

namespace Anemoi.Contract.Notification.Responses;

public sealed record NotificationResponse
{
    public string Id { get; init; }
    public string UserId { get; init; }
    public string WorkspaceId { get; init; }
    public string Title { get; init; }
    public string Content { get; init; }
    public string Category { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedTime { get; init; }
    public DateTime? ReadTime { get; init; }
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
    public bool IsHidden { get; init; }
    public bool IsArchived { get; init; }

    // Workflow metadata
    public string AggregateType { get; init; }
    public string AggregateId { get; init; }
    public string WorkflowType { get; init; }
    public string WorkflowState { get; init; }

    // Actions
    public List<NotificationActionResponse> Actions { get; init; } = [];
}
