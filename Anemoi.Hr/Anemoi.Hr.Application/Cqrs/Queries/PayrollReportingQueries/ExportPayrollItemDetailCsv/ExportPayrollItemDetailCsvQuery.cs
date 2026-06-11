#nullable enable

using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollItemDetailCsv;

public sealed record ExportPayrollItemDetailCsvQuery : IQuery<ExportResult>
{
    public PayrollPeriodId? PayrollPeriodId { get; init; }
    public PayrollRunId? PayrollRunId { get; init; }
    public EmployeeId? EmployeeId { get; init; }
    public string? Status { get; init; }
    public DateTime? FinalizedFrom { get; init; }
    public DateTime? FinalizedTo { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; } = "asc";
}
