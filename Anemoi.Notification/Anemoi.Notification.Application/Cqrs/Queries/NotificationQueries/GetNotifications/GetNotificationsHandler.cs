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
using Anemoi.Contract.Notification.Queries.NotificationQueries.GetNotifications;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Application.Mappings;
using Anemoi.Notification.Domain.Models;
using OneOf;
using Serilog;

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
        var filter = BuildFilterExpression(targetUserGuid, query);

        return fromFlow
            .WithFilter(filter)
            .WithSpecialAction(x => x)
            .WithSortFieldWhenNotSet(x => x.CreatedTime)
            .WithSortedDirectionWhenNotSet(SortedDirection.Descending);
    }

    private static Expression<Func<NotificationHistory, bool>> BuildFilterExpression(
        Guid userId, GetNotificationsQuery query)
    {
        Expression<Func<NotificationHistory, bool>> filter = x =>
            x.UserId == userId && !x.IsHidden;

        var param = filter.Parameters[0];

        // Status filter
        if (!string.IsNullOrEmpty(query.StatusFilter) &&
            !query.StatusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var isRead = query.StatusFilter.Equals("Read", StringComparison.OrdinalIgnoreCase);
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(
                Expression.AndAlso(filter.Body,
                    Expression.Equal(
                        Expression.Property(param, "IsRead"),
                        Expression.Constant(isRead))),
                param);
        }

        // Category filter
        if (!string.IsNullOrEmpty(query.Category))
        {
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(
                Expression.AndAlso(filter.Body,
                    Expression.Equal(
                        Expression.Property(param, "Category"),
                        Expression.Constant(query.Category))),
                param);
        }

        // Severity filter
        if (!string.IsNullOrEmpty(query.Severity))
        {
            filter = Expression.Lambda<Func<NotificationHistory, bool>>(
                Expression.AndAlso(filter.Body,
                    Expression.Equal(
                        Expression.Property(param, "Severity"),
                        Expression.Constant(query.Severity))),
                param);
        }

        // Keyword search
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

        // Date range
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
