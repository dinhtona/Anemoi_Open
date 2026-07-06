using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Notification.Domain.Models;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Services;

public sealed class LeaveApprovalNotificationExecutor : INotificationActionExecutor
{
    public string ActionCode => "LeaveApproval";

    public Task<OneOf<ExecuteActionResult, ErrorDetailResponse>> ExecuteAsync(
        NotificationHistory notification,
        NotificationAction action,
        CancellationToken cancellationToken)
    {
        var targetUrl = $"/leave-requests/{notification.AggregateId}";
        var result = new ExecuteActionResult(
            action.ActionType,
            targetUrl,
            true,
            $"Navigating to leave request details for {notification.AggregateId}"
        );
        return Task.FromResult<OneOf<ExecuteActionResult, ErrorDetailResponse>>(result);
    }
}
