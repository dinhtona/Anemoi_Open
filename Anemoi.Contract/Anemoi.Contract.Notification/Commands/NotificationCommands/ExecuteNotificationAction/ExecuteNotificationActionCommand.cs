using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.ExecuteNotificationAction;

public sealed record ExecuteNotificationActionCommand(
    NotificationHistoryId NotificationId,
    NotificationActionId ActionId,
    string UserId,
    string ClientIp,
    string UserAgent
) : ICommandResult<ExecuteNotificationActionResult>;

public sealed record ExecuteNotificationActionResult(
    string ActionType,
    string TargetUrl,
    bool Success,
    string Message
);
