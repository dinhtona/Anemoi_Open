using System;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Contract.Notification.Responses;

namespace Anemoi.Contract.Notification.Queries.NotificationQueries.GetArchivedNotifications;

public sealed record GetArchivedNotificationsQuery(string UserId) :
    GetManyQuery, IQueryPaged<NotificationResponse>
{
    public string Category { get; init; }
    public string Severity { get; init; }
    public string Keyword { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}
