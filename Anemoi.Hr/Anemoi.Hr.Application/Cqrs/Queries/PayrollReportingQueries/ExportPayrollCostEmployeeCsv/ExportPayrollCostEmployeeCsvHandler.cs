#nullable enable

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.Domain.Taxation;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollCostEmployeeCsv;

public sealed class ExportPayrollCostEmployeeCsvHandler(
    ISqlRepository<PayrollRun> runRepository,
    ISqlRepository<PayrollPeriod> periodRepository,
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Department> departmentRepository,
    ISqlRepository<Position> positionRepository,
    ISqlRepository<TaxCalculationSnapshot> taxRepository,
    ISqlRepository<InsuranceCalculationSnapshot> insuranceRepository,
    ISqlRepository<Payslip> payslipRepository,
    PayrollReportExportService exportService)
    : IQueryHandler<ExportPayrollCostEmployeeCsvQuery, ExportResult>
{
    public async Task<ExportResult> Handle(
        ExportPayrollCostEmployeeCsvQuery request,
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
            request.SortDirection,
            request.DepartmentId,
            request.PositionId);

        var rows = await PayrollReportingQueryExtensions.BuildPayrollCostEmployee(
                runRepository.GetQueryable().AsNoTracking(),
                periodRepository.GetQueryable().AsNoTracking(),
                employeeRepository.GetQueryable().AsNoTracking(),
                departmentRepository.GetQueryable().AsNoTracking(),
                positionRepository.GetQueryable().AsNoTracking(),
                taxRepository.GetQueryable().AsNoTracking(),
                insuranceRepository.GetQueryable().AsNoTracking(),
                payslipRepository.GetQueryable().AsNoTracking(),
                filter)
            .Take(ReportExportLimits.MaxRows + 1)
            .ToListAsync(cancellationToken);

        return await exportService.ExportAsync(
            rows,
            ReportTypes.PayrollCostEmployee,
            new
            {
                request.PayrollPeriodId,
                request.PayrollRunId,
                request.Status,
                request.FromDate,
                request.ToDate,
                request.DepartmentId,
                request.PositionId,
                request.SortBy,
                request.SortDirection
            },
            cancellationToken);
    }
}
