using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.ShiftManagement;
using Riok.Mapperly.Abstractions;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class ShiftManagementMapper
{
    public ShiftTemplateResponse ToShiftTemplateResponse(ShiftTemplate source)
    {
        return new ShiftTemplateResponse
        {
            Id = source.Id.Value,
            Code = source.Code,
            Name = source.Name,
            StartTime = source.StartTime,
            EndTime = source.EndTime,
            BreakMinutes = source.BreakMinutes,
            ExpectedWorkingHours = source.ExpectedWorkingHours,
            IsActive = source.IsActive,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public ShiftTemplateIdResponse ToShiftTemplateIdResponse(ShiftTemplate source)
    {
        return new ShiftTemplateIdResponse
        {
            Id = source.Id.Value,
            CreatedAt = source.CreatedAt
        };
    }

    public EmployeeShiftAssignmentResponse ToEmployeeShiftAssignmentResponse(EmployeeShiftAssignment source)
    {
        return new EmployeeShiftAssignmentResponse
        {
            Id = source.Id.Value,
            EmployeeId = source.EmployeeId.Value,
            EmployeeCode = source.Employee?.EmployeeCode,
            EmployeeName = source.Employee?.FullName,
            ShiftTemplateId = source.ShiftTemplateId.Value,
            ShiftTemplateCode = source.ShiftTemplate?.Code,
            ShiftTemplateName = source.ShiftTemplate?.Name,
            WorkDate = source.WorkDate,
            ShiftNameSnapshot = source.ShiftNameSnapshot,
            StartTimeSnapshot = source.StartTimeSnapshot,
            EndTimeSnapshot = source.EndTimeSnapshot,
            BreakMinutesSnapshot = source.BreakMinutesSnapshot,
            ExpectedWorkingHoursSnapshot = source.ExpectedWorkingHoursSnapshot,
            Status = source.Status,
            AssignedBy = source.AssignedBy,
            AssignedAt = source.AssignedAt,
            CancelledBy = source.CancelledBy,
            CancelledAt = source.CancelledAt,
            CancellationReason = source.CancellationReason
        };
    }
}
