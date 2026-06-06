using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class CompensationDashboardResponse
{
    public IReadOnlyCollection<PayrollProjectionByCurrencyResponse> PayrollProjectionByCurrency { get; set; } = [];
    public Dictionary<string, decimal> CostByGrade { get; set; } = [];
    public IReadOnlyCollection<AllowanceCostByTypeResponse> AllowanceCostByType { get; set; } = [];
    public IReadOnlyCollection<string> MissingSalaryEmployeeIds { get; set; } = [];
    public IReadOnlyCollection<SalaryValidationBypassLogResponse> RecentBypasses { get; set; } = [];
}

public sealed class PayrollProjectionByCurrencyResponse
{
    public string Currency { get; set; }
    public decimal MonthlySalaryTotal { get; set; }
    public decimal DailySalaryTotal { get; set; }
    public decimal AllowanceTotal { get; set; }
    public decimal Total { get; set; }
}

public sealed class AllowanceCostByTypeResponse
{
    public string AllowanceTypeCode { get; set; }
    public string Currency { get; set; }
    public decimal Total { get; set; }
}
