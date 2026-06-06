using Anemoi.Hr.Domain.Compensation;
using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeSalaryResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string SalaryGradeId { get; set; }
    public string GradeCodeSnapshot { get; set; }
    public decimal BaseSalary { get; set; }
    public SalaryType SalaryType { get; set; }
    public string Currency { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public CompensationChangeReason Reason { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
