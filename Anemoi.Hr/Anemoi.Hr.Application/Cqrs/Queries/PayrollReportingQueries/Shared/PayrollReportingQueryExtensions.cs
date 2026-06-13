#nullable enable

using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.Domain.Taxation;
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

public class PayrollReportingFilter
{
    public PayrollPeriodId? PayrollPeriodId { get; }
    public PayrollRunId? PayrollRunId { get; }
    public EmployeeId? EmployeeId { get; }
    public string? Status { get; }
    public DateTime? From { get; }
    public DateTime? To { get; }
    public string? SortBy { get; }
    public string? SortDirection { get; }
    public Guid? DepartmentId { get; }
    public Guid? PositionId { get; }

    public PayrollReportingFilter(
        PayrollPeriodId? payrollPeriodId,
        PayrollRunId? payrollRunId,
        EmployeeId? employeeId,
        string? status,
        DateTime? from,
        DateTime? to,
        string? sortBy,
        string? sortDirection,
        Guid? departmentId = null,
        Guid? positionId = null)
    {
        PayrollPeriodId = payrollPeriodId;
        PayrollRunId = payrollRunId;
        EmployeeId = employeeId;
        Status = status;
        From = from.ToUtc();
        To = to.ToUtc();
        SortBy = sortBy;
        SortDirection = sortDirection;
        DepartmentId = departmentId;
        PositionId = positionId;
    }
}

internal static class DateTimeHelpers
{
    internal static DateTime? ToUtc(this DateTime? dt) =>
        dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null;
}

public static class PayrollReportingQueryExtensions
{
    private static IQueryable<PayrollRun> ApplyBaseFilters(
        IQueryable<PayrollRun> runs,
        IQueryable<Employee> employees,
        PayrollReportingFilter filter)
    {
        if (filter.PayrollRunId is not null)
        {
            runs = runs.Where(x => x.Id == filter.PayrollRunId);
        }
        else if (filter.PayrollPeriodId is not null)
        {
            runs = runs.Where(x => x.PayrollPeriodId == filter.PayrollPeriodId);
        }
        else if (filter.From is not null || filter.To is not null)
        {
            if (filter.From is not null)
                runs = runs.Where(x => x.FinalizedAt >= filter.From);
            if (filter.To is not null)
                runs = runs.Where(x => x.FinalizedAt <= filter.To);
        }

        if (Enum.TryParse<PayrollRunStatus>(filter.Status, true, out var runStatus))
        {
            runs = runs.Where(x => x.Status == runStatus);
        }

        if (filter.DepartmentId is not null || filter.PositionId is not null)
        {
            runs = from run in runs
                   join emp in employees on run.EmployeeId equals emp.Id
                   where (filter.DepartmentId == null || emp.PrimaryDepartmentId == new DepartmentId(filter.DepartmentId.Value))
                      && (filter.PositionId == null || emp.PrimaryPositionId == new PositionId(filter.PositionId.Value))
                   select run;
        }

        return runs;
    }

