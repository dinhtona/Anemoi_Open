using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Notification.Domain.Models;
using OneOf;

namespace Anemoi.Notification.Application.Services;

public sealed class DefaultNotificationActionExecutor : INotificationActionExecutor
{
    public string ActionCode => "*";

    public Task<OneOf<ExecuteActionResult, ErrorDetailResponse>> ExecuteAsync(
        NotificationHistory notification,
        NotificationAction action,
        CancellationToken cancellationToken)
    {
        var targetUrl = action.ActionType switch
        {
            "Navigate" => action.ActionUrl,
            "ExternalLink" => action.ActionUrl,
            "Command" => null,
            _ => null
        };

        var result = new ExecuteActionResult(
            action.ActionType,
            targetUrl,
            true,
            $"Action '{action.ActionLabel}' executed successfully"
        );

        return Task.FromResult<OneOf<ExecuteActionResult, ErrorDetailResponse>>(result);
    }
}
