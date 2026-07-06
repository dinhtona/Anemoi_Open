using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Contract.Identity.Events;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Identity.Application.Abstractions;
using MassTransit;

namespace Anemoi.Identity.Application.Services;

public sealed class UserPermissionChangeNotifier(IPublishEndpoint publishEndpoint)
    : IUserPermissionChangeNotifier
{
    public async Task PublishAsync(IEnumerable<UserId> userIds, CancellationToken cancellationToken)
    {
        foreach (var userId in userIds.Distinct())
        {
            await publishEndpoint.Publish(new UserPermissionsChangedIntegrationEvent
            {
                UserId = userId.Value,
                ChangedAt = DateTime.UtcNow
            }, cancellationToken);
        }
    }
}
