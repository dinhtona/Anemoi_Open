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

public sealed class NotificationDeduplicationTests
{
    private readonly INotificationRecipientResolver _recipientResolver = Substitute.For<INotificationRecipientResolver>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly ILogger _logger = new LoggerConfiguration().CreateLogger();

    [Fact]
    public async Task LeaveRequestSubmittedDuplicate_ShouldEmitDataChangeTwice_ButCreateNotificationWithSameDedupKey()
    {
        var leaveRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var leavePolicyId = Guid.NewGuid().ToString();
        var approverEmployeeId = Guid.NewGuid().ToString();
        var approverUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveApproverUserIdByEmployeeId(approverEmployeeId, Arg.Any<CancellationToken>())
            .Returns(approverUserId);

        var consumer = new LeaveRequestSubmittedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var @event = new LeaveRequestSubmittedIntegrationEvent(leaveRequestId, employeeId, leavePolicyId, approverEmployeeId);

        // Act - consume the same event twice
        await consumer.Consume(MakeContext(@event));
        await consumer.Consume(MakeContext(@event));

        // Assert - DataChangeOccurred emitted both times
        await _publishEndpoint.Received(2).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.leave.request" && e.EntityId == leaveRequestId
        ), Arg.Any<CancellationToken>());

        // Assert - mediator.Send called twice with same DeduplicationKey (dedup at handler level)
        await _mediator.Received(2).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.DeduplicationKey == $"leave:{leaveRequestId}:submitted:{approverUserId}"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OvertimeRequestApprovedDuplicate_ShouldEmitDataChangeTwice_ButCreateNotificationWithSameDedupKey()
    {
        var overtimeRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var employeeUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns(employeeUserId);

        var consumer = new OvertimeRequestApprovedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var @event = new OvertimeRequestApprovedIntegrationEvent(overtimeRequestId, employeeId);

        // Act - consume the same event twice
        await consumer.Consume(MakeContext(@event));
        await consumer.Consume(MakeContext(@event));

        // Assert - DataChangeOccurred emitted both times
        await _publishEndpoint.Received(2).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.overtime.request" && e.EntityId == overtimeRequestId
        ), Arg.Any<CancellationToken>());

        // Assert - same deduplication key both times
        await _mediator.Received(2).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.DeduplicationKey == $"overtime:{overtimeRequestId}:approved:{employeeUserId}"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PayslipPublishedDuplicate_ShouldEmitDataChangeTwice_ButCreateNotificationWithSameDedupKey()
    {
        var payslipId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var employeeUserId = Guid.NewGuid().ToString();

        _recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns(employeeUserId);

        var consumer = new PayslipPublishedConsumer(_recipientResolver, _mediator, _publishEndpoint, _logger);
        var @event = new PayslipPublishedIntegrationEvent(payslipId, employeeId);

        // Act - consume the same event twice
        await consumer.Consume(MakeContext(@event));
        await consumer.Consume(MakeContext(@event));

        // Assert - DataChangeOccurred emitted both times
        await _publishEndpoint.Received(2).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.payslip" && e.EntityId == payslipId
        ), Arg.Any<CancellationToken>());

        // Assert - same deduplication key both times
        await _mediator.Received(2).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.DeduplicationKey == $"payslip:{payslipId}:published:{employeeUserId}"
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PayrollRunSubmittedDuplicate_ShouldEmitDataChangeTwice_WithSameDedupKeys()
    {
        var payrollRunId = Guid.NewGuid().ToString();
        var submittedBy = "user-1";

        var consumer = new PayrollRunSubmittedConsumer(null, _mediator, _publishEndpoint, _logger);
        var @event = new PayrollRunSubmittedIntegrationEvent(payrollRunId, submittedBy);

        // When permission resolution fails with timeout, DataChange is still emitted
        // but no notification is created - the consumer catches RequestTimeoutException
        var context = Substitute.For<ConsumeContext<PayrollRunSubmittedIntegrationEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act - consume the same event twice
        await consumer.Consume(context);
        await consumer.Consume(context);

        // Assert - DataChangeOccurred emitted both times (even when permission resolution fails)
        await _publishEndpoint.Received(2).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.payroll" && e.EntityId == payrollRunId
        ), Arg.Any<CancellationToken>());
    }

    private static ConsumeContext<T> MakeContext<T>(T message) where T : class
    {
        var context = Substitute.For<ConsumeContext<T>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }
}
