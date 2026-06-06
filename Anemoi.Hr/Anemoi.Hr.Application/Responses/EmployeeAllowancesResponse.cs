using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeAllowancesResponse
{
    public IReadOnlyCollection<EmployeeAllowanceResponse> DirectAllowances { get; set; } = [];
    public IReadOnlyCollection<PositionAllowanceResponse> PositionAllowances { get; set; } = [];
}
