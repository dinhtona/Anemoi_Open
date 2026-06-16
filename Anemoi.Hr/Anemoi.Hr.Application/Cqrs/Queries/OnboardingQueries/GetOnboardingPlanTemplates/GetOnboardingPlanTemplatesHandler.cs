using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingPlanTemplates;

public sealed class GetOnboardingPlanTemplatesHandler(
    ISqlRepository<OnboardingPlanTemplate> templateRepository,
    OnboardingMapper mapper)
    : IQueryHandler<GetOnboardingPlanTemplatesQuery, PaginationResponse<OnboardingPlanTemplateResponse>>
{
    public async Task<PaginationResponse<OnboardingPlanTemplateResponse>> Handle(
        GetOnboardingPlanTemplatesQuery request, CancellationToken cancellationToken)
    {
        var page = await templateRepository.GetManyByConditionWithPaginationAsync(
            x => string.IsNullOrEmpty(request.Status) || x.Status == request.Status,
            q => q.OrderByWithDynamic(request.SortedFieldName, x => x.Name,
                    request.SortedDirection ?? SortedDirection.Ascending)
                .Offset(request.GetSkip())
                .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<OnboardingPlanTemplateResponse>(
            page.Items.Select(mapper.ToResponse).ToList(),
            page.TotalRecord);
    }
}
