using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetEmployeeAllowances;

public sealed class GetEmployeeAllowancesHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeAllowance> employeeAllowanceRepository,
    ISqlRepository<PositionAllowance> positionAllowanceRepository,
    CompensationMapper mapper)
    : IQueryHandler<GetEmployeeAllowancesQuery, EmployeeAllowancesResponse>
{
    public async Task<EmployeeAllowancesResponse> Handle(
        GetEmployeeAllowancesQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            return new EmployeeAllowancesResponse();
        }

        // Fetch direct allowances
        var directAllowances = await employeeAllowanceRepository.GetQueryable()
            .Include(x => x.AllowanceType)
            .Where(x => x.EmployeeId == request.EmployeeId)
            .OrderByDescending(x => x.EffectiveFrom)
            .ToListAsync(cancellationToken);

        // Fetch position template allowances if employee has a position
        var positionAllowances = new List<PositionAllowance>();
        if (employee.PrimaryPositionId is not null)
        {
            positionAllowances = await positionAllowanceRepository.GetQueryable()
                .Include(x => x.AllowanceType)
                .Where(x => x.PositionId == employee.PrimaryPositionId && x.IsActive)
                .ToListAsync(cancellationToken);
        }

        return new EmployeeAllowancesResponse
        {
            DirectAllowances = directAllowances.Select(mapper.ToEmployeeAllowanceResponse).ToList(),
            PositionAllowances = positionAllowances.Select(mapper.ToPositionAllowanceResponse).ToList()
        };
    }
}
