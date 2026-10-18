#nullable enable

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.Domain.Taxation;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollCostDepartmentReport;

public sealed class GetPayrollCostDepartmentReportHandler(
    ISqlRepository<PayrollRun> runRepository,
    ISqlRepository<PayrollPeriod> periodRepository,
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Department> departmentRepository,
    ISqlRepository<TaxCalculationSnapshot> taxRepository,
    ISqlRepository<InsuranceCalculationSnapshot> insuranceRepository)
    : IQueryHandler<GetPayrollCostDepartmentReportQuery, PaginationResponse<PayrollCostDepartmentReportItem>>
{
    public async Task<PaginationResponse<PayrollCostDepartmentReportItem>> Handle(
        GetPayrollCostDepartmentReportQuery request,
        CancellationToken cancellationToken)
    {
        var filter = new PayrollReportingFilter(
            request.PayrollPeriodId,
            request.PayrollRunId,
            null,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.SortBy,
            request.SortDirection);

        var items = await PayrollReportingQueryExtensions.BuildPayrollCostDepartmentAsync(
            runRepository.GetQueryable().AsNoTracking(),
            periodRepository.GetQueryable().AsNoTracking(),
            employeeRepository.GetQueryable().AsNoTracking(),
            departmentRepository.GetQueryable().AsNoTracking(),
            taxRepository.GetQueryable().AsNoTracking(),
            insuranceRepository.GetQueryable().AsNoTracking(),
            filter,
            cancellationToken);

        var totalRecords = items.Count;
        var pagedItems = items
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginationResponse<PayrollCostDepartmentReportItem>(pagedItems, totalRecords);
    }
}
