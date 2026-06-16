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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateCandidateApplication;

public sealed class CreateCandidateApplicationHandler(
    ISqlRepository<Candidate> candidateRepository,
    ISqlRepository<JobPosting> postingRepository,
    ISqlRepository<CandidateApplication> applicationRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<CreateCandidateApplicationCommand, OneOf<CandidateApplicationResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CandidateApplicationResponse, ErrorDetailResponse>> Handle(
        CreateCandidateApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var candidate = await candidateRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.CandidateId, cancellationToken);
        if (candidate is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateNotFound);
        if (candidate.Status != CandidateStatusCode.Active)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateNotActive);

        var posting = await postingRepository.GetQueryable()
            .Include(x => x.JobRequisition)
            .FirstOrDefaultAsync(x => x.Id == request.JobPostingId, cancellationToken);
        if (posting is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.JobPostingNotFound);
        if (posting.Status != JobPostingStatusCode.Published)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PostingNotPublished);

        var duplicate = await applicationRepository.ExistByConditionAsync(
            x => x.CandidateId == request.CandidateId && x.JobPostingId == request.JobPostingId,
            cancellationToken);
        if (duplicate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ApplicationDuplicate);

        var now = DateTime.UtcNow;
        var applicationId = new CandidateApplicationId(IdGenerator.NextGuid());
        var application = CandidateApplication.Create(
            applicationId,
            request.CandidateId,
            request.JobPostingId,
            now);

        application.MoveToStage(
            CandidateApplicationStageCode.Applied,
            new CandidateApplicationStageHistoryId(IdGenerator.NextGuid()),
            request.CreatedBy,
            now,
            null);

        var createResult = await applicationRepository.CreateOneAsync(application, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(application);
    }
}
