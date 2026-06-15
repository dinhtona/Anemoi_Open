using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Contract.Hr.Events;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Constants;
using Anemoi.Contract.Notification.Events;
using Anemoi.Notification.Application.Consumers;
using Anemoi.Notification.Application.Services;
using MassTransit;
using MediatR;
using NSubstitute;
using Serilog;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class PayslipNotificationTests
{
    private readonly INotificationRecipientResolver _recipientResolver = Substitute.For<INotificationRecipientResolver>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly ILogger _logger = new LoggerConfiguration().CreateLogger();

    [Fact]
    public async Task PayslipPublishedConsumer_ShouldNotifyEmployeeAndEmitDataChange()
    {
        // Arrange
        var payslipId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var employeeUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns(employeeUserId);

        var consumer = new PayslipPublishedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<PayslipPublishedIntegrationEvent>>();
        context.Message.Returns(new PayslipPublishedIntegrationEvent(payslipId, employeeId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        // Verify DataChangeOccurred transient invalidation event is emitted with High sensitivity
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.payslip" &&
            e.Action == NotificationConstants.DataChangeActions.Update &&
            e.EntityId == payslipId &&
            e.Sensitivity == NotificationConstants.DataSensitivity.High &&
            e.QueryTags.Contains("hr") &&
            e.QueryTags.Contains("payroll") &&
            e.QueryTags.Contains("payslip")
        ), Arg.Any<CancellationToken>());

        // Verify notification is created for correct employee user
        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == employeeUserId &&
            c.TitleLocalizationKey == "notification.payslip.published.title" &&
            c.Category == NotificationConstants.Categories.Payroll &&
            c.ActionUrl == "/ess/payslips" &&
            c.DeduplicationKey == $"payslip:{payslipId}:published:{employeeUserId}"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PayslipCancelledConsumer_ShouldNotifyEmployeeAndEmitDataChange()
    {
        // Arrange
        var payslipId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var employeeUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns(employeeUserId);

        var consumer = new PayslipCancelledConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<PayslipCancelledIntegrationEvent>>();
        context.Message.Returns(new PayslipCancelledIntegrationEvent(payslipId, employeeId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        // Verify DataChangeOccurred is emitted with High sensitivity
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.payslip" &&
            e.Action == NotificationConstants.DataChangeActions.Update &&
            e.EntityId == payslipId &&
            e.Sensitivity == NotificationConstants.DataSensitivity.High
        ), Arg.Any<CancellationToken>());

        // Verify notification is created for correct employee user
        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == employeeUserId &&
            c.TitleLocalizationKey == "notification.payslip.cancelled.title" &&
            c.Category == NotificationConstants.Categories.Payroll &&
            c.ActionUrl == "/ess/payslips" &&
            c.DeduplicationKey == $"payslip:{payslipId}:cancelled:{employeeUserId}"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consumers_ShouldSkipNotification_WhenMappingIsMissing()
    {
        // Arrange
        var payslipId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns((string)null);

        var consumer = new PayslipPublishedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<PayslipPublishedIntegrationEvent>>();
        context.Message.Returns(new PayslipPublishedIntegrationEvent(payslipId, employeeId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        // Data change event must still be emitted
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.payslip"
        ), Arg.Any<CancellationToken>());

        // BUT mediator.Send command to create notification should NOT be called
        await _mediator.DidNotReceive().Send(Arg.Any<CreateNotificationCommand>(), Arg.Any<CancellationToken>());
    }
}
