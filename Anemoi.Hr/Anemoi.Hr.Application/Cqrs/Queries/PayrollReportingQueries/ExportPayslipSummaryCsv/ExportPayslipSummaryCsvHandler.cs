#nullable enable

using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayslipSummaryCsv;

public sealed class ExportPayslipSummaryCsvHandler(
    ISqlRepository<Payslip> repository,
    ISqlRepository<PayrollRun> payrollRunRepository,
    PayrollReportingMapper mapper,
    PayrollReportExportService exportService)
    : IQueryHandler<ExportPayslipSummaryCsvQuery, ExportResult>
{
    public async Task<ExportResult> Handle(
        ExportPayslipSummaryCsvQuery request,
        CancellationToken cancellationToken)
    {
        var filter = new PayrollReportingFilter(
            request.PayrollPeriodId,
            request.PayrollRunId,
            request.EmployeeId,
            request.Status,
            request.GeneratedFrom,
            request.GeneratedTo,
            request.SortBy,
            request.SortDirection);

        var rows = await repository.GetQueryable().AsNoTracking()
            .BuildPayslipSummary(
                payrollRunRepository.GetQueryable().AsNoTracking(),
                filter)
            .Take(ReportExportLimits.MaxRows + 1)
            .ToListAsync(cancellationToken);
        var reportItems = rows.Select(mapper.ToReportItem).ToList();

        return await exportService.ExportAsync(
            reportItems,
            ReportTypes.PayslipSummary,
            new
            {
                request.PayrollPeriodId,
                request.PayrollRunId,
                request.EmployeeId,
                request.Status,
                request.GeneratedFrom,
                request.GeneratedTo,
                request.SortBy,
                request.SortDirection
            },
            cancellationToken);
    }
}
