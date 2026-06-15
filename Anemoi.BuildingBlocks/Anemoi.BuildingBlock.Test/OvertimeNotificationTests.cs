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

public sealed class OvertimeNotificationTests
{
    private readonly INotificationRecipientResolver _recipientResolver = Substitute.For<INotificationRecipientResolver>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly ILogger _logger = new LoggerConfiguration().CreateLogger();

    [Fact]
    public async Task OvertimeRequestCreatedConsumer_WithManager_ShouldNotifyManagerAndEmitDataChange()
    {
        // Arrange
        var overtimeRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var managerEmployeeId = Guid.NewGuid().ToString();
        var managerUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveApproverUserIdByEmployeeId(managerEmployeeId, Arg.Any<CancellationToken>())
            .Returns(managerUserId);

        var consumer = new OvertimeRequestCreatedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<OvertimeRequestCreatedIntegrationEvent>>();
        context.Message.Returns(new OvertimeRequestCreatedIntegrationEvent(overtimeRequestId, employeeId, managerEmployeeId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        // Verify DataChangeOccurred transient invalidation event is emitted with NO reason/details
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.overtime.request" &&
            e.Action == NotificationConstants.DataChangeActions.Create &&
            e.EntityId == overtimeRequestId &&
            e.Sensitivity == NotificationConstants.DataSensitivity.Medium &&
            e.QueryTags.Contains("hr") &&
            e.QueryTags.Contains("overtime") &&
            e.QueryTags.Contains("overtime-request")
        ), Arg.Any<CancellationToken>());

        // Verify notification is created for correct manager user with specific deduplication key and action url
        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == managerUserId &&
            c.TitleLocalizationKey == "notification.overtime.submitted.title" &&
            c.Category == NotificationConstants.Categories.Overtime &&
            c.ActionUrl == "/hr/overtime" &&
            c.DeduplicationKey == $"overtime:{overtimeRequestId}:submitted:{managerUserId}"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OvertimeRequestCreatedConsumer_WithoutManager_ShouldSkipNotificationButEmitDataChange()
    {
        // Arrange
        var overtimeRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();

        var consumer = new OvertimeRequestCreatedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<OvertimeRequestCreatedIntegrationEvent>>();
        context.Message.Returns(new OvertimeRequestCreatedIntegrationEvent(overtimeRequestId, employeeId, null));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        // Verify DataChangeOccurred is still emitted
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.overtime.request" &&
            e.Action == NotificationConstants.DataChangeActions.Create &&
            e.EntityId == overtimeRequestId
        ), Arg.Any<CancellationToken>());

        // Verify NO notification is sent
        await _mediator.DidNotReceive().Send(Arg.Any<CreateNotificationCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OvertimeRequestApprovedConsumer_ShouldNotifyEmployeeAndEmitDataChange()
    {
        // Arrange
        var overtimeRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var employeeUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns(employeeUserId);

        var consumer = new OvertimeRequestApprovedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<OvertimeRequestApprovedIntegrationEvent>>();
        context.Message.Returns(new OvertimeRequestApprovedIntegrationEvent(overtimeRequestId, employeeId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.overtime.request" &&
            e.Action == NotificationConstants.DataChangeActions.Update &&
            e.EntityId == overtimeRequestId &&
            e.Sensitivity == NotificationConstants.DataSensitivity.Medium &&
            e.QueryTags.Contains("hr") &&
            e.QueryTags.Contains("overtime") &&
            e.QueryTags.Contains("overtime-request")
        ), Arg.Any<CancellationToken>());

        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == employeeUserId &&
            c.TitleLocalizationKey == "notification.overtime.approved.title" &&
            c.Category == NotificationConstants.Categories.Overtime &&
            c.ActionUrl == "/ess/overtime" &&
            c.DeduplicationKey == $"overtime:{overtimeRequestId}:approved:{employeeUserId}"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OvertimeRequestRejectedConsumer_ShouldNotifyEmployeeAndEmitDataChange()
    {
        // Arrange
        var overtimeRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var employeeUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns(employeeUserId);

        var consumer = new OvertimeRequestRejectedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<OvertimeRequestRejectedIntegrationEvent>>();
        context.Message.Returns(new OvertimeRequestRejectedIntegrationEvent(overtimeRequestId, employeeId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.overtime.request" &&
            e.Action == NotificationConstants.DataChangeActions.Update &&
            e.EntityId == overtimeRequestId
        ), Arg.Any<CancellationToken>());

        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == employeeUserId &&
            c.TitleLocalizationKey == "notification.overtime.rejected.title" &&
            c.Category == NotificationConstants.Categories.Overtime &&
            c.ActionUrl == "/ess/overtime" &&
            c.DeduplicationKey == $"overtime:{overtimeRequestId}:rejected:{employeeUserId}"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OvertimeRequestCancelledConsumer_ShouldNotifyEmployeeAndEmitDataChange()
    {
        // Arrange
        var overtimeRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var employeeUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns(employeeUserId);

        var consumer = new OvertimeRequestCancelledConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<OvertimeRequestCancelledIntegrationEvent>>();
        context.Message.Returns(new OvertimeRequestCancelledIntegrationEvent(overtimeRequestId, employeeId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.overtime.request" &&
            e.Action == NotificationConstants.DataChangeActions.Update &&
            e.EntityId == overtimeRequestId
        ), Arg.Any<CancellationToken>());

        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == employeeUserId &&
            c.TitleLocalizationKey == "notification.overtime.cancelled.title" &&
            c.Category == NotificationConstants.Categories.Overtime &&
            c.ActionUrl == "/ess/overtime" &&
            c.DeduplicationKey == $"overtime:{overtimeRequestId}:cancelled:{employeeUserId}"
        ), Arg.Any<CancellationToken>());
    }
}
