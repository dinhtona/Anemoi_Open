using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceContributionReport;

public sealed class GetInsuranceContributionReportHandler(
    ISqlRepository<InsuranceCalculationSnapshot> repository,
    InsuranceMapper mapper)
    : IQueryHandler<GetInsuranceContributionReportQuery, IReadOnlyCollection<InsuranceCalculationSnapshotResponse>>
{
    public async Task<IReadOnlyCollection<InsuranceCalculationSnapshotResponse>> Handle(
        GetInsuranceContributionReportQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(x =>
            x.CalculationPeriodStart >= request.PeriodStart &&
            x.CalculationPeriodEnd <= request.PeriodEnd);

        if (!string.IsNullOrWhiteSpace(request.CountryCode))
        {
            var country = request.CountryCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.CountryCode == country);
        }

        if (!string.IsNullOrWhiteSpace(request.InsuranceType))
        {
            var type = request.InsuranceType.Trim().ToUpperInvariant();
            query = query.Where(x => x.InsuranceType == type);
        }

        var results = await query
            .OrderByDescending(x => x.CalculatedAt)
            .ToListAsync(cancellationToken);

        return results.Select(mapper.ToInsuranceCalculationSnapshotResponse).ToList();
    }
}
