using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployee;

public sealed class GetEmployeeHandler(ISqlRepository<Employee> repository, EmployeeMapper mapper)
    : IQueryHandler<GetEmployeeQuery, OneOf<EmployeeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeResponse, ErrorDetailResponse>> Handle(GetEmployeeQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await repository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            q => q.Include(x => x.PrimaryDepartment)
                  .Include(x => x.PrimaryPosition)
                  .Include(x => x.DirectManager),
            cancellationToken);

        return employee is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound)
            : mapper.ToEmployeeResponse(employee);
    }
}
