using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.Workspace.Commands.OrganizationCommands.UpdateOrganization;
using Anemoi.Contract.Workspace.Errors;
using Anemoi.Contract.Workspace.ModelIds;
using Anemoi.Workspace.Application.Mappings;
using Serilog;
using Anemoi.Workspace.Domain.Models;

namespace Anemoi.Workspace.Application.Cqrs.Commands.OrganizationCommands.UpdateOrganization;

public sealed class UpdateOrganizationHandler(
    ISqlRepository<Organization> sqlRepository,
    IUnitOfWork unitOfWork,
    IWorkspaceIdGetter workspaceIdGetter,
    WorkspaceMapper mapper,
    ILogger logger)
    : EfCommandOneVoidHandler<Organization, UpdateOrganizationCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<Organization> BuildCommand(
        IStartOneCommandVoid<Organization> fromFlow, UpdateOrganizationCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .UpdateOne(x => x.Id == command.Id &&
                            x.WorkspaceId == new WorkspaceId(Guid.Parse(workspaceIdGetter.WorkspaceId)))
            .WithSpecialAction(null)
            .WithCondition(_ => None.Value)
            .WithModify(organization => mapper.UpdateOrganization(command, organization))
            .WithErrorIfNull(WorkspaceErrorDetail.OrganizationError.NotFound())
            .WithErrorIfSaveChange(WorkspaceErrorDetail.OrganizationError.UpdateFailed());
}
