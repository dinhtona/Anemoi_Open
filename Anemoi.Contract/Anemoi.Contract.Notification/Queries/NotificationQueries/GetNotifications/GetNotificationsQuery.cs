using System;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Contract.Notification.Responses;

namespace Anemoi.Contract.Notification.Queries.NotificationQueries.GetNotifications;

public sealed record GetNotificationsQuery(string UserId) :
    GetManyQuery, IQueryPaged<NotificationResponse>
{
    public string StatusFilter { get; init; } = "All";
    public string Category { get; init; }
    public string Severity { get; init; }
    public string Keyword { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}
