# Phase 24: Workforce Analytics Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement workforce analytics & HR dashboard 2.0 with 8 read-only analytics queries, API endpoints, and frontend dashboard at `/hr/analytics`.

**Architecture:** Clean Architecture, CQRS + MediatR, EF Core projection queries with AsNoTracking(). No new tables, no ETL worker, no recalculation. Snapshot-preferring with fallback to current data.

**Tech Stack:** .NET 10, EF Core/PostgreSQL, MediatR, OneOf, Next.js 14 App Router, TanStack Query v5, Recharts, shadcn/ui

---

### Task 1: Add Department Snapshot Fields to PayrollRun Domain Entity

**Files:**
- Modify: `Anemoi.Hr/Anemoi.Hr.Domain/Payroll/PayrollRun.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Configurations/PayrollModelMapping.cs`

- [ ] **Step 1: Add snapshot properties to PayrollRun entity**

Add after `// Employee snapshot` block in `PayrollRun.cs`:

```csharp
// Department snapshot
public DepartmentId? DepartmentIdSnapshot { get; set; }
public string DepartmentNameSnapshot { get; set; }
```

Add the import for `Anemoi.Hr.Domain.Departments` if not present (check existing imports — `EmployeeId` is already imported from `ModelIds`; we need `DepartmentId` which is in `Anemoi.Hr.Domain.Departments`).

- [ ] **Step 2: Add EF Core configuration for new fields**

In `PayrollModelMapping.cs`, inside `Configure(EntityTypeBuilder<PayrollRun> builder)`, add after `builder.Property(x => x.PayScheduleType)`:

```csharp
builder.Property(x => x.DepartmentIdSnapshot)
    .HasConversion(
        id => id != null ? id.Value : (Guid?)null,
        value => value.HasValue ? new DepartmentId(value.Value) : null)
    .IsRequired(false);

builder.Property(x => x.DepartmentNameSnapshot)
    .HasMaxLength(256)
    .IsRequired(false);
```

Add import for `using Anemoi.Hr.Domain.Departments;` at top of file.

- [ ] **Step 3: Create EF Core migration**

```bash
dotnet ef migrations add AddDepartmentSnapshotsToPayrollRun --project Anemoi.Hr/Anemoi.Hr.Infrastructure --startup-project Anemoi.Hr/Anemoi.Hr.Api
```

- [ ] **Step 4: Verify build**

```bash
dotnet build Anemoi.Hr/Anemoi.Hr.Api
```

Expected: Build succeeds, 0 errors.

---

### Task 2: Register Analytics Permission

**Files:**
- Modify: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/Permissions.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Application/Configurations/HrPermissions.cs`
- Modify: `cody-web-app/src/constants/permissions.ts`
- Modify: `cody-web-app/src/constants/api-endpoints.ts`

- [ ] **Step 1: Add permission to building blocks Permissions.cs**

Find the HR permissions section in `Permissions.cs` and add:

```csharp
public const string HrAnalyticsView = "hr.analytics.view";
```

(Add it near other HR permissions, e.g., after `HrDashboardView`)

- [ ] **Step 2: Add to HrPermissions.cs**

In `Anemoi.Hr/Anemoi.Hr.Application/Configurations/HrPermissions.cs`, add:

```csharp
public const string AnalyticsView = "hr.analytics.view";
```

- [ ] **Step 3: Add to frontend permissions.ts**

In `cody-web-app/src/constants/permissions.ts`, add to the PERMISSIONS object:

```typescript
HR_ANALYTICS_VIEW: "hr.analytics.view",
```

Also add to `ROUTE_PERMISSIONS`:

```typescript
"/hr/analytics": [PERMISSIONS.HR_ANALYTICS_VIEW],
```

- [ ] **Step 4: Add API endpoints config**

In `cody-web-app/src/constants/api-endpoints.ts`, add inside the `hr` section:

```typescript
analytics: {
  dashboard: "/api/hr/analytics/dashboard",
  workforce: "/api/hr/analytics/workforce",
  headcountTrend: "/api/hr/analytics/headcount-trend",
  payroll: "/api/hr/analytics/payroll",
  overtime: "/api/hr/analytics/overtime",
  attendance: "/api/hr/analytics/attendance",
  departmentCost: "/api/hr/analytics/department-cost",
  topEarners: "/api/hr/analytics/top-earners",
},
```

---

### Task 3: Create Analytics Response DTOs

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Responses/AnalyticsResponses.cs`

