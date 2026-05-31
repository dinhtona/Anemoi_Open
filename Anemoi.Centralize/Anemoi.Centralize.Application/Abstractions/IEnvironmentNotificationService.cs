using System.Threading.Tasks;

namespace Anemoi.Centralize.Application.Abstractions;

public interface IEnvironmentNotificationService
{
    Task NotifyEnvironmentActivityAsync(
        string serviceNameResourceKey,
        string actionResourceKey,
        params object[] arguments);
}
