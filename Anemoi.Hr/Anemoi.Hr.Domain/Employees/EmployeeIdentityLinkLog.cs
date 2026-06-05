using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Employees;

public sealed class EmployeeIdentityLinkLog : ValueObject
{
    public EmployeeIdentityLinkLogId Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public string WorkEmail { get; set; }
    public Guid? IdentityUserId { get; set; }
    public string MatchStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }

    public Employee Employee { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
