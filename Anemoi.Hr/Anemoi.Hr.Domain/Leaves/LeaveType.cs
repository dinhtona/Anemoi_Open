using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Leaves;

public sealed class LeaveType : Entity<LeaveTypeId>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public bool IsPaid { get; private set; }
    public bool RequiresApproval { get; private set; }
    public decimal AnnualEntitlement { get; private set; }
    public bool CarryForwardAllowed { get; private set; }
    public decimal MaxCarryForwardDays { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private LeaveType() { }

    public static LeaveType Create(
        LeaveTypeId id,
        string code,
        string name,
        bool isPaid,
        bool requiresApproval,
        decimal annualEntitlement,
        bool carryForwardAllowed,
        decimal maxCarryForwardDays)
    {
        return new LeaveType
        {
            Id = id,
            Code = code,
            Name = name,
            IsPaid = isPaid,
            RequiresApproval = requiresApproval,
            AnnualEntitlement = annualEntitlement,
            CarryForwardAllowed = carryForwardAllowed,
            MaxCarryForwardDays = maxCarryForwardDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateInfo(
        string code,
        string name,
        bool isPaid,
        bool requiresApproval,
        decimal annualEntitlement,
        bool carryForwardAllowed,
        decimal maxCarryForwardDays)
    {
        Code = code;
        Name = name;
        IsPaid = isPaid;
        RequiresApproval = requiresApproval;
        AnnualEntitlement = annualEntitlement;
        CarryForwardAllowed = carryForwardAllowed;
        MaxCarryForwardDays = maxCarryForwardDays;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
