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

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchHiringDecisions;

public sealed class SearchHiringDecisionsHandler(
    ISqlRepository<HiringDecision> decisionRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<SearchHiringDecisionsQuery, PaginationResponse<HiringDecisionResponse>>
{
    public async Task<PaginationResponse<HiringDecisionResponse>> Handle(
        SearchHiringDecisionsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await decisionRepository.GetManyByConditionWithPaginationAsync(
            x => string.IsNullOrEmpty(request.Decision) || x.Decision == request.Decision,
            q => q.Include(x => x.CandidateApplication).ThenInclude(x => x.Candidate)
                .Include(x => x.CandidateApplication).ThenInclude(x => x.JobPosting)
                .OrderByWithDynamic(request.SortedFieldName, x => x.DecidedAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                .Offset(request.GetSkip())
                .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<HiringDecisionResponse>(
            page.Items.Select(mapper.ToResponse).ToList(),
            page.TotalRecord);
    }
}
