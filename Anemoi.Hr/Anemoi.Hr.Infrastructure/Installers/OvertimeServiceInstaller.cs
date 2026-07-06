using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Infrastructure;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Overtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.Installers;

public sealed class OvertimeServiceInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOvertimeSnapshotProvider, OvertimeSnapshotProvider>();
        services.AddScoped<OvertimeMapper>();
    }
}
