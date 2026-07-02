using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployees;

public sealed class GetEmployeesHandler(ISqlRepository<Employee> repository, EmployeeMapper mapper)
    : IQueryHandler<GetEmployeesQuery, PaginationResponse<EmployeeResponse>>
{
    public async Task<PaginationResponse<EmployeeResponse>> Handle(GetEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var page = await repository.GetManyByConditionWithPaginationAsync(
            x => (string.IsNullOrEmpty(request.SearchKey) || 
                  x.EmployeeCode.Contains(request.SearchKey) ||
                  (x.FirstName + " " + x.LastName).Contains(request.SearchKey) ||
                  x.WorkEmail.Contains(request.SearchKey)) &&
                 (request.DepartmentId == null || x.PrimaryDepartmentId == request.DepartmentId) &&
                 (request.PositionId == null || x.PrimaryPositionId == request.PositionId),
            q => q.Include(x => x.PrimaryDepartment)
                  .Include(x => x.PrimaryPosition)
                  .Include(x => x.DirectManager)
                  .OrderByWithDynamic(request.SortedFieldName, x => x.CreatedAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                  .Offset(request.GetSkip())
                  .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<EmployeeResponse>(
            page.Items.Select(mapper.ToEmployeeResponse).ToList(),
            page.TotalRecord);
    }
}
