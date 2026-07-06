using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed record WorkforceOverviewResponse(
    int TotalEmployees,
    int ActiveEmployees,
    int InactiveEmployees,
    int TotalDepartments,
    int TotalPositions
);

public sealed record HeadcountTrendItem(DateOnly Month, int Headcount);

public sealed record PayrollAnalyticsResponse(
    decimal TotalPayrollCost,
    decimal AverageSalary,
    decimal HighestSalary,
    decimal LowestSalary,
    int EmployeeCount
);

public sealed record OvertimeAnalyticsResponse(
    decimal TotalOvertimeHours,
    decimal AverageOvertimeHours,
    int ApprovedRequestCount,
    int RejectedRequestCount
);

public sealed record AttendanceAnalyticsResponse(
    decimal AverageAttendanceRate,
    decimal AveragePaidDays,
    decimal AverageUnpaidDays,
    decimal AverageLeaveDays
);

public sealed record DepartmentCostItem(
    Guid DepartmentId,
    string DepartmentName,
    decimal PayrollCost,
    int EmployeeCount
);

public sealed record TopEarnerItem(
    Guid EmployeeId,
    string EmployeeName,
    string DepartmentName,
    decimal TotalSalary
);

public sealed record AnalyticsDashboardResponse(
    WorkforceOverviewResponse Workforce,
    PayrollAnalyticsResponse Payroll,
    AttendanceAnalyticsResponse Attendance,
    OvertimeAnalyticsResponse Overtime
);
