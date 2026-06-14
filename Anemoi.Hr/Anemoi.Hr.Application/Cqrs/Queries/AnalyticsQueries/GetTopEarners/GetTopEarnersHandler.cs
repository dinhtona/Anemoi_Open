using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetTopEarners;

public sealed class GetTopEarnersHandler(
    ISqlRepository<PayrollRun> payrollRunRepository)
    : IQueryHandler<GetTopEarnersQuery, ICollection<TopEarnerItem>>
{
    public async Task<ICollection<TopEarnerItem>> Handle(
        GetTopEarnersQuery request,
        CancellationToken cancellationToken)
    {
        var payrollRunId = request.PayrollRunId;

        if (payrollRunId is null)
        {
            var latestRun = await payrollRunRepository.GetQueryable().AsNoTracking()
                .Where(x => x.Status == PayrollRunStatus.Finalized)
                .OrderByDescending(x => x.FinalizedAt)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestRun is null)
                return [];

            payrollRunId = latestRun;
        }

        var raw = await payrollRunRepository.GetQueryable()
            .Where(x => x.Id == payrollRunId && x.Status == PayrollRunStatus.Finalized)
            .AsNoTracking()
            .OrderByDescending(x => x.NetAmount)
            .Take(request.Top)
            .Select(x => new
            {
                EmployeeId = x.EmployeeId.Value,
                x.EmployeeName,
                DepartmentName = x.DepartmentNameSnapshot ?? x.Employee.PrimaryDepartment.Name,
                x.NetAmount
            })
            .ToListAsync(cancellationToken);

        return raw.Select(x => new TopEarnerItem(
            x.EmployeeId,
            x.EmployeeName,
            x.DepartmentName,
            x.NetAmount
        )).ToList();
    }
}
