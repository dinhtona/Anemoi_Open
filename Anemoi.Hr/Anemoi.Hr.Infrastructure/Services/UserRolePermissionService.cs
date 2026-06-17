using Anemoi.Hr.Application.Abstractions;

namespace Anemoi.Hr.Infrastructure.Services;

public sealed class UserRolePermissionService : IUserRolePermissionService
{
    public bool UserHasRole(string userId, string role)
    {
        // Phase 28: stub — will integrate with Identity service in Phase 29+
        return false;
    }

    public bool UserHasPermission(string userId, string permission)
    {
        // Phase 28: stub — will integrate with Identity service in Phase 29+
        return false;
    }
}
