# Phase 24: Workforce Analytics & HR Dashboard 2.0 - Design Document

## Status: Approved

## 1. Overview

Provide management-level workforce analytics from existing HR data. Read-only analytics based on snapshots and historical data. No new transactional business processes.

## 2. Architecture Decisions

### 2.1 Backend Folder Structure
Use existing `Cqrs/Queries/AnalyticsQueries/` pattern (not `Features/Analytics/`) to maintain consistency with 21 existing query categories.

### 2.2 Frontend Route
New page at `/[locale]/hr/analytics` (separate from existing `/hr/dashboard`).

### 2.3 Chart Library
Add Recharts for the Headcount Trend line chart. No chart library existed in the project.

### 2.4 Read-Only Enforcement
- No AnalyticsSnapshot table
- No ETL worker
- No recalculation logic
- Projection queries only, `AsNoTracking()`, no N+1, database-side grouping

## 3. Schema Evolution

### PayrollRun entity — Add department snapshot fields
New columns (nullable for backward compatibility with existing data):

```
DepartmentIdSnapshot   DepartmentId?
DepartmentNameSnapshot string?
```

These will be populated by future payroll calculation updates. For Phase 24, the query logic uses COALESCE/fallback:
1. Prefer `DepartmentNameSnapshot` if non-null
2. Fallback to `Employee.PrimaryDepartment.Name` if snapshot is null

## 4. Backend — CQRS Queries

All under `Anemoi.Hr.Application/Cqrs/Queries/AnalyticsQueries/`.

### 4.1 GetWorkforceOverviewQuery
- **Source**: `Employee` table
- **Returns**: TotalEmployees, ActiveEmployees, InactiveEmployees, TotalDepartments, TotalPositions
- **No parameters**

### 4.2 GetHeadcountTrendQuery
- **Source**: `Employee` (CreatedAt, TerminationDate)
- **Parameters**: FromDate, ToDate
- **Returns**: Monthly trend — Month, Headcount
- **Note**: This is operational workforce trend, not payroll snapshot history

### 4.3 GetPayrollAnalyticsQuery
- **Source**: `PayrollRun` (Status == Finalized) + `PayrollItem` snapshots
- **Returns**: TotalPayrollCost (sum PayrollRun.NetAmount), AverageSalary (avg of NetAmount per employee), HighestSalary, LowestSalary, EmployeeCount
- **Never recalculate payroll**

### 4.4 GetOvertimeAnalyticsQuery
- **Source**: `OvertimeRequest`
- **Returns**: TotalOvertimeHours, AverageOvertimeHours, ApprovedRequestCount, RejectedRequestCount

### 4.5 GetAttendanceAnalyticsQuery
- **Source**: `AttendanceSummary`
- **Returns**: AverageAttendanceRate, AveragePaidDays, AverageUnpaidDays, AverageLeaveDays

### 4.6 GetDepartmentCostAnalyticsQuery
- **Source**: `PayrollRun` (finalized) joined with `Employee`
- **Returns**: DepartmentId, DepartmentName, PayrollCost, EmployeeCount
- **Logic**: Prefer `PayrollRun.DepartmentNameSnapshot` if non-null; fallback to `Employee.PrimaryDepartment.Name`
- **Order**: Payroll cost descending

### 4.7 GetTopEarnersQuery
- **Source**: `PayrollRun` (finalized)
- **Parameters**: Top = 10 (default), PayrollRunId (recommended)
- **Logic**: If PayrollRunId not provided, use latest finalized PayrollRun
- **Returns**: Employee, Department, TotalSalary (NetAmount)

### 4.8 GetAnalyticsDashboardQuery
- **Aggregates**: WorkforceOverview + PayrollAnalytics + AttendanceAnalytics + OvertimeAnalytics
- **Single endpoint for dashboard**

## 5. API Endpoints

Controller: `AnalyticsController` at `api/hr/analytics/`

