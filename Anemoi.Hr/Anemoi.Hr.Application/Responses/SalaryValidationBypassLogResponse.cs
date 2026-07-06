using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class SalaryValidationBypassLogResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string GradeCode { get; set; }
    public decimal RequestedSalary { get; set; }
    public string Currency { get; set; }
    public string BypassReason { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
