using System.Linq;
using System.Threading;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Serilog;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.Identity.Commands.RoleGroupCommands.CreateRoleGroup;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Identity.Application.Mappings;
using Anemoi.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Identity.Application.Cqrs.Commands.RoleGroupCommands.CreateRoleGroup;

public sealed class CreateRoleGroupHandler(
    ISqlRepository<RoleGroup> sqlRepository,
    IUnitOfWork unitOfWork,
    IdentityMapper mapper,
    ISqlRepository<Role> roleRepository,
    ILogger logger)
    : EfCommandOneVoidHandler<RoleGroup, CreateRoleGroupCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<RoleGroup> BuildCommand(IStartOneCommandVoid<RoleGroup> fromFlow,
        CreateRoleGroupCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .CreateOne(mapper.ToRoleGroup(command))
            .WithCondition(async roleGroup =>
            {
                if (command.IdentityRoleIds is null)
                    return IdentityErrorDetail.RoleError.RolesRequestMustNotBeNull();
                if (command.IdentityRoleIds.Distinct().Count() != command.IdentityRoleIds.Count)
                    return IdentityErrorDetail.RoleError.RolesRequestDuplicated();

                var duplicateName = await SqlRepository.ExistByConditionAsync(
                    x => x.Name == roleGroup.Name, cancellationToken);
                if (duplicateName) return IdentityErrorDetail.RoleGroupError.DuplicateNameFailed();

                var roleIds = command.IdentityRoleIds;
                var isSystemWide = !(command.RoleGroupClaims ?? []).Any(claim =>
                    claim.Key == AuthorizationClaimTypes.WorkspaceId);
                if (isSystemWide && await roleRepository.ExistByConditionAsync(
                        role => roleIds.Contains(role.RoleId) && role.Name == "Administrator",
                        cancellationToken))
                    return IdentityErrorDetail.RoleError.ReservedSystemRole();

                var validRoleCount = await roleRepository.GetQueryable(x => roleIds.Contains(x.RoleId))
                    .CountAsync(cancellationToken);
                return validRoleCount == roleIds.Count
                    ? None.Value
                    : IdentityErrorDetail.RoleError.RolesNotExist();
            })
            .WithErrorIfSaveChange(IdentityErrorDetail.RoleGroupError.CreateFailed());
}
