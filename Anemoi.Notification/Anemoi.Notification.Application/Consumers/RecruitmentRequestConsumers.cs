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

public sealed class RecruitmentRequestSubmittedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<RecruitmentRequestSubmittedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<RecruitmentRequestSubmittedIntegrationEvent> context)
    {
        var message = context.Message;

        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.recruitment.request",
            Action = NotificationConstants.DataChangeActions.Create,
            EntityId = message.RecruitmentRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "recruitment", "recruitment-request" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        if (string.IsNullOrEmpty(message.ApproverUserId))
        {
            logger.Warning("Recruitment request submitted event has empty ApproverUserId. Skipping notification.");
            return;
        }

        var userId = await recipientResolver.ResolveApproverUserIdByEmployeeId(message.ApproverUserId, context.CancellationToken);
        if (string.IsNullOrEmpty(userId))
        {
            logger.Warning("Could not resolve Approver User ID for EmployeeId: {ApproverId}. Skipping notification.", message.ApproverUserId);
            return;
        }

        var command = new CreateNotificationCommand(
            UserId: userId,
            TitleLocalizationKey: "notification.recruitment.request.submitted.title",
            ContentLocalizationKey: "notification.recruitment.request.submitted.content",
            Category: NotificationConstants.Categories.Recruitment,
            ActionUrl: $"/hr/recruitment/requests/{message.RecruitmentRequestId}",
            ActionType: NotificationConstants.ActionTypes.Navigate,
            DeduplicationKey: $"recruitment-request:{message.RecruitmentRequestId}:submitted:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed RecruitmentRequestSubmitted notification for Approver User ID: {UserId}", userId);
    }
}

public sealed class RecruitmentRequestApprovedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<RecruitmentRequestApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<RecruitmentRequestApprovedIntegrationEvent> context)
    {
        var message = context.Message;

        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.recruitment.request",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.RecruitmentRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "recruitment", "recruitment-request" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var userId = await recipientResolver.ResolveUserIdByEmployeeId(message.ApprovedBy, context.CancellationToken);
        if (string.IsNullOrEmpty(userId))
        {
            logger.Warning("Could not resolve Requester User ID for ApprovedBy: {ApprovedBy}. Skipping notification.", message.ApprovedBy);
            return;
        }

        var command = new CreateNotificationCommand(
            UserId: userId,
            TitleLocalizationKey: "notification.recruitment.request.approved.title",
            ContentLocalizationKey: "notification.recruitment.request.approved.content",
            Category: NotificationConstants.Categories.Recruitment,
            ActionUrl: $"/hr/recruitment/requests/{message.RecruitmentRequestId}",
            ActionType: NotificationConstants.ActionTypes.Navigate,
            DeduplicationKey: $"recruitment-request:{message.RecruitmentRequestId}:approved:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed RecruitmentRequestApproved notification for User ID: {UserId}", userId);
    }
}

public sealed class RecruitmentRequestRejectedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<RecruitmentRequestRejectedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<RecruitmentRequestRejectedIntegrationEvent> context)
    {
        var message = context.Message;

        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.recruitment.request",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.RecruitmentRequestId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "recruitment", "recruitment-request" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var userId = await recipientResolver.ResolveUserIdByEmployeeId(message.RejectedBy, context.CancellationToken);
        if (string.IsNullOrEmpty(userId))
        {
            logger.Warning("Could not resolve Requester User ID for RejectedBy: {RejectedBy}. Skipping notification.", message.RejectedBy);
            return;
        }

        var command = new CreateNotificationCommand(
            UserId: userId,
            TitleLocalizationKey: "notification.recruitment.request.rejected.title",
            ContentLocalizationKey: "notification.recruitment.request.rejected.content",
            Category: NotificationConstants.Categories.Recruitment,
            ActionUrl: $"/hr/recruitment/requests/{message.RecruitmentRequestId}",
            ActionType: NotificationConstants.ActionTypes.Navigate,
            DeduplicationKey: $"recruitment-request:{message.RecruitmentRequestId}:rejected:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed RecruitmentRequestRejected notification for User ID: {UserId}", userId);
    }
}