- [ ] **Step 1: Create response DTOs file**

```csharp
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

// === WORKFORCE OVERVIEW ===
public sealed record WorkforceOverviewResponse(
    int TotalEmployees,
    int ActiveEmployees,
    int InactiveEmployees,
    int TotalDepartments,
    int TotalPositions
);

// === HEADCOUNT TREND ===
public sealed record HeadcountTrendItem(DateOnly Month, int Headcount);

// === PAYROLL ANALYTICS ===
public sealed record PayrollAnalyticsResponse(
    decimal TotalPayrollCost,
    decimal AverageSalary,
    decimal HighestSalary,
    decimal LowestSalary,
    int EmployeeCount
);

// === OVERTIME ANALYTICS ===
public sealed record OvertimeAnalyticsResponse(
    decimal TotalOvertimeHours,
    decimal AverageOvertimeHours,
    int ApprovedRequestCount,
    int RejectedRequestCount
);

// === ATTENDANCE ANALYTICS ===
public sealed record AttendanceAnalyticsResponse(
    decimal AverageAttendanceRate,
    decimal AveragePaidDays,
    decimal AverageUnpaidDays,
    decimal AverageLeaveDays
);

// === DEPARTMENT COST ===
public sealed record DepartmentCostItem(
    Guid DepartmentId,
    string DepartmentName,
    decimal PayrollCost,
    int EmployeeCount
);

// === TOP EARNERS ===
public sealed record TopEarnerItem(
    Guid EmployeeId,
    string EmployeeName,
    string DepartmentName,
    decimal TotalSalary
);

// === ANALYTICS DASHBOARD (aggregate) ===
public sealed record AnalyticsDashboardResponse(
    WorkforceOverviewResponse Workforce,
    PayrollAnalyticsResponse Payroll,
    AttendanceAnalyticsResponse Attendance,
    OvertimeAnalyticsResponse Overtime
);
```

---

### Task 4: Create GetWorkforceOverview Query + Handler

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetWorkforceOverview/GetWorkforceOverviewQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetWorkforceOverview/GetWorkforceOverviewHandler.cs`

- [ ] **Step 1: Create query**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetWorkforceOverview;

public sealed record GetWorkforceOverviewQuery : IQuery<WorkforceOverviewResponse>;
```

- [ ] **Step 2: Create handler**

```csharp
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetWorkforceOverview;

public sealed class GetWorkforceOverviewHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Department> departmentRepository,
    ISqlRepository<Position> positionRepository)
    : IQueryHandler<GetWorkforceOverviewQuery, WorkforceOverviewResponse>
{
    public async Task<WorkforceOverviewResponse> Handle(
        GetWorkforceOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var totalEmployees = await employeeRepository.GetQueryable().LongCountAsync(cancellationToken);
        var activeEmployees = await employeeRepository.GetQueryable()
            .CountAsync(x => x.EmploymentStatusCode == "active", cancellationToken);
        var inactiveEmployees = totalEmployees - activeEmployees;
        var totalDepartments = await departmentRepository.GetQueryable().LongCountAsync(cancellationToken);
        var totalPositions = await positionRepository.GetQueryable().LongCountAsync(cancellationToken);

        return new WorkforceOverviewResponse(
            (int)totalEmployees,
            (int)activeEmployees,
            (int)inactiveEmployees,
            (int)totalDepartments,
            (int)totalPositions
        );
    }
}
```

---

### Task 5: Create GetHeadcountTrend Query + Handler

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetHeadcountTrend/GetHeadcountTrendQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetHeadcountTrend/GetHeadcountTrendHandler.cs`

- [ ] **Step 1: Create query**

```csharp
using System;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetHeadcountTrend;

public sealed record GetHeadcountTrendQuery(DateOnly FromDate, DateOnly ToDate)
    : IQuery<ICollection<HeadcountTrendItem>>;
