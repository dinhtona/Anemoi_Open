using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Notification.Domain.Models;
using OneOf;

namespace Anemoi.Notification.Application.Services;

public sealed record ExecuteActionResult(
    string ActionType,
    string? TargetUrl,
    bool Success,
    string Message
);

public interface INotificationActionExecutor
{
    public const string ExecutorKey = "";
    static string Key => ExecutorKey;
    string ActionCode { get; }
    Task<OneOf<ExecuteActionResult, ErrorDetailResponse>> ExecuteAsync(
        NotificationHistory notification,
        NotificationAction action,
        CancellationToken cancellationToken);
}
