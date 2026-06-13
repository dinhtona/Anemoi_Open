#nullable enable

using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollVarianceCsv;

public sealed record ExportPayrollVarianceCsvQuery : IQuery<ExportResult>
{
    public PayrollPeriodId? CurrentPayrollPeriodId { get; init; }
    public PayrollPeriodId? PreviousPayrollPeriodId { get; init; }
    public PayrollRunId? CurrentPayrollRunId { get; init; }
    public PayrollRunId? PreviousPayrollRunId { get; init; }
    public bool IncludeRemovedEmployees { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; } = "asc";
}
