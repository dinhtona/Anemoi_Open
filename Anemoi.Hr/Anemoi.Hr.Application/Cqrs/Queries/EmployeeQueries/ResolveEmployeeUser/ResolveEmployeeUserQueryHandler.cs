using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr.Queries;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.ResolveEmployeeUser;

public sealed class ResolveEmployeeUserQueryHandler(ISqlRepository<Employee> repository)
    : IQueryHandler<ResolveEmployeeUserQuery, OneOf<ResolveEmployeeUserResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<ResolveEmployeeUserResponse, ErrorDetailResponse>> Handle(ResolveEmployeeUserQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.EmployeeId, out var employeeGuid))
        {
            return new ResolveEmployeeUserResponse(null, null);
        }

        var employeeId = new EmployeeId(employeeGuid);
        var employee = await repository.GetFirstByConditionAsync(
            x => x.Id == employeeId,
            null,
            cancellationToken);

        if (employee is null)
        {
            return new ResolveEmployeeUserResponse(null, null);
        }

        return new ResolveEmployeeUserResponse(employee.IdentityUserId?.ToString(), employee.WorkEmail);
    }
}
