using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.DeactivateOnboardingPlanTemplate;

public sealed class DeactivateOnboardingPlanTemplateHandler(
    ISqlRepository<OnboardingPlanTemplate> templateRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<DeactivateOnboardingPlanTemplateCommand, OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>> Handle(
        DeactivateOnboardingPlanTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (template is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateNotFound);

        if (!template.Deactivate())
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateInUse);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(template);
    }
}
