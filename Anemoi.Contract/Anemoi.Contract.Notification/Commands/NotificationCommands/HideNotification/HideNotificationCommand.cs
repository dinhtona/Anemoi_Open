using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Notification.ModelIds;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.HideNotification;

public sealed record HideNotificationCommand(NotificationHistoryId Id, string UserId) : ICommandVoid;
