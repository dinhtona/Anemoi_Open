using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Domain.StronglyTypedHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.Installers;

public sealed class ControllerInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new StronglyTypedIdJsonConverterFactory());
            });
    }
}
