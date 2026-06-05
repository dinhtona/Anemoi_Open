using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed record DashboardOverviewResponse(
    DashboardSummarySection Summary,
    DashboardHeadcountSection Headcount,
    DashboardLeaveSection Leave,
    DashboardAlertsSection Alerts
);

public sealed record DashboardSummarySection(
    int TotalActiveEmployees,
    int PendingLeaveRequests,
    int NewEmployeesThisMonth,
    int EmployeesWithoutIdentityMapping,
    int EmployeesNearLeaveExhaustion
);

public sealed record DashboardHeadcountSection(
    IReadOnlyCollection<DepartmentHeadcountDto> ByDepartment,
    IReadOnlyCollection<PositionHeadcountDto> ByPosition
);

public sealed record DepartmentHeadcountDto(
    Guid DepartmentId,
    string DepartmentCode,
    string DepartmentName,
    int EmployeeCount
);

public sealed record PositionHeadcountDto(
    Guid PositionId,
    string PositionCode,
    string PositionName,
    int EmployeeCount
);

public sealed record DashboardLeaveSection(
    IReadOnlyCollection<AbsenceItemDto> WhosOutToday,
    IReadOnlyCollection<AbsenceItemDto> WhosOutThisWeek
);

public sealed record AbsenceItemDto(
    Guid EmployeeId,
    string EmployeeName,
    string DepartmentName,
    DateOnly StartDate,
    DateOnly EndDate,
    string LeaveTypeCode
);

public sealed record DashboardAlertsSection(
    IReadOnlyCollection<ContractExpirationDto> ContractExpirations,
    IReadOnlyCollection<NearExhaustionEmployeeDto> NearExhaustionEmployees
);

public sealed record NearExhaustionEmployeeDto(
    Guid EmployeeId,
    string EmployeeName,
    decimal RemainingDays,
    string LeavePolicyName
);

public sealed record ContractExpirationDto(
    Guid EmployeeId,
    string EmployeeName,
    string Message
);