```

- [ ] **Step 2: Create handler**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetHeadcountTrend;

public sealed class GetHeadcountTrendHandler(
    ISqlRepository<Employee> employeeRepository)
    : IQueryHandler<GetHeadcountTrendQuery, ICollection<HeadcountTrendItem>>
{
    public async Task<ICollection<HeadcountTrendItem>> Handle(
        GetHeadcountTrendQuery request,
        CancellationToken cancellationToken)
    {
        // Get monthly join counts from database (aggregated server-side)
        var monthlyJoins = await employeeRepository.GetQueryable()
            .Select(x => new { x.CreatedAt, x.EmploymentStatusCode })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Compute monthly active headcount as running total of active employees
        var trend = new List<HeadcountTrendItem>();
        var current = new DateOnly(request.FromDate.Year, request.FromDate.Month, 1);
        var end = new DateOnly(request.ToDate.Year, request.ToDate.Month, 1);

        while (current <= end)
        {
            var monthEnd = current.AddMonths(1);

            var headcount = monthlyJoins.Count(e =>
                DateOnly.FromDateTime(e.CreatedAt) < monthEnd &&
                e.EmploymentStatusCode == "active");

            trend.Add(new HeadcountTrendItem(current, headcount));
            current = current.AddMonths(1);
        }

        return trend;
    }
}
```

Note: This computes operational workforce trend (active employees who joined on or before each month), not payroll snapshot history. A future enhancement could store monthly snapshot data for precise historical tracking.

---

### Task 6: Create GetPayrollAnalytics Query + Handler

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetPayrollAnalytics/GetPayrollAnalyticsQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetPayrollAnalytics/GetPayrollAnalyticsHandler.cs`

- [ ] **Step 1: Create query**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetPayrollAnalytics;

public sealed record GetPayrollAnalyticsQuery : IQuery<PayrollAnalyticsResponse>;
```

- [ ] **Step 2: Create handler**

```csharp
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetPayrollAnalytics;

public sealed class GetPayrollAnalyticsHandler(
    ISqlRepository<PayrollRun> payrollRunRepository)
    : IQueryHandler<GetPayrollAnalyticsQuery, PayrollAnalyticsResponse>
{
    public async Task<PayrollAnalyticsResponse> Handle(
        GetPayrollAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var finalizedRuns = payrollRunRepository.GetQueryable()
            .Where(x => x.Status == PayrollRunStatus.Finalized)
            .AsNoTracking();

        var totals = await finalizedRuns
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalPayrollCost = g.Sum(x => x.NetAmount),
                AverageSalary = g.Average(x => x.NetAmount),
                HighestSalary = g.Max(x => x.NetAmount),
                LowestSalary = g.Min(x => x.NetAmount),
                EmployeeCount = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (totals is null)
            return new PayrollAnalyticsResponse(0, 0, 0, 0, 0);

        return new PayrollAnalyticsResponse(
            totals.TotalPayrollCost,
            totals.AverageSalary,
            totals.HighestSalary,
            totals.LowestSalary,
            totals.EmployeeCount
        );
    }
}
```

---

### Task 7: Create GetOvertimeAnalytics Query + Handler

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetOvertimeAnalytics/GetOvertimeAnalyticsQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetOvertimeAnalytics/GetOvertimeAnalyticsHandler.cs`

- [ ] **Step 1: Create query**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetOvertimeAnalytics;

public sealed record GetOvertimeAnalyticsQuery : IQuery<OvertimeAnalyticsResponse>;
```

- [ ] **Step 2: Create handler**

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetOvertimeAnalytics;

public sealed class GetOvertimeAnalyticsHandler(
    ISqlRepository<OvertimeRequest> overtimeRepository)
    : IQueryHandler<GetOvertimeAnalyticsQuery, OvertimeAnalyticsResponse>
{
    public async Task<OvertimeAnalyticsResponse> Handle(
        GetOvertimeAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var query = overtimeRepository.GetQueryable().AsNoTracking();

        var totals = await query
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalOvertimeHours = g.Sum(x => (decimal)(x.EndTime - x.StartTime).TotalHours),
                AverageOvertimeHours = g.Average(x => (decimal)(x.EndTime - x.StartTime).TotalHours),
                ApprovedCount = g.Count(x => x.Status == OvertimeStatusCode.Approved),
                RejectedCount = g.Count(x => x.Status == OvertimeStatusCode.Rejected)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (totals is null)
            return new OvertimeAnalyticsResponse(0, 0, 0, 0);

        return new OvertimeAnalyticsResponse(
            Math.Round(totals.TotalOvertimeHours, 2),
            Math.Round(totals.AverageOvertimeHours, 2),
            totals.ApprovedCount,
            totals.RejectedCount
        );
    }
}
```

---

### Task 8: Create GetAttendanceAnalytics Query + Handler

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetAttendanceAnalytics/GetAttendanceAnalyticsQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetAttendanceAnalytics/GetAttendanceAnalyticsHandler.cs`

- [ ] **Step 1: Create query**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAttendanceAnalytics;

