using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CancelOnboarding;

public sealed class CancelOnboardingHandler(
    ISqlRepository<OnboardingInstance> instanceRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<CancelOnboardingCommand, OneOf<OnboardingInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingInstanceResponse, ErrorDetailResponse>> Handle(
        CancelOnboardingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var instance = await instanceRepository.GetFirstByConditionAsync(
                x => x.Id == request.InstanceId, null, cancellationToken);
            if (instance == null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingInstanceNotFound);

            if (!instance.Cancel(request.CancelledBy))
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingInstanceAlreadyCancelled);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.ToResponse(instance);
        }
        catch (Exception exception)
        {
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
    }
}
