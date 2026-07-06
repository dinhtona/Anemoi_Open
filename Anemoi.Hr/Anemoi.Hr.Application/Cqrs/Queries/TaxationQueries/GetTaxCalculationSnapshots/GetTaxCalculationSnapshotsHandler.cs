using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Taxation;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxCalculationSnapshots;

public sealed class GetTaxCalculationSnapshotsHandler(
    ISqlRepository<TaxCalculationSnapshot> repository,
    TaxationMapper mapper)
    : IQueryHandler<GetTaxCalculationSnapshotsQuery, IReadOnlyCollection<TaxCalculationSnapshotResponse>>
{
    public async Task<IReadOnlyCollection<TaxCalculationSnapshotResponse>> Handle(
        GetTaxCalculationSnapshotsQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        if (!string.IsNullOrWhiteSpace(request.EmployeeId) && Guid.TryParse(request.EmployeeId, out var empGuid))
        {
            var empId = new EmployeeId(empGuid);
            query = query.Where(x => x.EmployeeId == empId);
        }

        if (!string.IsNullOrWhiteSpace(request.SourceModule))
        {
            var source = request.SourceModule.Trim();
            query = query.Where(x => x.SourceModule == source);
        }

        if (request.SourceReferenceId.HasValue)
        {
            query = query.Where(x => x.SourceReferenceId == request.SourceReferenceId.Value);
        }

        var results = await query
            .OrderByDescending(x => x.CalculatedAt)
            .ToListAsync(cancellationToken);

        return results.Select(mapper.ToTaxCalculationSnapshotResponse).ToList();
    }
}
