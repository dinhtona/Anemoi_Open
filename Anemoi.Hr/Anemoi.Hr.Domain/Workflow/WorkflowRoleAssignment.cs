using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowRoleAssignment : ValueObject
{
    public WorkflowRoleAssignmentId Id { get; set; }
    public string Role { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Employee Employee { get; set; }

    public static WorkflowRoleAssignment Create(WorkflowRoleAssignmentId id, string role, EmployeeId employeeId)
    {
        return new WorkflowRoleAssignment
        {
            Id = id,
            Role = role,
            EmployeeId = employeeId,
            CreatedAt = DateTime.UtcNow
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
