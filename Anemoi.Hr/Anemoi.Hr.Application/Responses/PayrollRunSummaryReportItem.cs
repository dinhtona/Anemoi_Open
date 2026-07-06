#nullable enable

using Anemoi.Hr.Domain.Payroll;

namespace Anemoi.Hr.Application.Responses;

public sealed record PayrollRunSummaryReportItem
{
    public string EmployeeCode { get; init; } = default!;
    public string EmployeeName { get; init; } = default!;
    public decimal BaseSalary { get; init; }
    public string CurrencyCode { get; init; } = default!;
    public decimal PaidWorkingDays { get; init; }
    public decimal UnpaidLeaveDays { get; init; }
    public decimal TotalAllowanceAmount { get; init; }
    public decimal TotalDeductionAmount { get; init; }
    public decimal GrossAmount { get; init; }
    public decimal NetAmount { get; init; }
    public PayrollRunStatus Status { get; init; }
    public DateTime? FinalizedAt { get; init; }
}
