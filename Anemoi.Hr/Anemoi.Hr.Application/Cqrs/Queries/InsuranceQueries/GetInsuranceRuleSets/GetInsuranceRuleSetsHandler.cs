using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceRuleSets;

public sealed class GetInsuranceRuleSetsHandler(
    ISqlRepository<InsuranceRuleSet> repository,
    InsuranceMapper mapper)
    : IQueryHandler<GetInsuranceRuleSetsQuery, IReadOnlyCollection<InsuranceRuleSetResponse>>
{
    public async Task<IReadOnlyCollection<InsuranceRuleSetResponse>> Handle(
        GetInsuranceRuleSetsQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

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
            .Include(x => x.ContributionRules)
            .OrderBy(x => x.CountryCode)
            .ThenBy(x => x.InsuranceType)
            .ThenByDescending(x => x.Version)
            .ToListAsync(cancellationToken);

        return results.Select(mapper.ToInsuranceRuleSetResponse).ToList();
    }
}
