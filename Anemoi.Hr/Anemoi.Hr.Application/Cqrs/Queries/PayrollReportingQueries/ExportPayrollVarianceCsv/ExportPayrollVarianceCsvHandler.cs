#nullable enable

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.Domain.Taxation;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollVarianceCsv;

public sealed class ExportPayrollVarianceCsvHandler(
    ISqlRepository<PayrollRun> runRepository,
    ISqlRepository<PayrollPeriod> periodRepository,
    ISqlRepository<TaxCalculationSnapshot> taxRepository,
    ISqlRepository<InsuranceCalculationSnapshot> insuranceRepository,
    PayrollReportExportService exportService)
    : IQueryHandler<ExportPayrollVarianceCsvQuery, ExportResult>
{
    public async Task<ExportResult> Handle(
        ExportPayrollVarianceCsvQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await PayrollReportingQueryExtensions.BuildPayrollVariance(
                runRepository.GetQueryable().AsNoTracking(),
                periodRepository.GetQueryable().AsNoTracking(),
                taxRepository.GetQueryable().AsNoTracking(),
                insuranceRepository.GetQueryable().AsNoTracking(),
                request.CurrentPayrollPeriodId,
                request.PreviousPayrollPeriodId,
                request.CurrentPayrollRunId,
                request.PreviousPayrollRunId,
                request.IncludeRemovedEmployees,
                request.SortBy,
                request.SortDirection)
            .Take(ReportExportLimits.MaxRows + 1)
            .ToListAsync(cancellationToken);

        return await exportService.ExportAsync(
            rows,
            ReportTypes.PayrollVariance,
            new
            {
                request.CurrentPayrollPeriodId,
                request.PreviousPayrollPeriodId,
                request.CurrentPayrollRunId,
                request.PreviousPayrollRunId,
                request.IncludeRemovedEmployees,
                request.SortBy,
                request.SortDirection
            },
            cancellationToken);
    }
}