    public static IQueryable<PayrollCostSummaryReportItem> BuildPayrollCostSummary(
        IQueryable<PayrollRun> runs,
        IQueryable<PayrollPeriod> periods,
        IQueryable<Employee> employees,
        IQueryable<TaxCalculationSnapshot> taxSnapshots,
        IQueryable<InsuranceCalculationSnapshot> insuranceSnapshots,
        PayrollReportingFilter filter)
    {
        var filteredRuns = ApplyBaseFilters(runs, employees, filter);

        var runDetails = from run in filteredRuns
                         join period in periods on run.PayrollPeriodId equals period.Id
                         join tax in taxSnapshots on run.Id equals tax.PayrollRunId into taxGroup
                         from tax in taxGroup.DefaultIfEmpty()
                         join ins in insuranceSnapshots on new { run.EmployeeId, Start = period.StartDate, End = period.EndDate } equals new { ins.EmployeeId, Start = ins.CalculationPeriodStart, End = ins.CalculationPeriodEnd } into insGroup
                         from ins in insGroup.DefaultIfEmpty()
                         select new
                         {
                             run.PayrollPeriodId,
                             period.PeriodCode,
                             run.Status,
                             run.GrossAmount,
                             TaxableIncome = tax != null ? tax.TaxableIncomeSnapshot : 0m,
                             TaxAmount = tax != null ? tax.TotalTaxAmount : 0m,
                             EmployeeInsurance = ins != null ? ins.TotalEmployeeContribution : 0m,
                             EmployerInsurance = ins != null ? ins.TotalEmployerContribution : 0m,
                             run.TotalDeductionAmount,
                             run.NetAmount,
                             run.FinalizedAt
                         };

        var grouped = from item in runDetails
                      group item by new { item.PayrollPeriodId, item.PeriodCode, item.Status } into g
                      select new PayrollCostSummaryReportItem
                      {
                          PayrollPeriodId = g.Key.PayrollPeriodId.Value,
                          PeriodCode = g.Key.PeriodCode,
                          Status = g.Key.Status.ToString(),
                          EmployeeCount = g.Count(),
                          TotalGrossIncome = g.Sum(x => x.GrossAmount),
                          TotalTaxableIncome = g.Sum(x => x.TaxableIncome),
                          TotalEmployeeTax = g.Sum(x => x.TaxAmount),
                          TotalEmployeeInsurance = g.Sum(x => x.EmployeeInsurance),
                          TotalEmployerInsurance = g.Sum(x => x.EmployerInsurance),
                          TotalDeductions = g.Sum(x => x.TotalDeductionAmount),
                          TotalNetPay = g.Sum(x => x.NetAmount),
                          FinalizedAt = g.Max(x => x.FinalizedAt)
                      };

        var descending = IsDescending(filter.SortDirection);
        return filter.SortBy?.ToLowerInvariant() switch
        {
            "periodcode" => descending
                ? grouped.OrderByDescending(x => x.PeriodCode).ThenByDescending(x => x.Status)
                : grouped.OrderBy(x => x.PeriodCode).ThenBy(x => x.Status),
            "employeecount" => descending
                ? grouped.OrderByDescending(x => x.EmployeeCount).ThenByDescending(x => x.PeriodCode)
                : grouped.OrderBy(x => x.EmployeeCount).ThenBy(x => x.PeriodCode),
            "totalnetpay" => descending
                ? grouped.OrderByDescending(x => x.TotalNetPay).ThenByDescending(x => x.PeriodCode)
                : grouped.OrderBy(x => x.TotalNetPay).ThenBy(x => x.PeriodCode),
            _ => descending
                ? grouped.OrderByDescending(x => x.PeriodCode)
                : grouped.OrderBy(x => x.PeriodCode)
        };
    }

