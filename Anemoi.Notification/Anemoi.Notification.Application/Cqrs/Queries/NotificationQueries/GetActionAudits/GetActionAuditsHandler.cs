using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.Queries.NotificationQueries.GetActionAudits;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Domain.Models;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Queries.NotificationQueries.GetActionAudits;

public sealed class GetActionAuditsHandler(
    ISqlRepository<NotificationActionAudit> sqlRepository,
    ILogger logger)
    : EfQueryPaginationHandler<NotificationActionAudit, GetActionAuditsQuery, NotificationActionAuditResponse>(
        sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<NotificationActionAudit, NotificationActionAuditResponse> BuildQueryFlow(
        IQueryListFilter<NotificationActionAudit, NotificationActionAuditResponse> fromFlow,
        GetActionAuditsQuery query)
    {
        var filter = BuildFilterExpression(query);

        return fromFlow
            .WithFilter(filter)
            .WithSpecialAction(x => x)
            .WithSortFieldWhenNotSet(x => x.ExecutedAt)
            .WithSortedDirectionWhenNotSet(SortedDirection.Descending);
    }

    private static Expression<Func<NotificationActionAudit, bool>> BuildFilterExpression(
        GetActionAuditsQuery query)
    {
        Expression<Func<NotificationActionAudit, bool>> filter = x => true;
        var param = filter.Parameters[0];

        if (query.NotificationId != null)
        {
            filter = Expression.Lambda<Func<NotificationActionAudit, bool>>(
                Expression.AndAlso(filter.Body,
                    Expression.Equal(
                        Expression.Property(param, "NotificationId"),
                        Expression.Constant(query.NotificationId.Value))),
                param);
        }

        if (query.ExecutedBy.HasValue)
        {
            filter = Expression.Lambda<Func<NotificationActionAudit, bool>>(
                Expression.AndAlso(filter.Body,
                    Expression.Equal(
                        Expression.Property(param, "ExecutedBy"),
                        Expression.Constant(new UserId(query.ExecutedBy.Value)))),
                param);
        }

        if (query.Success.HasValue)
        {
            filter = Expression.Lambda<Func<NotificationActionAudit, bool>>(
                Expression.AndAlso(filter.Body,
                    Expression.Equal(
                        Expression.Property(param, "Success"),
                        Expression.Constant(query.Success.Value))),
                param);
        }

        return filter;
    }

    protected override Task<PaginationResponse<NotificationActionAuditResponse>> MapToResultAsync(
        GetActionAuditsQuery query,
        OneOf<List<NotificationActionAudit>, List<NotificationActionAuditResponse>> modelsOrResponses,
        long totalRecord)
    {
        return modelsOrResponses.Match(
            models =>
            {
                var list = models.Select(MapToResponse).ToList();
                return Task.FromResult(new PaginationResponse<NotificationActionAuditResponse>(list, totalRecord));
            },
            responses =>
                Task.FromResult(new PaginationResponse<NotificationActionAuditResponse>(responses, totalRecord))
        );
    }

    private static NotificationActionAuditResponse MapToResponse(NotificationActionAudit audit)
    {
        return new NotificationActionAuditResponse
        {
            Id = audit.Id.Value.ToString(),
            NotificationId = audit.NotificationId.Value.ToString(),
            ActionId = audit.ActionId.Value.ToString(),
            ExecutedBy = audit.ExecutedBy.ToString(),
            ExecutedAt = audit.ExecutedAt,
            Success = audit.Success,
            Result = audit.Result,
            ClientIp = audit.ClientIp,
            UserAgent = audit.UserAgent
        };
    }
}
