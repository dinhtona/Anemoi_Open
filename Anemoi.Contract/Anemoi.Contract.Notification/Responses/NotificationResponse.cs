using System;

namespace Anemoi.Contract.Notification.Responses;

public sealed record NotificationResponse
{
    public string Id { get; init; }
    public string UserId { get; init; }
    public string Title { get; init; }
    public string Content { get; init; }
    public string Category { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedTime { get; init; }
    public DateTime? ReadTime { get; init; }
}
