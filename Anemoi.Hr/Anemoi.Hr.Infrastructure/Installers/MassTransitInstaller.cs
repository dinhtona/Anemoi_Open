using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Configurations;
using Anemoi.Hr.Application;
using Anemoi.Hr.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.Installers;

public sealed class MassTransitInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        var masstransitSetting = configuration.GetSection(nameof(MassTransitSetting)).Get<MassTransitSetting>();
        services.AddMassTransit(configurator =>
        {
            configurator.SetKebabCaseEndpointNameFormatter();

            if (masstransitSetting is not null)
            {
                configurator.AddEntityFrameworkOutbox<HrDbContext>(outbox =>
                {
                    outbox.UsePostgres();
                    outbox.UseBusOutbox();
                });
            }

            configurator.AddConsumersFromNamespaceContaining<IHrApplicationAssemblyMarker>();

            var serviceConsumer = BuildingBlock.Infrastructure.HandlerConsumers.ConsumersHelper
                .CreateDynamicConsumerHandlers<Anemoi.Contract.Hr.IContractHrAssemblyMarker>("HrHandlersConsumer");
            configurator.AddConsumer(serviceConsumer);

            if (masstransitSetting is null)
            {
                configurator.UsingInMemory((context, bus) => bus.ConfigureEndpoints(context));
                return;
            }

            var (host, virtualHost, userName, password, _, _) = masstransitSetting;
            configurator.UsingRabbitMq((context, bus) =>
            {
                bus.Host(host, virtualHost, c =>
                {
                    c.Username(userName);
                    c.Password(password);
                });
                bus.ConfigureEndpoints(context);
            });
        });
    }
}
