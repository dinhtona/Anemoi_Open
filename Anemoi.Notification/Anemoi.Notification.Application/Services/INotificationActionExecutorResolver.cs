using System.Collections.Generic;
using System.Linq;

namespace Anemoi.Notification.Application.Services;

public interface INotificationActionExecutorResolver
{
    INotificationActionExecutor? Resolve(string actionCode);
}

public sealed class NotificationActionExecutorResolver(
    IEnumerable<INotificationActionExecutor> executors
) : INotificationActionExecutorResolver
{
    public INotificationActionExecutor? Resolve(string actionCode)
        => executors.FirstOrDefault(e => e.ActionCode == actionCode);
}
