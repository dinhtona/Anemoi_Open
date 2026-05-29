using System;
using System.Linq;
using System.Linq.Expressions;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Identity.Queries.RoleGroupQueries.GetRoleGroups;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Identity.Application.Mappings;
using Anemoi.Identity.Domain.Models;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Queries.RoleGroupQueries.GetRoleGroups;

public sealed class GetRoleGroupsHandler(ISqlRepository<RoleGroup> sqlRepository, IdentityMapper mapper, ILogger logger)
    : EfQueryPaginationHandler<RoleGroup, GetRoleGroupsQuery, RoleGroupsResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<RoleGroup, RoleGroupsResponse> BuildQueryFlow(
        IQueryListFilter<RoleGroup, RoleGroupsResponse> fromFlow, GetRoleGroupsQuery query)
    {
        Expression<Func<RoleGroup, bool>> searchKeyFilter = query.SearchKey switch
        {
            { } val => r => r.SearchHint.Contains(val.GenerateSearchHint()),
            _ => _ => true
        };
        Expression<Func<RoleGroup, bool>> defaultFilter = query.IsDefault switch
        {
            { } val => r => r.IsDefault == val,
            _ => _ => true
        };

        Expression<Func<RoleGroup, bool>> creatorIdsFilter = query.CreatorIds switch
        {
            { Count: > 0 } val => r => val.Select(x => new UserId(Guid.Parse(x))).Contains(r.CreatorId),
            _ => _ => true
        };

        var roleGroupClaimsFilter = query.RoleGroupClaimKeys switch
        {
            { Count: > 0 } keys => keys
                .Select<string, Expression<Func<RoleGroup, bool>>>(k => r => r.RoleGroupClaims.Any(x => x.Key == k))
                .Aggregate(ExpressionHelper.OrElse),
            _ => _ => true
        };

        var finalFilter = ExpressionHelper
            .CombineAnd(searchKeyFilter, defaultFilter, creatorIdsFilter, roleGroupClaimsFilter);
        return fromFlow
            .WithFilter(finalFilter)
            .WithSpecialAction(q => q.Select(r => new RoleGroupsResponse
            {
                Id = r.Id.ToString(),
                Name = r.Name,
                Description = r.Description,
                CreatedTime = r.CreatedTime,
                CreatorId = r.CreatorId != null ? r.CreatorId.ToString() : null,
                CreatorName = r.Creator != null ? r.Creator.FirstName : null,
                CreatorEmail = r.Creator != null ? r.Creator.Email : null,
                UpdaterId = r.UpdaterId != null ? r.UpdaterId.ToString() : null,
                UpdaterName = r.Updater != null ? r.Updater.FirstName : null,
                UpdaterEmail = r.Updater != null ? r.Updater.Email : null,
                UpdatedTime = r.UpdatedTime,
                IsDefault = r.IsDefault
            }))
            .WithSortFieldWhenNotSet(x => x.CreatedTime)
            .WithSortedDirectionWhenNotSet(SortedDirection.Descending);
    }
}