using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class CompensationSnapshotResponse
{
    public EmployeeSalaryResponse ActiveSalary { get; set; }
    public IReadOnlyCollection<EmployeeAllowanceResponse> ActiveAllowances { get; set; } = [];
    public SalaryAmountResponse BaseSalary { get; set; }
    public IReadOnlyCollection<CurrencyTotalResponse> Allowances { get; set; } = [];
    public IReadOnlyCollection<CurrencyTotalResponse> TotalsByCurrency { get; set; } = [];
}

public sealed class SalaryAmountResponse
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}

public sealed class CurrencyTotalResponse
{
    public string Currency { get; set; }
    public decimal TotalAmount { get; set; }
}
