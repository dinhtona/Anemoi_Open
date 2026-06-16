using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchJobPostings;

public sealed class SearchJobPostingsHandler(
    ISqlRepository<JobPosting> postingRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<SearchJobPostingsQuery, PaginationResponse<JobPostingResponse>>
{
    public async Task<PaginationResponse<JobPostingResponse>> Handle(
        SearchJobPostingsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await postingRepository.GetManyByConditionWithPaginationAsync(
            x => (string.IsNullOrEmpty(request.SearchTerm) ||
                    x.PostingTitle.ToLower().Contains(request.SearchTerm.ToLower())) &&
                (string.IsNullOrEmpty(request.Status) || x.Status == request.Status) &&
                (request.JobRequisitionId == null || x.JobRequisitionId == request.JobRequisitionId),
            q => q.OrderByWithDynamic(request.SortedFieldName, x => x.CreatedAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                .Offset(request.GetSkip())
                .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<JobPostingResponse>(
            page.Items.Select(mapper.ToResponse).ToList(),
            page.TotalRecord);
    }
}
