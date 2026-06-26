using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anemoi.Contract.Identity.Queries.PermissionQueries.ResolvePermissions;
using Anemoi.Identity.Infrastructure.DataContext;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Anemoi.Identity.Infrastructure.Consumers;

public sealed class ResolvePermissionsQueryConsumer : IConsumer<ResolvePermissionsQuery>
{
    private readonly IdentityDbContext _dbContext;
    private readonly ILogger<ResolvePermissionsQueryConsumer> _logger;

    public ResolvePermissionsQueryConsumer(IdentityDbContext dbContext,
        ILogger<ResolvePermissionsQueryConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ResolvePermissionsQuery> context)
    {
        var query = context.Message;
        if (query.RoleGroupCodes is not { Count: > 0 })
        {
            await context.RespondAsync(new ResolvePermissionsResponse());
            return;
        }

        var permissions = await _dbContext.RoleGroups
            .Where(rg => query.RoleGroupCodes.Contains(rg.Code))
            .SelectMany(rg => rg.RoleGroupMapRoles)
            .Select(rm => rm.Role.Name)
            .Distinct()
            .ToListAsync(context.CancellationToken);

        _logger.LogDebug("Resolved {Count} permissions for role groups {Groups}",
            permissions.Count, string.Join(",", query.RoleGroupCodes));

        await context.RespondAsync(new ResolvePermissionsResponse { Permissions = permissions ?? [] });
    }
}
