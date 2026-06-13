using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;
using Riok.Mapperly.Abstractions;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class OvertimeMapper
{
    public OvertimeRequestResponse ToOvertimeRequestResponse(OvertimeRequest source)
    {
        return new OvertimeRequestResponse
        {
            Id = source.Id.Value,
            EmployeeId = source.EmployeeId.Value,
            EmployeeCode = source.Employee?.EmployeeCode,
            EmployeeName = source.Employee?.FullName,
            OvertimeDate = source.OvertimeDate,
            StartTime = source.StartTime,
            EndTime = source.EndTime,
            DurationHours = source.CalculateDurationHours(),
            Reason = source.Reason,
            Status = source.Status,
            ApprovedBy = source.ApprovedBy,
            ApprovedAt = source.ApprovedAt,
            RejectedBy = source.RejectedBy,
            RejectedAt = source.RejectedAt,
            CancelledBy = source.CancelledBy,
            CancelledAt = source.CancelledAt,
            CreatedAt = source.CreatedAt
        };
    }

    public OvertimeRequestIdResponse ToOvertimeRequestIdResponse(OvertimeRequest source)
    {
        return new OvertimeRequestIdResponse
        {
            Id = source.Id.Value,
            CreatedAt = source.CreatedAt
        };
    }
}
