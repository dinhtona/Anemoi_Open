#nullable enable

namespace Anemoi.Hr.Application.Responses;

public sealed record PayrollItemDetailReportItem
{
    public Guid PayrollRunId { get; init; }
    public Guid PayrollPeriodId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = default!;
    public string EmployeeName { get; init; } = default!;
    public string ItemCode { get; init; } = default!;
    public string ItemName { get; init; } = default!;
    public string ItemTypeCode { get; init; } = default!;
    public decimal Amount { get; init; }
    public string CurrencyCode { get; init; } = default!;
    public Guid? AttendanceSummaryId { get; init; }
    public decimal PaidWorkingDays { get; init; }
    public decimal PaidLeaveDays { get; init; }
    public decimal UnpaidLeaveDays { get; init; }
    public decimal BaseSalarySnapshot { get; init; }
    public decimal DailyRateSnapshot { get; init; }
    public decimal BasePayAmount { get; init; }
}
