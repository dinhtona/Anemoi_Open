using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Payroll;

public sealed class PayrollPeriod : ValueObject
{
    public PayrollPeriodId Id { get; set; }
    public string PeriodCode { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StatusCode { get; set; }
    public decimal StandardWorkingDays { get; set; }
    public AttendancePeriodId? AttendancePeriodId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }

    // Navigation property
    public AttendancePeriod AttendancePeriod { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