public sealed record GetAttendanceAnalyticsQuery : IQuery<AttendanceAnalyticsResponse>;
```

- [ ] **Step 2: Create handler**

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAttendanceAnalytics;

public sealed class GetAttendanceAnalyticsHandler(
    ISqlRepository<AttendanceSummary> attendanceRepository)
    : IQueryHandler<GetAttendanceAnalyticsQuery, AttendanceAnalyticsResponse>
{
    public async Task<AttendanceAnalyticsResponse> Handle(
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
```

---

### Task 9: Create GetDepartmentCostAnalytics Query + Handler

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetDepartmentCostAnalytics/GetDepartmentCostAnalyticsQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetDepartmentCostAnalytics/GetDepartmentCostAnalyticsHandler.cs`

- [ ] **Step 1: Create query**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetDepartmentCostAnalytics;

public sealed record GetDepartmentCostAnalyticsQuery
    : IQuery<ICollection<DepartmentCostItem>>;
```

- [ ] **Step 2: Create handler**

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetDepartmentCostAnalytics;

public sealed class GetDepartmentCostAnalyticsHandler(
    ISqlRepository<PayrollRun> payrollRunRepository)
    : IQueryHandler<GetDepartmentCostAnalyticsQuery, ICollection<DepartmentCostItem>>
{
    public async Task<ICollection<DepartmentCostItem>> Handle(
        GetDepartmentCostAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var payrollData = await payrollRunRepository.GetQueryable()
            .Where(x => x.Status == PayrollRunStatus.Finalized)
            .AsNoTracking()
            .Select(x => new
            {
                DeptId = x.DepartmentIdSnapshot != null
                    ? x.DepartmentIdSnapshot.Value
                    : x.Employee.PrimaryDepartmentId.Value,
                DeptName = x.DepartmentNameSnapshot ?? x.Employee.PrimaryDepartment.Name,
                x.NetAmount,
                x.EmployeeId
            })
            .ToListAsync(cancellationToken);

        var result = payrollData
            .GroupBy(x => new { x.DeptId, x.DeptName })
            .Select(g => new DepartmentCostItem(
                g.Key.DeptId,
                g.Key.DeptName,
                g.Sum(x => x.NetAmount),
                g.Select(x => x.EmployeeId).Distinct().Count()
            ))
            .OrderByDescending(x => x.PayrollCost)
            .ToList();

        return result;
    }
}
```

---

### Task 10: Create GetTopEarners Query + Handler

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetTopEarners/GetTopEarnersQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetTopEarners/GetTopEarnersHandler.cs`

- [ ] **Step 1: Create query**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetTopEarners;

public sealed record GetTopEarnersQuery(
    PayrollRunId? PayrollRunId,
    int Top = 10
) : IQuery<ICollection<TopEarnerItem>>;
```

- [ ] **Step 2: Create handler**

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetTopEarners;

public sealed class GetTopEarnersHandler(
    ISqlRepository<PayrollRun> payrollRunRepository)
    : IQueryHandler<GetTopEarnersQuery, ICollection<TopEarnerItem>>
{
    public async Task<ICollection<TopEarnerItem>> Handle(
        GetTopEarnersQuery request,
        CancellationToken cancellationToken)
    {
        var payrollRunId = request.PayrollRunId;

        if (payrollRunId is null)
        {
            var latestRun = await payrollRunRepository.GetQueryable()
                .Where(x => x.Status == PayrollRunStatus.Finalized)
                .OrderByDescending(x => x.FinalizedAt)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestRun is null)
                return [];

            payrollRunId = latestRun;
        }

        var query = payrollRunRepository.GetQueryable()
            .Where(x => x.Id == payrollRunId && x.Status == PayrollRunStatus.Finalized)
            .AsNoTracking()
            .OrderByDescending(x => x.NetAmount)
            .Take(request.Top);

        var raw = await query
            .Select(x => new
            {
                EmployeeId = x.EmployeeId.Value,
                x.EmployeeName,
                DepartmentName = x.DepartmentNameSnapshot ?? x.Employee.PrimaryDepartment.Name,
                x.NetAmount
            })
            .ToListAsync(cancellationToken);

        return raw.Select(x => new TopEarnerItem(
            x.EmployeeId,
            x.EmployeeName,
            x.DepartmentName,
            x.NetAmount
        )).ToList();
    }
}
```

---

