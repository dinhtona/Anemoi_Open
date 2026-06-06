using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using System.Collections.Generic;
using System.Linq;

namespace Anemoi.Hr.Application.Mappings;

public sealed class PayslipMapper
{
    public PayslipResponse ToResponse(Payslip payslip)
    {
        if (payslip is null) return null;
        return new PayslipResponse
        {
            Id = payslip.Id.Value.ToString(),
            PayrollRunId = payslip.PayrollRunId.Value.ToString(),
            EmployeeId = payslip.EmployeeId.Value.ToString(),
            PeriodCode = payslip.PeriodCode,
            EmployeeCode = payslip.EmployeeCode,
            EmployeeName = payslip.EmployeeName,
            BaseSalarySnapshot = payslip.BaseSalarySnapshot,
            DailyRateSnapshot = payslip.DailyRateSnapshot,
            PaidWorkingDays = payslip.PaidWorkingDays,
            PaidLeaveDays = payslip.PaidLeaveDays,
            UnpaidLeaveDays = payslip.UnpaidLeaveDays,
            BasePayAmount = payslip.BasePayAmount,
            AllowanceTotal = payslip.AllowanceTotal,
            DeductionTotal = payslip.DeductionTotal,
            GrossPay = payslip.GrossPay,
            NetPay = payslip.NetPay,
            Status = payslip.Status.ToString(),
            GeneratedAt = payslip.GeneratedAt,
            GeneratedBy = payslip.GeneratedBy,
            PublishedAt = payslip.PublishedAt,
            PublishedBy = payslip.PublishedBy,
            CancelledAt = payslip.CancelledAt,
            CancelledBy = payslip.CancelledBy
        };
    }

    public IReadOnlyCollection<PayslipResponse> ToResponses(IEnumerable<Payslip> payslips)
    {
        if (payslips is null) return [];
        return payslips.Select(ToResponse).ToList();
    }
}
