using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.Workspace.Commands.WorkspaceCommands.UpdateWorkspace;
using Anemoi.Contract.Workspace.Errors;
using Anemoi.Contract.Workspace.ModelIds;
using Anemoi.Workspace.Application.Mappings;
using Serilog;

namespace Anemoi.Workspace.Application.Cqrs.Commands.WorkspaceCommands.UpdateWorkspace;

public sealed class UpdateWorkspaceHandler(
    ISqlRepository<Anemoi.Workspace.Domain.Models.Workspace> sqlRepository,
    IUnitOfWork unitOfWork,
    IWorkspaceIdGetter workspaceIdGetter,
    WorkspaceMapper mapper,
    ILogger logger)
    : EfCommandOneVoidHandler<Anemoi.Workspace.Domain.Models.Workspace, UpdateWorkspaceCommand>(sqlRepository,
        unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<Anemoi.Workspace.Domain.Models.Workspace> BuildCommand(
        IStartOneCommandVoid<Anemoi.Workspace.Domain.Models.Workspace> fromFlow, UpdateWorkspaceCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .UpdateOne(x => x.Id == command.Id &&
                            x.Id == new WorkspaceId(Guid.Parse(workspaceIdGetter.WorkspaceId)))
            .WithSpecialAction(null)
            .WithCondition(existOne => None.Value)
            .WithModify(workspace => mapper.UpdateWorkspace(command, workspace))
            .WithErrorIfNull(WorkspaceErrorDetail.WorkspaceError.NotFound())
            .WithErrorIfSaveChange(WorkspaceErrorDetail.WorkspaceError.UpdateFailed());
}
