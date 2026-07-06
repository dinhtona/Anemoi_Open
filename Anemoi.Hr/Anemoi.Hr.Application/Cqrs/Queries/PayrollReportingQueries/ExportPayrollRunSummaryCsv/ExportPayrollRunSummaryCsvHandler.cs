#nullable enable

using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollRunSummaryCsv;

public sealed class ExportPayrollRunSummaryCsvHandler(
    ISqlRepository<PayrollRun> repository,
    PayrollReportingMapper mapper,
    PayrollReportExportService exportService)
    : IQueryHandler<ExportPayrollRunSummaryCsvQuery, ExportResult>
{
    public async Task<ExportResult> Handle(
        ExportPayrollRunSummaryCsvQuery request,
        CancellationToken cancellationToken)
    {
        var filter = new PayrollReportingFilter(
            request.PayrollPeriodId,
            request.PayrollRunId,
            request.EmployeeId,
            request.Status,
            request.FinalizedFrom,
            request.FinalizedTo,
            request.SortBy,
            request.SortDirection);

        var rows = await repository.GetQueryable().AsNoTracking()
            .BuildPayrollRunSummary(filter)
            .Take(ReportExportLimits.MaxRows + 1)
            .ToListAsync(cancellationToken);
        var reportItems = rows.Select(mapper.ToReportItem).ToList();

        return await exportService.ExportAsync(
            reportItems,
            ReportTypes.PayrollRunSummary,
            new
            {
                request.PayrollPeriodId,
                request.PayrollRunId,
                request.EmployeeId,
                request.Status,
                request.FinalizedFrom,
                request.FinalizedTo,
                request.SortBy,
                request.SortDirection
            },
            cancellationToken);
    }
}
