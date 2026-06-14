#nullable enable

using System;
using System.Text.Json.Serialization;
using Anemoi.Hr.Domain.Payroll;

namespace Anemoi.Hr.Application.Responses;

public sealed record PayrollCostSummaryReportItem
{
    public Guid PayrollPeriodId { get; init; }
    public string PeriodCode { get; init; } = default!;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PayrollRunStatus Status { get; init; }
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
