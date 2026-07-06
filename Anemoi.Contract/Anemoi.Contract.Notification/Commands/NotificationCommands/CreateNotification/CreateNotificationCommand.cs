using System;
using System.Collections.Generic;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Notification.Responses;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;

public sealed record CreateNotificationCommand(
    string UserId,
    string WorkspaceId = null,
    string Title = null,
    string Content = null,
    string Category = null,
    string TitleLocalizationKey = null,
    IReadOnlyList<string>? TitleLocalizationArgs = null,
    string ContentLocalizationKey = null,
    IReadOnlyList<string>? ContentLocalizationArgs = null,
    string ActionUrl = null,
    string ActionType = null,
    string DeduplicationKey = null,
    Guid? CorrelationId = null,
    Guid? CausationId = null,
    string Type = null,
    string Severity = null,
    IReadOnlyList<CreateNotificationActionInput>? Actions = null,
    string AggregateType = null,
    string AggregateId = null,
    string WorkflowType = null,
    string WorkflowState = null) : ICommandResult<NotificationResponse>;
