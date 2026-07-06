namespace Anemoi.Contract.Notification.Responses;

public sealed record NotificationSettingResponse
{
    public string Category { get; init; }
    public bool IsEnabled { get; init; }
}
