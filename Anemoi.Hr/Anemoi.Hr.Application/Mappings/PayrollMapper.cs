using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anemoi.Hr.Application.Mappings;

public sealed class PayrollMapper
{
    public PayrollPeriodResponse ToResponse(PayrollPeriod period)
    {
        if (period is null) return null;
        return new PayrollPeriodResponse
        {
            Id = period.Id.Value.ToString(),
            PeriodCode = period.PeriodCode,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            StatusCode = period.StatusCode,
            StandardWorkingDays = period.StandardWorkingDays,
            CreatedAt = period.CreatedAt,
            CreatedBy = period.CreatedBy,
            UpdatedAt = period.UpdatedAt,
            UpdatedBy = period.UpdatedBy
        };
    }

    public IReadOnlyCollection<PayrollPeriodResponse> ToResponses(IEnumerable<PayrollPeriod> periods)
    {
        if (periods is null) return [];
        return periods.Select(ToResponse).ToList();
    }

    public PayrollRunResponse ToResponse(PayrollRun run)
    {
        if (run is null) return null;
        return new PayrollRunResponse
        {
            Id = run.Id.Value.ToString(),
            PayrollPeriodId = run.PayrollPeriodId.Value.ToString(),
            EmployeeId = run.EmployeeId.Value.ToString(),
            EmployeeCode = run.EmployeeCode,
            EmployeeName = run.EmployeeName,
            BaseSalary = run.BaseSalary,
            CurrencyCode = run.CurrencyCode,
            PayScheduleType = run.PayScheduleType,
            StandardWorkingDays = run.StandardWorkingDays,
            PaidWorkingDays = run.PaidWorkingDays,
            UnpaidLeaveDays = run.UnpaidLeaveDays,
            DailyRate = run.DailyRate,
            BasePayAmount = run.BasePayAmount,
            TotalAllowanceAmount = run.TotalAllowanceAmount,
            GrossAmount = run.GrossAmount,
            TotalDeductionAmount = run.TotalDeductionAmount,
            NetAmount = run.NetAmount,
            CalculatedAt = run.CalculatedAt,
            CalculatedBy = run.CalculatedBy
        };
    }

    public IReadOnlyCollection<PayrollRunResponse> ToResponses(IEnumerable<PayrollRun> runs)
    {
        if (runs is null) return [];
        return runs.Select(ToResponse).ToList();
    }

    public PayrollItemResponse ToResponse(PayrollItem item)
    {
        if (item is null) return null;
        return new PayrollItemResponse
        {
            Id = item.Id.Value.ToString(),
            PayrollRunId = item.PayrollRunId.Value.ToString(),
            ItemCode = item.ItemCode,
            ItemName = item.ItemName,
            ItemTypeCode = item.ItemTypeCode,
            Amount = item.Amount,
            CurrencyCode = item.CurrencyCode
        };
    }

    public List<PayrollItemResponse> ToResponses(IEnumerable<PayrollItem> items)
    {
        if (items is null) return [];
        return items.Select(ToResponse).ToList();
    }

    public PayrollRunDetailResponse ToDetailResponse(PayrollRun run)
    {
        if (run is null) return null;
        return new PayrollRunDetailResponse
        {
            Id = run.Id.Value.ToString(),
            PayrollPeriodId = run.PayrollPeriodId.Value.ToString(),
            EmployeeId = run.EmployeeId.Value.ToString(),
            EmployeeCode = run.EmployeeCode,
            EmployeeName = run.EmployeeName,
            BaseSalary = run.BaseSalary,
            CurrencyCode = run.CurrencyCode,
            PayScheduleType = run.PayScheduleType,
            StandardWorkingDays = run.StandardWorkingDays,
            PaidWorkingDays = run.PaidWorkingDays,
            UnpaidLeaveDays = run.UnpaidLeaveDays,
            DailyRate = run.DailyRate,
            BasePayAmount = run.BasePayAmount,
            TotalAllowanceAmount = run.TotalAllowanceAmount,
            GrossAmount = run.GrossAmount,
            TotalDeductionAmount = run.TotalDeductionAmount,
            NetAmount = run.NetAmount,
            CalculatedAt = run.CalculatedAt,
            CalculatedBy = run.CalculatedBy,
            PayrollItems = ToResponses(run.PayrollItems)
        };
    }
}
