using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Abstractions;

public interface IOvertimeSnapshotProvider
{
    Task<IEnumerable<OvertimeRequestResponse>> GetApprovedOvertimeRequestsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken);
}
