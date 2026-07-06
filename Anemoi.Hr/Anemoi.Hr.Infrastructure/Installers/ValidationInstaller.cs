using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.Installers;

public sealed class ValidationInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(IHrApplicationAssemblyMarker).Assembly);
    }
}
