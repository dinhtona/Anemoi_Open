using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.Queries.UserQueries.ResolveUsersByPermission;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Identity.Application.Cqrs.Queries.UserQueries.ResolveUsersByPermission;

public sealed class ResolveUsersByPermissionQueryHandler(
    IUserRepository userRepository,
    ISqlRepository<UserMapRoleGroup> userMapRoleGroupRepository)
    : IQueryHandler<ResolveUsersByPermissionQuery, OneOf<ResolveUsersByPermissionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<ResolveUsersByPermissionResponse, ErrorDetailResponse>> Handle(
        ResolveUsersByPermissionQuery request,
        CancellationToken cancellationToken)
    {
        var permission = request.Permission;

        // 1. Direct Roles / Admin Roles
        var directUsers = await userRepository.GetUsersInRoleAsync(permission);
        var adminUsers = await userRepository.GetUsersInRoleAsync(SystemRoles.Administrator);

        var allDirectUsers = directUsers.Concat(adminUsers)
            .Select(u => new PermissionUser(u.UserId.Value.ToString(), u.Email))
            .ToList();

        // 2. Group Roles
        var groupUsers = await userMapRoleGroupRepository.GetQueryable()
            .Where(um => (um.RoleGroup.RoleGroupMapRoles.Any(r => r.Role.Name == permission) ||
                          um.RoleGroup.RoleGroupMapRoles.Any(r => r.Role.Name == SystemRoles.Administrator)) &&
                         !um.RoleGroup.RoleGroupClaims.Any(claim => claim.Key == AuthorizationClaimTypes.WorkspaceId))
            .Select(um => new { UserId = um.UserId.Value.ToString(), Email = um.User.Email })
            .ToListAsync(cancellationToken);

        var allUsers = allDirectUsers
            .Concat(groupUsers.Select(g => new PermissionUser(g.UserId, g.Email)))
            .GroupBy(u => u.UserId)
            .Select(g => g.First())
            .ToList();

        return new ResolveUsersByPermissionResponse(allUsers);
    }
}
