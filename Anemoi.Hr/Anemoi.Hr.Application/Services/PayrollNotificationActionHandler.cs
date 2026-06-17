using System;
using System.Linq;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.ApprovePayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.RejectPayrollRun;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Serilog;

namespace Anemoi.Hr.Application.Services;

public sealed class PayrollNotificationActionHandler(
    IMediator mediator,
    ILogger logger) : INotificationActionHandler
{
    public string TargetService => NotificationWorkflowConstants.TargetServices.Payroll;

    public async Task<NotificationActionResult> HandleAsync(NotificationActionCommand cmd)
    {
        var payrollRunId = Guid.Parse(cmd.AggregateId);

        var approveCmd = new ApprovePayrollRunCommand(
            new PayrollRunId(payrollRunId));
        var rejectCmd = new RejectPayrollRunCommand(
            new PayrollRunId(payrollRunId),
            cmd.Comment ?? "");

        return cmd.ActionCode switch
        {
            NotificationWorkflowConstants.ActionCodes.ViewPayrollRun =>
                new NotificationActionResult(true, "Navigating to payroll run"),
            NotificationWorkflowConstants.ActionCodes.ApprovePayrollRun =>
                await SendResult<PayrollRunDetailResponse>(approveCmd),
            NotificationWorkflowConstants.ActionCodes.RejectPayrollRun =>
                await SendResult<PayrollRunDetailResponse>(rejectCmd),
            _ => new NotificationActionResult(false, $"Unknown payroll action: {cmd.ActionCode}")
        };
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
