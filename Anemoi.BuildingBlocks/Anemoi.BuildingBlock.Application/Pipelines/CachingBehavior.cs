using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Anemoi.BuildingBlock.Application.Pipelines;

/// <summary>
/// MediatR pipeline behavior that caches query results for requests implementing <see cref="ICacheable"/>.
/// Only requests that implement ICacheable will be cached; all others are passed through unchanged.
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse>(IMemoryCache cache, ILogger logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only cache requests that implement ICacheable
        if (request is not ICacheable cacheable)
            return await next();

        var cacheKey = cacheable.CacheKey;

        if (cache.TryGetValue(cacheKey, out TResponse cachedResponse))
        {
            logger.Information("Cache HIT for {@RequestType} with key: {@CacheKey}",
                typeof(TRequest).Name, cacheKey);
            return cachedResponse;
        }

        logger.Information("Cache MISS for {@RequestType} with key: {@CacheKey}",
            typeof(TRequest).Name, cacheKey);

        var response = await next();

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(cacheable.CacheDuration)
            .SetPriority(CacheItemPriority.Normal);

        cache.Set(cacheKey, response, cacheEntryOptions);

        return response;
    }
}
