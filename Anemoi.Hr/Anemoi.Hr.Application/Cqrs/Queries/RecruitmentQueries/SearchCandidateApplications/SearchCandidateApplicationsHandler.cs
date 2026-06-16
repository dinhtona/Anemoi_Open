using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchCandidateApplications;

public sealed class SearchCandidateApplicationsHandler(
    ISqlRepository<CandidateApplication> applicationRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<SearchCandidateApplicationsQuery, PaginationResponse<CandidateApplicationResponse>>
{
    public async Task<PaginationResponse<CandidateApplicationResponse>> Handle(
        SearchCandidateApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await applicationRepository.GetManyByConditionWithPaginationAsync(
            x => (string.IsNullOrEmpty(request.CurrentStage) || x.CurrentStage == request.CurrentStage) &&
                (request.CandidateId == null || x.CandidateId == request.CandidateId) &&
                (request.JobPostingId == null || x.JobPostingId == request.JobPostingId),
            q => q.Include(x => x.Candidate)
                .Include(x => x.JobPosting).ThenInclude(x => x.JobRequisition)
                .OrderByWithDynamic(request.SortedFieldName, x => x.AppliedAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                .Offset(request.GetSkip())
                .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<CandidateApplicationResponse>(
            page.Items.Select(mapper.ToResponse).ToList(),
            page.TotalRecord);
    }
}
