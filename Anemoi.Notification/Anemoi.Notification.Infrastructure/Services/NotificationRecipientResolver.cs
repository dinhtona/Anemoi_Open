#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Anemoi.Contract.Hr.Queries;
using Anemoi.Notification.Application.Services;
using MassTransit;

namespace Anemoi.Notification.Infrastructure.Services;

public sealed class NotificationRecipientResolver(IRequestClient<ResolveEmployeeUserQuery> requestClient)
    : INotificationRecipientResolver
{
    public async Task<string?> ResolveUserIdByEmployeeId(string employeeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(employeeId)) return null;
        try
        {
            var response = await requestClient.GetResponse<ResolveEmployeeUserResponse>(
                new ResolveEmployeeUserQuery(employeeId), cancellationToken);
            return response.Message.UserId;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> ResolveApproverUserIdByEmployeeId(string approverEmployeeId, CancellationToken cancellationToken = default)
    {
        return await ResolveUserIdByEmployeeId(approverEmployeeId, cancellationToken);
    }
}
