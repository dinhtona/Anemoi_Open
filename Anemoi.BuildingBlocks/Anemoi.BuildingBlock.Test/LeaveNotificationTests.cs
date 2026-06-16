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

public sealed class LeaveNotificationTests
{
    private readonly INotificationRecipientResolver _recipientResolver = Substitute.For<INotificationRecipientResolver>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly ILogger _logger = new LoggerConfiguration().CreateLogger();

    [Fact]
    public async Task LeaveRequestSubmittedConsumer_ShouldNotifyApproverAndEmitDataChange()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var leavePolicyId = Guid.NewGuid().ToString();
        var approverEmployeeId = Guid.NewGuid().ToString();
        var approverUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveApproverUserIdByEmployeeId(approverEmployeeId, Arg.Any<CancellationToken>())
            .Returns(approverUserId);

        var consumer = new LeaveRequestSubmittedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<LeaveRequestSubmittedIntegrationEvent>>();
        context.Message.Returns(new LeaveRequestSubmittedIntegrationEvent(leaveRequestId, employeeId, leavePolicyId, approverEmployeeId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        // Verify DataChangeOccurred transient invalidation event is emitted with NO reason/details
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.leave.request" &&
            e.Action == NotificationConstants.DataChangeActions.Create &&
            e.EntityId == leaveRequestId &&
            e.Sensitivity == NotificationConstants.DataSensitivity.Low &&
            e.QueryTags.Contains("hr") &&
            e.QueryTags.Contains("leave") &&
            e.QueryTags.Contains("leave-request")
        ), Arg.Any<CancellationToken>());

        // Verify notification is created for correct approver user with specific deduplication key and action url
        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == approverUserId &&
            c.TitleLocalizationKey == "notification.leave.submitted.title" &&
            c.ContentLocalizationKey == "notification.leave.submitted.content" &&
            c.Category == NotificationConstants.Categories.Leave &&
            c.ActionUrl == "/hr/leave" &&
            c.DeduplicationKey == $"leave:{leaveRequestId}:submitted:{approverUserId}"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LeaveRequestApprovedConsumer_ShouldNotifyEmployeeAndEmitDataChange()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var leavePolicyId = Guid.NewGuid().ToString();
        var employeeUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns(employeeUserId);

        var consumer = new LeaveRequestApprovedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<LeaveRequestApprovedIntegrationEvent>>();
        context.Message.Returns(new LeaveRequestApprovedIntegrationEvent(leaveRequestId, employeeId, leavePolicyId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.leave.request" &&
            e.Action == NotificationConstants.DataChangeActions.Update &&
            e.EntityId == leaveRequestId &&
            e.QueryTags.Contains("hr")
        ), Arg.Any<CancellationToken>());

        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == employeeUserId &&
            c.TitleLocalizationKey == "notification.leave.approved.title" &&
            c.Category == NotificationConstants.Categories.Leave &&
            c.ActionUrl == "/ess/leave" &&
            c.DeduplicationKey == $"leave:{leaveRequestId}:approved:{employeeUserId}"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LeaveRequestRejectedConsumer_ShouldNotifyEmployeeAndEmitDataChange()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var leavePolicyId = Guid.NewGuid().ToString();
        var employeeUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns(employeeUserId);

        var consumer = new LeaveRequestRejectedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<LeaveRequestRejectedIntegrationEvent>>();
        context.Message.Returns(new LeaveRequestRejectedIntegrationEvent(leaveRequestId, employeeId, leavePolicyId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.leave.request" &&
            e.Action == NotificationConstants.DataChangeActions.Update &&
            e.EntityId == leaveRequestId
        ), Arg.Any<CancellationToken>());

        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == employeeUserId &&
            c.TitleLocalizationKey == "notification.leave.rejected.title" &&
            c.Category == NotificationConstants.Categories.Leave &&
            c.ActionUrl == "/ess/leave" &&
            c.DeduplicationKey == $"leave:{leaveRequestId}:rejected:{employeeUserId}"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LeaveRequestCancelledConsumer_ShouldNotifyEmployeeAndEmitDataChange()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var leavePolicyId = Guid.NewGuid().ToString();
        var employeeUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns(employeeUserId);

        var consumer = new LeaveRequestCancelledConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<LeaveRequestCancelledIntegrationEvent>>();
        context.Message.Returns(new LeaveRequestCancelledIntegrationEvent(leaveRequestId, employeeId, leavePolicyId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.leave.request" &&
            e.Action == NotificationConstants.DataChangeActions.Update &&
            e.EntityId == leaveRequestId
        ), Arg.Any<CancellationToken>());

        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == employeeUserId &&
            c.TitleLocalizationKey == "notification.leave.cancelled.title" &&
            c.Category == NotificationConstants.Categories.Leave &&
            c.ActionUrl == "/ess/leave" &&
            c.DeduplicationKey == $"leave:{leaveRequestId}:cancelled:{employeeUserId}"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consumers_ShouldSkipNotification_WhenMappingIsMissing()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var leavePolicyId = Guid.NewGuid().ToString();

        // ResolveUserId returns null
        _recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns((string)null);

        var consumer = new LeaveRequestApprovedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<LeaveRequestApprovedIntegrationEvent>>();
        context.Message.Returns(new LeaveRequestApprovedIntegrationEvent(leaveRequestId, employeeId, leavePolicyId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        // Data change event must still be emitted
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.leave.request"
        ), Arg.Any<CancellationToken>());

        // BUT mediator.Send command to create notification should NOT be called
        await _mediator.DidNotReceive().Send(Arg.Any<CreateNotificationCommand>(), Arg.Any<CancellationToken>());
    }
}
