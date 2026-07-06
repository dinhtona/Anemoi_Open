using System.Collections.Generic;

namespace Anemoi.Contract.Identity.Queries.PermissionQueries.ResolvePermissions;

public sealed record ResolvePermissionsQuery
{
    public List<string> RoleGroupCodes { get; init; } = [];
}

public sealed record ResolvePermissionsResponse
{
    public List<string> Permissions { get; init; } = [];
}
