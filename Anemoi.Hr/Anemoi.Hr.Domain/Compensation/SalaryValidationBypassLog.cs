using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Compensation;

public sealed class SalaryValidationBypassLog : ValueObject
{
    public SalaryValidationBypassLogId Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public string GradeCode { get; set; }
    public decimal RequestedSalary { get; set; }
    public string Currency { get; set; }
    public string BypassReason { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
