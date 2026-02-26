using System.Threading;
using Anemoi.BuildingBlock.Application.Abstractions;
using Serilog;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.Identity.Commands.RoleGroupCommands.CreateRoleGroup;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Identity.Application.Mappings;
using Anemoi.Identity.Domain.Models;

namespace Anemoi.Identity.Application.Cqrs.Commands.RoleGroupCommands.CreateRoleGroup;

public sealed class CreateRoleGroupHandler(
    ISqlRepository<RoleGroup> sqlRepository,
    IUnitOfWork unitOfWork,
    IdentityMapper mapper,
    ILogger logger)
    : EfCommandOneVoidHandler<RoleGroup, CreateRoleGroupCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<RoleGroup> BuildCommand(IStartOneCommandVoid<RoleGroup> fromFlow,
        CreateRoleGroupCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .CreateOne(mapper.ToRoleGroup(command))
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(IdentityErrorDetail.RoleGroupError.CreateFailed());
}