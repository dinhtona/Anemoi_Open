using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAttendanceAnalytics;

public sealed class GetAttendanceAnalyticsHandler(
    ISqlRepository<AttendanceSummary> attendanceRepository)
    : IQueryHandler<GetAttendanceAnalyticsQuery, OneOf<AttendanceAnalyticsResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AttendanceAnalyticsResponse, ErrorDetailResponse>> Handle(
        GetAttendanceAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var query = attendanceRepository.GetQueryable().AsNoTracking();

        var totals = await query
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalWorkedDays = g.Sum(x => x.WorkedDays),
                TotalDays = g.Sum(x => x.WorkedDays + x.LeaveDays + x.AbsentDays + x.HolidayDays),
                AvgPaidDays = g.Average(x => x.PaidWorkingDays + x.PaidLeaveDays),
                AvgUnpaidDays = g.Average(x => x.UnpaidLeaveDays),
                AvgLeaveDays = g.Average(x => x.LeaveDays),
                RecordCount = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (totals is null || totals.RecordCount == 0)
            return new AttendanceAnalyticsResponse(0, 0, 0, 0);

        var attendanceRate = totals.TotalDays > 0
            ? Math.Round(totals.TotalWorkedDays / totals.TotalDays * 100, 2)
            : 0;

        return new AttendanceAnalyticsResponse(
            attendanceRate,
            Math.Round(totals.AvgPaidDays, 2),
            Math.Round(totals.AvgUnpaidDays, 2),
            Math.Round(totals.AvgLeaveDays, 2)
        );
    }
}
