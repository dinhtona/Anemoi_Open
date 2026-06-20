using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;

namespace Anemoi.Hr.Application.Mappings;

public sealed class WorkflowRoleAssignmentMapper
{
    public WorkflowRoleAssignmentResponse ToResponse(WorkflowRoleAssignment assignment)
    {
        if (assignment is null) return null;
        return new WorkflowRoleAssignmentResponse(
            Id: assignment.Id,
            Role: assignment.Role,
            EmployeeId: assignment.EmployeeId,
            EmployeeName: assignment.Employee?.FullName,
            EmployeeCode: assignment.Employee?.EmployeeCode,
            DepartmentName: assignment.Employee?.PrimaryDepartment?.Name,
            PositionName: assignment.Employee?.PrimaryPosition?.Name,
            CreatedAt: assignment.CreatedAt);
    }
}
