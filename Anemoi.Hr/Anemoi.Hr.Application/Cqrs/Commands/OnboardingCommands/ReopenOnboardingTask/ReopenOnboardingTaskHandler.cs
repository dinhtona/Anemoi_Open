using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ReopenOnboardingTask;

public sealed class ReopenOnboardingTaskHandler(
    ISqlRepository<OnboardingInstance> instanceRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<ReopenOnboardingTaskCommand, OneOf<OnboardingInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingInstanceResponse, ErrorDetailResponse>> Handle(
        ReopenOnboardingTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var instance = await instanceRepository.GetFirstByConditionAsync(
                x => x.Tasks.Any(t => t.Id == request.TaskId), null, cancellationToken);
            if (instance == null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTaskNotFound);

            if (instance.Status != OnboardingInstanceStatusCode.InProgress)
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingInstanceNotInProgress);

            if (!instance.ReopenTask(request.TaskId, request.ReopenedBy, request.Reason))
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTaskAlreadyReopened);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.ToResponse(instance);
        }
        catch (Exception exception)
        {
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
    }
}
