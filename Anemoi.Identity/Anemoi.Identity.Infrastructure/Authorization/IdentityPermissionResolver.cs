using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Identity.Infrastructure.Authorization;

public sealed class IdentityPermissionResolver : IPermissionResolver
{
    private readonly DataContext.IdentityDbContext _dbContext;

    public IdentityPermissionResolver(DataContext.IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<bool> HasPermissionAsync(IEnumerable<string> roleGroups, string permission,
        CancellationToken cancellationToken = default)
    {
        var roleGroupList = roleGroups.Where(c => !string.IsNullOrEmpty(c)).ToList();
        if (roleGroupList.Count == 0) return false;

        var permissions = await ResolvePermissionsAsync(roleGroupList, cancellationToken);
        return permissions.Contains(permission);
    }

    public void InvalidateRoleGroup(string roleGroupCode) { }

    public void InvalidateAll() { }

    public async Task<HashSet<string>> ResolvePermissionSetAsync(List<string> roleGroupCodes,
        CancellationToken cancellationToken = default)
    {
        var permissions = await _dbContext.RoleGroups
            .Where(rg => roleGroupCodes.Contains(rg.Code))
            .SelectMany(rg => rg.RoleGroupMapRoles)
            .Select(rm => rm.Role.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new HashSet<string>(permissions, System.StringComparer.OrdinalIgnoreCase);
    }

    private async Task<HashSet<string>> ResolvePermissionsAsync(List<string> roleGroupCodes,
        CancellationToken cancellationToken)
    {
        return await ResolvePermissionSetAsync(roleGroupCodes, cancellationToken);
    }
}
