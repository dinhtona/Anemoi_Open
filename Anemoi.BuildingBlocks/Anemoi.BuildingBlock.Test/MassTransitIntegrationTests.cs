using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Contract.Hr.Events;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Events;
using Anemoi.Notification.Application.Consumers;
using Anemoi.Notification.Application.Services;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Serilog;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class MassTransitIntegrationTests
{
    [Fact]
    public async Task Consumer_ShouldBeInvoked_WhenEventPublishedViaHarness()
    {
        var payslipId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var employeeUserId = Guid.NewGuid().ToString();

        var recipientResolver = Substitute.For<INotificationRecipientResolver>();
        recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns(employeeUserId);

        var mediator = Substitute.For<IMediator>();
        var logger = new LoggerConfiguration().CreateLogger();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<PayslipPublishedConsumer>();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            })
            .AddSingleton(recipientResolver)
            .AddSingleton(mediator)
            .AddSingleton<ILogger>(logger)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            await harness.Bus.Publish(new PayslipPublishedIntegrationEvent(payslipId, employeeId));

            var consumed = await harness.Consumed.Any<PayslipPublishedIntegrationEvent>(CancellationToken.None);
            Assert.True(consumed, "PayslipPublishedConsumer should have consumed the event");

            await mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
                c.DeduplicationKey == $"payslip:{payslipId}:published:{employeeUserId}"
            ), Arg.Any<CancellationToken>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Consumer_ShouldEmitDataChangeOccurred_ToHarnessPublished()
    {
        var overtimeRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var employeeUserId = Guid.NewGuid().ToString();

        var recipientResolver = Substitute.For<INotificationRecipientResolver>();
        recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns(employeeUserId);

        var mediator = Substitute.For<IMediator>();
        var logger = new LoggerConfiguration().CreateLogger();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<OvertimeRequestApprovedConsumer>();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            })
            .AddSingleton(recipientResolver)
            .AddSingleton(mediator)
            .AddSingleton<ILogger>(logger)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            await harness.Bus.Publish(new OvertimeRequestApprovedIntegrationEvent(overtimeRequestId, employeeId));

            var consumed = await harness.Consumed.Any<OvertimeRequestApprovedIntegrationEvent>(CancellationToken.None);
            Assert.True(consumed, "OvertimeRequestApprovedConsumer should have consumed the event");

            var dataChangePublished = await harness.Published.Any<DataChangeOccurredIntegrationEvent>(CancellationToken.None);
            Assert.True(dataChangePublished, "DataChangeOccurred should be published");

            await mediator.Received(1).Send(Arg.Is<CreateNotificationCommand>(c =>
                c.DeduplicationKey == $"overtime:{overtimeRequestId}:approved:{employeeUserId}"
            ), Arg.Any<CancellationToken>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task ConsumerThatSkipsNotification_ShouldStillBeConsumed_ByHarness()
    {
        var leaveRequestId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();
        var leavePolicyId = Guid.NewGuid().ToString();

        var recipientResolver = Substitute.For<INotificationRecipientResolver>();
        recipientResolver.ResolveApproverUserIdByEmployeeId(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((string)null);

        var mediator = Substitute.For<IMediator>();
        var logger = new LoggerConfiguration().CreateLogger();

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<LeaveRequestSubmittedConsumer>();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            })
            .AddSingleton(recipientResolver)
            .AddSingleton(mediator)
            .AddSingleton<ILogger>(logger)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            await harness.Bus.Publish(new LeaveRequestSubmittedIntegrationEvent(
                leaveRequestId, employeeId, leavePolicyId, null));

            var consumed = await harness.Consumed.Any<LeaveRequestSubmittedIntegrationEvent>(CancellationToken.None);
            Assert.True(consumed, "LeaveRequestSubmittedConsumer should have consumed the event");

            await mediator.DidNotReceive().Send(Arg.Any<CreateNotificationCommand>(), Arg.Any<CancellationToken>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
