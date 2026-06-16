using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Configurations;
using Anemoi.BuildingBlock.Infrastructure.Filters;
using Anemoi.BuildingBlock.Infrastructure.HandlerConsumers;
using Anemoi.Contract.Notification;
using Anemoi.Notification.Application;
using Anemoi.Notification.Infrastructure.DataContext;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Notification.Infrastructure.Installers;

public sealed class MassTransitInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        var masstransitSetting = configuration.GetSection(nameof(MassTransitSetting)).Get<MassTransitSetting>()!;
        var (host, virtualHost, userName, password, _, _) = masstransitSetting;
        services.AddMassTransit(configurator =>
        {
            configurator.SetKebabCaseEndpointNameFormatter();
            configurator.AddEntityFrameworkOutbox<NotificationDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });
            configurator.AddConsumersFromNamespaceContaining<INotificationApplicationAssemblyMarker>();
            var serviceConsumer = ConsumersHelper
                .CreateDynamicConsumerHandlers<INotificationContractAssemblyMarker>("NotificationHandlersConsumer");
            configurator.AddConsumer(serviceConsumer);
            configurator.UsingRabbitMq((context, bus) =>
            {
                bus.Host(host, virtualHost, c =>
                {
                    c.Username(userName);
                    c.Password(password);
                });
                bus.UseConsumeFilter(typeof(RequestHeaderFilter<>), context);
                bus.ConfigureEndpoints(context);
            });
        });
    }
}
