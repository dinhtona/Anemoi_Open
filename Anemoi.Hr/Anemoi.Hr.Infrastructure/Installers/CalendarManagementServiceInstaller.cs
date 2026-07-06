using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Infrastructure;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.Installers;

public sealed class CalendarManagementServiceInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CalendarManagementMapper>();
        services.AddScoped<IWorkingCalendarEngine, WorkingCalendarEngine>();
    }
}
