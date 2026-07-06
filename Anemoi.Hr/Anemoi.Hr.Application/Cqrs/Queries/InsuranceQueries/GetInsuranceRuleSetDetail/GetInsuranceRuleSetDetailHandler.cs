using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceRuleSetDetail;

public sealed class GetInsuranceRuleSetDetailHandler(
    ISqlRepository<InsuranceRuleSet> repository,
    InsuranceMapper mapper)
    : IQueryHandler<GetInsuranceRuleSetDetailQuery, OneOf<InsuranceRuleSetResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<InsuranceRuleSetResponse, ErrorDetailResponse>> Handle(
        GetInsuranceRuleSetDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var guid))
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.InsuranceRuleSetNotFound);
        }

        var ruleSetId = new InsuranceRuleSetId(guid);

        var ruleSet = await repository.GetQueryable(x => x.Id == ruleSetId)
            .Include(x => x.ContributionRules)
            .FirstOrDefaultAsync(cancellationToken);

        if (ruleSet is null)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.InsuranceRuleSetNotFound);
        }

        ruleSet.ContributionRules = ruleSet.ContributionRules.OrderBy(r => r.SortOrder).ToList();

        return mapper.ToInsuranceRuleSetResponse(ruleSet);
    }
}
