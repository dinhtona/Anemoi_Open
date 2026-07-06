#nullable enable

using Anemoi.Hr.Domain.Payroll;

namespace Anemoi.Hr.Application.Responses;

public sealed record PayslipSummaryReportItem
{
    public Guid PayrollRunId { get; init; }
    public Guid PayrollPeriodId { get; init; }
    public Guid EmployeeId { get; init; }
    public string PeriodCode { get; init; } = default!;
    public string EmployeeCode { get; init; } = default!;
    public string EmployeeName { get; init; } = default!;
    public decimal BaseSalarySnapshot { get; init; }
    public decimal DailyRateSnapshot { get; init; }
    public decimal PaidWorkingDays { get; init; }
    public decimal PaidLeaveDays { get; init; }
    public decimal UnpaidLeaveDays { get; init; }
    public decimal BasePayAmount { get; init; }
    public decimal AllowanceTotal { get; init; }
    public decimal DeductionTotal { get; init; }
    public decimal GrossPay { get; init; }
    public decimal NetPay { get; init; }
    public PayslipStatus Status { get; init; }
    public DateTime GeneratedAt { get; init; }
    public string GeneratedBy { get; init; } = default!;
    public DateTime? PublishedAt { get; init; }
    public string? PublishedBy { get; init; }
    public DateTime? CancelledAt { get; init; }
    public string? CancelledBy { get; init; }
}
