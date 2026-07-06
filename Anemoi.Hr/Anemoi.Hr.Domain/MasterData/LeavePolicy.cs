using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.MasterData;

public sealed class LeavePolicy : Entity<LeavePolicyId>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public LeaveTypeId LeaveTypeId { get; private set; }
    public string ApplicableGradeCode { get; private set; }
    public decimal AnnualEntitlement { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public LeaveType LeaveType { get; private set; }

    private LeavePolicy() { }

    public static LeavePolicy Create(
        LeavePolicyId id,
        string code,
        string name,
        LeaveTypeId leaveTypeId,
        string applicableGradeCode,
        decimal annualEntitlement)
    {
        return new LeavePolicy
        {
            Id = id,
            Code = code,
            Name = name,
            LeaveTypeId = leaveTypeId,
            ApplicableGradeCode = applicableGradeCode,
            AnnualEntitlement = annualEntitlement,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateInfo(
        string code,
        string name,
        LeaveTypeId leaveTypeId,
        string applicableGradeCode,
        decimal annualEntitlement)
    {
        Code = code;
        Name = name;
        LeaveTypeId = leaveTypeId;
        ApplicableGradeCode = applicableGradeCode;
        AnnualEntitlement = annualEntitlement;
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
