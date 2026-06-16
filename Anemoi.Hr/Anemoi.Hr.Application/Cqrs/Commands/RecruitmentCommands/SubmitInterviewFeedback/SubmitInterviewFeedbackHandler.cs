using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitInterviewFeedback;

public sealed class SubmitInterviewFeedbackHandler(
    ISqlRepository<InterviewSchedule> interviewRepository,
    ISqlRepository<InterviewFeedback> feedbackRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<SubmitInterviewFeedbackCommand, OneOf<InterviewFeedbackResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<InterviewFeedbackResponse, ErrorDetailResponse>> Handle(
        SubmitInterviewFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        var interview = await interviewRepository.ExistByConditionAsync(
            x => x.Id == request.InterviewScheduleId, cancellationToken);
        if (!interview)
            return HrErrorResponses.Create(HrBusinessErrorCodes.InterviewNotFound);

        var existing = await feedbackRepository.ExistByConditionAsync(
            x => x.InterviewScheduleId == request.InterviewScheduleId &&
                 x.InterviewerEmployeeId == request.InterviewerEmployeeId, cancellationToken);
        if (existing)
            return HrErrorResponses.Create(HrBusinessErrorCodes.InterviewFeedbackAlreadyExists);

        var now = DateTime.UtcNow;
        var feedback = InterviewFeedback.Create(
            new InterviewFeedbackId(IdGenerator.NextGuid()),
            request.InterviewScheduleId,
            request.InterviewerEmployeeId,
            request.Rating,
            request.Strengths,
            request.Concerns,
            request.Recommendation,
            now);

        if (feedback is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ValAmountMustBePositive);

        var createResult = await feedbackRepository.CreateOneAsync(feedback, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(feedback);
    }
}
