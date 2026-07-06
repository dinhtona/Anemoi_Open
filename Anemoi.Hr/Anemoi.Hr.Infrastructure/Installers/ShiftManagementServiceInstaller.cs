using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Infrastructure;
using Anemoi.Hr.Application.Mappings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.Installers;

public sealed class ShiftManagementServiceInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ShiftManagementMapper>();
    }
}
