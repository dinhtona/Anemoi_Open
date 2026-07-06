using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Contracts;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.DashboardQueries.GetDashboardOverview;

public sealed class GetDashboardOverviewHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<LeaveRequest> leaveRequestRepository,
    ISqlRepository<LeaveBalance> leaveBalanceRepository,
    ISqlRepository<Department> departmentRepository,
    ISqlRepository<Position> positionRepository,
    ISqlRepository<EmployeeContract> contractRepository,
    HrSettings hrSettings)
    : IQueryHandler<GetDashboardOverviewQuery, DashboardOverviewResponse>
{
    public async Task<DashboardOverviewResponse> Handle(GetDashboardOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = GetBusinessToday();

        // 1. Summary Metrics
        var totalActiveEmployees = (int)await employeeRepository.CountByConditionAsync(
            x => x.EmploymentStatusCode == EmploymentStatusCode.Active,
            null,
            cancellationToken);

        var pendingLeaveRequests = (int)await leaveRequestRepository.CountByConditionAsync(
            x => x.StatusCode == LeaveRequestStatusCode.Pending,
            null,
            cancellationToken);

        var firstDayOfMonth = new DateOnly(now.Year, now.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        var newEmployeesThisMonth = (int)await employeeRepository.CountByConditionAsync(
            x => x.EmploymentStatusCode == EmploymentStatusCode.Active && x.JoinDate >= firstDayOfMonth && x.JoinDate <= lastDayOfMonth,
            null,
            cancellationToken);

        var employeesWithoutIdentityMapping = (int)await employeeRepository.CountByConditionAsync(
            x => x.EmploymentStatusCode == EmploymentStatusCode.Active && x.IdentityUserId == null,
            null,
            cancellationToken);

        // 2. Near Leave Exhaustion (Annual leave balance <= 3 days) - limit to 20
        var nearExhaustionBalances = await leaveBalanceRepository.GetQueryable(
                x => x.Year == now.Year && x.RemainingDays <= 3m && x.LeavePolicy.LeaveType.Code == LeaveTypeCode.Annual && x.Employee.EmploymentStatusCode == EmploymentStatusCode.Active)
            .Include(x => x.Employee)
            .Include(x => x.LeavePolicy)
            .OrderBy(x => x.RemainingDays)
            .Take(20)
            .ToListAsync(cancellationToken);

        var employeesNearLeaveExhaustion = nearExhaustionBalances.Count;

        var nearExhaustionEmployees = nearExhaustionBalances.Select(x => new NearExhaustionEmployeeDto(
            x.EmployeeId.Value,
            x.Employee?.FullName ?? "",
            x.RemainingDays,
            x.LeavePolicy?.Name ?? ""
        )).ToList();

        // 3. Headcount Sections - limit to top 20 by headcount
        var departmentHeadcounts = await employeeRepository.GetQueryable(x => x.EmploymentStatusCode == EmploymentStatusCode.Active && x.PrimaryDepartmentId != null)
            .GroupBy(x => x.PrimaryDepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(20)
            .ToListAsync(cancellationToken);

        var departmentHeadcountDict = departmentHeadcounts.ToDictionary(x => x.DepartmentId.Value, x => x.Count);
        var departmentIds = departmentHeadcounts.Select(x => x.DepartmentId).ToList();

        var activeDepartments = await departmentRepository.GetManyByConditionAsync(
            x => x.IsActive && departmentIds.Contains(x.Id),
            null,
            cancellationToken);

        var byDepartment = activeDepartments.Select(d => new DepartmentHeadcountDto(
            d.Id.Value,
            d.Code,
            d.Name,
            departmentHeadcountDict.GetValueOrDefault(d.Id.Value, 0)
        )).ToList();

        var positionHeadcounts = await employeeRepository.GetQueryable(x => x.EmploymentStatusCode == EmploymentStatusCode.Active && x.PrimaryPositionId != null)
            .GroupBy(x => x.PrimaryPositionId)
            .Select(g => new { PositionId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(20)
            .ToListAsync(cancellationToken);

        var positionHeadcountDict = positionHeadcounts.ToDictionary(x => x.PositionId.Value, x => x.Count);
        var positionIds = positionHeadcounts.Select(x => x.PositionId).ToList();

        var activePositions = await positionRepository.GetManyByConditionAsync(
            x => x.IsActive && positionIds.Contains(x.Id),
            null,
            cancellationToken);

        var byPosition = activePositions.Select(p => new PositionHeadcountDto(
            p.Id.Value,
            p.Code,
            p.Name,
            positionHeadcountDict.GetValueOrDefault(p.Id.Value, 0)
        )).ToList();

        // 4. Leave Section (Who's Out Today / This Week) - limit to 50 each
        var outTodayRequests = await leaveRequestRepository.GetQueryable(
                x => x.StatusCode == LeaveRequestStatusCode.Approved && x.StartDate <= today && x.EndDate >= today)
            .Include(x => x.Employee)
            .ThenInclude(x => x.PrimaryDepartment)
            .OrderBy(x => x.StartDate)
            .Take(50)
            .ToListAsync(cancellationToken);

        var whosOutToday = outTodayRequests.Select(x => new AbsenceItemDto(
            x.EmployeeId.Value,
            x.Employee?.FullName ?? "",
            x.Employee?.PrimaryDepartment?.Name ?? "",
            x.StartDate,
            x.EndDate,
            x.LeaveTypeCode
        )).ToList();

        var dayOfWeek = (int)now.DayOfWeek;
        var diffToMonday = dayOfWeek == 0 ? -6 : 1 - dayOfWeek;
        var startOfWeekDateTime = now.AddDays(diffToMonday);
        var endOfWeekDateTime = startOfWeekDateTime.AddDays(6);

        var startOfWeek = DateOnly.FromDateTime(startOfWeekDateTime);
        var endOfWeek = DateOnly.FromDateTime(endOfWeekDateTime);

        var outThisWeekRequests = await leaveRequestRepository.GetQueryable(
                x => x.StatusCode == LeaveRequestStatusCode.Approved && x.StartDate <= endOfWeek && x.EndDate >= startOfWeek)
            .Include(x => x.Employee)
            .ThenInclude(x => x.PrimaryDepartment)
            .OrderBy(x => x.StartDate)
            .Take(50)
            .ToListAsync(cancellationToken);

        var whosOutThisWeek = outThisWeekRequests.Select(x => new AbsenceItemDto(
            x.EmployeeId.Value,
            x.Employee?.FullName ?? "",
            x.Employee?.PrimaryDepartment?.Name ?? "",
            x.StartDate,
            x.EndDate,
            x.LeaveTypeCode
        )).ToList();

        // 5. Alerts Section - limit to 20
        var thresholdDate = today.AddDays(hrSettings.ContractExpirationAlertDays);
        var expiringContracts = await contractRepository.GetQueryable(
                x => x.StatusCode == ContractStatusCode.Active && x.EndDate != null && x.EndDate >= today && x.EndDate <= thresholdDate)
            .Include(x => x.Employee)
            .OrderBy(x => x.EndDate)
            .Take(20)
            .ToListAsync(cancellationToken);

        var contractExpirations = expiringContracts.Select(c => new ContractExpirationDto(
            c.EmployeeId.Value,
            c.Employee?.FullName ?? "",
            $"Contract {c.ContractNumber} expires on {c.EndDate:yyyy-MM-dd}"
        )).ToList();

        return new DashboardOverviewResponse(
            new DashboardSummarySection(
                totalActiveEmployees,
                pendingLeaveRequests,
                newEmployeesThisMonth,
                employeesWithoutIdentityMapping,
                employeesNearLeaveExhaustion
            ),
            new DashboardHeadcountSection(byDepartment, byPosition),
            new DashboardLeaveSection(whosOutToday, whosOutThisWeek),
            new DashboardAlertsSection(contractExpirations, nearExhaustionEmployees)
        );
    }

    private DateOnly GetBusinessToday()
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(hrSettings.BusinessTimeZone);
            var localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
            return DateOnly.FromDateTime(localTime.DateTime);
        }
        catch
        {
            return DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }
}
