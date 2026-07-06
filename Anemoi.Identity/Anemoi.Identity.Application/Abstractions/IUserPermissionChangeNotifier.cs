using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Contract.Identity.ModelIds;

namespace Anemoi.Identity.Application.Abstractions;

public interface IUserPermissionChangeNotifier
{
    Task PublishAsync(IEnumerable<UserId> userIds, CancellationToken cancellationToken);
}