| Endpoint | Query | Permission |
|---|---|---|
| GET /api/hr/analytics/dashboard | GetAnalyticsDashboardQuery | AnalyticsView |
| GET /api/hr/analytics/workforce | GetWorkforceOverviewQuery | AnalyticsView |
| GET /api/hr/analytics/headcount-trend | GetHeadcountTrendQuery | AnalyticsView |
| GET /api/hr/analytics/payroll | GetPayrollAnalyticsQuery | AnalyticsView |
| GET /api/hr/analytics/overtime | GetOvertimeAnalyticsQuery | AnalyticsView |
| GET /api/hr/analytics/attendance | GetAttendanceAnalyticsQuery | AnalyticsView |
| GET /api/hr/analytics/department-cost | GetDepartmentCostAnalyticsQuery | AnalyticsView |
| GET /api/hr/analytics/top-earners | GetTopEarnersQuery | AnalyticsView |

## 6. Permissions

Add `hr.analytics.view` to:
- `Anemoi.BuildingBlock.Application/Authorization/Permissions.cs`
- `Anemoi.Hr.Application/Configurations/HrPermissions.cs`
- `cody-web-app/src/constants/permissions.ts`

## 7. Frontend — Pages & Components

### Route: `/[locale]/hr/analytics`

### Components

#### Summary Cards (4 groups)
- **Workforce Summary**: Total/Active/Inactive employees, Total Departments, Total Positions
- **Payroll Summary**: Total Payroll Cost, Average Salary, Highest Salary, Lowest Salary
- **Attendance Summary**: Average Attendance Rate, Average Paid Days, Average Unpaid Days, Average Leave Days
- **Overtime Summary**: Total Overtime Hours, Average Overtime Hours, Approved/Rejected Requests

#### Tables
- **Department Cost Ranking**: Department, Cost, EmployeeCount — sorted descending by cost
- **Top Earners**: Employee name, Department, Total Salary — filtered by PayrollRun

#### Chart
- **Headcount Trend**: Line chart using Recharts, monthly data points filtered by date range
  - Library: `recharts` (to be added to package.json)

### Files to Create

| File | Purpose |
|---|---|
| `services/hr/analyticsService.ts` | API calls |
| `hooks/hr/useAnalytics.ts` | React Query hooks |
| `types/hr/analytics.ts` | TypeScript interfaces |
| `components/features/hr/analytics/WorkforceSummaryCards.tsx` | Workforce KPI cards |
| `components/features/hr/analytics/PayrollSummaryCards.tsx` | Payroll KPI cards |
| `components/features/hr/analytics/AttendanceSummaryCards.tsx` | Attendance KPI cards |
| `components/features/hr/analytics/OvertimeSummaryCards.tsx` | Overtime KPI cards |
| `components/features/hr/analytics/DepartmentCostTable.tsx` | Department cost table |
| `components/features/hr/analytics/TopEarnersTable.tsx` | Top earners table |
| `components/features/hr/analytics/HeadcountTrendChart.tsx` | Headcount trend line chart |
| `app/[locale]/(dashboard)/hr/analytics/page.tsx` | Analytics dashboard page |

## 8. Localization

Add `HrAnalytics` section to:
- `messages/en.json`
- `messages/vi.json`

Keys:
- `title`, `description`
- `workforceSummary.*`, `payrollSummary.*`, `attendanceSummary.*`, `overtimeSummary.*`
- `departmentCost.*`, `topEarners.*`, `headcountTrend.*`
- `filters.*`

## 9. Sidebar

Add `/hr/analytics` entry under `hrWorkspace` group in `Sidebar.tsx` with `BarChart3` icon.

Add route-to-permission mapping in `ROUTE_PERMISSIONS`.

## 10. Tests

Backend only (following existing convention):

`HrAnalyticsTests` covering:
- Dashboard aggregation (all sub-queries compose correctly)
- Payroll analytics (sum, avg, min/max from finalized runs)
- Attendance analytics (rate calculations)
- Overtime analytics (hours, approval counts)
- Department cost ranking (ordering, fallback logic)
- Top earners (filtering by PayrollRunId, latest-fallback logic)

## 11. Performance Requirements

- All queries: `AsNoTracking()`, projection selects
- No loading full entities into memory
- Database-side grouping and aggregation
- No N+1 queries

## 12. Acceptance Criteria

- [ ] Build succeeds (both backend and frontend)
- [ ] Tests pass
- [ ] Dashboard renders at /hr/analytics
- [ ] No payroll recalculation logic anywhere
- [ ] Snapshot-based analytics (prefer snapshot, fallback to current)
- [ ] Permission-protected endpoints (hr.analytics.view required)
- [ ] Localization EN/VI complete
- [ ] Recharts chart renders headcount trend
