using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class SalaryGradeResponse
{
    public string Id { get; set; }
    public string GradeCode { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyCollection<SalaryRangeResponse> Ranges { get; set; } = [];
}
