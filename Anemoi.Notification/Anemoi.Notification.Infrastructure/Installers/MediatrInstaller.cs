using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Notification.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Notification.Infrastructure.Installers;

public sealed class MediatrInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(INotificationApplicationAssemblyMarker).Assembly));
    }
}
