using System;
using System.Linq;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr;
using Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.ApproveOvertimeRequest;
using Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.RejectOvertimeRequest;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Serilog;

namespace Anemoi.Hr.Application.Services;

public sealed class OvertimeNotificationActionHandler(
    IMediator mediator,
    ILogger logger) : INotificationActionHandler
{
    public string TargetService => NotificationWorkflowConstants.TargetServices.Overtime;

    public async Task<NotificationActionResult> HandleAsync(NotificationActionCommand cmd)
    {
        var overtimeId = Guid.Parse(cmd.AggregateId);

        return cmd.ActionCode switch
        {
            NotificationWorkflowConstants.ActionCodes.ViewOvertimeRequest =>
                new NotificationActionResult(true, "Navigating to overtime request"),
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
