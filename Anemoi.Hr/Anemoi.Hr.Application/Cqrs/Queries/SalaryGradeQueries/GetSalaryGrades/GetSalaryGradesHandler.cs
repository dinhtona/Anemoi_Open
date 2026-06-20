using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;

namespace Anemoi.Hr.Application.Cqrs.Queries.SalaryGradeQueries.GetSalaryGrades;

public sealed class GetSalaryGradesHandler(ISqlRepository<SalaryGrade> repository, CompensationMapper mapper)
    : IQueryHandler<GetSalaryGradesQuery, PaginationResponse<SalaryGradeResponse>>
{
    public async Task<PaginationResponse<SalaryGradeResponse>> Handle(GetSalaryGradesQuery request,
        CancellationToken cancellationToken)
    {
        var page = await repository.GetManyByConditionWithPaginationAsync(
            x => (string.IsNullOrEmpty(request.SearchKey) ||
                  x.GradeCode.Contains(request.SearchKey) ||
                  x.Name.Contains(request.SearchKey)) &&
                 (request.IsActive == null || x.IsActive == request.IsActive),
            q => q.OrderByWithDynamic(request.SortedFieldName, x => x.CreatedAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                  .Offset(request.GetSkip())
                  .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<SalaryGradeResponse>(
            page.Items.Select(mapper.ToSalaryGradeResponse).ToList(),
            page.TotalRecord);
    }
}
