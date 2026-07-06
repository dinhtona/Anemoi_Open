using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeAllowanceResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string AllowanceTypeId { get; set; }
    public string AllowanceTypeCode { get; set; }
    public string AllowanceTypeName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
