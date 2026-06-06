using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Payroll;

public sealed class Payslip : ValueObject
{
    public PayslipId Id { get; set; }
    public PayrollRunId PayrollRunId { get; set; }
    public EmployeeId EmployeeId { get; set; }

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

    public PayslipStatus Status { get; private set; } = PayslipStatus.Generated;
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; }

    public DateTime? PublishedAt { get; private set; }
    public string PublishedBy { get; private set; }

    public DateTime? CancelledAt { get; private set; }
    public string CancelledBy { get; private set; }

    // Navigation
    public PayrollRun PayrollRun { get; set; }
    public Employee Employee { get; set; }

    public bool Publish(string actor, DateTime now)
    {
        if (Status != PayslipStatus.Generated)
            return false;

        Status = PayslipStatus.Published;
        PublishedBy = actor;
        PublishedAt = now;
        return true;
    }

    public bool Cancel(string actor, DateTime now)
    {
        if (Status == PayslipStatus.Cancelled)
            return false;

        Status = PayslipStatus.Cancelled;
        CancelledBy = actor;
        CancelledAt = now;
        return true;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
