using Anemoi.BuildingBlock.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Anemoi.BuildingBlock.Infrastructure.Authorization;

public sealed class HasPermissionHandler : AuthorizationHandler<HasPermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        HasPermissionRequirement requirement)
    {
        if (context.User.IsInRole(SystemRoles.Administrator) ||
            context.User.IsInRole(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
