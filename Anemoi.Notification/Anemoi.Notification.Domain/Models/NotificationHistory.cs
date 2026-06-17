using System;
using System.Collections.Generic;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Contract.Notification.Constants;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationHistory : Entity<NotificationHistoryId>
{
    public UserId UserId { get; init; }
    public Guid? WorkspaceId { get; init; }
    public string Title { get; init; }
    public string Content { get; init; }
    public string Category { get; init; }
    public bool IsRead { get; private set; }
    public DateTime CreatedTime { get; init; }
    public DateTime? ReadTime { get; private set; }
    public string TitleLocalizationKey { get; init; }
    public List<string>? TitleLocalizationArgs { get; init; }
    public string ContentLocalizationKey { get; init; }
    public List<string>? ContentLocalizationArgs { get; init; }
    public string ActionUrl { get; init; }
    public string ActionType { get; init; }
    public string DeduplicationKey { get; init; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Type { get; init; } = NotificationConstants.Types.Business;
    public string Severity { get; init; } = NotificationConstants.Severities.Info;
    public bool IsHidden { get; private set; }
    public DateTime? HiddenAt { get; private set; }

    // Part 2: Workflow metadata
    public string AggregateType { get; init; }
    public string AggregateId { get; init; }
    public string WorkflowType { get; init; }
    public string WorkflowState { get; init; }

    // Part 5: Archive support
    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public Guid? ArchivedBy { get; private set; }

    // Part 1: Notification actions
    public ICollection<NotificationAction> Actions { get; init; } = new List<NotificationAction>();

    public void MarkAsRead()
    {
        IsRead = true;
        ReadTime = DateTime.UtcNow;
    }

    public void Hide()
    {
        IsHidden = true;
        HiddenAt = DateTime.UtcNow;
    }

    public void Archive(Guid archivedBy)
    {
        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
        ArchivedBy = archivedBy;
    }

    public void Unarchive()
    {
        IsArchived = false;
        ArchivedAt = null;
        ArchivedBy = null;
    }
}
