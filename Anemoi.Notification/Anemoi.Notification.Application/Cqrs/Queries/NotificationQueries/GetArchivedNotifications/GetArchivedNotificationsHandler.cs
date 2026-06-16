using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.Notification.Queries.NotificationQueries.GetArchivedNotifications;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Application.Mappings;
using Anemoi.Notification.Domain.Models;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Queries.NotificationQueries.GetArchivedNotifications;

public sealed class GetArchivedNotificationsHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    NotificationMapper mapper,
    ILogger logger)
    : EfQueryPaginationHandler<NotificationHistory, GetArchivedNotificationsQuery, NotificationResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<NotificationHistory, NotificationResponse> BuildQueryFlow(
        IQueryListFilter<NotificationHistory, NotificationResponse> fromFlow, GetArchivedNotificationsQuery query)
    {
        var targetUserGuid = Guid.Parse(query.UserId);
        var filter = BuildFilterExpression(targetUserGuid, query);

        return fromFlow
            .WithFilter(filter)
            .WithSpecialAction(x => x)
            .WithSortFieldWhenNotSet(x => x.CreatedTime)
            .WithSortedDirectionWhenNotSet(SortedDirection.Descending);
    }

    private static Expression<Func<NotificationHistory, bool>> BuildFilterExpression(
        Guid userId, GetArchivedNotificationsQuery query)
    {
        Expression<Func<NotificationHistory, bool>> filter = x =>
            x.UserId == userId && x.IsArchived;

        var param = filter.Parameters[0];

        if (!string.IsNullOrEmpty(query.Category))
        {
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(
                Expression.AndAlso(filter.Body,
                    Expression.Equal(
                        Expression.Property(param, "Category"),
                        Expression.Constant(query.Category))),
                param);
        }

        if (!string.IsNullOrEmpty(query.Severity))
        {
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(
                Expression.AndAlso(filter.Body,
                    Expression.Equal(
                        Expression.Property(param, "Severity"),
                        Expression.Constant(query.Severity))),
                param);
        }

        if (!string.IsNullOrEmpty(query.Keyword))
        {
            var keyword = query.Keyword.ToLower();
            var titleContains = Expression.Call(
                Expression.Property(param, "Title"),
                "Contains", null, Expression.Constant(keyword));
            var contentContains = Expression.Call(
                Expression.Property(param, "Content"),
                "Contains", null, Expression.Constant(keyword));
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(
                Expression.AndAlso(filter.Body,
                    Expression.OrElse(titleContains, contentContains)),
                param);
        }

        if (query.DateFrom.HasValue)
        {
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(
                Expression.AndAlso(filter.Body,
                    Expression.GreaterThanOrEqual(
                        Expression.Property(param, "CreatedTime"),
                        Expression.Constant(query.DateFrom.Value))),
                param);
        }

        if (query.DateTo.HasValue)
        {
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(
                Expression.AndAlso(filter.Body,
                    Expression.LessThanOrEqual(
                        Expression.Property(param, "CreatedTime"),
                        Expression.Constant(query.DateTo.Value))),
                param);
        }

        return filter;
    }

    protected override Task<PaginationResponse<NotificationResponse>> MapToResultAsync(
        GetArchivedNotificationsQuery query,
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
