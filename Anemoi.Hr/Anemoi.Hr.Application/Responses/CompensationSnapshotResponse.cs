using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class CompensationSnapshotResponse
{
    public EmployeeSalaryResponse ActiveSalary { get; set; }
    public IReadOnlyCollection<EmployeeAllowanceResponse> ActiveAllowances { get; set; } = [];
    public decimal TotalMonthlyCost { get; set; }
}
