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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateInterviewSchedule;

public sealed class CreateInterviewScheduleHandler(
    ISqlRepository<CandidateApplication> applicationRepository,
    ISqlRepository<InterviewSchedule> interviewRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<CreateInterviewScheduleCommand, OneOf<InterviewScheduleResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<InterviewScheduleResponse, ErrorDetailResponse>> Handle(
        CreateInterviewScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var app = await applicationRepository.GetQueryable()
            .Include(x => x.Candidate)
            .Include(x => x.JobPosting)
            .FirstOrDefaultAsync(x => x.Id == request.CandidateApplicationId, cancellationToken);
        if (app is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ApplicationNotFound);
        if (app.CurrentStage != CandidateApplicationStageCode.Interview)
            return HrErrorResponses.Create(HrBusinessErrorCodes.InvalidApplicationStage);
        if (request.DurationMinutes <= 0)
            return HrErrorResponses.Create(HrBusinessErrorCodes.InterviewDurationInvalid);

        var now = DateTime.UtcNow;
        var interview = new InterviewSchedule
        {
            Id = new InterviewScheduleId(IdGenerator.NextGuid()),
            CandidateApplicationId = request.CandidateApplicationId,
            InterviewType = request.InterviewType,
            ScheduledAt = request.ScheduledAt,
            DurationMinutes = request.DurationMinutes,
            InterviewerEmployeeId = request.InterviewerEmployeeId,
            Notes = request.Notes,
            CreatedBy = request.CreatedBy,
            CreatedAt = now,
            UpdatedBy = request.CreatedBy,
            UpdatedAt = now
        };

        var createResult = await interviewRepository.CreateOneAsync(interview, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(interview);
    }
}
