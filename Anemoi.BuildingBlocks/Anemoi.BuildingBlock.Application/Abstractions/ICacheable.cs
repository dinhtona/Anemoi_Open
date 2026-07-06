using System;

namespace Anemoi.BuildingBlock.Application.Abstractions;

/// <summary>
/// Implement this interface on a MediatR Query to enable automatic caching via CachingBehavior pipeline.
/// </summary>
public interface ICacheable
{
    /// <summary>Unique cache key for this query. Must be deterministic based on query parameters.</summary>
    string CacheKey { get; }

    /// <summary>How long to cache the result. Defaults to 5 minutes.</summary>
    TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}