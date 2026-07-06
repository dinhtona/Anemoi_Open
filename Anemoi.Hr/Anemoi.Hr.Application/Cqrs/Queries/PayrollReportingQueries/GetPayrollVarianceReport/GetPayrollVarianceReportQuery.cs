#nullable enable

using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollVarianceReport;

public sealed record GetPayrollVarianceReportQuery : IQuery<PaginationResponse<PayrollVarianceReportItem>>
{
    public PayrollPeriodId? CurrentPayrollPeriodId { get; init; }
    public PayrollPeriodId? PreviousPayrollPeriodId { get; init; }
    public PayrollRunId? CurrentPayrollRunId { get; init; }
    public PayrollRunId? PreviousPayrollRunId { get; init; }
    public bool IncludeRemovedEmployees { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; } = "asc";
}
