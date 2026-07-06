using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.UnarchiveNotification;

public sealed record UnarchiveNotificationCommand(NotificationHistoryId Id, string UserId) : ICommandVoid;
