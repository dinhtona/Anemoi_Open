using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Responses;

public sealed record WorkflowRoleAssignmentResponse(
    WorkflowRoleAssignmentId Id,
    string Role,
    EmployeeId EmployeeId,
    string EmployeeName,
    string EmployeeCode,
    string DepartmentName,
    string PositionName,
    DateTime CreatedAt);
