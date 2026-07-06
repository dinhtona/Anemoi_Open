using System;
using System.Globalization;
using System.Threading.Tasks;
using Anemoi.Centralize.Api.Hubs;
using Anemoi.Contract.Identity.Events;
using Anemoi.BuildingBlock.Application.Configurations;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;

namespace Anemoi.Centralize.Api.Consumers;

public sealed class UserTokenRevokedIntegrationEventConsumer(
    IDistributedCache distributedCache,
    IHubContext<NotificationHub> hubContext,
    JwtSetting jwtSetting,
    Serilog.ILogger logger)
    : IConsumer<UserTokenRevokedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserTokenRevokedIntegrationEvent> context)
    {
        var @event = context.Message;
        var cacheKey = $"revoked_user:{@event.UserId}";
        await distributedCache.SetStringAsync(cacheKey,
            @event.RevokedAt.Ticks.ToString(CultureInfo.InvariantCulture),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = jwtSetting.TokenLifetime },
            context.CancellationToken);
        await hubContext.Clients.User(@event.UserId.ToString())
            .SendAsync("UserSessionRevoked", cancellationToken: context.CancellationToken);
        logger.Information("Cached token revocation for user {UserId} at {RevokedAt}", @event.UserId, @event.RevokedAt);
    }
}
