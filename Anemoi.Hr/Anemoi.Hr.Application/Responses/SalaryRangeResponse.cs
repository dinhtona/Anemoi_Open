using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class SalaryRangeResponse
{
    public string Id { get; set; }
    public string SalaryGradeId { get; set; }
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public string Currency { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}
