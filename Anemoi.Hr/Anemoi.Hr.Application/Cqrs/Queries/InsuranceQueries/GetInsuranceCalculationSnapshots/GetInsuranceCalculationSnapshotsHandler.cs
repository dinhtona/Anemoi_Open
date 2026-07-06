using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceCalculationSnapshots;

public sealed class GetInsuranceCalculationSnapshotsHandler(
    ISqlRepository<InsuranceCalculationSnapshot> repository,
    InsuranceMapper mapper)
    : IQueryHandler<GetInsuranceCalculationSnapshotsQuery, IReadOnlyCollection<InsuranceCalculationSnapshotResponse>>
{
    public async Task<IReadOnlyCollection<InsuranceCalculationSnapshotResponse>> Handle(
        GetInsuranceCalculationSnapshotsQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        if (!string.IsNullOrWhiteSpace(request.EmployeeId) && Guid.TryParse(request.EmployeeId, out var empGuid))
        {
            var empId = new EmployeeId(empGuid);
            query = query.Where(x => x.EmployeeId == empId);
        }

        if (!string.IsNullOrWhiteSpace(request.InsuranceType))
        {
            var type = request.InsuranceType.Trim().ToUpperInvariant();
            query = query.Where(x => x.InsuranceType == type);
        }

        if (!string.IsNullOrWhiteSpace(request.SourceModule))
        {
            var source = request.SourceModule.Trim();
            query = query.Where(x => x.SourceModule == source);
        }

        if (!string.IsNullOrWhiteSpace(request.SourceReferenceId))
        {
            query = query.Where(x => x.SourceReferenceId == request.SourceReferenceId);
        }

        var results = await query
            .OrderByDescending(x => x.CalculatedAt)
            .ToListAsync(cancellationToken);

        return results.Select(mapper.ToInsuranceCalculationSnapshotResponse).ToList();
    }
}
