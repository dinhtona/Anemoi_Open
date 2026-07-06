using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetEmployeeSalaryHistory;

public sealed class GetEmployeeSalaryHistoryHandler(
    ISqlRepository<EmployeeSalary> repository,
    CompensationMapper mapper)
    : IQueryHandler<GetEmployeeSalaryHistoryQuery, IReadOnlyCollection<EmployeeSalaryResponse>>
{
    public async Task<IReadOnlyCollection<EmployeeSalaryResponse>> Handle(
        GetEmployeeSalaryHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var salaries = await repository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId)
            .OrderByDescending(x => x.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return salaries.Select(mapper.ToEmployeeSalaryResponse).ToList();
    }
}
