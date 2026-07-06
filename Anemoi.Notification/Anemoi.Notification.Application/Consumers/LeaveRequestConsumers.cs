using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Anemoi.Contract.Hr;
using Anemoi.Contract.Hr.Events;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Constants;
using Anemoi.Contract.Notification.Events;
using Anemoi.Notification.Application.Services;
using MassTransit;
using MediatR;
using Serilog;

namespace Anemoi.Notification.Application.Consumers;

public sealed class LeaveRequestSubmittedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<LeaveRequestSubmittedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<LeaveRequestSubmittedIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.leave.request",
            Action = NotificationConstants.DataChangeActions.Create,
            EntityId = message.LeaveRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "leave", "leave-request" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        if (string.IsNullOrEmpty(message.ApproverEmployeeId))
        {
            logger.Warning("Leave request submitted event has empty ApproverEmployeeId. Skipping notification.");
            return;
        }

        var userId = await recipientResolver.ResolveApproverUserIdByEmployeeId(message.ApproverEmployeeId, context.CancellationToken);
        if (string.IsNullOrEmpty(userId))
        {
            logger.Warning("Could not resolve Approver User ID for EmployeeId: {ApproverId}. Skipping notification.", message.ApproverEmployeeId);
            return;
        }

        var command = new CreateNotificationCommand(
            UserId: userId,
            Title: "New Leave Request",
            Content: "A new leave request requires your approval",
            TitleLocalizationKey: "notification.leave.submitted.title",
            ContentLocalizationKey: "notification.leave.submitted.content",
            Category: NotificationConstants.Categories.Leave,
            ActionUrl: "/manager/approvals",
            DeduplicationKey: $"leave:{message.LeaveRequestId}:submitted:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info,
            AggregateType: "LeaveRequest",
            AggregateId: message.LeaveRequestId,
            WorkflowType: "Approval",
            Actions:
            [
                new CreateNotificationActionInput(
                    ActionCode: NotificationWorkflowConstants.ActionCodes.ApproveLeaveRequest,
                    ActionLabel: "Approve",
                    ActionType: "Command",
                    RequiresConfirmation: true,
                    SortOrder: 1),
                new CreateNotificationActionInput(
                    ActionCode: NotificationWorkflowConstants.ActionCodes.RejectLeaveRequest,
                    ActionLabel: "Reject",
                    ActionType: "Command",
                    RequiresConfirmation: true,
                    SortOrder: 2),
                new CreateNotificationActionInput(
                    ActionCode: NotificationWorkflowConstants.ActionCodes.ViewLeaveRequest,
                    ActionLabel: "View",
                    ActionType: "Navigate",
                    ActionUrl: "/manager/approvals",
                    SortOrder: 0)
            ]
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed LeaveRequestSubmitted notification for Approver User ID: {UserId}", userId);
    }
}

public sealed class LeaveRequestApprovedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<LeaveRequestApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<LeaveRequestApprovedIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.leave.request",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.LeaveRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Medium,
            QueryTags = new List<string> { "hr", "leave", "leave-request" },
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
            TitleLocalizationKey: "notification.leave.approved.title",
            ContentLocalizationKey: "notification.leave.approved.content",
            Category: NotificationConstants.Categories.Leave,
            ActionUrl: "/ess/leave",
            DeduplicationKey: $"leave:{message.LeaveRequestId}:approved:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed LeaveRequestApproved notification for Employee User ID: {UserId}", userId);
    }
}

public sealed class LeaveRequestRejectedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<LeaveRequestRejectedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<LeaveRequestRejectedIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.leave.request",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.LeaveRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Medium,
            QueryTags = new List<string> { "hr", "leave", "leave-request" },
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
            TitleLocalizationKey: "notification.leave.rejected.title",
            ContentLocalizationKey: "notification.leave.rejected.content",
            Category: NotificationConstants.Categories.Leave,
            ActionUrl: "/ess/leave",
            DeduplicationKey: $"leave:{message.LeaveRequestId}:rejected:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed LeaveRequestRejected notification for Employee User ID: {UserId}", userId);
    }
}

public sealed class LeaveRequestCancelledConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<LeaveRequestCancelledIntegrationEvent>
{
    public async Task Consume(ConsumeContext<LeaveRequestCancelledIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.leave.request",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.LeaveRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Medium,
            QueryTags = new List<string> { "hr", "leave", "leave-request" },
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
            TitleLocalizationKey: "notification.leave.cancelled.title",
            ContentLocalizationKey: "notification.leave.cancelled.content",
            Category: NotificationConstants.Categories.Leave,
            ActionUrl: "/ess/leave",
            DeduplicationKey: $"leave:{message.LeaveRequestId}:cancelled:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed LeaveRequestCancelled notification for Employee User ID: {UserId}", userId);
    }
}
