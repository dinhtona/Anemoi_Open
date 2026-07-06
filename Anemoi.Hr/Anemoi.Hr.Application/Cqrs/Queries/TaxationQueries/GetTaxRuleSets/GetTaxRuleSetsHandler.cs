using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Taxation;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxRuleSets;

public sealed class GetTaxRuleSetsHandler(
    ISqlRepository<TaxRuleSet> repository,
    TaxationMapper mapper)
    : IQueryHandler<GetTaxRuleSetsQuery, IReadOnlyCollection<TaxRuleSetResponse>>
{
    public async Task<IReadOnlyCollection<TaxRuleSetResponse>> Handle(
        GetTaxRuleSetsQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        if (!string.IsNullOrWhiteSpace(request.CountryCode))
        {
            var country = request.CountryCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.CountryCode == country);
        }

        if (!string.IsNullOrWhiteSpace(request.TaxType))
        {
            var type = request.TaxType.Trim().ToUpperInvariant();
            query = query.Where(x => x.TaxType == type);
        }

        var results = await query
            .Include(x => x.Brackets)
            .Include(x => x.DeductionRules)
            .OrderBy(x => x.CountryCode)
            .ThenBy(x => x.TaxType)
            .ThenByDescending(x => x.Version)
            .ToListAsync(cancellationToken);

        return results.Select(mapper.ToTaxRuleSetResponse).ToList();
    }
}
