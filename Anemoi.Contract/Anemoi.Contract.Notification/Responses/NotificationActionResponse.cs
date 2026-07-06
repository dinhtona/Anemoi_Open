namespace Anemoi.Contract.Notification.Responses;

public sealed record NotificationActionResponse
{
    public string Id { get; init; }
    public string Label { get; init; }
    public string ActionType { get; init; }
    public bool RequiresConfirmation { get; init; }
}
