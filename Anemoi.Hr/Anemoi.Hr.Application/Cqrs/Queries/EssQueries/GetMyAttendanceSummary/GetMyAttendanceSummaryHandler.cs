using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyAttendanceSummary;

public sealed class GetMyAttendanceSummaryHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<AttendanceSummary> attendanceSummaryRepository)
    : IQueryHandler<GetMyAttendanceSummaryQuery, OneOf<IReadOnlyCollection<EssAttendanceSummaryResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<EssAttendanceSummaryResponse>, ErrorDetailResponse>> Handle(
        GetMyAttendanceSummaryQuery request, CancellationToken cancellationToken)
    {
        Employee employee = null;
        if (Guid.TryParse(request.UserId, out var identityUserId))
            employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.IdentityUserId == identityUserId, null, cancellationToken);

        if (employee is null && !string.IsNullOrEmpty(request.Email))
        {
            var searchEmail = request.Email.ToLower();
            employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.WorkEmail != null && x.WorkEmail.ToLower() == searchEmail, null, cancellationToken);
        }

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var summaries = await attendanceSummaryRepository.GetManyByConditionAsync(
            x => x.EmployeeId == employee.Id,
            query => query.Include(s => s.AttendancePeriod),
            cancellationToken);

        var results = summaries.Select(s => new EssAttendanceSummaryResponse
        {
            WorkedDays = s.WorkedDays,
            LeaveDays = s.LeaveDays,
            AbsentDays = s.AbsentDays,
            HolidayDays = s.HolidayDays,
            WorkedHours = s.WorkedHours,
            PaidWorkingDays = s.PaidWorkingDays,
            UnpaidLeaveDays = s.UnpaidLeaveDays,
            PaidLeaveDays = s.PaidLeaveDays
        }).ToList();

        return results;
    }
}
