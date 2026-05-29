using System.Threading.Tasks;

namespace Anemoi.Centralize.Application.Abstractions;

public interface IEnvironmentNotificationService
{
    Task NotifyEnvironmentActivityAsync(string serviceName, string actionDetails);
}
