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
                NotificationWorkflowConstants.TargetServices.Leave => await HandleLeaveAction(cmd),
                NotificationWorkflowConstants.TargetServices.Overtime => await HandleOvertimeAction(cmd),
                NotificationWorkflowConstants.TargetServices.Payroll => await HandlePayrollAction(cmd),
                NotificationWorkflowConstants.TargetServices.Onboarding => await HandleOnboardingAction(cmd),
                NotificationWorkflowConstants.TargetServices.Recruitment => new NotificationActionResult(true, "Navigating to recruitment request"),
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
            NotificationWorkflowConstants.ActionCodes.ViewLeaveRequest => new NotificationActionResult(true, "Navigating to leave request"),
            NotificationWorkflowConstants.ActionCodes.ApproveLeaveRequest => await SendVoid(
                new ApproveLeaveRequestCommand(
                    new LeaveRequestId(leaveRequestId),
                    new EmployeeId(employeeId),
                    cmd.Comment ?? "")),
            NotificationWorkflowConstants.ActionCodes.RejectLeaveRequest => await SendVoid(
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
            NotificationWorkflowConstants.ActionCodes.ViewOvertimeRequest => new NotificationActionResult(true, "Navigating to overtime request"),
            NotificationWorkflowConstants.ActionCodes.ApproveOvertimeRequest => await SendResult(
                new ApproveOvertimeRequestCommand(
                    new OvertimeRequestId(overtimeId),
                    cmd.UserId)),
            NotificationWorkflowConstants.ActionCodes.RejectOvertimeRequest => await SendResult(
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
            NotificationWorkflowConstants.ActionCodes.ViewPayrollRun => new NotificationActionResult(true, "Navigating to payroll run"),
            NotificationWorkflowConstants.ActionCodes.ApprovePayrollRun => await SendResult<PayrollRunDetailResponse>(approveCmd),
            NotificationWorkflowConstants.ActionCodes.RejectPayrollRun => await SendResult<PayrollRunDetailResponse>(rejectCmd),
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
            NotificationWorkflowConstants.ActionCodes.ViewOnboardingTask => new NotificationActionResult(true, "Navigating to onboarding task"),
            NotificationWorkflowConstants.ActionCodes.CompleteOnboardingTask => await SendResult<OnboardingInstanceResponse>(completeCmd),
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
