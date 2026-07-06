using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class PayslipResponse
{
    public string Id { get; set; }
    public string PayrollRunId { get; set; }
    public string EmployeeId { get; set; }

    public string PeriodCode { get; set; }
    public string EmployeeCode { get; set; }
    public string EmployeeName { get; set; }

    public decimal BaseSalarySnapshot { get; set; }
    public decimal DailyRateSnapshot { get; set; }
    public decimal PaidWorkingDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal BasePayAmount { get; set; }
    public decimal AllowanceTotal { get; set; }
    public decimal DeductionTotal { get; set; }
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }

    public string Status { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; }

    public DateTime? PublishedAt { get; set; }
    public string PublishedBy { get; set; }

    public DateTime? CancelledAt { get; set; }
    public string CancelledBy { get; set; }
}
