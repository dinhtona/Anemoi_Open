using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.Installers;

public sealed class SettingsInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration.GetSection(nameof(HrSettings)).Get<HrSettings>() ?? new HrSettings());
    }
}
