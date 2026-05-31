using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandManyFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandMany;
using Anemoi.Contract.Workspace.Commands.MemberInvitationCommands.ResendMemberInvitations;
using Anemoi.Contract.Workspace.Errors;
using Anemoi.Contract.Workspace.ModelIds;
using Serilog;
using Anemoi.Workspace.Domain.Models;

namespace Anemoi.Workspace.Application.Cqrs.Commands.MemberInvitationCommands.ResendMemberInvitations;

public sealed class ResendMemberInvitationsHandler(
    ISqlRepository<MemberInvitation> sqlRepository,
    IUnitOfWork unitOfWork,
    IWorkspaceIdGetter workspaceIdGetter,
    ILogger logger)
    : EfCommandManyVoidHandler<MemberInvitation, ResendMemberInvitationsCommand>(sqlRepository, unitOfWork,
        logger)
{
    protected override ICommandManyFlowBuilderVoid<MemberInvitation> BuildCommand(
        IStartManyCommandVoid<MemberInvitation> fromFlow, ResendMemberInvitationsCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .UpdateMany(x => command.Ids.Contains(x.Id) &&
                             x.WorkspaceId == new WorkspaceId(Guid.Parse(workspaceIdGetter.WorkspaceId)))
            .WithSpecialAction(null)
            .WithCondition(_ => None.Value)
            .WithModify(_ => { })
            .WithErrorIfSaveChange(WorkspaceErrorDetail.MemberInvitationError.UpdateFailed());
}
