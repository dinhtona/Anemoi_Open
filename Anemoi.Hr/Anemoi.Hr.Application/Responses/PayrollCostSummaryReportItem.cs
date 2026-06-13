#nullable enable

using System;

namespace Anemoi.Hr.Application.Responses;

public sealed record PayrollCostSummaryReportItem
{
    public Guid PayrollPeriodId { get; init; }
    public string PeriodCode { get; init; } = default!;
    public string Status { get; init; } = default!;
    public int EmployeeCount { get; init; }
    public decimal TotalGrossIncome { get; init; }
    public decimal TotalTaxableIncome { get; init; }
    public decimal TotalEmployeeTax { get; init; }
    public decimal TotalEmployeeInsurance { get; init; }
    public decimal TotalEmployerInsurance { get; init; }
    public decimal TotalDeductions { get; init; }
    public decimal TotalNetPay { get; init; }
    public DateTime? FinalizedAt { get; init; }
}
