using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandManyFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandMany;
using Anemoi.Contract.Workspace.Commands.MemberCommands.RemoveMembers;
using Anemoi.Contract.Workspace.Errors;
using Anemoi.Contract.Workspace.ModelIds;
using Serilog;
using Anemoi.Workspace.Domain.Models;

namespace Anemoi.Workspace.Application.Cqrs.Commands.MemberCommands.RemovedMembers;

public sealed class RemovedMembersHandler(
    ISqlRepository<Member> sqlRepository,
    IWorkspaceIdGetter workspaceIdGetter,
    IUnitOfWork unitOfWork,
    ILogger logger) : EfCommandManyVoidHandler<Member, RemoveMembersCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandManyFlowBuilderVoid<Member> BuildCommand(IStartManyCommandVoid<Member> fromFlow,
        RemoveMembersCommand command, CancellationToken cancellationToken)
        => fromFlow
            .RemoveMany(x => command.Ids.Contains(x.Id) &&
                             x.WorkspaceId == new WorkspaceId(Guid.Parse(workspaceIdGetter.WorkspaceId)))
            .WithSpecialAction(x => x)
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(WorkspaceErrorDetail.MemberError.RemoveFailed());
}
