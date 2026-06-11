#nullable enable

using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayslipSummaryReport;

public sealed record GetPayslipSummaryReportQuery : IQuery<PaginationResponse<PayslipSummaryReportItem>>
{
    public PayrollPeriodId? PayrollPeriodId { get; init; }
    public PayrollRunId? PayrollRunId { get; init; }
    public EmployeeId? EmployeeId { get; init; }
    public string? Status { get; init; }
    public DateTime? GeneratedFrom { get; init; }
    public DateTime? GeneratedTo { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; } = "asc";
}
