namespace Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;

public sealed record CreateNotificationActionInput(
    string ActionCode,
    string ActionLabel,
    string ActionType,
    string? ActionUrl = null,
    bool RequiresConfirmation = false,
    int SortOrder = 0);
