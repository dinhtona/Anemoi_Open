using Anemoi.BuildingBlock.Application.Cqrs.Commands;

namespace Anemoi.Contract.Notification.Commands.NotificationPreferenceCommands.CreateOrUpdateNotificationPreference;

public sealed record CreateOrUpdateNotificationPreferenceCommand(
    string UserId,
    bool EnableInApp,
    bool EnableEmail) : ICommandVoid;
