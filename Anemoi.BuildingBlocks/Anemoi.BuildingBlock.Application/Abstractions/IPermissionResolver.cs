using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.BuildingBlock.Application.Abstractions;

public interface IPermissionResolver
{
    ValueTask<bool> HasPermissionAsync(IEnumerable<string> roleGroups, string permission,
        CancellationToken cancellationToken = default);

    void InvalidateRoleGroup(string roleGroupName);
    void InvalidateAll();
}
