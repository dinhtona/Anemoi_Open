using System;

namespace Anemoi.Contract.Notification.Responses;

public sealed record NotificationActionAuditResponse
{
    public string Id { get; init; }
    public string NotificationId { get; init; }
    public string ActionId { get; init; }
    public string ExecutedBy { get; init; }
    public DateTime ExecutedAt { get; init; }
    public bool Success { get; init; }
    public string Result { get; init; }
    public string ClientIp { get; init; }
    public string UserAgent { get; init; }
}
