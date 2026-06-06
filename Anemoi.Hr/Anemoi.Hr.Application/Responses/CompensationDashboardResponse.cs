using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class CompensationDashboardResponse
{
    public decimal TotalMonthlyPayrollProjection { get; set; }
    public Dictionary<string, decimal> CostByGrade { get; set; } = [];
    public IReadOnlyCollection<string> MissingSalaryEmployeeIds { get; set; } = [];
    public IReadOnlyCollection<SalaryValidationBypassLogResponse> RecentBypasses { get; set; } = [];
}
