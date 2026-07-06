using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MarkInterviewFailed;

public sealed class MarkInterviewFailedHandler(
    ISqlRepository<InterviewSchedule> interviewRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<MarkInterviewFailedCommand, OneOf<InterviewScheduleResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<InterviewScheduleResponse, ErrorDetailResponse>> Handle(
        MarkInterviewFailedCommand request,
        CancellationToken cancellationToken)
    {
        var interview = await interviewRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (interview is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.InterviewNotFound);

        if (!interview.MarkResult(InterviewResultCode.Failed, request.UpdatedBy, DateTime.UtcNow))
            return HrErrorResponses.Create(HrBusinessErrorCodes.InterviewAlreadyCompleted);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(interview);
    }
}
