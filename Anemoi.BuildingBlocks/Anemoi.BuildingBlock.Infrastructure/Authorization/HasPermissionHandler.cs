using System.Linq;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace Anemoi.BuildingBlock.Infrastructure.Authorization;

public sealed class HasPermissionHandler : AuthorizationHandler<HasPermissionRequirement>
{
    private readonly IPermissionResolver _permissionResolver;

    public HasPermissionHandler(IPermissionResolver permissionResolver)
    {
        _permissionResolver = permissionResolver;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        HasPermissionRequirement requirement)
    {
        var roleGroups = context.User.FindAll(AuthorizationClaimTypes.RoleGroup)
            .Select(c => c.Value)
            .ToList();

        if (roleGroups.Count == 0)
            return;

        if (await _permissionResolver.HasPermissionAsync(roleGroups, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
