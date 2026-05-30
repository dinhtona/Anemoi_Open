using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.Identity.Commands.RoleGroupCommands.UpdateRoleGroup;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Identity.Application.Mappings;
using Anemoi.Identity.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Anemoi.Identity.Domain.Models;

namespace Anemoi.Identity.Application.Cqrs.Commands.RoleGroupCommands.UpdateRoleGroup;

public sealed class UpdateRoleGroupHandler(
    ISqlRepository<RoleGroup> sqlRepository,
    IUnitOfWork unitOfWork,
    IdentityMapper mapper,
    ILogger logger,
    ISqlRepository<RoleGroupMapRole> dbRoleGroupIdentityRoleRepository,
    ISqlRepository<Role> roleRepository,
    ISqlRepository<UserMapRoleGroup> userMapRoleGroupRepository,
    ISqlRepository<User> userDbRepository,
    IUserRepository userRepository,
    IUserSessionRevocationService sessionRevocationService,
    IUserPermissionChangeNotifier permissionChangeNotifier)
    : EfCommandOneVoidHandler<RoleGroup, UpdateRoleGroupCommand>(sqlRepository,
        unitOfWork, logger)
{
    private PreparedSessionRevocation _preparedRevocation;
    private List<UserId> _permissionChangedUserIds = [];

    protected override ICommandOneFlowBuilderVoid<RoleGroup> BuildCommand(IStartOneCommandVoid<RoleGroup> fromFlow,
        UpdateRoleGroupCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .UpdateOne(x => x.Id == command.Id)
            .WithSpecialAction(r => r
                .Include(x => x.RoleGroupMapRoles)
                .Include(x => x.RoleGroupClaims))
            .WithCondition(async roleGroup =>
            {
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

                if (command.IdentityRoleIds is null)
                    return IdentityErrorDetail.RoleError.RolesRequestMustNotBeNull();
                if (command.IdentityRoleIds.Distinct().Count() != command.IdentityRoleIds.Count)
                    return IdentityErrorDetail.RoleError.RolesRequestDuplicated();

                var roleIds = command.IdentityRoleIds;
                var isSystemWide = !roleGroup.RoleGroupClaims.Any(claim =>
                    claim.Key == AuthorizationClaimTypes.WorkspaceId);
                if (isSystemWide && await roleRepository.ExistByConditionAsync(
                        role => roleIds.Contains(role.RoleId) && role.Name == "Administrator",
                        cancellationToken))
                    return IdentityErrorDetail.RoleError.ReservedSystemRole();

                var validRoleCount = await roleRepository.GetQueryable(x => roleIds.Contains(x.RoleId))
                    .CountAsync(cancellationToken);
                if (validRoleCount != roleIds.Distinct().Count())
                    return IdentityErrorDetail.RoleError.RolesNotExist();

                var duplicateName = await SqlRepository.ExistByConditionAsync(
                    x => x.Id != command.Id && x.Name == command.Name, cancellationToken);
                if (duplicateName) return IdentityErrorDetail.RoleGroupError.DuplicateNameFailed();

                var roleGroupIdentityRoleIds = roleGroup.RoleGroupMapRoles
                    .Select(x => x.RoleId).ToList();
                var idRoleGroupIdentityRolesRemoving = roleGroupIdentityRoleIds.Except(roleIds).ToList();
                var idsAdding = roleIds.Except(roleGroupIdentityRoleIds);
                var idsRemoving = roleGroup.RoleGroupMapRoles.Where(x =>
                    idRoleGroupIdentityRolesRemoving.Contains(x.RoleId)).ToList();
                await dbRoleGroupIdentityRoleRepository.RemoveManyAsync(idsRemoving, cancellationToken);
                var roleGroupIdentityRoles = idsAdding.Select(roleId =>
                    new RoleGroupMapRole
                    {
                        Id = new RoleGroupMapUserRoleId(IdGenerator.NextGuid()),
                        RoleGroupId = roleGroup.Id,
                        RoleId = roleId
                    }).ToList();
                await dbRoleGroupIdentityRoleRepository.CreateManyAsync(roleGroupIdentityRoles, cancellationToken);

                var userIds = await userMapRoleGroupRepository.GetQueryable(x => x.RoleGroupId == command.Id)
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                var workspaceId = roleGroup.RoleGroupClaims
                    .FirstOrDefault(claim => claim.Key == AuthorizationClaimTypes.WorkspaceId)?.Value;
                var otherRoleMaps = await userMapRoleGroupRepository.GetQueryable(x =>
                        userIds.Contains(x.UserId) &&
                        x.RoleGroupId != command.Id &&
                        (isSystemWide
                            ? !x.RoleGroup.RoleGroupClaims.Any(claim =>
                                claim.Key == AuthorizationClaimTypes.WorkspaceId)
                            : x.RoleGroup.RoleGroupClaims.Any(claim =>
                                claim.Key == AuthorizationClaimTypes.WorkspaceId &&
                                claim.Value == workspaceId)))
                    .SelectMany(x => x.RoleGroup.RoleGroupMapRoles.Select(map =>
                        new { x.UserId, map.RoleId }))
                    .ToListAsync(cancellationToken);
                var otherRolesByUser = otherRoleMaps
                    .GroupBy(x => x.UserId)
                    .ToDictionary(group => group.Key,
                        group => group.Select(x => x.RoleId).ToHashSet());
                var revokedUserIds = new List<UserId>();
                var users = await userDbRepository.GetQueryable(x => userIds.Contains(x.UserId))
                    .ToListAsync(cancellationToken);
                foreach (var userId in userIds)
                {
                    var user = users.Single(x => x.UserId == userId);
                    var directRoles = await userRepository.GetDirectRolesAsync(user);
                    if (directRoles.Contains("Administrator"))
                        continue;

                    var otherRoles = otherRolesByUser.GetValueOrDefault(userId) ?? [];
                    var previousRoles = otherRoles.Union(roleGroupIdentityRoleIds);
                    var nextRoles = otherRoles.Union(roleIds);
                    if (previousRoles.Except(nextRoles).Any())
                        revokedUserIds.Add(userId);
                    else if (nextRoles.Except(previousRoles).Any())
                        _permissionChangedUserIds.Add(userId);
                }

                if (revokedUserIds.Count > 0)
                {
                    var prepareResult = await sessionRevocationService.PrepareAsync(
                        revokedUserIds, cancellationToken);
                    if (prepareResult.IsT1) return prepareResult.AsT1;
                    _preparedRevocation = prepareResult.AsT0;
                }
                return None.Value;
            })
            .WithModify(roleGroup => mapper.UpdateRoleGroup(command, roleGroup))
            .WithErrorIfNull(IdentityErrorDetail.RoleGroupError.NotFound())
            .WithErrorIfSaveChange(IdentityErrorDetail.RoleGroupError.UpdateFailed());

    protected override async Task AfterSaveChangesAsync(UpdateRoleGroupCommand command,
        RoleGroup model, CancellationToken cancellationToken)
    {
        if (_preparedRevocation is not null)
            await sessionRevocationService.PublishAsync(_preparedRevocation, cancellationToken);
        await permissionChangeNotifier.PublishAsync(_permissionChangedUserIds, cancellationToken);
    }
}
