using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.Workspace.Queries.MemberMapRoleGroupQueries.GetRoleGroupsByMember;
using Anemoi.Contract.Workspace.Responses;
using Anemoi.Workspace.Domain.Models;
using Anemoi.Workspace.Application.Mappings;
using Serilog;

namespace Anemoi.Workspace.Application.Cqrs.Queries.MemberMapRoleGroupQueries.GetRoleGroupsByMember;

public sealed class GetRoleGroupsByMemberHandler(
    ISqlRepository<MemberMapRoleGroup> sqlRepository,
    WorkspaceMapper mapper,
    ILogger logger)
    : EfQueryCollectionHandler<MemberMapRoleGroup, GetRoleGroupsByMemberQuery, MemberMapRoleGroupResponse>
        (sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<MemberMapRoleGroup, MemberMapRoleGroupResponse> BuildQueryFlow(
        IQueryListFilter<MemberMapRoleGroup, MemberMapRoleGroupResponse> fromFlow, GetRoleGroupsByMemberQuery query)
        => fromFlow
            .WithFilter(x => x.Member.UserId == query.UserId && x.Member.WorkspaceId == query.WorkspaceId)
            .WithSpecialAction(x => mapper.ProjectToMemberMapRoleGroupResponse(x.SelectMany(a => a.Member.MemberMapRoleGroups)))
            .WithSortFieldWhenNotSet(a => a.Order)
            .WithSortedDirectionWhenNotSet(SortedDirection.Ascending);
}