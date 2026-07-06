using System;
using System.Linq;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ApproveLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.RejectLeaveRequest;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Serilog;

namespace Anemoi.Hr.Application.Services;

public sealed class LeaveNotificationActionHandler(
    IMediator mediator,
    ILogger logger) : INotificationActionHandler
{
    public string TargetService => NotificationWorkflowConstants.TargetServices.Leave;

    public async Task<NotificationActionResult> HandleAsync(NotificationActionCommand cmd)
    {
        var leaveRequestId = Guid.Parse(cmd.AggregateId);
        var employeeId = Guid.Parse(cmd.UserId);

        return cmd.ActionCode switch
        {
            NotificationWorkflowConstants.ActionCodes.ViewLeaveRequest =>
                new NotificationActionResult(true, "Navigating to leave request"),
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
}
