using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class PositionResponse
{
    public string Id { get; set; }
    public string DepartmentId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string PositionTypeCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
