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

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ReopenOnboarding;

public sealed class ReopenOnboardingHandler(
    ISqlRepository<OnboardingInstance> instanceRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<ReopenOnboardingCommand, OneOf<OnboardingInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingInstanceResponse, ErrorDetailResponse>> Handle(
        ReopenOnboardingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var instance = await instanceRepository.GetFirstByConditionAsync(
                x => x.Id == request.InstanceId, null, cancellationToken);
            if (instance == null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingInstanceNotFound);

            if (!instance.Reopen(request.ReopenedBy))
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingInstanceInvalidStatus);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.ToResponse(instance);
        }
        catch (Exception exception)
        {
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
    }
}
