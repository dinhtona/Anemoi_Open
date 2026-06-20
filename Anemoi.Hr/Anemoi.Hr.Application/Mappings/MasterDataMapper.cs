using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.Domain.Overtime;
using Riok.Mapperly.Abstractions;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class MasterDataMapper
{
    public LeaveTypeResponse ToLeaveTypeResponse(LeaveType entity)
    {
        if (entity is null) return null;
        return new LeaveTypeResponse
        {
            Id = entity.Id.Value.ToString(),
            Code = entity.Code,
            Name = entity.Name,
            IsPaid = entity.IsPaid,
            RequiresApproval = entity.RequiresApproval,
            AnnualEntitlement = entity.AnnualEntitlement,
            CarryForwardAllowed = entity.CarryForwardAllowed,
            MaxCarryForwardDays = entity.MaxCarryForwardDays,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public LeavePolicyResponse ToLeavePolicyResponse(Anemoi.Hr.Domain.MasterData.LeavePolicy entity)
    {
        if (entity is null) return null;
        return new LeavePolicyResponse
        {
            Id = entity.Id.Value.ToString(),
            Code = entity.Code,
            Name = entity.Name,
            LeaveTypeId = entity.LeaveTypeId.Value.ToString(),
            LeaveTypeCode = entity.LeaveType?.Code,
            LeaveTypeName = entity.LeaveType != null ? entity.LeaveType.Name : null,
            ApplicableGradeCode = entity.ApplicableGradeCode,
            AnnualEntitlement = entity.AnnualEntitlement,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public OvertimeRuleResponse ToOvertimeRuleResponse(OvertimeRule entity)
    {
        if (entity is null) return null;
        return new OvertimeRuleResponse
        {
            Id = entity.Id.Value.ToString(),
            Code = entity.Code,
            Name = entity.Name,
            WeekdayMultiplier = entity.WeekdayMultiplier,
            WeekendMultiplier = entity.WeekendMultiplier,
            HolidayMultiplier = entity.HolidayMultiplier,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
