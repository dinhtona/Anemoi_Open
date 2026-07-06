using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Mappings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.Installers;

public sealed class RecruitmentServiceInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<RecruitmentMapper>();
    }
}
