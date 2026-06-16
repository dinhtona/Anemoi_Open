using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentOverview;

public sealed class GetRecruitmentOverviewHandler(
    ISqlRepository<JobRequisition> requisitionRepository,
    ISqlRepository<JobPosting> postingRepository,
    ISqlRepository<Candidate> candidateRepository,
    ISqlRepository<CandidateApplication> applicationRepository,
    ISqlRepository<InterviewSchedule> interviewRepository)
    : IQueryHandler<GetRecruitmentOverviewQuery, RecruitmentOverviewResponse>
{
    public async Task<RecruitmentOverviewResponse> Handle(
        GetRecruitmentOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate?.ToDateTime(new TimeOnly(0, 0));
        var toDate = request.ToDate?.ToDateTime(new TimeOnly(23, 59, 59));

        var openRequisitions = await requisitionRepository.CountByConditionAsync(
            x => (x.Status == RequisitionStatusCode.Submitted ||
                  x.Status == RequisitionStatusCode.Approved) &&
                 (!fromDate.HasValue || x.CreatedAt >= fromDate) &&
                 (!toDate.HasValue || x.CreatedAt <= toDate), token: cancellationToken);

        var approvedRequisitions = await requisitionRepository.CountByConditionAsync(
            x => x.Status == RequisitionStatusCode.Approved &&
                 (!fromDate.HasValue || x.CreatedAt >= fromDate) &&
                 (!toDate.HasValue || x.CreatedAt <= toDate), token: cancellationToken);

        var publishedPostings = await postingRepository.CountByConditionAsync(
            x => x.Status == JobPostingStatusCode.Published &&
                 (!fromDate.HasValue || x.CreatedAt >= fromDate) &&
                 (!toDate.HasValue || x.CreatedAt <= toDate), token: cancellationToken);

        var activeCandidates = await candidateRepository.CountByConditionAsync(
            x => x.Status == CandidateStatusCode.Active &&
                 (!fromDate.HasValue || x.CreatedAt >= fromDate) &&
                 (!toDate.HasValue || x.CreatedAt <= toDate), token: cancellationToken);

        var applicationsInPipeline = await applicationRepository.CountByConditionAsync(
            x => (x.CurrentStage == CandidateApplicationStageCode.Applied ||
                  x.CurrentStage == CandidateApplicationStageCode.Screening ||
                  x.CurrentStage == CandidateApplicationStageCode.Interview ||
                  x.CurrentStage == CandidateApplicationStageCode.Offer) &&
                 (!fromDate.HasValue || x.AppliedAt >= fromDate) &&
                 (!toDate.HasValue || x.AppliedAt <= toDate), token: cancellationToken);

        var hiredCandidates = await applicationRepository.CountByConditionAsync(
            x => x.CurrentStage == CandidateApplicationStageCode.Hired &&
                 (!fromDate.HasValue || x.AppliedAt >= fromDate) &&
                 (!toDate.HasValue || x.AppliedAt <= toDate), token: cancellationToken);

        var convertedCandidates = await candidateRepository.CountByConditionAsync(
            x => x.EmployeeId != null &&
                 (!fromDate.HasValue || x.ConvertedAt >= fromDate) &&
                 (!toDate.HasValue || x.ConvertedAt <= toDate), token: cancellationToken);

        var pendingInterviews = await interviewRepository.CountByConditionAsync(
            x => x.Result == InterviewResultCode.Pending &&
                 (!fromDate.HasValue || x.CreatedAt >= fromDate) &&
                 (!toDate.HasValue || x.CreatedAt <= toDate), token: cancellationToken);

        return new RecruitmentOverviewResponse(
            (int)openRequisitions,
            (int)approvedRequisitions,
            (int)publishedPostings,
            (int)activeCandidates,
            (int)applicationsInPipeline,
            (int)hiredCandidates,
            (int)convertedCandidates,
            (int)pendingInterviews
        );
    }
}
