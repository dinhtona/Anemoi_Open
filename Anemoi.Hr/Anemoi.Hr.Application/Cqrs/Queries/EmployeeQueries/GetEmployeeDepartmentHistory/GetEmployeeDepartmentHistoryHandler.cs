using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployeeDepartmentHistory;

public sealed class GetEmployeeDepartmentHistoryHandler(
    ISqlRepository<EmployeeDepartmentHistory> repository,
    EmployeeMapper mapper)
    : IQueryHandler<GetEmployeeDepartmentHistoryQuery, OneOf<IReadOnlyCollection<EmployeeDepartmentHistoryResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<EmployeeDepartmentHistoryResponse>, ErrorDetailResponse>> Handle(
        GetEmployeeDepartmentHistoryQuery request, CancellationToken cancellationToken)
    {
        var dbQuery = repository.GetQueryable();
        
        var histories = await dbQuery
            .Include(x => x.Department)
            .Include(x => x.OldDepartment)
            .Where(x => x.EmployeeId == request.EmployeeId)
            .OrderByDescending(x => x.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return histories.Select(mapper.ToEmployeeDepartmentHistoryResponse).ToList();
    }
}
