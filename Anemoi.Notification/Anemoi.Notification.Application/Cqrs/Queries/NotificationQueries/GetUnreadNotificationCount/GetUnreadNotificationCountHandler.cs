using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.CountingFlow;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Queries.EntityFramework.EfQueryCounting;
using Anemoi.Contract.Notification.Queries.NotificationQueries.GetUnreadNotificationCount;
using Anemoi.Notification.Domain.Models;
using Serilog;
using System;

namespace Anemoi.Notification.Application.Cqrs.Queries.NotificationQueries.GetUnreadNotificationCount;

public sealed class GetUnreadNotificationCountHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    ILogger logger)
    : EfQueryCountingHandler<NotificationHistory, GetUnreadNotificationCountQuery>(sqlRepository, logger)
{
    protected override ICountingFlowBuilder<NotificationHistory> BuildQueryFlow(
        ICountingFilter<NotificationHistory> fromFlow, GetUnreadNotificationCountQuery query)
    {
        var targetUserGuid = Guid.Parse(query.UserId);
        return fromFlow
            .WithFilter(x => x.UserId == targetUserGuid && !x.IsRead);
    }
}