    public static IQueryable<PayrollCostDepartmentReportItem> BuildPayrollCostDepartment(
        IQueryable<PayrollRun> runs,
        IQueryable<PayrollPeriod> periods,
        IQueryable<Employee> employees,
        IQueryable<Department> departments,
        IQueryable<TaxCalculationSnapshot> taxSnapshots,
        IQueryable<InsuranceCalculationSnapshot> insuranceSnapshots,
        PayrollReportingFilter filter)
    {
        var filteredRuns = ApplyBaseFilters(runs, employees, filter);

        var runDetails = from run in filteredRuns
                         join period in periods on run.PayrollPeriodId equals period.Id
                         join emp in employees on run.EmployeeId equals emp.Id
                         join dept in departments on emp.PrimaryDepartmentId equals dept.Id into deptGroup
                         from dept in deptGroup.DefaultIfEmpty()
                         join tax in taxSnapshots on run.Id equals tax.PayrollRunId into taxGroup
                         from tax in taxGroup.DefaultIfEmpty()
                         join ins in insuranceSnapshots on new { run.EmployeeId, Start = period.StartDate, End = period.EndDate } equals new { ins.EmployeeId, Start = ins.CalculationPeriodStart, End = ins.CalculationPeriodEnd } into insGroup
                         from ins in insGroup.DefaultIfEmpty()
                         select new
                         {
                             DepartmentId = emp.PrimaryDepartmentId != null ? (Guid?)emp.PrimaryDepartmentId.Value : null,
                             DepartmentName = dept != null ? dept.Name : "No Department",
                             run.GrossAmount,
                             TaxAmount = tax != null ? tax.TotalTaxAmount : 0m,
                             EmployeeInsurance = ins != null ? ins.TotalEmployeeContribution : 0m,
                             EmployerInsurance = ins != null ? ins.TotalEmployerContribution : 0m,
                             run.NetAmount
                         };

        var grouped = from item in runDetails
                      group item by new { item.DepartmentId, item.DepartmentName } into g
                      select new PayrollCostDepartmentReportItem
                      {
                          DepartmentId = g.Key.DepartmentId,
                          DepartmentName = g.Key.DepartmentName,
                          EmployeeCount = g.Count(),
                          TotalGrossIncome = g.Sum(x => x.GrossAmount),
                          TotalTax = g.Sum(x => x.TaxAmount),
                          TotalEmployeeInsurance = g.Sum(x => x.EmployeeInsurance),
                          TotalEmployerInsurance = g.Sum(x => x.EmployerInsurance),
                          TotalNetPay = g.Sum(x => x.NetAmount)
                      };

        var descending = IsDescending(filter.SortDirection);
        return filter.SortBy?.ToLowerInvariant() switch
        {
            "departmentname" => descending
                ? grouped.OrderByDescending(x => x.DepartmentName).ThenByDescending(x => x.TotalNetPay)
                : grouped.OrderBy(x => x.DepartmentName).ThenBy(x => x.TotalNetPay),
            "employeecount" => descending
                ? grouped.OrderByDescending(x => x.EmployeeCount).ThenByDescending(x => x.DepartmentName)
                : grouped.OrderBy(x => x.EmployeeCount).ThenBy(x => x.DepartmentName),
            "totalnetpay" => descending
                ? grouped.OrderByDescending(x => x.TotalNetPay).ThenByDescending(x => x.DepartmentName)
                : grouped.OrderBy(x => x.TotalNetPay).ThenBy(x => x.DepartmentName),
            _ => descending
                ? grouped.OrderByDescending(x => x.DepartmentName)
                : grouped.OrderBy(x => x.DepartmentName)
        };
    }

