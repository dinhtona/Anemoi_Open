using System;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Contract.Notification.Responses;

namespace Anemoi.Contract.Notification.Queries.NotificationQueries.GetActionAudits;

public sealed record GetActionAuditsQuery : GetManyQuery, IQueryPaged<NotificationActionAuditResponse>
{
    public NotificationHistoryId? NotificationId { get; init; }
    public Guid? ExecutedBy { get; init; }
    public bool? Success { get; init; }
}
