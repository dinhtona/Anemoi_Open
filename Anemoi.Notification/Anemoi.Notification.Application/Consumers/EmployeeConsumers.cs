using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Anemoi.Contract.Hr.Events;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Constants;
using Anemoi.Contract.Notification.Events;
using MassTransit;
using MediatR;
using Serilog;

namespace Anemoi.Notification.Application.Consumers;

public sealed class EmployeeCreatedConsumer(
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<EmployeeCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EmployeeCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.employee",
            Action = NotificationConstants.DataChangeActions.Create,
            EntityId = message.EmployeeId.ToString(),
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "employee" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var command = new CreateNotificationCommand(
            UserId: message.EmployeeId.ToString(),
            Title: "Employee Created",
            Content: $"Employee {message.FullName} ({message.EmployeeCode}) has been created",
            Category: "HR",
            ActionUrl: $"/hr/employees/{message.EmployeeId}",
            DeduplicationKey: $"employee:{message.EmployeeId}:created",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed EmployeeCreated notification for Employee ID: {EmployeeId}", message.EmployeeId);
    }
}

public sealed class TransferApprovedConsumer(
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<TransferApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<TransferApprovedIntegrationEvent> context)
    {
        var message = context.Message;

        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.employee.transfer",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.TransferId.ToString(),
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Medium,
            QueryTags = new List<string> { "hr", "employee", "transfer" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var command = new CreateNotificationCommand(
            UserId: message.EmployeeId.ToString(),
            Title: "Transfer Approved",
            Content: "Your transfer has been approved",
            Category: "HR",
            ActionUrl: $"/hr/employees/transfers/{message.TransferId}",
            DeduplicationKey: $"transfer:{message.TransferId}:approved:{message.EmployeeId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed TransferApproved notification for Employee ID: {EmployeeId}", message.EmployeeId);
    }
}

public sealed class SeparationApprovedConsumer(
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<SeparationApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SeparationApprovedIntegrationEvent> context)
    {
        var message = context.Message;

        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.employee.separation",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.SeparationId.ToString(),
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Medium,
            QueryTags = new List<string> { "hr", "employee", "separation" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var command = new CreateNotificationCommand(
            UserId: message.EmployeeId.ToString(),
            Title: "Separation Approved",
            Content: "Your separation has been approved",
            Category: "HR",
            ActionUrl: $"/hr/employees/separations/{message.SeparationId}",
            DeduplicationKey: $"separation:{message.SeparationId}:approved:{message.EmployeeId}",
            Type: NotificationConstants.Types.Business,
            Severity: NotificationConstants.Severities.Info
        );

        await mediator.Send(command, context.CancellationToken);
        logger.Information("Processed SeparationApproved notification for Employee ID: {EmployeeId}", message.EmployeeId);
    }
}
