using Anemoi.BuildingBlock.Application.Cqrs.Commands;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.MarkAllAsRead;

public sealed record MarkAllNotificationsAsReadCommand(string UserId) : ICommandVoid;
