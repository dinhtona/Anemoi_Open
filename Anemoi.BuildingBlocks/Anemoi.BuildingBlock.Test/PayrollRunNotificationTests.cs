using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.Contract.Hr.Events;
using Anemoi.Contract.Identity.Queries.UserQueries.ResolveUsersByPermission;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Constants;
using Anemoi.Contract.Notification.Events;
using Anemoi.Notification.Application.Consumers;
using MassTransit;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Serilog;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class PayrollRunNotificationTests
{
    private readonly IRequestClient<ResolveUsersByPermissionQuery> _permissionClient = Substitute.For<IRequestClient<ResolveUsersByPermissionQuery>>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly ILogger _logger = new LoggerConfiguration().CreateLogger();

    [Fact]
    public async Task PayrollRunSubmittedConsumer_ShouldNotifyApprovers_ExcludingSubmitter_AndDeduplicate()
    {
        // Arrange
        var payrollRunId = Guid.NewGuid().ToString();
        var submitterId = "user-submitter";
        var approver1 = "user-approver-1";
        var approver2 = "user-approver-2";

        var response = Substitute.For<Response<ResolveUsersByPermissionResponse>>();
        response.Message.Returns(new ResolveUsersByPermissionResponse(new List<PermissionUser>
        {
            new(approver1, "approver1@example.com"),
            new(approver2, "approver2@example.com"),
            new(submitterId, "submitter@example.com"), // Exclude submitter
            new(approver1, "approver1@example.com") // Deduplicate approver1
        }));

        _permissionClient.GetResponse<ResolveUsersByPermissionResponse>(
            Arg.Is<ResolveUsersByPermissionQuery>(q => q.Permission == Permissions.HrPayrollApprove),
            Arg.Any<CancellationToken>()
        ).Returns(response);

        var consumer = new PayrollRunSubmittedConsumer(_permissionClient, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<PayrollRunSubmittedIntegrationEvent>>();
        context.Message.Returns(new PayrollRunSubmittedIntegrationEvent(payrollRunId, submitterId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        // Verify DataChangeOccurred transient invalidation event is emitted with no salary details
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.payroll" &&
            e.Action == NotificationConstants.DataChangeActions.Update &&
            e.EntityId == payrollRunId &&
            e.Sensitivity == NotificationConstants.DataSensitivity.Low &&
            e.QueryTags.Contains("hr") &&
            e.QueryTags.Contains("payroll")
        ), Arg.Any<CancellationToken>());

        // Verify notifications sent to resolved approvers excluding submitter, deduplicated
        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == approver1 &&
            c.TitleLocalizationKey == "notification.payroll.submitted.title" &&
            c.Category == NotificationConstants.Categories.Payroll &&
            c.ActionUrl == "/hr/payroll" &&
            c.DeduplicationKey == $"payroll:{payrollRunId}:submitted:{approver1}"
        ), Arg.Any<CancellationToken>());

        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == approver2 &&
            c.TitleLocalizationKey == "notification.payroll.submitted.title" &&
            c.Category == NotificationConstants.Categories.Payroll &&
            c.ActionUrl == "/hr/payroll" &&
            c.DeduplicationKey == $"payroll:{payrollRunId}:submitted:{approver2}"
        ), Arg.Any<CancellationToken>());

        await _mediator.DidNotReceive().Send(Arg.Is<CreateNotificationCommand>(c => c.UserId == submitterId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PayrollRunApprovedConsumer_ShouldNotifyViewers_ExcludingApprover()
    {
        // Arrange
        var payrollRunId = Guid.NewGuid().ToString();
        var approverId = "user-approver";
        var viewerId = "user-viewer";

        var response = Substitute.For<Response<ResolveUsersByPermissionResponse>>();
        response.Message.Returns(new ResolveUsersByPermissionResponse(new List<PermissionUser>
        {
            new(viewerId, "viewer@example.com"),
            new(approverId, "approver@example.com") // Exclude approver
        }));

        _permissionClient.GetResponse<ResolveUsersByPermissionResponse>(
            Arg.Is<ResolveUsersByPermissionQuery>(q => q.Permission == Permissions.HrPayrollView),
            Arg.Any<CancellationToken>()
        ).Returns(response);

        var consumer = new PayrollRunApprovedConsumer(_permissionClient, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<PayrollRunApprovedIntegrationEvent>>();
        context.Message.Returns(new PayrollRunApprovedIntegrationEvent(payrollRunId, approverId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.payroll" &&
            e.EntityId == payrollRunId
        ), Arg.Any<CancellationToken>());

        await _mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
            c.UserId == viewerId &&
            c.TitleLocalizationKey == "notification.payroll.approved.title" &&
            c.Category == NotificationConstants.Categories.Payroll &&
            c.ActionUrl == "/hr/payroll" &&
            c.DeduplicationKey == $"payroll:{payrollRunId}:approved:{viewerId}"
        ), Arg.Any<CancellationToken>());

        await _mediator.DidNotReceive().Send(Arg.Is<CreateNotificationCommand>(c => c.UserId == approverId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consumer_ShouldHandleTimeout_ButStillEmitDataChange()
    {
        // Arrange
        var payrollRunId = Guid.NewGuid().ToString();
        var submitterId = "user-submitter";

        _permissionClient.GetResponse<ResolveUsersByPermissionResponse>(
            Arg.Any<ResolveUsersByPermissionQuery>(),
            Arg.Any<CancellationToken>()
        ).Throws(new RequestTimeoutException("Timeout"));

        var consumer = new PayrollRunSubmittedConsumer(_permissionClient, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<PayrollRunSubmittedIntegrationEvent>>();
        context.Message.Returns(new PayrollRunSubmittedIntegrationEvent(payrollRunId, submitterId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        // Data change must be emitted even on timeout
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.payroll" &&
            e.EntityId == payrollRunId
        ), Arg.Any<CancellationToken>());

        // No business notifications sent due to timeout
        await _mediator.DidNotReceive().Send(Arg.Any<CreateNotificationCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consumer_WithActorOnlyAudience_ShouldSendZeroNotifications_ButStillEmitDataChange()
    {
        // Arrange
        var payrollRunId = Guid.NewGuid().ToString();
        var submitterId = "user-submitter";

        var response = Substitute.For<Response<ResolveUsersByPermissionResponse>>();
        response.Message.Returns(new ResolveUsersByPermissionResponse(new List<PermissionUser>
        {
            new(submitterId, "submitter@example.com") // Target list contains only the actor
        }));

        _permissionClient.GetResponse<ResolveUsersByPermissionResponse>(
            Arg.Any<ResolveUsersByPermissionQuery>(),
            Arg.Any<CancellationToken>()
        ).Returns(response);

        var consumer = new PayrollRunSubmittedConsumer(_permissionClient, _mediator, _publishEndpoint, _logger);
        var context = Substitute.For<ConsumeContext<PayrollRunSubmittedIntegrationEvent>>();
        context.Message.Returns(new PayrollRunSubmittedIntegrationEvent(payrollRunId, submitterId));
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        // Data change must be emitted
        await _publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.payroll" &&
            e.EntityId == payrollRunId
        ), Arg.Any<CancellationToken>());

        // 0 notifications sent (since submitter is excluded)
        await _mediator.DidNotReceive().Send(Arg.Any<CreateNotificationCommand>(), Arg.Any<CancellationToken>());
    }
}
