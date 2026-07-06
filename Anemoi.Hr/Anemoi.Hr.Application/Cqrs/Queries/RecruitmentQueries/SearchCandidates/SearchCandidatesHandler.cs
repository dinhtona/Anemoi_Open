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

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchCandidates;

public sealed class SearchCandidatesHandler(
    ISqlRepository<Candidate> candidateRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<SearchCandidatesQuery, PaginationResponse<CandidateResponse>>
{
    public async Task<PaginationResponse<CandidateResponse>> Handle(
        SearchCandidatesQuery request,
        CancellationToken cancellationToken)
    {
        var page = await candidateRepository.GetManyByConditionWithPaginationAsync(
            x => (string.IsNullOrEmpty(request.SearchTerm) ||
                    x.FullName.ToLower().Contains(request.SearchTerm.ToLower()) ||
                    x.Email.ToLower().Contains(request.SearchTerm.ToLower()) ||
                    (!string.IsNullOrEmpty(x.PhoneNumber) && x.PhoneNumber.Contains(request.SearchTerm))) &&
                (string.IsNullOrEmpty(request.Status) || x.Status == request.Status) &&
                (string.IsNullOrEmpty(request.Source) || x.Source == request.Source),
            q => q.OrderByWithDynamic(request.SortedFieldName, x => x.CreatedAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                .Offset(request.GetSkip())
                .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<CandidateResponse>(
            page.Items.Select(mapper.ToResponse).ToList(),
            page.TotalRecord);
    }
}
