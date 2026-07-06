using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Infrastructure.GeneralInstaller;
using Anemoi.MasterData.Application.Abstractions;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Application.Services;
using Anemoi.MasterData.Domain;
using Anemoi.MasterData.Infrastructure.DataContext;
using Anemoi.MasterData.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.MasterData.Infrastructure.Installers;

public sealed class ServiceInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<MasterDataMapper>();
        services.AddEfRepositoriesAsScope<MasterDataDbContext>(typeof(IMasterDataDomainAssemblyMarker).Assembly);
        services.AddEfUnitOfWorkAsScope<MasterDataDbContext>();

        // Seed Data Services
        services.AddScoped<IDbDiscoveryService, DbDiscoveryService>();
        services.AddScoped<IDataGeneratorService, DataGeneratorService>();
        services.AddScoped<IDataIngestionService, DataIngestionService>();
    }
}