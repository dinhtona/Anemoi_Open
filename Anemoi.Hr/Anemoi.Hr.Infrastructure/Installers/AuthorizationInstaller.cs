using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Anemoi.Hr.Infrastructure.Installers;

public sealed class AuthorizationInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<IPermissionResolver>(sp =>
            new CachedPermissionResolver(
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<IMemoryCache>(),
                sp.GetRequiredService<ILogger<CachedPermissionResolver>>()));
        services.AddSingleton<IAuthorizationHandler, HasPermissionHandler>();
        services.AddAuthorization(options =>
        {
            foreach (var permission in HrPermissions.All)
            {
                options.AddPolicy($"{HasPermissionAttribute.PolicyPrefix}{permission}",
                    builder => builder.Requirements.Add(new HasPermissionRequirement(permission)));
            }
        });
    }
}
