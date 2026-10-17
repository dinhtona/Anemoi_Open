using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class OrganizationNodeResponse
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string EmployeeCode { get; set; }
    public string ManagerId { get; set; }
    public string ManagerName { get; set; }
    public string DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public string PositionName { get; set; }
    public string GradeCode { get; set; }
    public List<OrganizationNodeResponse> DirectReports { get; set; } = new();
}
