using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Compensation;

public sealed class SalaryRange : ValueObject
{
    public SalaryRangeId Id { get; set; }
    public SalaryGradeId SalaryGradeId { get; set; }
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public string Currency { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public SalaryGrade SalaryGrade { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
