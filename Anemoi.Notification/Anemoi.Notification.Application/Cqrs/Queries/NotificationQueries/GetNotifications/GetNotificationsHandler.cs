using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.Notification.Queries.NotificationQueries.GetNotifications;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Application.Mappings;
using Anemoi.Notification.Domain.Models;
using OneOf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Anemoi.Notification.Application.Cqrs.Queries.NotificationQueries.GetNotifications;

public sealed class GetNotificationsHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    NotificationMapper mapper,
    ILogger logger)
    : EfQueryPaginationHandler<NotificationHistory, GetNotificationsQuery, NotificationResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<NotificationHistory, NotificationResponse> BuildQueryFlow(
        IQueryListFilter<NotificationHistory, NotificationResponse> fromFlow, GetNotificationsQuery query)
    {
        var targetUserGuid = Guid.Parse(query.UserId);
        return fromFlow
            .WithFilter(x => x.UserId == targetUserGuid)
            .WithSpecialAction(x => x)
            .WithSortFieldWhenNotSet(x => x.CreatedTime)
            .WithSortedDirectionWhenNotSet(SortedDirection.Descending);
    }

    protected override Task<PaginationResponse<NotificationResponse>> MapToResultAsync(
        GetNotificationsQuery query,
        OneOf<List<NotificationHistory>, List<NotificationResponse>> modelsOrResponses,
        long totalRecord)
    {
        return modelsOrResponses.Match(
            models =>
            {
                var list = models.Select(mapper.ToNotificationResponse).ToList();
                return Task.FromResult(new PaginationResponse<NotificationResponse>(list, totalRecord));
            },
            responses => Task.FromResult(new PaginationResponse<NotificationResponse>(responses, totalRecord))
        );
    }
}

