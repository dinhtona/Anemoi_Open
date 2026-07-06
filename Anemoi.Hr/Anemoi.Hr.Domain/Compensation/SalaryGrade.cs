using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Compensation;

public sealed class SalaryGrade : ValueObject
{
    public SalaryGradeId Id { get; set; }
    public string GradeCode { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SalaryRange> Ranges { get; set; } = [];

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
