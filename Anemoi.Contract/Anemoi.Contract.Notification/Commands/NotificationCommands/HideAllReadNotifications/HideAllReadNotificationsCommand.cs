using Anemoi.BuildingBlock.Application.Cqrs.Commands;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.HideAllReadNotifications;

public sealed record HideAllReadNotificationsCommand(string UserId) : ICommandVoid;
