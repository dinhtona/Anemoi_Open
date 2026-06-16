using System;
using System.Collections.Generic;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Contract.Notification.Constants;

namespace Anemoi.Notification.Domain.Models;

public sealed class NotificationHistory : Entity<NotificationHistoryId>
{
    public Guid UserId { get; set; }
    public Guid? WorkspaceId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Category { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime? ReadTime { get; set; }
    public string TitleLocalizationKey { get; set; }
    public List<string>? TitleLocalizationArgs { get; set; }
    public string ContentLocalizationKey { get; set; }
    public List<string>? ContentLocalizationArgs { get; set; }
    public string ActionUrl { get; set; }
    public string ActionType { get; set; }
    public string DeduplicationKey { get; set; }
    public Guid? CorrelationId { get; set; }
    public Guid? CausationId { get; set; }
    public string Type { get; set; } = NotificationConstants.Types.Business;
    public string Severity { get; set; } = NotificationConstants.Severities.Info;
    public bool IsHidden { get; set; }
    public DateTime? HiddenAt { get; set; }

    // Part 2: Workflow metadata
    public string AggregateType { get; set; }
    public string AggregateId { get; set; }
    public string WorkflowType { get; set; }
    public string WorkflowState { get; set; }

    // Part 5: Archive support
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public Guid? ArchivedBy { get; set; }

    // Part 1: Notification actions
    public ICollection<NotificationAction> Actions { get; set; } = new List<NotificationAction>();

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
