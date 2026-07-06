using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.BuildingBlock.Infrastructure.PolicyHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Centralize.Infrastructure.Installers;

public sealed class AuthorizationInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<IPermissionResolver>(sp =>
            new CachedPermissionResolver(
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CachedPermissionResolver>>()));
        services.AddSingleton<IAuthorizationHandler, HasOneOfPolicyHandler>();
        services.AddSingleton<IAuthorizationHandler, HasPermissionHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.Internal,
                builder => builder.RequireClaim("applicationPolicyInternal", AuthorizationPolicies.Internal));
            options.AddPolicy(AuthorizationPolicies.Agency,
                builder => builder.RequireClaim("applicationPolicyAgency", AuthorizationPolicies.Agency));
            options.AddPolicy(AuthorizationPolicies.User,
                builder => builder.RequireClaim("applicationPolicyUser", AuthorizationPolicies.User));
            options.AddPolicy(AuthorizationPolicies.InternalOrAgency, builder => builder.Requirements
                .Add(new HasOneOfPolicyRequirement($"{AuthorizationPolicies.Internal},{AuthorizationPolicies.Agency}")));
            foreach (var permission in Permissions.All)
            {
                options.AddPolicy($"{HasPermissionAttribute.PolicyPrefix}{permission}",
                    builder => builder.Requirements.Add(new HasPermissionRequirement(permission)));
            }
        });
    }
}
