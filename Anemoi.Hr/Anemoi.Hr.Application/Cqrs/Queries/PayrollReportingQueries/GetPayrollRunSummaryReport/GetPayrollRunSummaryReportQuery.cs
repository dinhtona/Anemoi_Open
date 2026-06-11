#nullable enable

using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollRunSummaryReport;

public sealed record GetPayrollRunSummaryReportQuery : IQuery<PaginationResponse<PayrollRunSummaryReportItem>>
{
    public PayrollPeriodId? PayrollPeriodId { get; init; }
    public PayrollRunId? PayrollRunId { get; init; }
    public EmployeeId? EmployeeId { get; init; }
    public string? Status { get; init; }
    public DateTime? FinalizedFrom { get; init; }
    public DateTime? FinalizedTo { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; } = "asc";
}
