namespace Anemoi.Hr.Application.Abstractions;

public interface IUserRolePermissionService
{
    bool UserHasRole(string userId, string role);
    bool UserHasPermission(string userId, string permission);
}
