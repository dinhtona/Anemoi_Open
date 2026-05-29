using System;
using System.Threading.Tasks;
using Anemoi.Contract.Identity.Events;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Anemoi.Centralize.Api.Consumers;

public sealed class UserTokenRevokedIntegrationEventConsumer(
    IMemoryCache memoryCache,
    Serilog.ILogger logger)
    : IConsumer<UserTokenRevokedIntegrationEvent>
{
    public Task Consume(ConsumeContext<UserTokenRevokedIntegrationEvent> context)
    {
        var @event = context.Message;
        var cacheKey = $"revoked_user:{@event.UserId}";
        // Cache the revocation time for 2 hours (covers standard JWT lifespan)
        memoryCache.Set(cacheKey, @event.RevokedAt, TimeSpan.FromHours(2));
        logger.Information("Cached token revocation for user {UserId} at {RevokedAt}", @event.UserId, @event.RevokedAt);
        return Task.CompletedTask;
    }
}
