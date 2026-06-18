using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowRoleAssignment : ValueObject
{
    public WorkflowRoleAssignmentId Id { get; set; }
    public string Role { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public DateTime CreatedAt { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
