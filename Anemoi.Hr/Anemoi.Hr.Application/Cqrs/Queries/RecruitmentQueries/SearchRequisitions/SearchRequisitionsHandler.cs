using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchRequisitions;

public sealed class SearchRequisitionsHandler(
    ISqlRepository<JobRequisition> requisitionRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<SearchRequisitionsQuery, PaginationResponse<JobRequisitionResponse>>
{
    public async Task<PaginationResponse<JobRequisitionResponse>> Handle(
        SearchRequisitionsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await requisitionRepository.GetManyByConditionWithPaginationAsync(
            x => (string.IsNullOrEmpty(request.SearchTerm) ||
                    x.RequisitionCode.ToLower().Contains(request.SearchTerm.ToLower()) ||
                    x.Title.ToLower().Contains(request.SearchTerm.ToLower())) &&
                (string.IsNullOrEmpty(request.Status) || x.Status == request.Status) &&
                (request.DepartmentId == null || x.DepartmentId == request.DepartmentId),
            q => q.OrderByWithDynamic(request.SortedFieldName, x => x.CreatedAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                .Offset(request.GetSkip())
                .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<JobRequisitionResponse>(
            page.Items.Select(mapper.ToResponse).ToList(),
            page.TotalRecord);
    }
}
