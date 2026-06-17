using System.Collections.Generic;
using System.Linq;

namespace Anemoi.Hr.Application.Services;

public interface INotificationActionHandlerRegistry
{
    INotificationActionHandler? GetHandler(string targetService);
}

public sealed class NotificationActionHandlerRegistry(
    IEnumerable<INotificationActionHandler> handlers) : INotificationActionHandlerRegistry
{
    public INotificationActionHandler? GetHandler(string targetService)
        => handlers.FirstOrDefault(h => h.TargetService == targetService);
}
