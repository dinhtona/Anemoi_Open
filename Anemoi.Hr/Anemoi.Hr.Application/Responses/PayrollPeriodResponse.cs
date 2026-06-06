using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class PayrollPeriodResponse
{
    public string Id { get; set; }
    public string PeriodCode { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StatusCode { get; set; }
    public decimal StandardWorkingDays { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
}
