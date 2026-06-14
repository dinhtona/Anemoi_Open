using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Attendance;
using System.Collections.Generic;
using System.Linq;

namespace Anemoi.Hr.Application.Services;

public static class AttendanceSummaryCalculator
{
    public static AttendanceSummaryTotals Calculate(IEnumerable<AttendanceRecord> records)
    {
        var attendanceRecords = records.ToList();

        return new AttendanceSummaryTotals(
            attendanceRecords.Sum(x => x.WorkedDays),
            attendanceRecords.Sum(x => x.WorkedHours),
            attendanceRecords.Where(x => x.Status == AttendanceStatusCodes.Leave).Sum(x => x.WorkedDays),
            attendanceRecords.Where(x => x.Status == AttendanceStatusCodes.Absent).Sum(x => x.WorkedDays),
            attendanceRecords.Where(x => x.Status == AttendanceStatusCodes.Holiday).Sum(x => x.WorkedDays),
            attendanceRecords.Sum(x => x.WorkedDays),
            0,
            0);
    }
}

public sealed record AttendanceSummaryTotals(
    decimal WorkedDays,
    decimal WorkedHours,
    decimal LeaveDays,
    decimal AbsentDays,
    decimal HolidayDays,
    decimal PaidWorkingDays,
    decimal PaidLeaveDays,
    decimal UnpaidLeaveDays);
