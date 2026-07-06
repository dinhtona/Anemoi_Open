using Anemoi.BuildingBlock.Application.Cqrs.Commands;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.ArchiveAllReadNotifications;

public sealed record ArchiveAllReadNotificationsCommand(string UserId) : ICommandVoid;
