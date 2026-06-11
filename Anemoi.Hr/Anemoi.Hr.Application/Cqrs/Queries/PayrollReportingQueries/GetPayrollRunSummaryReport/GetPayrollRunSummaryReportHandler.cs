#nullable enable

using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollRunSummaryReport;

public sealed class GetPayrollRunSummaryReportHandler(
    ISqlRepository<PayrollRun> repository,
    PayrollReportingMapper mapper)
    : IQueryHandler<GetPayrollRunSummaryReportQuery, PaginationResponse<PayrollRunSummaryReportItem>>
{
    public async Task<PaginationResponse<PayrollRunSummaryReportItem>> Handle(
        GetPayrollRunSummaryReportQuery request,
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

        var query = repository.GetQueryable().AsNoTracking()
            .BuildPayrollRunSummary(filter);
        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<PayrollRunSummaryReportItem>(
            rows.Select(mapper.ToReportItem).ToList(),
            totalRecords);
    }
}
