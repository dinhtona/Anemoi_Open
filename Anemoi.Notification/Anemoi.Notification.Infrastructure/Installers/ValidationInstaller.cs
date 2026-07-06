using Anemoi.BuildingBlock.Application.Abstractions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;

namespace Anemoi.Notification.Infrastructure.Installers;

public sealed class ValidationInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(CreateNotificationCommandValidator).Assembly);
    }
}
