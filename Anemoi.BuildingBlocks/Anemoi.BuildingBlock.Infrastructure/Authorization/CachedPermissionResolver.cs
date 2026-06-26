using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Contract.Identity.Queries.PermissionQueries.ResolvePermissions;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Anemoi.BuildingBlock.Infrastructure.Authorization;

public sealed class CachedPermissionResolver : IPermissionResolver, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedPermissionResolver> _logger;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public CachedPermissionResolver(IServiceScopeFactory scopeFactory,
        IMemoryCache cache, ILogger<CachedPermissionResolver> logger)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
    }

    public async ValueTask<bool> HasPermissionAsync(IEnumerable<string> roleGroups, string permission,
        CancellationToken cancellationToken = default)
    {
        var roleGroupList = roleGroups.Where(c => !string.IsNullOrEmpty(c)).OrderBy(c => c).ToList();
        if (roleGroupList.Count == 0) return false;

        var permissions = await GetOrResolveAsync(roleGroupList, cancellationToken);
        if (permissions is null) return false;
        return permissions.Contains(permission);
    }

    public void InvalidateRoleGroup(string roleGroupCode)
    {
        _cache.Remove(CacheKeyForGroup(roleGroupCode));
    }

    public void InvalidateAll()
    {
        if (_cache is MemoryCache memCache)
            memCache.Compact(1.0);
    }

    private async Task<HashSet<string>?> GetOrResolveAsync(List<string> roleGroupCodes,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey(roleGroupCodes);
        if (_cache.TryGetValue<HashSet<string>>(cacheKey, out var cached))
            return cached!;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<ResolvePermissionsQuery>>();

            var response = await client.GetResponse<ResolvePermissionsResponse>(
                new ResolvePermissionsQuery { RoleGroupCodes = roleGroupCodes },
                cancellationToken);

            var permissions = new HashSet<string>(
                response.Message.Permissions ?? [],
                StringComparer.OrdinalIgnoreCase);

            _cache.Set(cacheKey, permissions, CacheTtl);
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve permissions for {Key}", cacheKey);
            return null;
        }
    }

    private static string CacheKey(List<string> codes) => $"perm:{string.Join(",", codes)}";
    private static string CacheKeyForGroup(string code) => $"perm:{code}";

    public void Dispose()
    {
        if (_cache is IDisposable disposable)
            disposable.Dispose();
    }
}
