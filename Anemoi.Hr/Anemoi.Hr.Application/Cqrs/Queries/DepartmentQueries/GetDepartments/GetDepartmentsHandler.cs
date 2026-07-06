using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;

namespace Anemoi.Hr.Application.Cqrs.Queries.DepartmentQueries.GetDepartments;

public sealed class GetDepartmentsHandler(ISqlRepository<Department> repository, EmployeeMapper mapper)
    : IQueryHandler<GetDepartmentsQuery, PaginationResponse<DepartmentResponse>>
{
    public async Task<PaginationResponse<DepartmentResponse>> Handle(GetDepartmentsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await repository.GetManyByConditionWithPaginationAsync(
            x => (string.IsNullOrEmpty(request.SearchKey) || 
                  x.Code.Contains(request.SearchKey) ||
                  x.Name.Contains(request.SearchKey)) &&
                 (request.IsActive == null || x.IsActive == request.IsActive),
            q => q.OrderByWithDynamic(request.SortedFieldName, x => x.CreatedAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                  .Offset(request.GetSkip())
                  .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<DepartmentResponse>(
            page.Items.Select(mapper.ToDepartmentResponse).ToList(),
            page.TotalRecord);
    }
}
