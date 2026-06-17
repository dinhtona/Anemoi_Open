using System.Threading.Tasks;
using Anemoi.Contract.Hr;

namespace Anemoi.Hr.Application.Services;

public interface INotificationActionHandler
{
    string TargetService { get; }
    Task<NotificationActionResult> HandleAsync(NotificationActionCommand command);
}
