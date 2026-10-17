#nullable enable

using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.Domain.Taxation;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollCostDepartmentCsv;

public sealed class ExportPayrollCostDepartmentCsvHandler(
    ISqlRepository<PayrollRun> runRepository,
    ISqlRepository<PayrollPeriod> periodRepository,
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Department> departmentRepository,
    ISqlRepository<TaxCalculationSnapshot> taxRepository,
    ISqlRepository<InsuranceCalculationSnapshot> insuranceRepository,
    PayrollReportExportService exportService)
    : IQueryHandler<ExportPayrollCostDepartmentCsvQuery, OneOf<ExportResult, ErrorDetailResponse>>
{
    public async Task<OneOf<ExportResult, ErrorDetailResponse>> Handle(
        ExportPayrollCostDepartmentCsvQuery request,
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

        var rows = await PayrollReportingQueryExtensions.BuildPayrollCostDepartment(
                runRepository.GetQueryable().AsNoTracking(),
                periodRepository.GetQueryable().AsNoTracking(),
                employeeRepository.GetQueryable().AsNoTracking(),
                departmentRepository.GetQueryable().AsNoTracking(),
                taxRepository.GetQueryable().AsNoTracking(),
                insuranceRepository.GetQueryable().AsNoTracking(),
                filter)
            .Take(ReportExportLimits.MaxRows + 1)
            .ToListAsync(cancellationToken);

        return await exportService.ExportAsync(
            rows,
            ReportTypes.PayrollCostDepartment,
            new
            {
                request.PayrollPeriodId,
                request.PayrollRunId,
                request.Status,
                request.FromDate,
                request.ToDate,
                request.SortBy,
                request.SortDirection
            },
            cancellationToken);
    }
}
