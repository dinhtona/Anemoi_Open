using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Application.Services;
using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.AttendanceQueries.GetEmployeeAttendanceSummary;

public sealed class GetEmployeeAttendanceSummaryHandler(
    ISqlRepository<AttendancePeriod> attendancePeriodRepository,
    ISqlRepository<AttendanceRecord> attendanceRecordRepository,
    ISqlRepository<Employee> employeeRepository)
    : IQueryHandler<GetEmployeeAttendanceSummaryQuery, OneOf<AttendanceSummaryResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AttendanceSummaryResponse, ErrorDetailResponse>> Handle(
        GetEmployeeAttendanceSummaryQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validate period
        var periodExists = await attendancePeriodRepository.ExistByConditionAsync(
            x => x.Id == request.AttendancePeriodId,
            cancellationToken);

        if (!periodExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodNotFound);

        // 2. Validate employee
        var employeeExists = await employeeRepository.ExistByConditionAsync(
            x => x.Id == request.EmployeeId,
            cancellationToken);

        if (!employeeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        // 3. Fetch records and aggregate
        var records = await attendanceRecordRepository.GetManyByConditionAsync(
            x => x.AttendancePeriodId == request.AttendancePeriodId && x.EmployeeId == request.EmployeeId,
            null,
            cancellationToken);

        var totals = AttendanceSummaryCalculator.Calculate(records);

        return new AttendanceSummaryResponse
        {
            WorkedDays = totals.WorkedDays,
            WorkedHours = totals.WorkedHours,
            LeaveDays = totals.LeaveDays,
            AbsentDays = totals.AbsentDays,
            HolidayDays = totals.HolidayDays,
            PaidWorkingDays = totals.PaidWorkingDays,
            PaidLeaveDays = totals.PaidLeaveDays,
            UnpaidLeaveDays = totals.UnpaidLeaveDays
        };
    }
}
