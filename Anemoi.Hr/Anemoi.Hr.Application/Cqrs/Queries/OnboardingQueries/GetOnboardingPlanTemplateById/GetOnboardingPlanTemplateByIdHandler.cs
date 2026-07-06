using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingPlanTemplateById;

public sealed class GetOnboardingPlanTemplateByIdHandler(
    ISqlRepository<OnboardingPlanTemplate> templateRepository,
    OnboardingMapper mapper)
    : IQueryHandler<GetOnboardingPlanTemplateByIdQuery, OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>> Handle(
        GetOnboardingPlanTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null, cancellationToken);

        if (template is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateNotFound);

        return mapper.ToResponse(template);
    }
}
