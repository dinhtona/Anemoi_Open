using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetWorkforceOverview;

public sealed class GetWorkforceOverviewHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Department> departmentRepository,
    ISqlRepository<Position> positionRepository)
    : IQueryHandler<GetWorkforceOverviewQuery, WorkforceOverviewResponse>
{
    public async Task<WorkforceOverviewResponse> Handle(
        GetWorkforceOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var totalEmployees = await employeeRepository.GetQueryable().AsNoTracking().LongCountAsync(cancellationToken);
        var activeEmployees = await employeeRepository.GetQueryable().AsNoTracking()
            .CountAsync(x => x.EmploymentStatusCode == EmploymentStatusCode.Active, cancellationToken);
        var inactiveEmployees = totalEmployees - activeEmployees;
        var totalDepartments = await departmentRepository.GetQueryable().AsNoTracking().LongCountAsync(cancellationToken);
        var totalPositions = await positionRepository.GetQueryable().AsNoTracking().LongCountAsync(cancellationToken);

        return new WorkforceOverviewResponse(
            (int)totalEmployees,
            (int)activeEmployees,
            (int)inactiveEmployees,
            (int)totalDepartments,
            (int)totalPositions
        );
    }
}
