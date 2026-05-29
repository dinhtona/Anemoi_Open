using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Notification.Responses;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;

public sealed record CreateNotificationCommand(string UserId, string Title, string Content, string Category)
    : ICommandResult<NotificationResponse>;