    public static IQueryable<PayrollCostEmployeeReportItem> BuildPayrollCostEmployee(
        IQueryable<PayrollRun> runs,
        IQueryable<PayrollPeriod> periods,
        IQueryable<Employee> employees,
        IQueryable<Department> departments,
        IQueryable<Position> positions,
        IQueryable<TaxCalculationSnapshot> taxSnapshots,
        IQueryable<InsuranceCalculationSnapshot> insuranceSnapshots,
        IQueryable<Payslip> payslips,
        PayrollReportingFilter filter)
    {
        var filteredRuns = ApplyBaseFilters(runs, employees, filter);

        var projected = from run in filteredRuns
                        join period in periods on run.PayrollPeriodId equals period.Id
                        join emp in employees on run.EmployeeId equals emp.Id
                        join dept in departments on emp.PrimaryDepartmentId equals dept.Id into deptGroup
                        from dept in deptGroup.DefaultIfEmpty()
                        join pos in positions on emp.PrimaryPositionId equals pos.Id into posGroup
                        from pos in posGroup.DefaultIfEmpty()
                        join tax in taxSnapshots on run.Id equals tax.PayrollRunId into taxGroup
                        from tax in taxGroup.DefaultIfEmpty()
                        join ins in insuranceSnapshots on new { run.EmployeeId, Start = period.StartDate, End = period.EndDate } equals new { ins.EmployeeId, Start = ins.CalculationPeriodStart, End = ins.CalculationPeriodEnd } into insGroup
                        from ins in insGroup.DefaultIfEmpty()
                        join ps in payslips on run.Id equals ps.PayrollRunId into psGroup
                        from ps in psGroup.DefaultIfEmpty()
                        select new PayrollCostEmployeeReportItem
                        {
                            EmployeeId = run.EmployeeId.Value,
                            EmployeeCode = run.EmployeeCode,
                            EmployeeName = run.EmployeeName,
                            DepartmentName = dept != null ? dept.Name : "No Department",
                            PositionName = pos != null ? pos.Name : "No Position",
                            GrossIncome = run.GrossAmount,
                            TaxableIncome = tax != null ? tax.TaxableIncomeSnapshot : 0m,
                            EmployeeTax = tax != null ? tax.TotalTaxAmount : 0m,
                            EmployeeInsurance = ins != null ? ins.TotalEmployeeContribution : 0m,
                            EmployerInsurance = ins != null ? ins.TotalEmployerContribution : 0m,
                            TotalDeductions = run.TotalDeductionAmount,
                            NetPay = run.NetAmount,
                            PayslipStatus = ps != null ? ps.Status.ToString() : "NotGenerated",
                            PayslipPublishedAt = ps != null ? ps.PublishedAt : null
                        };

        var descending = IsDescending(filter.SortDirection);
        return filter.SortBy?.ToLowerInvariant() switch
        {
            "employeecode" => descending
                ? projected.OrderByDescending(x => x.EmployeeCode).ThenByDescending(x => x.EmployeeName)
                : projected.OrderBy(x => x.EmployeeCode).ThenBy(x => x.EmployeeName),
            "employeename" => descending
                ? projected.OrderByDescending(x => x.EmployeeName).ThenByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.EmployeeName).ThenBy(x => x.EmployeeCode),
            "netpay" => descending
                ? projected.OrderByDescending(x => x.NetPay).ThenByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.NetPay).ThenBy(x => x.EmployeeCode),
            _ => descending
                ? projected.OrderByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.EmployeeCode)
        };
    }

    public static IQueryable<PayrollVarianceReportItem> BuildPayrollVariance(
        IQueryable<PayrollRun> runs,
        IQueryable<PayrollPeriod> periods,
        IQueryable<TaxCalculationSnapshot> taxSnapshots,
        IQueryable<InsuranceCalculationSnapshot> insuranceSnapshots,
        PayrollPeriodId? currentPeriodId,
        PayrollPeriodId? previousPeriodId,
        PayrollRunId? currentRunId,
        PayrollRunId? previousRunId,
        bool includeRemovedEmployees,
        string? sortBy,
        string? sortDirection)
    {
        var currentRuns = runs.AsQueryable();
        if (currentRunId is not null)
            currentRuns = currentRuns.Where(x => x.Id == currentRunId);
        else if (currentPeriodId is not null)
            currentRuns = currentRuns.Where(x => x.PayrollPeriodId == currentPeriodId);
        else
            currentRuns = currentRuns.Where(x => false);

        var previousRuns = runs.AsQueryable();
        if (previousRunId is not null)
            previousRuns = previousRuns.Where(x => x.Id == previousRunId);
        else if (previousPeriodId is not null)
            previousRuns = previousRuns.Where(x => x.PayrollPeriodId == previousPeriodId);
        else
            previousRuns = previousRuns.Where(x => false);

        var currentDetails = from run in currentRuns
                             join period in periods on run.PayrollPeriodId equals period.Id
                             join tax in taxSnapshots on run.Id equals tax.PayrollRunId into taxGroup
                             from tax in taxGroup.DefaultIfEmpty()
                             join ins in insuranceSnapshots on new { run.EmployeeId, Start = period.StartDate, End = period.EndDate } equals new { ins.EmployeeId, Start = ins.CalculationPeriodStart, End = ins.CalculationPeriodEnd } into insGroup
                             from ins in insGroup.DefaultIfEmpty()
                             select new
                             {
                                 run.Id,
                                 run.EmployeeId,
                                 run.EmployeeCode,
                                 run.EmployeeName,
                                 run.GrossAmount,
                                 run.NetAmount,
                                 TaxAmount = tax != null ? tax.TotalTaxAmount : 0m,
                                 InsuranceAmount = ins != null ? ins.TotalEmployeeContribution : 0m
                             };

        var previousDetails = from run in previousRuns
                              join period in periods on run.PayrollPeriodId equals period.Id
                              join tax in taxSnapshots on run.Id equals tax.PayrollRunId into taxGroup
                              from tax in taxGroup.DefaultIfEmpty()
                              join ins in insuranceSnapshots on new { run.EmployeeId, Start = period.StartDate, End = period.EndDate } equals new { ins.EmployeeId, Start = ins.CalculationPeriodStart, End = ins.CalculationPeriodEnd } into insGroup
                              from ins in insGroup.DefaultIfEmpty()
                              select new
                              {
                                  run.Id,
                                  run.EmployeeId,
                                  run.EmployeeCode,
                                  run.EmployeeName,
                                  run.GrossAmount,
                                  run.NetAmount,
                                  TaxAmount = tax != null ? tax.TotalTaxAmount : 0m,
                                  InsuranceAmount = ins != null ? ins.TotalEmployeeContribution : 0m
                              };

        var leftJoin = from curr in currentDetails
                       join prev in previousDetails on curr.EmployeeId equals prev.EmployeeId into prevGroup
                       from prev in prevGroup.DefaultIfEmpty()
                       select new PayrollVarianceReportItem
                       {
                           CurrentPayrollRunId = curr.Id.Value,
                           PreviousPayrollRunId = prev != null ? (Guid?)prev.Id.Value : null,
                           EmployeeId = curr.EmployeeId.Value,
                           EmployeeCode = curr.EmployeeCode,
                           EmployeeName = curr.EmployeeName,
                           CurrentGrossIncome = curr.GrossAmount,
                           PreviousGrossIncome = prev != null ? prev.GrossAmount : 0m,
                           GrossIncomeDifference = curr.GrossAmount - (prev != null ? prev.GrossAmount : 0m),
                           CurrentNetPay = curr.NetAmount,
                           PreviousNetPay = prev != null ? prev.NetAmount : 0m,
                           NetPayDifference = curr.NetAmount - (prev != null ? prev.NetAmount : 0m),
                           CurrentTax = curr.TaxAmount,
                           PreviousTax = prev != null ? prev.TaxAmount : 0m,
                           TaxDifference = curr.TaxAmount - (prev != null ? prev.TaxAmount : 0m),
                           CurrentInsurance = curr.InsuranceAmount,
                           PreviousInsurance = prev != null ? prev.InsuranceAmount : 0m,
                           InsuranceDifference = curr.InsuranceAmount - (prev != null ? prev.InsuranceAmount : 0m)
                       };

        IQueryable<PayrollVarianceReportItem> varianceQuery;

        if (includeRemovedEmployees)
        {
            var rightJoin = from prev in previousDetails
                            where !currentDetails.Select(x => x.EmployeeId).Contains(prev.EmployeeId)
                            select new PayrollVarianceReportItem
                            {
                                CurrentPayrollRunId = null,
                                PreviousPayrollRunId = prev.Id.Value,
                                EmployeeId = prev.EmployeeId.Value,
                                EmployeeCode = prev.EmployeeCode,
                                EmployeeName = prev.EmployeeName,
                                CurrentGrossIncome = 0m,
                                PreviousGrossIncome = prev.GrossAmount,
                                GrossIncomeDifference = -prev.GrossAmount,
                                CurrentNetPay = 0m,
                                PreviousNetPay = prev.NetAmount,
                                NetPayDifference = -prev.NetAmount,
                                CurrentTax = 0m,
                                PreviousTax = prev.TaxAmount,
                                TaxDifference = -prev.TaxAmount,
                                CurrentInsurance = 0m,
                                PreviousInsurance = prev.InsuranceAmount,
                                InsuranceDifference = -prev.InsuranceAmount
                            };

            varianceQuery = leftJoin.Concat(rightJoin);
        }
        else
        {
            varianceQuery = leftJoin;
        }

        var descending = IsDescending(sortDirection);
        return sortBy?.ToLowerInvariant() switch
        {
            "employeecode" => descending
                ? varianceQuery.OrderByDescending(x => x.EmployeeCode).ThenByDescending(x => x.EmployeeName)
                : varianceQuery.OrderBy(x => x.EmployeeCode).ThenBy(x => x.EmployeeName),
            "employeename" => descending
                ? varianceQuery.OrderByDescending(x => x.EmployeeName).ThenByDescending(x => x.EmployeeCode)
                : varianceQuery.OrderBy(x => x.EmployeeName).ThenBy(x => x.EmployeeCode),
            "netpaydifference" => descending
                ? varianceQuery.OrderByDescending(x => x.NetPayDifference).ThenByDescending(x => x.EmployeeCode)
                : varianceQuery.OrderBy(x => x.NetPayDifference).ThenBy(x => x.EmployeeCode),
            _ => descending
                ? varianceQuery.OrderByDescending(x => x.EmployeeCode)
                : varianceQuery.OrderBy(x => x.EmployeeCode)
        };
    }

    public static IQueryable<PayslipDeliveryReportItem> BuildPayslipDeliveryReport(
        IQueryable<PayrollRun> runs,
        IQueryable<Employee> employees,
        IQueryable<Payslip> payslips,
        IQueryable<PayslipDocument> payslipDocuments,
        IQueryable<PayslipEmailDelivery> emailDeliveries,
        PayrollReportingFilter filter)
    {
        var filteredRuns = ApplyBaseFilters(runs, employees, filter);

        var projected = from run in filteredRuns
                        join ps in payslips on run.Id equals ps.PayrollRunId into psGroup
                        from ps in psGroup.DefaultIfEmpty()
                        let doc = payslipDocuments.Where(d => d.PayslipId == ps.Id && d.IsActive).OrderByDescending(d => d.CreatedAt).FirstOrDefault()
                        let email = emailDeliveries.Where(e => e.PayslipId == ps.Id).OrderByDescending(e => e.CreatedAt).FirstOrDefault()
                        select new PayslipDeliveryReportItem
                        {
                            PayrollRunId = run.Id.Value,
                            EmployeeId = run.EmployeeId.Value,
                            EmployeeCode = run.EmployeeCode,
                            EmployeeName = run.EmployeeName,
                            PayslipStatus = ps != null ? ps.Status.ToString() : "NotGenerated",
                            PdfGenerated = doc != null,
                            PublishedAt = ps != null ? ps.PublishedAt : null,
                            EmailSent = email != null && email.Status == PayslipEmailDeliveryStatus.Sent,
                            EmailSentAt = email != null ? email.SentAt : null,
                            EmailFailureReason = email != null ? email.ErrorMessage : null,
                            DownloadAvailable = doc != null
                        };

        var descending = IsDescending(filter.SortDirection);
        return filter.SortBy?.ToLowerInvariant() switch
        {
            "employeecode" => descending
                ? projected.OrderByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.EmployeeCode),
            "employeename" => descending
                ? projected.OrderByDescending(x => x.EmployeeName)
                : projected.OrderBy(x => x.EmployeeName),
            "payslipstatus" => descending
                ? projected.OrderByDescending(x => x.PayslipStatus)
                : projected.OrderBy(x => x.PayslipStatus),
            _ => descending
                ? projected.OrderByDescending(x => x.EmployeeCode)
                : projected.OrderBy(x => x.EmployeeCode)
        };
    }

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
        string.Equals(sortDirection, SortDirectionConstants.Desc, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(sortDirection, SortDirectionConstants.Descending, StringComparison.OrdinalIgnoreCase);
}
