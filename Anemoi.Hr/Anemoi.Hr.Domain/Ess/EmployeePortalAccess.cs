using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Ess;

public sealed class EmployeePortalAccess : Entity<EmployeePortalAccessId>
{
    public EmployeeId EmployeeId { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LastAccessAt { get; private set; }

    private EmployeePortalAccess() { }

    public static EmployeePortalAccess Create(EmployeeId employeeId)
    {
        return new EmployeePortalAccess
        {
            Id = new EmployeePortalAccessId(Guid.NewGuid()),
            EmployeeId = employeeId
        };
    }

    public void RecordLogin()
    {
        var now = DateTime.UtcNow;
        LastLoginAt = now;
        LastAccessAt = now;
    }

    public void RecordAccess()
    {
        LastAccessAt = DateTime.UtcNow;
    }
}
