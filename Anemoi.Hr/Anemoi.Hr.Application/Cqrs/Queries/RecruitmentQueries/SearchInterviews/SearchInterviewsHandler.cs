using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchInterviews;

public sealed class SearchInterviewsHandler(
    ISqlRepository<InterviewSchedule> interviewRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<SearchInterviewsQuery, PaginationResponse<InterviewScheduleResponse>>
{
    public async Task<PaginationResponse<InterviewScheduleResponse>> Handle(
        SearchInterviewsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await interviewRepository.GetManyByConditionWithPaginationAsync(
            x => (string.IsNullOrEmpty(request.Result) || x.Result == request.Result) &&
                (request.InterviewerEmployeeId == null || x.InterviewerEmployeeId == request.InterviewerEmployeeId) &&
                (request.FromDate == null || x.ScheduledAt >= request.FromDate) &&
                (request.ToDate == null || x.ScheduledAt <= request.ToDate),
            q => q.Include(x => x.CandidateApplication).ThenInclude(x => x.Candidate)
                .Include(x => x.CandidateApplication).ThenInclude(x => x.JobPosting)
                .Include(x => x.Interviewer)
                .OrderByWithDynamic(request.SortedFieldName, x => x.ScheduledAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                .Offset(request.GetSkip())
                .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<InterviewScheduleResponse>(
            page.Items.Select(mapper.ToResponse).ToList(),
            page.TotalRecord);
    }
}
