using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeDepartmentHistoryResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string DepartmentId { get; set; }
    public string DepartmentCode { get; set; }
    public string DepartmentName { get; set; }
    public string OldDepartmentId { get; set; }
    public string OldDepartmentCode { get; set; }
    public string OldDepartmentName { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ReasonCode { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
