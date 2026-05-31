using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryOneFlow;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryOne;
using Anemoi.Contract.Workspace.Errors;
using Anemoi.Contract.Workspace.Queries.MemberInvitationQueries.GetMemberInvitation;
using Anemoi.Contract.Workspace.ModelIds;
using Anemoi.Contract.Workspace.Responses;
using Anemoi.Workspace.Application.Mappings;
using Serilog;
using Anemoi.Workspace.Domain.Models;

namespace Anemoi.Workspace.Application.Cqrs.Queries.MemberInvitationQueries.GetMemberInvitation;

public sealed class
    GetMemberInvitationHandler(ISqlRepository<MemberInvitation> sqlRepository, IWorkspaceIdGetter workspaceIdGetter,
        WorkspaceMapper mapper, ILogger logger)
    : EfQueryOneHandler<MemberInvitation, GetMemberInvitationQuery, MemberInvitationResponse>(sqlRepository,
        logger)
{
    protected override IQueryOneFlowBuilder<MemberInvitation, MemberInvitationResponse> BuildQueryFlow(
        IQueryOneFilter<MemberInvitation, MemberInvitationResponse> fromFlow, GetMemberInvitationQuery query)
        => fromFlow
            .WithFilter(x => x.Id == query.Id &&
                             x.WorkspaceId == new WorkspaceId(Guid.Parse(workspaceIdGetter.WorkspaceId)))
            .WithSpecialAction(mapper.ProjectToMemberInvitationResponse)
            .WithErrorIfNull(WorkspaceErrorDetail.MemberInvitationError.NotFound());
}
