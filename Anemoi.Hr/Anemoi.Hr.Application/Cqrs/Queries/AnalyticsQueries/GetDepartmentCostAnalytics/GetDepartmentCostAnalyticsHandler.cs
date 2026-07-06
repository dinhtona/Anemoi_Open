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
    ISqlRepository<PayrollItem> payrollItemRepository)
    : IQueryHandler<GetDepartmentCostAnalyticsQuery, ICollection<DepartmentCostItem>>
{
    public async Task<ICollection<DepartmentCostItem>> Handle(
        GetDepartmentCostAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await payrollItemRepository.GetQueryable()
            .Where(x => x.PayrollRun.Status == PayrollRunStatus.Finalized)
            .AsNoTracking()
            .GroupBy(x => new
            {
                DeptId = x.DepartmentIdSnapshot != null
                    ? x.DepartmentIdSnapshot.Value
                    : x.PayrollRun.Employee.PrimaryDepartmentId.Value,
                DeptName = x.DepartmentNameSnapshot != null
                    ? x.DepartmentNameSnapshot
                    : x.PayrollRun.Employee.PrimaryDepartment.Name
            })
            .Select(g => new DepartmentCostItem(
                g.Key.DeptId,
                g.Key.DeptName,
                g.Sum(x => x.BasePayAmount),
                g.Select(x => x.PayrollRun.EmployeeId).Distinct().Count()
            ))
            .OrderByDescending(x => x.PayrollCost)
            .ToListAsync(cancellationToken);

        return result;
    }
}
