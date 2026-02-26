using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.Workspace.Commands.OrganizationCommands.CreateOrganization;
using Anemoi.Contract.Workspace.Errors;
using Anemoi.Workspace.Domain.Models;
using Anemoi.Workspace.Application.Mappings;
using Serilog;

namespace Anemoi.Workspace.Application.Cqrs.Commands.OrganizationCommands.CreateOrganization;

public sealed class CreateOrganizationHandler(
    ISqlRepository<Organization> sqlRepository,
    IUnitOfWork unitOfWork,
    WorkspaceMapper mapper,
    ILogger logger)
    : EfCommandOneVoidHandler<Organization, CreateOrganizationCommand>(sqlRepository, unitOfWork,
        logger)
{
    protected override ICommandOneFlowBuilderVoid<Organization> BuildCommand(
        IStartOneCommandVoid<Organization> fromFlow, CreateOrganizationCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .CreateOne(mapper.ToOrganization(command))
            .WithCondition(async organization =>
            {
                if (command.SubDomain is null) return None.Value;
                var existedWorkspaceDomain = await SqlRepository
                    .ExistByConditionAsync(x => x.SubDomain == organization.SubDomain,
                        cancellationToken);
                if (existedWorkspaceDomain)
                    return WorkspaceErrorDetail.WorkspaceError.DomainExist();
                return None.Value;
            })
            .WithErrorIfSaveChange(WorkspaceErrorDetail.WorkspaceError.CreateFailed());
}