### Task 11: Create GetAnalyticsDashboard Query + Handler

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetAnalyticsDashboard/GetAnalyticsDashboardQuery.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/GetAnalyticsDashboard/GetAnalyticsDashboardHandler.cs`

- [ ] **Step 1: Create query**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAnalyticsDashboard;

public sealed record GetAnalyticsDashboardQuery : IQuery<AnalyticsDashboardResponse>;
```

- [ ] **Step 2: Create handler**

```csharp
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAttendanceAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetOvertimeAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetPayrollAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetWorkforceOverview;
using Anemoi.Hr.Application.Responses;
using MediatR;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAnalyticsDashboard;

public sealed class GetAnalyticsDashboardHandler(ISender sender)
    : IQueryHandler<GetAnalyticsDashboardQuery, AnalyticsDashboardResponse>
{
    public async Task<AnalyticsDashboardResponse> Handle(
        GetAnalyticsDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var workforceTask = sender.Send(new GetWorkforceOverviewQuery(), cancellationToken);
        var payrollTask = sender.Send(new GetPayrollAnalyticsQuery(), cancellationToken);
        var attendanceTask = sender.Send(new GetAttendanceAnalyticsQuery(), cancellationToken);
        var overtimeTask = sender.Send(new GetOvertimeAnalyticsQuery(), cancellationToken);

        await Task.WhenAll(workforceTask, payrollTask, attendanceTask, overtimeTask);

        return new AnalyticsDashboardResponse(
            workforceTask.Result,
            payrollTask.Result,
            attendanceTask.Result,
            overtimeTask.Result
        );
    }
}
```

---

### Task 12: Create AnalyticsController

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Api/Controllers/Analytics/AnalyticsController.cs`

- [ ] **Step 1: Create controller**

```csharp
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAnalyticsDashboard;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAttendanceAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetDepartmentCostAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetHeadcountTrend;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetOvertimeAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetPayrollAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetTopEarners;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetWorkforceOverview;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Analytics;

