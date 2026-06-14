using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetDepartmentCostAnalytics;

public sealed class GetDepartmentCostAnalyticsHandler(
    ISqlRepository<PayrollRun> payrollRunRepository)
    : IQueryHandler<GetDepartmentCostAnalyticsQuery, ICollection<DepartmentCostItem>>
{
    public async Task<ICollection<DepartmentCostItem>> Handle(
        GetDepartmentCostAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var payrollData = await payrollRunRepository.GetQueryable()
            .Where(x => x.Status == PayrollRunStatus.Finalized)
            .AsNoTracking()
            .Select(x => new
            {
                DeptId = x.DepartmentIdSnapshot != null
                    ? x.DepartmentIdSnapshot.Value
                    : x.Employee.PrimaryDepartmentId.Value,
                DeptName = x.DepartmentNameSnapshot != null
                    ? x.DepartmentNameSnapshot
                    : x.Employee.PrimaryDepartment.Name,
                x.NetAmount,
                x.EmployeeId
            })
            .ToListAsync(cancellationToken);

        var result = payrollData
            .GroupBy(x => new { x.DeptId, x.DeptName })
            .Select(g => new DepartmentCostItem(
                g.Key.DeptId,
                g.Key.DeptName,
                g.Sum(x => x.NetAmount),
                g.Select(x => x.EmployeeId).Distinct().Count()
            ))
            .OrderByDescending(x => x.PayrollCost)
            .ToList();

        return result;
    }
}
