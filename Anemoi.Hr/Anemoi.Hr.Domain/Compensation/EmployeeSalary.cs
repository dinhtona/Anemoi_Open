using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Compensation;

public sealed class EmployeeSalary : ValueObject
{
    public EmployeeSalaryId Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public SalaryGradeId? SalaryGradeId { get; set; }
    public string GradeCodeSnapshot { get; set; }
    public decimal BaseSalary { get; set; }
    public SalaryType SalaryType { get; set; }
    public string Currency { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public CompensationChangeReason Reason { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Employee Employee { get; set; }
    public SalaryGrade SalaryGrade { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
