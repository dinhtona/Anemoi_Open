using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Abstractions;

public sealed class OvertimeSnapshotProvider(
    ISqlRepository<OvertimeRequest> overtimeRequestRepository,
    OvertimeMapper mapper)
    : IOvertimeSnapshotProvider
{
    public async Task<IEnumerable<OvertimeRequestResponse>> GetApprovedOvertimeRequestsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        var query = overtimeRequestRepository.GetQueryable().AsNoTracking()
            .Where(x => x.Status == OvertimeStatusCode.Approved && x.ApprovedAt >= fromDate && x.ApprovedAt <= toDate);

        var overtimeRequests = await query.ToListAsync(cancellationToken);
        return overtimeRequests.Select(mapper.ToOvertimeRequestResponse).ToList();
    }
}
