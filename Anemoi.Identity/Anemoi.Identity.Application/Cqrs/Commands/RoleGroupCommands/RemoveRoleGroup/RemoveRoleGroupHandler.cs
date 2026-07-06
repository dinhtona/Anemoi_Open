using System.Linq;
using System.Threading;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.Identity.Commands.RoleGroupCommands.RemoveRoleGroup;
using Anemoi.Contract.Identity.Errors;
using Serilog;
using Anemoi.Identity.Domain.Models;
using Anemoi.BuildingBlock.Application.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Identity.Application.Cqrs.Commands.RoleGroupCommands.RemoveRoleGroup;

public sealed class RemoveRoleGroupHandler(
    ISqlRepository<RoleGroup> sqlRepository,
    IUnitOfWork unitOfWork,
    ILogger logger,
    ISqlRepository<UserMapRoleGroup> userRoleGroupDbRepository)
    : EfCommandOneVoidHandler<RoleGroup, RemoveRoleGroupCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<RoleGroup> BuildCommand(IStartOneCommandVoid<RoleGroup> fromFlow,
        RemoveRoleGroupCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .RemoveOne(a => a.Id == command.Id)
            .WithSpecialAction(query => query.Include(x => x.RoleGroupClaims))
            .WithCondition(async roleGroup =>
            {
                var isApplied = await userRoleGroupDbRepository
                    .ExistByConditionAsync(a => a.RoleGroupId == command.Id, cancellationToken);
                if (isApplied) return IdentityErrorDetail.RoleGroupError.Applied();

                var checkDefault = await SqlRepository.ExistByConditionAsync(
                    x => x.Id == command.Id && x.IsDefault, cancellationToken);
                if (checkDefault) return IdentityErrorDetail.RoleGroupError.RoleGroupDefault();

                if (command.RequireSystemWide &&
                    roleGroup.RoleGroupClaims.Any(x => x.Key == AuthorizationClaimTypes.WorkspaceId))
                    return IdentityErrorDetail.RoleGroupError.WorkspaceRoleGroup();

                if (command.WorkspaceId is not null &&
                    !roleGroup.RoleGroupClaims.Any(x => x.Key == AuthorizationClaimTypes.WorkspaceId &&
                                                        x.Value == command.WorkspaceId))
                    return IdentityErrorDetail.RoleGroupError.WorkspaceScope();

                return None.Value;
            })
            .WithErrorIfNull(IdentityErrorDetail.RoleGroupError.NotFound())
            .WithErrorIfSaveChange(IdentityErrorDetail.RoleGroupError.RemoveFailed());
}
