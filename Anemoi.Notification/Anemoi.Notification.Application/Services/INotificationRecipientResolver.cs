#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Notification.Application.Services;

public interface INotificationRecipientResolver
{
    Task<string?> ResolveUserIdByEmployeeId(string employeeId, CancellationToken cancellationToken = default);
    Task<string?> ResolveApproverUserIdByEmployeeId(string approverEmployeeId, CancellationToken cancellationToken = default);
}
