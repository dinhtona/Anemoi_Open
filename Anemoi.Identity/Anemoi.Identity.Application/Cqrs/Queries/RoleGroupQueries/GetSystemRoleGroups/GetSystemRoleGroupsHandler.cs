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
using Anemoi.Contract.Identity.Queries.RoleGroupQueries.GetSystemRoleGroups;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Identity.Domain.Models;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Queries.RoleGroupQueries.GetSystemRoleGroups;

public sealed class GetSystemRoleGroupsHandler(ISqlRepository<RoleGroup> sqlRepository, ILogger logger)
    : EfQueryPaginationHandler<RoleGroup, GetSystemRoleGroupsQuery, RoleGroupsResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<RoleGroup, RoleGroupsResponse> BuildQueryFlow(
        IQueryListFilter<RoleGroup, RoleGroupsResponse> fromFlow, GetSystemRoleGroupsQuery query)
    {
        Expression<Func<RoleGroup, bool>> searchKeyFilter = query.SearchKey switch
        {
            { } val => roleGroup => roleGroup.SearchHint.Contains(val.GenerateSearchHint()),
            _ => _ => true
        };
        Expression<Func<RoleGroup, bool>> defaultFilter = query.IsDefault switch
        {
            { } val => roleGroup => roleGroup.IsDefault == val,
            _ => _ => true
        };
        Expression<Func<RoleGroup, bool>> creatorIdsFilter = query.CreatorIds switch
        {
            { Count: > 0 } val => roleGroup =>
                val.Select(x => new UserId(Guid.Parse(x))).Contains(roleGroup.CreatorId),
            _ => _ => true
        };
        Expression<Func<RoleGroup, bool>> systemWideFilter = roleGroup =>
            !roleGroup.RoleGroupClaims.Any(claim => claim.Key == AuthorizationClaimTypes.WorkspaceId);

        return fromFlow
            .WithFilter(ExpressionHelper.CombineAnd(
                searchKeyFilter, defaultFilter, creatorIdsFilter, systemWideFilter))
            .WithSpecialAction(queryable => queryable.Select(roleGroup => new RoleGroupsResponse
            {
                Id = roleGroup.Id.ToString(),
                Name = roleGroup.Name,
                Description = roleGroup.Description,
                CreatedTime = roleGroup.CreatedTime,
                CreatorId = roleGroup.CreatorId != null ? roleGroup.CreatorId.ToString() : null,
                CreatorName = roleGroup.Creator != null ? roleGroup.Creator.FirstName : null,
                CreatorEmail = roleGroup.Creator != null ? roleGroup.Creator.Email : null,
                UpdaterId = roleGroup.UpdaterId != null ? roleGroup.UpdaterId.ToString() : null,
                UpdaterName = roleGroup.Updater != null ? roleGroup.Updater.FirstName : null,
                UpdaterEmail = roleGroup.Updater != null ? roleGroup.Updater.Email : null,
                UpdatedTime = roleGroup.UpdatedTime,
                IsDefault = roleGroup.IsDefault
            }))
            .WithSortFieldWhenNotSet(x => x.CreatedTime)
            .WithSortedDirectionWhenNotSet(SortedDirection.Descending);
    }
}
