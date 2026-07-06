#nullable enable

using System;

namespace Anemoi.Hr.Application.Responses;

public sealed record PayrollCostDepartmentReportItem
{
    public Guid? DepartmentId { get; init; }
    public string DepartmentName { get; init; } = default!;
    public int EmployeeCount { get; init; }
    public decimal TotalGrossIncome { get; init; }
    public decimal TotalTax { get; init; }
    public decimal TotalEmployeeInsurance { get; init; }
    public decimal TotalEmployerInsurance { get; init; }
    public decimal TotalNetPay { get; init; }
}
