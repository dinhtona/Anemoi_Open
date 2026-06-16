using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.ArchiveSelectedNotifications;

public sealed record ArchiveSelectedNotificationsCommand(
    List<NotificationHistoryId> NotificationIds,
    string UserId
) : ICommandVoid;
