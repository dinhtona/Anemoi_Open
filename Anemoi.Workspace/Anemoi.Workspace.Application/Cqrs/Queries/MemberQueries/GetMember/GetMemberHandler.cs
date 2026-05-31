using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryOneFlow;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryOne;
using Anemoi.Contract.Workspace.Errors;
using Anemoi.Contract.Workspace.Queries.MemberQueries.GetMember;
using Anemoi.Contract.Workspace.ModelIds;
using Anemoi.Contract.Workspace.Responses;
using Anemoi.Workspace.Application.Mappings;
using Serilog;
using Anemoi.Workspace.Domain.Models;

namespace Anemoi.Workspace.Application.Cqrs.Queries.MemberQueries.GetMember;

public sealed class GetMemberHandler(ISqlRepository<Member> sqlRepository, IWorkspaceIdGetter workspaceIdGetter,
    WorkspaceMapper mapper, ILogger logger)
    : EfQueryOneHandler<Member, GetMemberQuery, MemberResponse>(sqlRepository,
        logger)
{
    protected override IQueryOneFlowBuilder<Member, MemberResponse> BuildQueryFlow(
        IQueryOneFilter<Member, MemberResponse> fromFlow, GetMemberQuery query)
        => fromFlow
            .WithFilter(x => x.Id == query.Id &&
                             x.WorkspaceId == new WorkspaceId(Guid.Parse(workspaceIdGetter.WorkspaceId)))
            .WithSpecialAction(mapper.ProjectToMemberResponse)
            .WithErrorIfNull(WorkspaceErrorDetail.MemberError.NotFound());
}
