using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Riok.Mapperly.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class AttendanceMapper
{
    public AttendancePeriodResponse ToResponse(AttendancePeriod period)
    {
        if (period is null) return null;
        return new AttendancePeriodResponse
        {
            Id = period.Id.Value.ToString(),
            PeriodCode = period.PeriodCode,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            StatusCode = period.StatusCode,
            CreatedAt = period.CreatedAt,
            CreatedBy = period.CreatedBy,
            UpdatedAt = period.UpdatedAt,
            UpdatedBy = period.UpdatedBy
        };
    }

    public IReadOnlyCollection<AttendancePeriodResponse> ToResponses(IEnumerable<AttendancePeriod> periods)
    {
        if (periods is null) return [];
        return periods.Select(ToResponse).ToList();
    }

    public AttendanceRecordResponse ToResponse(AttendanceRecord record)
    {
        if (record is null) return null;
        return new AttendanceRecordResponse
        {
            Id = record.Id.Value.ToString(),
            AttendancePeriodId = record.AttendancePeriodId.Value.ToString(),
            EmployeeId = record.EmployeeId.Value.ToString(),
            EmployeeCode = record.Employee?.EmployeeCode,
            EmployeeName = record.Employee?.FullName,
            WorkDate = record.WorkDate,
            CheckInTime = record.CheckInTime,
            CheckOutTime = record.CheckOutTime,
            WorkedHours = record.WorkedHours,
            WorkedDays = record.WorkedDays,
            Status = record.Status,
            LeaveRequestId = record.LeaveRequestId?.Value.ToString(),
            CreatedAt = record.CreatedAt,
            CreatedBy = record.CreatedBy,
            UpdatedAt = record.UpdatedAt,
            UpdatedBy = record.UpdatedBy
        };
    }

    public IReadOnlyCollection<AttendanceRecordResponse> ToResponses(IEnumerable<AttendanceRecord> records)
    {
        if (records is null) return [];
        return records.Select(ToResponse).ToList();
    }

    public AttendanceRecordDetailResponse ToDetailResponse(AttendanceRecord record)
    {
        if (record is null) return null;
        return new AttendanceRecordDetailResponse
        {
            Id = record.Id.Value.ToString(),
            AttendancePeriodId = record.AttendancePeriodId.Value.ToString(),
            EmployeeId = record.EmployeeId.Value.ToString(),
            WorkDate = record.WorkDate,
            CheckInTime = record.CheckInTime,
            CheckOutTime = record.CheckOutTime,
            WorkedHours = record.WorkedHours,
            WorkedDays = record.WorkedDays,
            Status = record.Status,
            LeaveRequestId = record.LeaveRequestId?.Value.ToString(),
            CreatedAt = record.CreatedAt,
            CreatedBy = record.CreatedBy,
            UpdatedAt = record.UpdatedAt,
            UpdatedBy = record.UpdatedBy,
            EmployeeCode = record.Employee?.EmployeeCode,
            EmployeeName = record.Employee?.FullName,
            PeriodCode = record.AttendancePeriod?.PeriodCode
        };
    }

    public IReadOnlyCollection<AttendanceRecordDetailResponse> ToDetailResponses(IEnumerable<AttendanceRecord> records)
    {
        if (records is null) return [];
        return records.Select(ToDetailResponse).ToList();
    }
}
