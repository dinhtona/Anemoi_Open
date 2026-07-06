using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Taxation;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxRuleSetDetail;

public sealed class GetTaxRuleSetDetailHandler(
    ISqlRepository<TaxRuleSet> repository,
    TaxationMapper mapper)
    : IQueryHandler<GetTaxRuleSetDetailQuery, OneOf<TaxRuleSetResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<TaxRuleSetResponse, ErrorDetailResponse>> Handle(
        GetTaxRuleSetDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var guid))
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetNotFound);
        }

        var ruleSetId = new TaxRuleSetId(guid);

        var ruleSet = await repository.GetQueryable(x => x.Id == ruleSetId)
            .Include(x => x.Brackets)
            .Include(x => x.DeductionRules)
            .FirstOrDefaultAsync(cancellationToken);

        if (ruleSet is null)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetNotFound);
        }

        // Sort brackets before returning
        ruleSet.Brackets = ruleSet.Brackets.OrderBy(b => b.SortOrder).ToList();
        ruleSet.DeductionRules = ruleSet.DeductionRules.OrderBy(d => d.DeductionType).ToList();

        return mapper.ToTaxRuleSetResponse(ruleSet);
    }
}
