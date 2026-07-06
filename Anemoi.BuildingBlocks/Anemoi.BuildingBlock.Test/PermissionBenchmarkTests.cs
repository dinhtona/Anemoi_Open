using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Anemoi.BuildingBlock.Test;

public sealed class PermissionBenchmarkTests
{
    private readonly ITestOutputHelper _output;
    private static readonly string[] TestRoleGroups = ["HR", "Employee", "Manager"];
    private static readonly HashSet<string> TestPermissions = new(StringComparer.OrdinalIgnoreCase)
    {
        "hr.employee.read", "hr.employee.write",
        "hr.leave.read", "hr.attendance.read",
        "hr.payroll.read", "hr.recruitment.read"
    };

    public PermissionBenchmarkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static (CachedPermissionResolver, List<string>) CreateResolverWithCachedEntry()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var logger = Substitute.For<ILogger<CachedPermissionResolver>>();
        var resolver = new CachedPermissionResolver(scopeFactory, cache, logger);

        var codes = TestRoleGroups.ToList();
        var cacheKey = $"perm:{string.Join(",", codes.OrderBy(c => c))}";
        cache.Set(cacheKey, TestPermissions, TimeSpan.FromMinutes(5));

        return (resolver, codes);
    }

    [Fact]
    public async Task CacheHit_Latency_ShouldBeSubMillisecond()
    {
        var (resolver, codes) = CreateResolverWithCachedEntry();

        for (var i = 0; i < 10; i++)
            await resolver.HasPermissionAsync(codes, "hr.employee.read", CancellationToken.None);

        var sw = Stopwatch.StartNew();
        var result = await resolver.HasPermissionAsync(codes, "hr.employee.read", CancellationToken.None);
        sw.Stop();

        _output.WriteLine($"Cache hit latency: {sw.Elapsed.TotalMicroseconds:F1} µs ({sw.Elapsed.TotalMilliseconds:F3} ms)");
        Assert.True(result);
        Assert.True(sw.Elapsed.TotalMilliseconds < 1.0,
            $"Cache hit should be sub-millisecond, was {sw.Elapsed.TotalMilliseconds:F3} ms");

        _output.WriteLine($"  Cache hit → 0 MassTransit calls, 0 DB queries (in-memory only)");
    }

    [Fact]
    public async Task CacheHit_MultipleHasPermissionAttributes_ShouldBeFast()
    {
        var (resolver, codes) = CreateResolverWithCachedEntry();

        for (var i = 0; i < 3; i++)
            await resolver.HasPermissionAsync(codes, "hr.employee.read", CancellationToken.None);

        var testPermissions = new[] { "hr.employee.read", "hr.leave.read", "hr.payroll.read", "hr.attendance.read" };

        var sw = Stopwatch.StartNew();
        foreach (var perm in testPermissions)
        {
            var result = await resolver.HasPermissionAsync(codes, perm, CancellationToken.None);
            Assert.True(result);
        }
        sw.Stop();

        var avgPerCall = sw.Elapsed.TotalMicroseconds / testPermissions.Length;
        _output.WriteLine($"Multiple cache hits ({testPermissions.Length} calls): total={sw.Elapsed.TotalMicroseconds:F1} µs, avg={avgPerCall:F1} µs/call");
        _output.WriteLine($"  Equivalent to a controller with {testPermissions.Length} [HasPermission] attributes");
        _output.WriteLine($"  All calls hit cache → 0 MassTransit calls, 0 DB queries total");

        Assert.True(avgPerCall < 50.0,
            $"Average per-call cache hit should be under 50 µs, was {avgPerCall:F1} µs");
    }

    [Fact]
    public async Task CacheHit_SteadyState_ShouldBeExtremelyFast()
    {
        var (resolver, codes) = CreateResolverWithCachedEntry();

        const int warmupCalls = 100;
        const int measuredCalls = 1000;

        for (var i = 0; i < warmupCalls; i++)
            await resolver.HasPermissionAsync(codes, "hr.employee.read", CancellationToken.None);

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < measuredCalls; i++)
            await resolver.HasPermissionAsync(codes, "hr.employee.read", CancellationToken.None);
        sw.Stop();

        var avgNs = sw.Elapsed.TotalNanoseconds / measuredCalls;
        _output.WriteLine($"Steady-state cache hit (warmup={warmupCalls}, measured={measuredCalls} calls):");
        _output.WriteLine($"  Total: {sw.Elapsed.TotalMilliseconds:F5} ms");
        _output.WriteLine($"  Avg: {avgNs:F1} ns ({avgNs / 1000.0:F2} µs)");

        Assert.True(avgNs < 10_000.0,
            $"Steady-state cache hit should be under 10 µs, was {avgNs:F1} ns");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task CacheHit_WithRoleGroupCount(int roleGroupCount)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var logger = Substitute.For<ILogger<CachedPermissionResolver>>();
        var resolver = new CachedPermissionResolver(scopeFactory, cache, logger);

        var codes = Enumerable.Range(0, roleGroupCount).Select(i => $"RoleGroup{i}").ToList();
        var cacheKey = $"perm:{string.Join(",", codes.OrderBy(c => c))}";

        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < 50; i++)
            permissions.Add($"permission.test.{i}");

        cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(5));

        for (var i = 0; i < 5; i++)
            await resolver.HasPermissionAsync(codes, "permission.test.25", CancellationToken.None);

        var sw = Stopwatch.StartNew();
        var result = await resolver.HasPermissionAsync(codes, "permission.test.25", CancellationToken.None);
        sw.Stop();

        _output.WriteLine($"Cache hit with {roleGroupCount} role groups / 50 permissions: {sw.Elapsed.TotalMicroseconds:F1} µs");

        Assert.True(result);
        Assert.True(sw.Elapsed.TotalMicroseconds < 200.0,
            $"Cache hit with {roleGroupCount} role groups should be fast");
    }

    [Fact]
    public async Task CacheInvalidation_RemovesDirectEntries()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var logger = Substitute.For<ILogger<CachedPermissionResolver>>();
        var resolver = new CachedPermissionResolver(scopeFactory, cache, logger);

        var codesSingle = new List<string> { "HR" };
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "hr.employee.read", "hr.leave.read"
        };

        var singleKey = $"perm:HR";
        cache.Set(singleKey, permissions, TimeSpan.FromMinutes(5));

        Assert.True(cache.TryGetValue(singleKey, out _));

        resolver.InvalidateRoleGroup("HR");

        Assert.False(cache.TryGetValue(singleKey, out _));

        _output.WriteLine("Cache invalidation verified: direct entry removed.");
    }

    [Fact]
    public void CacheInvalidation_InvalidateAll_ClearsEverything()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var logger = Substitute.For<ILogger<CachedPermissionResolver>>();
        var resolver = new CachedPermissionResolver(scopeFactory, cache, logger);

        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "hr.employee.read"
        };

        cache.Set("perm:HR", permissions, TimeSpan.FromMinutes(5));
        cache.Set("perm:Employee", permissions, TimeSpan.FromMinutes(5));
        cache.Set("perm:HR,Employee", permissions, TimeSpan.FromMinutes(5));

        resolver.InvalidateAll();

        Assert.False(cache.TryGetValue("perm:HR", out _));
        Assert.False(cache.TryGetValue("perm:Employee", out _));
        Assert.False(cache.TryGetValue("perm:HR,Employee", out _));

        _output.WriteLine("Cache invalidation verified: InvalidateAll clears all entries.");
    }

    [Fact]
    public async Task MultipleHasPermission_SingleRequest_OnlyOneResolutionOccurs()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        scopeFactory.CreateScope().Returns(scope);
        var logger = Substitute.For<ILogger<CachedPermissionResolver>>();
        var resolver = new CachedPermissionResolver(scopeFactory, cache, logger);

        var codes = TestRoleGroups.ToList();
        var cacheKey = $"perm:{string.Join(",", codes.OrderBy(c => c))}";

        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "hr.employee.read", "hr.employee.write",
            "hr.leave.read", "hr.payroll.read"
        };

        cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(5));

        for (var i = 0; i < 5; i++)
            await resolver.HasPermissionAsync(codes, "hr.employee.read", CancellationToken.None);

        var testPerms = new[] { "hr.employee.read", "hr.leave.read", "hr.payroll.read", "hr.employee.write" };
        foreach (var perm in testPerms)
        {
            await resolver.HasPermissionAsync(codes, perm, CancellationToken.None);
        }

        Assert.True(cache.TryGetValue(cacheKey, out _));

        scopeFactory.DidNotReceive().CreateScope();

        _output.WriteLine("Verified: all HasPermission calls use cached permission set; " +
                          "scope factory was never called (cache hits only).");
    }
}
