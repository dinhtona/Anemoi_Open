using System;
using System.Linq;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ApproveLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.RejectLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.ApproveOvertimeRequest;
using Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.RejectOvertimeRequest;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.ApprovePayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.RejectPayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CompleteOnboardingTask;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using MediatR;
using Serilog;

namespace Anemoi.Hr.Application.Consumers;

public sealed class NotificationActionCommandConsumer(
    IMediator mediator,
    ILogger logger)
    : IConsumer<NotificationActionCommand>
{
    public async Task Consume(ConsumeContext<NotificationActionCommand> context)
    {
        var cmd = context.Message;
        try
        {
            var result = cmd.TargetService switch
            {
                "Leave" => await HandleLeaveAction(cmd),
                "Overtime" => await HandleOvertimeAction(cmd),
                "Payroll" => await HandlePayrollAction(cmd),
                "Onboarding" => await HandleOnboardingAction(cmd),
                "Recruitment" => new NotificationActionResult(true, "Navigating to recruitment request"),
                _ => new NotificationActionResult(false, $"Unknown target service: {cmd.TargetService}")
            };

            await context.RespondAsync(result);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error handling notification action for {TargetService}/{ActionCode}",
                cmd.TargetService, cmd.ActionCode);
            await context.RespondAsync(new NotificationActionResult(false, "Internal error"));
        }
    }

    private async Task<NotificationActionResult> HandleLeaveAction(
        NotificationActionCommand cmd)
    {
        var leaveRequestId = Guid.Parse(cmd.AggregateId);
        var employeeId = Guid.Parse(cmd.UserId);

        return cmd.ActionCode switch
        {
            "ViewLeaveRequest" => new NotificationActionResult(true, "Navigating to leave request"),
            "ApproveLeaveRequest" => await SendVoid(
                new ApproveLeaveRequestCommand(
                    new LeaveRequestId(leaveRequestId),
                    new EmployeeId(employeeId),
                    cmd.Comment ?? "")),
            "RejectLeaveRequest" => await SendVoid(
                new RejectLeaveRequestCommand(
                    new LeaveRequestId(leaveRequestId),
                    new EmployeeId(employeeId),
                    cmd.Comment ?? "")),
            _ => new NotificationActionResult(false, $"Unknown leave action: {cmd.ActionCode}")
        };
    }

    private async Task<NotificationActionResult> HandleOvertimeAction(
        NotificationActionCommand cmd)
    {
        var overtimeId = Guid.Parse(cmd.AggregateId);

        return cmd.ActionCode switch
        {
            "ViewOvertimeRequest" => new NotificationActionResult(true, "Navigating to overtime request"),
            "ApproveOvertimeRequest" => await SendResult(
                new ApproveOvertimeRequestCommand(
                    new OvertimeRequestId(overtimeId),
                    cmd.UserId)),
            "RejectOvertimeRequest" => await SendResult(
                new RejectOvertimeRequestCommand(
                    new OvertimeRequestId(overtimeId),
                    cmd.UserId,
                    cmd.Comment)),
            _ => new NotificationActionResult(false, $"Unknown overtime action: {cmd.ActionCode}")
        };
    }

    private async Task<NotificationActionResult> HandlePayrollAction(
        NotificationActionCommand cmd)
    {
        var payrollRunId = Guid.Parse(cmd.AggregateId);

        var approveCmd = new ApprovePayrollRunCommand(
            new PayrollRunId(payrollRunId));
        var rejectCmd = new RejectPayrollRunCommand(
            new PayrollRunId(payrollRunId),
            cmd.Comment ?? "");

        return cmd.ActionCode switch
        {
            "ViewPayrollRun" => new NotificationActionResult(true, "Navigating to payroll run"),
            "ApprovePayrollRun" => await SendResult<PayrollRunDetailResponse>(approveCmd),
            "RejectPayrollRun" => await SendResult<PayrollRunDetailResponse>(rejectCmd),
            _ => new NotificationActionResult(false, $"Unknown payroll action: {cmd.ActionCode}")
        };
    }

    private async Task<NotificationActionResult> HandleOnboardingAction(
        NotificationActionCommand cmd)
    {
        var completeCmd = new CompleteOnboardingTaskCommand(
            new OnboardingTaskId(Guid.Parse(cmd.AggregateId)),
            cmd.Comment)
        {
            CompletedBy = cmd.UserId
        };

        return cmd.ActionCode switch
        {
            "ViewOnboardingTask" => new NotificationActionResult(true, "Navigating to onboarding task"),
            "CompleteOnboardingTask" => await SendResult<OnboardingInstanceResponse>(completeCmd),
            _ => new NotificationActionResult(false, $"Unknown onboarding action: {cmd.ActionCode}"
            )
        };
    }

    private async Task<NotificationActionResult> SendVoid(
        BuildingBlock.Application.Cqrs.Commands.ICommandVoid command)
    {
        try
        {
            var result = await mediator.Send(command);
            return result.Match(
                _ => new NotificationActionResult(true, "Action completed successfully"),
                error => new NotificationActionResult(false,
                    string.Join("; ", error.Messages ?? Enumerable.Empty<string>())));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error executing void command {CommandType}", command.GetType().Name);
            return new NotificationActionResult(false, ex.Message);
        }
    }

    private async Task<NotificationActionResult> SendResult<TResponse>(
        BuildingBlock.Application.Cqrs.Commands.ICommandResult<TResponse> command)
        where TResponse : class
    {
        try
        {
            var result = await mediator.Send(command);
            return result.Match(
                _ => new NotificationActionResult(true, "Action completed successfully"),
                error => new NotificationActionResult(false,
                    string.Join("; ", error.Messages ?? Enumerable.Empty<string>())));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error executing result command {CommandType}", command.GetType().Name);
            return new NotificationActionResult(false, ex.Message);
        }
    }
}
