using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;
using Anemoi.Hr.Domain.Onboarding.Events;
using MediatR;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ForceCompleteOnboarding;

public sealed class ForceCompleteOnboardingHandler(
    ISqlRepository<OnboardingInstance> instanceRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper,
    IMediator mediator)
    : ICommandHandler<ForceCompleteOnboardingCommand, OneOf<OnboardingInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingInstanceResponse, ErrorDetailResponse>> Handle(
        ForceCompleteOnboardingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var instance = await instanceRepository.GetFirstByConditionAsync(
                x => x.Id == request.InstanceId, null, cancellationToken);
            if (instance == null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingInstanceNotFound);

            if (instance.Status != OnboardingInstanceStatusCode.InProgress)
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingInstanceNotInProgress);

            if (!instance.ForceComplete(request.CompletedBy, request.Reason))
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingInstanceAlreadyCompleted);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (instance.Status == OnboardingInstanceStatusCode.Completed)
                await mediator.Publish(new OnboardingInstanceCompletedDomainEvent(
                    InstanceId: instance.Id,
                    EmployeeId: instance.EmployeeId,
                    CompletedBy: request.CompletedBy,
                    CompletionType: "ForceComplete"), cancellationToken);

            return mapper.ToResponse(instance);
        }
        catch (Exception exception)
        {
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
    }
}
