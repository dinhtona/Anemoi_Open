using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Anemoi.Contract.Hr.Events;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Constants;
using Anemoi.Contract.Notification.Events;
using Anemoi.Notification.Application.Services;
using MassTransit;
using MediatR;
using Serilog;

namespace Anemoi.Notification.Application.Consumers;

public sealed class OvertimeRequestCreatedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<OvertimeRequestCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OvertimeRequestCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.overtime.request",
            Action = NotificationConstants.DataChangeActions.Create,
            EntityId = message.OvertimeRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "overtime", "overtime-request" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        if (string.IsNullOrEmpty(message.ManagerEmployeeId))
        {
            logger.Warning("Overtime request created event has empty ManagerEmployeeId. Skipping business notification but data-change event was emitted.");
            return;
        }

        var userId = await recipientResolver.ResolveApproverUserIdByEmployeeId(message.ManagerEmployeeId, context.CancellationToken);
        if (string.IsNullOrEmpty(userId))
        {
            logger.Warning("Could not resolve Approver User ID for EmployeeId: {ApproverId}. Skipping business notification.", message.ManagerEmployeeId);
            return;
        }

        var command = new CreateNotificationCommand(
            UserId: userId,
            TitleLocalizationKey: "notification.overtime.submitted.title",
            ContentLocalizationKey: "notification.overtime.submitted.content",
            Category: NotificationConstants.Categories.Overtime,
            ActionUrl: "/manager/approvals",
            DeduplicationKey: $"overtime:{message.OvertimeRequestId}:submitted:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed OvertimeRequestCreated notification for Approver User ID: {UserId}", userId);
    }
}

public sealed class OvertimeRequestApprovedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<OvertimeRequestApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OvertimeRequestApprovedIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.overtime.request",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.OvertimeRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "overtime", "overtime-request" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var userId = await recipientResolver.ResolveUserIdByEmployeeId(message.EmployeeId, context.CancellationToken);
        if (string.IsNullOrEmpty(userId))
        {
            logger.Warning("Could not resolve Employee User ID for EmployeeId: {EmployeeId}. Skipping notification.", message.EmployeeId);
            return;
        }

        var command = new CreateNotificationCommand(
            UserId: userId,
            TitleLocalizationKey: "notification.overtime.approved.title",
            ContentLocalizationKey: "notification.overtime.approved.content",
            Category: NotificationConstants.Categories.Overtime,
            ActionUrl: "/ess/overtime",
            DeduplicationKey: $"overtime:{message.OvertimeRequestId}:approved:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed OvertimeRequestApproved notification for Employee User ID: {UserId}", userId);
    }
}

public sealed class OvertimeRequestRejectedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<OvertimeRequestRejectedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OvertimeRequestRejectedIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.overtime.request",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.OvertimeRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "overtime", "overtime-request" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var userId = await recipientResolver.ResolveUserIdByEmployeeId(message.EmployeeId, context.CancellationToken);
        if (string.IsNullOrEmpty(userId))
        {
            logger.Warning("Could not resolve Employee User ID for EmployeeId: {EmployeeId}. Skipping notification.", message.EmployeeId);
            return;
        }

        var command = new CreateNotificationCommand(
            UserId: userId,
            TitleLocalizationKey: "notification.overtime.rejected.title",
            ContentLocalizationKey: "notification.overtime.rejected.content",
            Category: NotificationConstants.Categories.Overtime,
            ActionUrl: "/ess/overtime",
            DeduplicationKey: $"overtime:{message.OvertimeRequestId}:rejected:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed OvertimeRequestRejected notification for Employee User ID: {UserId}", userId);
    }
}

public sealed class OvertimeRequestCancelledConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<OvertimeRequestCancelledIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OvertimeRequestCancelledIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.overtime.request",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.OvertimeRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "overtime", "overtime-request" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var userId = await recipientResolver.ResolveUserIdByEmployeeId(message.EmployeeId, context.CancellationToken);
        if (string.IsNullOrEmpty(userId))
        {
            logger.Warning("Could not resolve Employee User ID for EmployeeId: {EmployeeId}. Skipping notification.", message.EmployeeId);
            return;
        }

        var command = new CreateNotificationCommand(
            UserId: userId,
            TitleLocalizationKey: "notification.overtime.cancelled.title",
            ContentLocalizationKey: "notification.overtime.cancelled.content",
            Category: NotificationConstants.Categories.Overtime,
            ActionUrl: "/ess/overtime",
            DeduplicationKey: $"overtime:{message.OvertimeRequestId}:cancelled:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed OvertimeRequestCancelled notification for Employee User ID: {UserId}", userId);
    }
}