[ApiController]
[Route("api/hr/analytics")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class AnalyticsController(ISender sender) : ControllerBase
{
    [HttpGet("dashboard")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(AnalyticsDashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAnalyticsDashboardQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("workforce")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(WorkforceOverviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkforce(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkforceOverviewQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("headcount-trend")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(ICollection<HeadcountTrendItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHeadcountTrend(
        [FromQuery] GetHeadcountTrendQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("payroll")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(PayrollAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayroll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPayrollAnalyticsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("overtime")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(OvertimeAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOvertime(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOvertimeAnalyticsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("attendance")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(AttendanceAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendance(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAttendanceAnalyticsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("department-cost")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(ICollection<DepartmentCostItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartmentCost(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDepartmentCostAnalyticsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("top-earners")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(ICollection<TopEarnerItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopEarners(
        [FromQuery] GetTopEarnersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
```

---

### Task 13: Frontend — Types, Service, and Hooks

**Files:**
- Create: `cody-web-app/src/types/hr/analytics.ts`
- Create: `cody-web-app/src/services/hr/analyticsService.ts`
- Create: `cody-web-app/src/hooks/hr/useAnalytics.ts`

- [ ] **Step 1: Create TypeScript type definitions**

```typescript
export interface WorkforceOverviewResponse {
  totalEmployees: number;
  activeEmployees: number;
  inactiveEmployees: number;
  totalDepartments: number;
  totalPositions: number;
}

export interface HeadcountTrendItem {
  month: string;
  headcount: number;
}

export interface PayrollAnalyticsResponse {
  totalPayrollCost: number;
  averageSalary: number;
  highestSalary: number;
  lowestSalary: number;
  employeeCount: number;
}

export interface OvertimeAnalyticsResponse {
  totalOvertimeHours: number;
  averageOvertimeHours: number;
  approvedRequestCount: number;
  rejectedRequestCount: number;
}

export interface AttendanceAnalyticsResponse {
  averageAttendanceRate: number;
  averagePaidDays: number;
  averageUnpaidDays: number;
  averageLeaveDays: number;
}

export interface DepartmentCostItem {
  departmentId: string;
  departmentName: string;
  payrollCost: number;
  employeeCount: number;
}

export interface TopEarnerItem {
  employeeId: string;
  employeeName: string;
  departmentName: string;
  totalSalary: number;
}

export interface AnalyticsDashboardResponse {
  workforce: WorkforceOverviewResponse;
  payroll: PayrollAnalyticsResponse;
  attendance: AttendanceAnalyticsResponse;
  overtime: OvertimeAnalyticsResponse;
}
```

- [ ] **Step 2: Create API service**

```typescript
import { API_ENDPOINTS } from "@/constants/api-endpoints";
import apiClient from "@/lib/axios";
import type {
  AnalyticsDashboardResponse,
  WorkforceOverviewResponse,
  HeadcountTrendItem,
  PayrollAnalyticsResponse,
  OvertimeAnalyticsResponse,
  AttendanceAnalyticsResponse,
  DepartmentCostItem,
  TopEarnerItem,
} from "@/types/hr/analytics";

export const analyticsService = {
  getDashboard: async (): Promise<AnalyticsDashboardResponse> => {
    const { data } = await apiClient.get<AnalyticsDashboardResponse>(
      API_ENDPOINTS.hr.analytics.dashboard
    );
    return data;
  },

  getWorkforce: async (): Promise<WorkforceOverviewResponse> => {
    const { data } = await apiClient.get<WorkforceOverviewResponse>(
      API_ENDPOINTS.hr.analytics.workforce
    );
    return data;
  },

  getHeadcountTrend: async (
    fromDate: string,
    toDate: string
  ): Promise<HeadcountTrendItem[]> => {
    const { data } = await apiClient.get<HeadcountTrendItem[]>(
      API_ENDPOINTS.hr.analytics.headcountTrend,
      { params: { fromDate, toDate } }
    );
    return data;
  },

  getPayroll: async (): Promise<PayrollAnalyticsResponse> => {
    const { data } = await apiClient.get<PayrollAnalyticsResponse>(
      API_ENDPOINTS.hr.analytics.payroll
    );
    return data;
  },

  getOvertime: async (): Promise<OvertimeAnalyticsResponse> => {
    const { data } = await apiClient.get<OvertimeAnalyticsResponse>(
      API_ENDPOINTS.hr.analytics.overtime
    );
    return data;
  },

  getAttendance: async (): Promise<AttendanceAnalyticsResponse> => {
    const { data } = await apiClient.get<AttendanceAnalyticsResponse>(
      API_ENDPOINTS.hr.analytics.attendance
    );
    return data;
  },

  getDepartmentCost: async (): Promise<DepartmentCostItem[]> => {
    const { data } = await apiClient.get<DepartmentCostItem[]>(
      API_ENDPOINTS.hr.analytics.departmentCost
    );
    return data;
  },

  getTopEarners: async (
    payrollRunId?: string,
    top: number = 10
  ): Promise<TopEarnerItem[]> => {
    const { data } = await apiClient.get<TopEarnerItem[]>(
      API_ENDPOINTS.hr.analytics.topEarners,
      { params: { payrollRunId, top } }
    );
    return data;
  },
};
```

- [ ] **Step 3: Create React Query hooks**

```typescript
"use client";

import { useQuery } from "@tanstack/react-query";
import { analyticsService } from "@/services/hr/analyticsService";

export const ANALYTICS_QUERY_KEYS = {
  dashboard: () => ["hr", "analytics", "dashboard"] as const,
  workforce: () => ["hr", "analytics", "workforce"] as const,
  headcountTrend: (fromDate: string, toDate: string) =>
    ["hr", "analytics", "headcount-trend", fromDate, toDate] as const,
  payroll: () => ["hr", "analytics", "payroll"] as const,
  overtime: () => ["hr", "analytics", "overtime"] as const,
  attendance: () => ["hr", "analytics", "attendance"] as const,
  departmentCost: () => ["hr", "analytics", "department-cost"] as const,
  topEarners: (payrollRunId?: string) =>
    ["hr", "analytics", "top-earners", payrollRunId] as const,
};

export function useAnalyticsDashboard() {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.dashboard(),
    queryFn: () => analyticsService.getDashboard(),
  });
}

export function useWorkforceOverview() {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.workforce(),
    queryFn: () => analyticsService.getWorkforce(),
  });
}

export function useHeadcountTrend(fromDate: string, toDate: string) {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.headcountTrend(fromDate, toDate),
    queryFn: () => analyticsService.getHeadcountTrend(fromDate, toDate),
  });
}

export function usePayrollAnalytics() {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.payroll(),
    queryFn: () => analyticsService.getPayroll(),
  });
}

export function useOvertimeAnalytics() {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.overtime(),
    queryFn: () => analyticsService.getOvertime(),
  });
}

export function useAttendanceAnalytics() {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.attendance(),
    queryFn: () => analyticsService.getAttendance(),
  });
}

export function useDepartmentCost() {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.departmentCost(),
    queryFn: () => analyticsService.getDepartmentCost(),
  });
}

export function useTopEarners(payrollRunId?: string) {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.topEarner(payrollRunId),
    queryFn: () => analyticsService.getTopEarners(payrollRunId),
  });
}
```

---

### Task 14: Frontend — Analytics Widget Components

**Files:**
- Create: `cody-web-app/src/components/features/hr/analytics/WorkforceSummaryCards.tsx`
- Create: `cody-web-app/src/components/features/hr/analytics/PayrollSummaryCards.tsx`
- Create: `cody-web-app/src/components/features/hr/analytics/AttendanceSummaryCards.tsx`
- Create: `cody-web-app/src/components/features/hr/analytics/OvertimeSummaryCards.tsx`
- Create: `cody-web-app/src/components/features/hr/analytics/DepartmentCostTable.tsx`
- Create: `cody-web-app/src/components/features/hr/analytics/TopEarnersTable.tsx`
- Create: `cody-web-app/src/components/features/hr/analytics/HeadcountTrendChart.tsx`

- [ ] **Step 1: WorkforceSummaryCards**
- [ ] **Step 2: PayrollSummaryCards**
- [ ] **Step 3: AttendanceSummaryCards**
- [ ] **Step 4: OvertimeSummaryCards**
- [ ] **Step 5: DepartmentCostTable**
- [ ] **Step 6: TopEarnersTable**
- [ ] **Step 7: HeadcountTrendChart** (using Recharts)

Each component follows the pattern:
- Receive data as props (from parent page)
- Use `useTranslations("HrAnalytics")` for labels
- Render KPI cards using shadcn Card component
- Tables use shadcn Table component
- Chart uses Recharts `LineChart`, `Line`, `XAxis`, `YAxis`, `Tooltip`, `ResponsiveContainer`

---

### Task 15: Frontend — Analytics Dashboard Page

**Files:**
- Create: `cody-web-app/src/app/[locale]/(dashboard)/hr/analytics/page.tsx`

- [ ] **Step 1: Create the analytics page**

The page follows the existing HR page pattern:
- `"use client"` directive
- Permission check with `canAccessRoute("/hr/analytics")`
- Loading state with Skeleton components
- Error state with error card + retry
- Access denied state
- Fetches dashboard data via `useAnalyticsDashboard()`
- Renders all widget components in a responsive grid layout
- Filters section for Headcount Trend date range

---

### Task 16: Frontend — Localization and Sidebar

**Files:**
- Modify: `cody-web-app/messages/en.json`
- Modify: `cody-web-app/messages/vi.json`
- Modify: `cody-web-app/src/components/shared/Sidebar.tsx`

- [ ] **Step 1: Add EN localization**

Add to `messages/en.json`:

```json
"HrAnalytics": {
  "title": "Workforce Analytics",
  "description": "Management-level workforce analytics and insights",
  "workforceSummary": "Workforce Summary",
  "totalEmployees": "Total Employees",
  "activeEmployees": "Active Employees",
  "inactiveEmployees": "Inactive Employees",
  "totalDepartments": "Departments",
  "totalPositions": "Positions",
  "payrollSummary": "Payroll Summary",
  "totalPayrollCost": "Total Payroll Cost",
  "averageSalary": "Average Salary",
  "highestSalary": "Highest Salary",
  "lowestSalary": "Lowest Salary",
  "employeeCount": "Employees",
  "attendanceSummary": "Attendance Summary",
  "averageAttendanceRate": "Attendance Rate",
  "averagePaidDays": "Avg Paid Days",
  "averageUnpaidDays": "Avg Unpaid Days",
  "averageLeaveDays": "Avg Leave Days",
  "overtimeSummary": "Overtime Summary",
  "totalHours": "Total Hours",
  "averageHours": "Avg Hours",
  "approvedRequests": "Approved",
  "rejectedRequests": "Rejected",
  "departmentCost": "Department Cost",
  "departmentName": "Department",
  "payrollCost": "Payroll Cost",
  "departmentEmployeeCount": "Employees",
  "topEarners": "Top Earners",
  "employeeName": "Employee",
  "department": "Department",
  "totalSalary": "Total Salary",
  "headcountTrend": "Headcount Trend",
  "fromDate": "From",
  "toDate": "To",
  "month": "Month",
  "headcount": "Headcount",
  "loading": "Loading analytics...",
  "error": "Failed to load analytics",
  "retry": "Retry",
  "noData": "No analytics data available",
  "accessDenied": "You do not have permission to view analytics"
}
```

- [ ] **Step 2: Add VI localization**

Add to `messages/vi.json`:

```json
"HrAnalytics": {
  "title": "Phân tích Nhân sự",
  "description": "Phân tích và thông tin chi tiết nhân sự cấp quản lý",
  "workforceSummary": "Tổng quan Nhân sự",
  "totalEmployees": "Tổng nhân viên",
  "activeEmployees": "Đang làm việc",
  "inactiveEmployees": "Đã nghỉ việc",
  "totalDepartments": "Phòng ban",
  "totalPositions": "Vị trí",
  "payrollSummary": "Tổng quan Lương",
  "totalPayrollCost": "Tổng chi phí lương",
  "averageSalary": "Lương trung bình",
  "highestSalary": "Lương cao nhất",
  "lowestSalary": "Lương thấp nhất",
  "employeeCount": "Nhân viên",
  "attendanceSummary": "Tổng quan Chấm công",
  "averageAttendanceRate": "Tỷ lệ chấm công",
  "averagePaidDays": "Ngày công có lương",
  "averageUnpaidDays": "Ngày công không lương",
  "averageLeaveDays": "Ngày nghỉ",
  "overtimeSummary": "Tổng quan Tăng ca",
  "totalHours": "Tổng giờ",
  "averageHours": "Giờ trung bình",
  "approvedRequests": "Đã duyệt",
  "rejectedRequests": "Từ chối",
  "departmentCost": "Chi phí Phòng ban",
  "departmentName": "Phòng ban",
  "payrollCost": "Chi phí lương",
  "departmentEmployeeCount": "Nhân viên",
  "topEarners": "Nhân viên có lương cao nhất",
  "employeeName": "Nhân viên",
  "department": "Phòng ban",
  "totalSalary": "Tổng lương",
  "headcountTrend": "Xu hướng Nhân sự",
  "fromDate": "Từ",
  "toDate": "Đến",
  "month": "Tháng",
  "headcount": "Số lượng",
  "loading": "Đang tải phân tích...",
  "error": "Không thể tải dữ liệu phân tích",
  "retry": "Thử lại",
  "noData": "Không có dữ liệu phân tích",
  "accessDenied": "Bạn không có quyền xem phân tích này"
}
```

- [ ] **Step 3: Update Sidebar**

In `Sidebar.tsx`, add to the `hrWorkspace` group items (after dashboard link or in alphabetical position):

```tsx
{
  label: t("hrAnalytics"),
  href: "/hr/analytics",
  icon: BarChart3,
},
```

Add `BarChart3` to lucide-react imports.

Also add the sidebar label to translations:
- `en.json`: `"hrAnalytics": "Analytics",`
- `vi.json`: `"hrAnalytics": "Phân tích",`
- Add to `Dashboard` section (where other sidebar labels like `hrDashboard`, `hrPayroll` are defined)

---

### Task 17: Install Recharts

**Files:**
- Modify: `cody-web-app/package.json`

- [ ] **Step 1: Install Recharts**

```bash
npm install recharts
```

---

### Task 18: Add Backend Tests

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application.Tests/Analytics/HrAnalyticsTests.cs`

(Note: Verify actual test project name and location. If test project uses a different naming convention, adjust accordingly.)

- [ ] **Step 1: Create analytics tests**

The tests should cover:
- Dashboard aggregation returns all 4 sub-sections
- Payroll analytics returns correct sum/avg/min/max from finalized runs
- Attendance analytics calculates rates correctly
- Overtime analytics returns correct hours and counts
- Department cost ranked by cost descending
- Top earners filtered by PayrollRunId
- Top earners uses latest run when no PayrollRunId specified

Use in-memory database or repository mocking following existing test patterns in the project.

- [ ] **Step 2: Run tests to verify**

```bash
dotnet test Anemoi.Hr/Anemoi.Hr.Application.Tests
```

Expected: All tests pass.

---

### Task 19: Build Verification

- [ ] **Step 1: Build backend**

```bash
dotnet build Anemoi.sln
```

Expected: 0 errors.

- [ ] **Step 2: Build frontend**

```bash
cd cody-web-app && npm run build
```

Expected: Build succeeds.

- [ ] **Step 3: Lint frontend**

```bash
cd cody-web-app && npm run lint
```

Expected: No lint errors for new files.
