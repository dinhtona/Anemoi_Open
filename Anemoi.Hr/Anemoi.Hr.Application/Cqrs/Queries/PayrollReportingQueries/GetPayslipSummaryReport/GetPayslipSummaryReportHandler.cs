#nullable enable

using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayslipSummaryReport;

public sealed class GetPayslipSummaryReportHandler(
    ISqlRepository<Payslip> repository,
    ISqlRepository<PayrollRun> payrollRunRepository,
    PayrollReportingMapper mapper)
    : IQueryHandler<GetPayslipSummaryReportQuery, PaginationResponse<PayslipSummaryReportItem>>
{
    public async Task<PaginationResponse<PayslipSummaryReportItem>> Handle(
        GetPayslipSummaryReportQuery request,
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

        var query = repository.GetQueryable().AsNoTracking()
            .BuildPayslipSummary(
                payrollRunRepository.GetQueryable().AsNoTracking(),
                filter);
        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<PayslipSummaryReportItem>(
            rows.Select(mapper.ToReportItem).ToList(),
            totalRecords);
    }
}
