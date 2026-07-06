#nullable enable

using System;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollCostSummaryReport;

public sealed record GetPayrollCostSummaryReportQuery : IQuery<PaginationResponse<PayrollCostSummaryReportItem>>
{
    public PayrollPeriodId? PayrollPeriodId { get; init; }
    public PayrollRunId? PayrollRunId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? PositionId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; } = "asc";
}
