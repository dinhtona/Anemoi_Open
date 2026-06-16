using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
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
        var openRequisitions = await requisitionRepository.CountByConditionAsync(
            x => x.Status == RequisitionStatusCode.Submitted ||
                 x.Status == RequisitionStatusCode.Approved, token: cancellationToken);

        var approvedRequisitions = await requisitionRepository.CountByConditionAsync(
            x => x.Status == RequisitionStatusCode.Approved, token: cancellationToken);

        var publishedPostings = await postingRepository.CountByConditionAsync(
            x => x.Status == JobPostingStatusCode.Published, token: cancellationToken);

        var activeCandidates = await candidateRepository.CountByConditionAsync(
            x => x.Status == CandidateStatusCode.Active, token: cancellationToken);

        var applicationsInPipeline = await applicationRepository.CountByConditionAsync(
            x => x.CurrentStage == CandidateApplicationStageCode.Applied ||
                 x.CurrentStage == CandidateApplicationStageCode.Screening ||
                 x.CurrentStage == CandidateApplicationStageCode.Interview ||
                 x.CurrentStage == CandidateApplicationStageCode.Offer, token: cancellationToken);

        var hiredCandidates = await applicationRepository.CountByConditionAsync(
            x => x.CurrentStage == CandidateApplicationStageCode.Hired, token: cancellationToken);

        var convertedCandidates = await candidateRepository.CountByConditionAsync(
            x => x.EmployeeId != null, token: cancellationToken);

        var pendingInterviews = await interviewRepository.CountByConditionAsync(
            x => x.Result == InterviewResultCode.Pending, token: cancellationToken);

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
