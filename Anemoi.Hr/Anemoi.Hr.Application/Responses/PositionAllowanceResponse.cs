using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class PositionAllowanceResponse
{
    public string Id { get; set; }
    public string PositionId { get; set; }
    public string AllowanceTypeId { get; set; }
    public string AllowanceTypeCode { get; set; }
    public string AllowanceTypeName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
