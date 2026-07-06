using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Organization;

public sealed record OrganizationNode(
    EmployeeId Id,
    string FullName,
    string EmployeeCode,
    EmployeeId? ManagerId,
    string? ManagerName,
    DepartmentId DepartmentId,
    string DepartmentName,
    string PositionName,
    string GradeCode,
    List<OrganizationNode> DirectReports);
