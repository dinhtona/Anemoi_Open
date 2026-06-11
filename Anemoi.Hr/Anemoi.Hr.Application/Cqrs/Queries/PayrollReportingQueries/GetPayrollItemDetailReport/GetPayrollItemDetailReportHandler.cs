#nullable enable

using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollItemDetailReport;

public sealed class GetPayrollItemDetailReportHandler(
    ISqlRepository<PayrollItem> repository,
    ISqlRepository<PayrollRun> payrollRunRepository,
    PayrollReportingMapper mapper)
    : IQueryHandler<GetPayrollItemDetailReportQuery, PaginationResponse<PayrollItemDetailReportItem>>
{
    public async Task<PaginationResponse<PayrollItemDetailReportItem>> Handle(
        GetPayrollItemDetailReportQuery request,
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
            .BuildPayrollItemDetail(
                payrollRunRepository.GetQueryable().AsNoTracking(),
                filter);
        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<PayrollItemDetailReportItem>(
            rows.Select(mapper.ToReportItem).ToList(),
            totalRecords);
    }
}
