using Anemoi.BuildingBlock.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Centralize.Infrastructure.Installers;

public sealed class PolicyInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("CorsSetting:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        var publicAppOrigin = configuration["PUBLIC_APP_ORIGIN"]?.TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(publicAppOrigin))
        {
            allowedOrigins = allowedOrigins
                .Append(publicAppOrigin)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        services.AddCors(options => options
            .AddPolicy("CorsPolicy", builder => builder
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("X-Original-File-Name", "X-OnMeeting-Token")));
    }
}
