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

public sealed class PayslipPublishedConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<PayslipPublishedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PayslipPublishedIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.payslip",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.PayslipId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.High,
            QueryTags = new List<string> { "hr", "payroll", "payslip" },
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
            TitleLocalizationKey: "notification.payslip.published.title",
            ContentLocalizationKey: null,
            Category: NotificationConstants.Categories.Payroll,
            ActionUrl: "/ess/payslips",
            DeduplicationKey: $"payslip:{message.PayslipId}:published:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed PayslipPublished notification for Employee User ID: {UserId}", userId);
    }
}

public sealed class PayslipCancelledConsumer(
    INotificationRecipientResolver recipientResolver,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<PayslipCancelledIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PayslipCancelledIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.payslip",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.PayslipId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.High,
            QueryTags = new List<string> { "hr", "payroll", "payslip" },
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
            TitleLocalizationKey: "notification.payslip.cancelled.title",
            ContentLocalizationKey: null,
            Category: NotificationConstants.Categories.Payroll,
            ActionUrl: "/ess/payslips",
            DeduplicationKey: $"payslip:{message.PayslipId}:cancelled:{userId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed PayslipCancelled notification for Employee User ID: {UserId}", userId);
    }
}
