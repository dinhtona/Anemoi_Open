using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class DepartmentResponse
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string DepartmentTypeCode { get; set; }
    public string ParentDepartmentId { get; set; }
    public string ManagerEmployeeId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
