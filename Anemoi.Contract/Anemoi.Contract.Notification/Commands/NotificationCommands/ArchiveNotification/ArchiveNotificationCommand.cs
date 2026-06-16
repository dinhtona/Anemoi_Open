using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.ArchiveNotification;

public sealed record ArchiveNotificationCommand(NotificationHistoryId Id, string UserId) : ICommandVoid;
