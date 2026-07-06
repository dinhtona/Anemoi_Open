using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.ShiftManagement;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetEmployeeScheduleCalendar;

public sealed class GetEmployeeScheduleCalendarHandler(
    ISqlRepository<EmployeeShiftAssignment> assignmentRepository)
    : IQueryHandler<GetEmployeeScheduleCalendarQuery, List<EmployeeScheduleCalendarResponse>>
{
    public async Task<List<EmployeeScheduleCalendarResponse>> Handle(
        GetEmployeeScheduleCalendarQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<EmployeeShiftAssignment> assignmentsQuery = assignmentRepository.GetQueryable()
            .AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.ShiftTemplate);

        if (request.EmployeeId is not null)
            assignmentsQuery = assignmentsQuery.Where(x => x.EmployeeId == request.EmployeeId);

        if (request.FromDate.HasValue)
            assignmentsQuery = assignmentsQuery.Where(x => x.WorkDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            assignmentsQuery = assignmentsQuery.Where(x => x.WorkDate <= request.ToDate.Value);

        var assignments = await assignmentsQuery
            .OrderBy(x => x.Employee.FirstName + " " + x.Employee.LastName)
            .ThenBy(x => x.WorkDate)
            .ToListAsync(cancellationToken);

        var grouped = assignments
            .GroupBy(x => new { x.EmployeeId, x.Employee?.EmployeeCode, EmployeeFullName = x.Employee.FirstName + " " + x.Employee.LastName })
            .Select(g => new EmployeeScheduleCalendarResponse
            {
                EmployeeId = g.Key.EmployeeId.Value,
                EmployeeCode = g.Key.EmployeeCode,
                EmployeeName = g.Key.EmployeeFullName,
                Days = g.Select(a => new CalendarDayResponse
                {
                    Date = a.WorkDate,
                    HasAssignment = true,
                    AssignmentId = a.Id.Value,
                    ShiftName = a.ShiftNameSnapshot,
                    StartTime = a.StartTimeSnapshot,
                    EndTime = a.EndTimeSnapshot,
                    ExpectedWorkingHours = a.ExpectedWorkingHoursSnapshot,
                    Status = a.Status
                }).ToList()
            })
            .ToList();

        return grouped;
    }
}
