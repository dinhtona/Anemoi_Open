#nullable enable

using System;

namespace Anemoi.Hr.Application.Responses;

public sealed record PayrollCostEmployeeReportItem
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = default!;
    public string EmployeeName { get; init; } = default!;
    public string DepartmentName { get; init; } = default!;
    public string PositionName { get; init; } = default!;
    public decimal GrossIncome { get; init; }
    public decimal TaxableIncome { get; init; }
    public decimal EmployeeTax { get; init; }
    public decimal EmployeeInsurance { get; init; }
    public decimal EmployerInsurance { get; init; }
    public decimal TotalDeductions { get; init; }
    public decimal NetPay { get; init; }
    public string PayslipStatus { get; init; } = default!;
    public DateTime? PayslipPublishedAt { get; init; }
}
