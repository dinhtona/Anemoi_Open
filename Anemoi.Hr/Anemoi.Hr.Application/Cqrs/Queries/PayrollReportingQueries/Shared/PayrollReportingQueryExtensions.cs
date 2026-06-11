#nullable enable

using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;

public sealed record PayrollRunSummaryProjection
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

public sealed record PayrollItemDetailProjection
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

public sealed record PayslipSummaryProjection
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

internal sealed record PayrollReportingFilter(
    PayrollPeriodId? PayrollPeriodId,
    PayrollRunId? PayrollRunId,
    EmployeeId? EmployeeId,
    string? Status,
    DateTime? From,
    DateTime? To,
    string? SortBy,
    string? SortDirection);

internal static class PayrollReportingQueryExtensions
{
    public static IQueryable<PayrollRunSummaryProjection> BuildPayrollRunSummary(
        this IQueryable<PayrollRun> runs,
        PayrollReportingFilter filter)
    {
        if (filter.PayrollPeriodId is not null)
            runs = runs.Where(x => x.PayrollPeriodId == filter.PayrollPeriodId);
        if (filter.PayrollRunId is not null)
            runs = runs.Where(x => x.Id == filter.PayrollRunId);
        if (filter.EmployeeId is not null)
            runs = runs.Where(x => x.EmployeeId == filter.EmployeeId);
        if (Enum.TryParse<PayrollRunStatus>(filter.Status, true, out var status))
            runs = runs.Where(x => x.Status == status);
        if (filter.From is not null)
            runs = runs.Where(x => x.FinalizedAt >= filter.From);
        if (filter.To is not null)
            runs = runs.Where(x => x.FinalizedAt <= filter.To);

        var projected = runs.Select(x => new PayrollRunSummaryProjection
        {
            EmployeeCode = x.EmployeeCode,
            EmployeeName = x.EmployeeName,
            BaseSalary = x.BaseSalary,
            CurrencyCode = x.CurrencyCode,
            PaidWorkingDays = x.PaidWorkingDays,
            UnpaidLeaveDays = x.UnpaidLeaveDays,
            TotalAllowanceAmount = x.TotalAllowanceAmount,
            TotalDeductionAmount = x.TotalDeductionAmount,
            GrossAmount = x.GrossAmount,
            NetAmount = x.NetAmount,
            Status = x.Status,
            FinalizedAt = x.FinalizedAt
        });

        var descending = IsDescending(filter.SortDirection);
        return filter.SortBy?.ToLowerInvariant() switch
        {
            "employeecode" => descending
                ? projected.OrderByDescending(x => x.EmployeeCode).ThenByDescending(x => x.FinalizedAt)
                : projected.OrderBy(x => x.EmployeeCode).ThenBy(x => x.FinalizedAt),
            "employeename" => descending
                ? projected.OrderByDescending(x => x.EmployeeName).ThenByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.EmployeeName).ThenBy(x => x.EmployeeCode),
            "netamount" => descending
                ? projected.OrderByDescending(x => x.NetAmount).ThenByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.NetAmount).ThenBy(x => x.EmployeeCode),
            "status" => descending
                ? projected.OrderByDescending(x => x.Status).ThenByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.Status).ThenBy(x => x.EmployeeCode),
            _ => descending
                ? projected.OrderByDescending(x => x.FinalizedAt).ThenByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.FinalizedAt).ThenBy(x => x.EmployeeCode)
        };
    }

    public static IQueryable<PayrollItemDetailProjection> BuildPayrollItemDetail(
        this IQueryable<PayrollItem> items,
        IQueryable<PayrollRun> runs,
        PayrollReportingFilter filter)
    {
        if (filter.PayrollPeriodId is not null)
            runs = runs.Where(x => x.PayrollPeriodId == filter.PayrollPeriodId);
        if (filter.PayrollRunId is not null)
            runs = runs.Where(x => x.Id == filter.PayrollRunId);
        if (filter.EmployeeId is not null)
            runs = runs.Where(x => x.EmployeeId == filter.EmployeeId);
        if (Enum.TryParse<PayrollRunStatus>(filter.Status, true, out var status))
            runs = runs.Where(x => x.Status == status);
        if (filter.From is not null)
            runs = runs.Where(x => x.FinalizedAt >= filter.From);
        if (filter.To is not null)
            runs = runs.Where(x => x.FinalizedAt <= filter.To);

        var projected = from item in items
            join run in runs on item.PayrollRunId equals run.Id
            select new PayrollItemDetailProjection
            {
                PayrollRunId = run.Id.Value,
                PayrollPeriodId = run.PayrollPeriodId.Value,
                EmployeeId = run.EmployeeId.Value,
                EmployeeCode = run.EmployeeCode,
                EmployeeName = run.EmployeeName,
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                ItemTypeCode = item.ItemTypeCode,
                Amount = item.Amount,
                CurrencyCode = item.CurrencyCode,
                AttendanceSummaryId = item.AttendanceSummaryId == null ? null : item.AttendanceSummaryId.Value,
                PaidWorkingDays = item.PaidWorkingDays,
                PaidLeaveDays = item.PaidLeaveDays,
                UnpaidLeaveDays = item.UnpaidLeaveDays,
                BaseSalarySnapshot = item.BaseSalarySnapshot,
                DailyRateSnapshot = item.DailyRateSnapshot,
                BasePayAmount = item.BasePayAmount
            };

        var descending = IsDescending(filter.SortDirection);
        return filter.SortBy?.ToLowerInvariant() switch
        {
            "employeecode" => descending
                ? projected.OrderByDescending(x => x.EmployeeCode).ThenByDescending(x => x.ItemCode)
                : projected.OrderBy(x => x.EmployeeCode).ThenBy(x => x.ItemCode),
            "itemname" => descending
                ? projected.OrderByDescending(x => x.ItemName).ThenByDescending(x => x.ItemCode)
                : projected.OrderBy(x => x.ItemName).ThenBy(x => x.ItemCode),
            "itemtypecode" => descending
                ? projected.OrderByDescending(x => x.ItemTypeCode).ThenByDescending(x => x.ItemCode)
                : projected.OrderBy(x => x.ItemTypeCode).ThenBy(x => x.ItemCode),
            "amount" => descending
                ? projected.OrderByDescending(x => x.Amount).ThenByDescending(x => x.ItemCode)
                : projected.OrderBy(x => x.Amount).ThenBy(x => x.ItemCode),
            _ => descending
                ? projected.OrderByDescending(x => x.EmployeeCode).ThenByDescending(x => x.ItemCode)
                : projected.OrderBy(x => x.EmployeeCode).ThenBy(x => x.ItemCode)
        };
    }

    public static IQueryable<PayslipSummaryProjection> BuildPayslipSummary(
        this IQueryable<Payslip> payslips,
        IQueryable<PayrollRun> runs,
        PayrollReportingFilter filter)
    {
        if (filter.PayrollPeriodId is not null)
            runs = runs.Where(x => x.PayrollPeriodId == filter.PayrollPeriodId);
        if (filter.PayrollRunId is not null)
            payslips = payslips.Where(x => x.PayrollRunId == filter.PayrollRunId);
        if (filter.EmployeeId is not null)
            payslips = payslips.Where(x => x.EmployeeId == filter.EmployeeId);
        if (Enum.TryParse<PayslipStatus>(filter.Status, true, out var status))
            payslips = payslips.Where(x => x.Status == status);
        if (filter.From is not null)
            payslips = payslips.Where(x => x.GeneratedAt >= filter.From);
        if (filter.To is not null)
            payslips = payslips.Where(x => x.GeneratedAt <= filter.To);

        var projected = from payslip in payslips
            join run in runs on payslip.PayrollRunId equals run.Id
            select new PayslipSummaryProjection
            {
                PayrollRunId = payslip.PayrollRunId.Value,
                PayrollPeriodId = run.PayrollPeriodId.Value,
                EmployeeId = payslip.EmployeeId.Value,
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
                Status = payslip.Status,
                GeneratedAt = payslip.GeneratedAt,
                GeneratedBy = payslip.GeneratedBy,
                PublishedAt = payslip.PublishedAt,
                PublishedBy = payslip.PublishedBy,
                CancelledAt = payslip.CancelledAt,
                CancelledBy = payslip.CancelledBy
            };

        var descending = IsDescending(filter.SortDirection);
        return filter.SortBy?.ToLowerInvariant() switch
        {
            "employeecode" => descending
                ? projected.OrderByDescending(x => x.EmployeeCode).ThenByDescending(x => x.GeneratedAt)
                : projected.OrderBy(x => x.EmployeeCode).ThenBy(x => x.GeneratedAt),
            "periodcode" => descending
                ? projected.OrderByDescending(x => x.PeriodCode).ThenByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.PeriodCode).ThenBy(x => x.EmployeeCode),
            "netpay" => descending
                ? projected.OrderByDescending(x => x.NetPay).ThenByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.NetPay).ThenBy(x => x.EmployeeCode),
            "status" => descending
                ? projected.OrderByDescending(x => x.Status).ThenByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.Status).ThenBy(x => x.EmployeeCode),
            _ => descending
                ? projected.OrderByDescending(x => x.GeneratedAt).ThenByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.GeneratedAt).ThenBy(x => x.EmployeeCode)
        };
    }

    private static bool IsDescending(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}
