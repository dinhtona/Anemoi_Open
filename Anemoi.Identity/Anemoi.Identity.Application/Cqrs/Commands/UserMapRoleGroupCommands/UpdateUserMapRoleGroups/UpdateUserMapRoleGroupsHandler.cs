using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandManyFlow;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandMany;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Domain.Models;
using Anemoi.Contract.Identity.Commands.UserMapRoleGroupCommands.UpdateUserMapRoleGroups;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Commands.UserMapRoleGroupCommands.UpdateUserMapRoleGroups;

public sealed class UpdateUserMapRoleGroupsHandler(
    ISqlRepository<UserMapRoleGroup> sqlRepository,
    ISqlRepository<RoleGroup> roleGroupRepository,
    ISqlRepository<User> userDbRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IUserSessionRevocationService sessionRevocationService,
    IUserPermissionChangeNotifier permissionChangeNotifier,
    ILogger logger)
    : EfCommandManyVoidHandler<UserMapRoleGroup, UpdateUserMapRoleGroupsCommand>(sqlRepository, unitOfWork,
        logger)
{
    protected override ICommandManyFlowBuilderVoid<UserMapRoleGroup> BuildCommand(
        IStartManyCommandVoid<UserMapRoleGroup> fromFlow, UpdateUserMapRoleGroupsCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .CreateMany(() => Task.FromResult((command.RoleGroupIds ?? []).Select(roleGroupId => new UserMapRoleGroup
                {
                    Id = new UserMapRoleGroupId(IdGenerator.NextGuid()), UserId = command.UserId,
                    RoleGroupId = roleGroupId
                }).ToList()))
            .WithCondition(async _ =>
            {
                var roleGroupIds = command.RoleGroupIds ?? [];
                if (roleGroupIds.Distinct().Count() != roleGroupIds.Count)
                    return IdentityErrorDetail.UserMapRoleGroupError.RoleGroupsRequestDuplicated();

                var validRoleGroupCount = await roleGroupRepository
                    .GetQueryable(roleGroup => roleGroupIds.Contains(roleGroup.Id) &&
                        !roleGroup.RoleGroupClaims.Any(claim =>
                            claim.Key == AuthorizationClaimTypes.WorkspaceId))
                    .CountAsync(cancellationToken);
                if (validRoleGroupCount != roleGroupIds.Count)
                    return IdentityErrorDetail.RoleGroupError.NotFound();

                var existRoleGroups = await SqlRepository
                    .GetQueryable(a => a.UserId == command.UserId &&
                        !a.RoleGroup.RoleGroupClaims.Any(claim =>
                            claim.Key == AuthorizationClaimTypes.WorkspaceId))
                    .ToListAsync(cancellationToken);
                var previousRoles = await SqlRepository
                    .GetQueryable(a => a.UserId == command.UserId &&
                        !a.RoleGroup.RoleGroupClaims.Any(claim =>
                            claim.Key == AuthorizationClaimTypes.WorkspaceId))
                    .SelectMany(a => a.RoleGroup.RoleGroupMapRoles)
                    .Select(map => map.Role.Name)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                var nextRoles = await roleGroupRepository
                    .GetQueryable(roleGroup => roleGroupIds.Contains(roleGroup.Id))
                    .SelectMany(roleGroup => roleGroup.RoleGroupMapRoles)
                    .Select(map => map.Role.Name)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                await SqlRepository.RemoveManyAsync(existRoleGroups, cancellationToken);

                var user = await userDbRepository.GetQueryable(x => x.UserId == command.UserId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (user is null) return IdentityErrorDetail.UserError.NotFound();
                var directRoles = await userRepository.GetDirectRolesAsync(user);
                if (directRoles.Contains(SystemRoles.Administrator))
                    return None.Value;

                if (previousRoles.Except(nextRoles).Any())
                {
                    var prepareResult = await sessionRevocationService.PrepareAsync(
                        [command.UserId], cancellationToken);
                    if (prepareResult.IsT1) return prepareResult.AsT1;

                    await sessionRevocationService.PublishAsync(prepareResult.AsT0, cancellationToken);
                }
                else
                {
                    if (nextRoles.Except(previousRoles).Any())
                        await permissionChangeNotifier.PublishAsync([command.UserId], cancellationToken);
                }
                return None.Value;
            })
            .WithErrorIfSaveChange(IdentityErrorDetail.UserMapRoleGroupError.CreateFailed());
}
