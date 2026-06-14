using System.Linq.Expressions;
using System.Reflection;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Domain;
using Anemoi.BuildingBlock.Domain.Models;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAttendanceAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetDepartmentCostAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetOvertimeAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetPayrollAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetTopEarners;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetWorkforceOverview;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAnalyticsDashboard;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Overtime;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using OneOf;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class HrAnalyticsTests
{
    [Fact]
    public async Task WorkforceOverview_ReturnsCorrectCounts()
    {
        var deptId1 = new DepartmentId(Guid.NewGuid());
        var deptId2 = new DepartmentId(Guid.NewGuid());

        var employees = new List<Employee>
        {
            CreateEmployee("10000000-0000-0000-0000-000000000001", deptId1, "active"),
            CreateEmployee("20000000-0000-0000-0000-000000000002", deptId1, "active"),
            CreateEmployee("30000000-0000-0000-0000-000000000003", deptId2, "inactive")
        };

        var departments = new List<Department>
        {
            CreateDepartment(deptId1, "Engineering"),
            CreateDepartment(deptId2, "Marketing")
        };

        var positions = new List<Position>
        {
            CreatePosition(new PositionId(Guid.NewGuid())),
            CreatePosition(new PositionId(Guid.NewGuid())),
            CreatePosition(new PositionId(Guid.NewGuid()))
        };

        var handler = new GetWorkforceOverviewHandler(
            new FakeRepository<Employee>(employees),
            new FakeRepository<Department>(departments),
            new FakeRepository<Position>(positions));

        var result = await handler.Handle(new GetWorkforceOverviewQuery(), CancellationToken.None);

        Assert.Equal(3, result.TotalEmployees);
        Assert.Equal(2, result.ActiveEmployees);
        Assert.Equal(1, result.InactiveEmployees);
        Assert.Equal(2, result.TotalDepartments);
        Assert.Equal(3, result.TotalPositions);
    }

    [Fact]
    public async Task PayrollAnalytics_ReturnsCorrectAggregates()
    {
        var runs = new List<PayrollRun>
        {
            CreateFinalizedPayrollRun("10000000-0000-0000-0000-000000000001", "20000000-0000-0000-0000-000000000010", "30000000-0000-0000-0000-000000000020", 1000),
            CreateFinalizedPayrollRun("10000000-0000-0000-0000-000000000002", "20000000-0000-0000-0000-000000000011", "30000000-0000-0000-0000-000000000020", 2000),
            CreateFinalizedPayrollRun("10000000-0000-0000-0000-000000000003", "20000000-0000-0000-0000-000000000012", "30000000-0000-0000-0000-000000000020", 3000)
        };

        var handler = new GetPayrollAnalyticsHandler(new FakeRepository<PayrollRun>(runs));

        var result = await handler.Handle(new GetPayrollAnalyticsQuery(), CancellationToken.None);

        Assert.Equal(6000, result.TotalPayrollCost);
        Assert.Equal(2000, result.AverageSalary);
        Assert.Equal(3000, result.HighestSalary);
        Assert.Equal(1000, result.LowestSalary);
        Assert.Equal(3, result.EmployeeCount);
    }

    [Fact]
    public async Task PayrollAnalytics_ExcludesNonFinalizedRuns()
    {
        var finalizedRun = CreateFinalizedPayrollRun(
            "10000000-0000-0000-0000-000000000001",
            "20000000-0000-0000-0000-000000000010",
            "30000000-0000-0000-0000-000000000020",
            1000);

        var calculatedRun = new PayrollRun
        {
            Id = new PayrollRunId(Guid.Parse("10000000-0000-0000-0000-000000000002")),
            PayrollPeriodId = new PayrollPeriodId(Guid.Parse("30000000-0000-0000-0000-000000000021")),
            EmployeeId = new EmployeeId(Guid.Parse("20000000-0000-0000-0000-000000000011")),
            EmployeeCode = "EMP002",
            EmployeeName = "Employee 2",
            PayScheduleType = "Monthly",
            NetAmount = 5000,
            CurrencyCode = "VND",
            BaseSalary = 5000,
            GrossAmount = 5000,
            TotalAllowanceAmount = 0,
            TotalDeductionAmount = 0,
            StandardWorkingDays = 22,
            PaidWorkingDays = 22,
            UnpaidLeaveDays = 0,
            DailyRate = 5000 / 22,
            BasePayAmount = 5000,
            CalculatedAt = DateTime.UtcNow,
            CalculatedBy = "test"
        };

        var handler = new GetPayrollAnalyticsHandler(
            new FakeRepository<PayrollRun>([finalizedRun, calculatedRun]));

        var result = await handler.Handle(new GetPayrollAnalyticsQuery(), CancellationToken.None);

        Assert.Equal(1000, result.TotalPayrollCost);
        Assert.Equal(1, result.EmployeeCount);
    }

    [Fact]
    public async Task AttendanceAnalytics_ReturnsCorrectAverages()
    {
        var summaries = new List<AttendanceSummary>
        {
            CreateAttendanceSummary("10000000-0000-0000-0000-000000000001", 20, 2, 0, 0, 20, 2, 0),
            CreateAttendanceSummary("10000000-0000-0000-0000-000000000002", 18, 1, 3, 2, 18, 1, 1)
        };

        var handler = new GetAttendanceAnalyticsHandler(new FakeRepository<AttendanceSummary>(summaries));

        var result = await handler.Handle(new GetAttendanceAnalyticsQuery(), CancellationToken.None);

        // TotalWorkedDays = 38, TotalDays = 46, Rate = 38/46*100 = 82.61
        Assert.Equal(82.61m, result.AverageAttendanceRate);
        // AvgPaidDays = (22 + 19) / 2 = 20.50
        Assert.Equal(20.50m, result.AveragePaidDays);
        // AvgUnpaidDays = (0 + 1) / 2 = 0.50
        Assert.Equal(0.50m, result.AverageUnpaidDays);
        // AvgLeaveDays = (2 + 1) / 2 = 1.50
        Assert.Equal(1.50m, result.AverageLeaveDays);
    }

    [Fact]
    public async Task OvertimeAnalytics_ReturnsCorrectCounts()
    {
        var requests = new List<OvertimeRequest>
        {
            CreateOvertimeRequest("10000000-0000-0000-0000-000000000001", OvertimeStatusCode.Approved, 2),
            CreateOvertimeRequest("10000000-0000-0000-0000-000000000002", OvertimeStatusCode.Approved, 2),
            CreateOvertimeRequest("10000000-0000-0000-0000-000000000003", OvertimeStatusCode.Rejected, 3),
            CreateOvertimeRequest("10000000-0000-0000-0000-000000000004", OvertimeStatusCode.Pending, 1)
        };

        var handler = new GetOvertimeAnalyticsHandler(new FakeRepository<OvertimeRequest>(requests));

        var result = await handler.Handle(new GetOvertimeAnalyticsQuery(), CancellationToken.None);

        Assert.Equal(2, result.ApprovedRequestCount);
        Assert.Equal(1, result.RejectedRequestCount);
        // Total hours: 2 + 2 + 3 + 1 = 8
        Assert.Equal(8m, result.TotalOvertimeHours);
        // Average hours: 8 / 4 = 2
        Assert.Equal(2m, result.AverageOvertimeHours);
    }

    [Fact]
    public async Task DepartmentCost_RankedByCostDescending()
    {
        var deptAId = new DepartmentId(Guid.Parse("10000000-0000-0000-0000-000000000001"));
        var deptBId = new DepartmentId(Guid.Parse("20000000-0000-0000-0000-000000000002"));

        var runs = new List<PayrollRun>
        {
            CreateFinalizedPayrollRun(
                "30000000-0000-0000-0000-000000000001",
                "40000000-0000-0000-0000-000000000010",
                "50000000-0000-0000-0000-000000000020",
                1000, "10000000-0000-0000-0000-000000000001", "Engineering"),
            CreateFinalizedPayrollRun(
                "30000000-0000-0000-0000-000000000002",
                "40000000-0000-0000-0000-000000000011",
                "50000000-0000-0000-0000-000000000020",
                1000, "10000000-0000-0000-0000-000000000001", "Engineering"),
            CreateFinalizedPayrollRun(
                "30000000-0000-0000-0000-000000000003",
                "40000000-0000-0000-0000-000000000012",
                "50000000-0000-0000-0000-000000000020",
                500, "20000000-0000-0000-0000-000000000002", "Marketing")
        };

        var handler = new GetDepartmentCostAnalyticsHandler(new FakeRepository<PayrollRun>(runs));

        var result = await handler.Handle(new GetDepartmentCostAnalyticsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Engineering", result.First().DepartmentName);
        Assert.Equal(2000, result.First().PayrollCost);
        Assert.Equal(2, result.First().EmployeeCount);
        Assert.Equal("Marketing", result.Last().DepartmentName);
        Assert.Equal(500, result.Last().PayrollCost);
        Assert.Equal(1, result.Last().EmployeeCount);
    }

    [Fact]
    public async Task DepartmentCost_UsesSnapshotWhenAvailable()
    {
        var deptAId = new DepartmentId(Guid.Parse("10000000-0000-0000-0000-000000000001"));
        var deptBId = new DepartmentId(Guid.Parse("20000000-0000-0000-0000-000000000002"));

        var employee = CreateEmployee("30000000-0000-0000-0000-000000000010", deptAId, "active");
        employee.PrimaryDepartment = CreateDepartment(deptAId, "Engineering");

        var run = CreateFinalizedPayrollRun(
            "40000000-0000-0000-0000-000000000001",
            "30000000-0000-0000-0000-000000000010",
            "50000000-0000-0000-0000-000000000020",
            1000, "20000000-0000-0000-0000-000000000002", "Marketing");
        run.Employee = employee;

        var handler = new GetDepartmentCostAnalyticsHandler(new FakeRepository<PayrollRun>([run]));

        var result = await handler.Handle(new GetDepartmentCostAnalyticsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Marketing", result.Single().DepartmentName);
        Assert.Equal(deptBId.Value, result.Single().DepartmentId);
    }

    [Fact]
    public async Task TopEarners_FiltersByPayrollRunId()
    {
        var runId1 = new PayrollRunId(Guid.Parse("10000000-0000-0000-0000-000000000001"));
        var runId2 = new PayrollRunId(Guid.Parse("20000000-0000-0000-0000-000000000002"));

        var emp1 = new EmployeeId(Guid.Parse("30000000-0000-0000-0000-000000000010"));
        var emp2 = new EmployeeId(Guid.Parse("30000000-0000-0000-0000-000000000011"));
        var emp3 = new EmployeeId(Guid.Parse("30000000-0000-0000-0000-000000000012"));

        var periodId = new PayrollPeriodId(Guid.Parse("40000000-0000-0000-0000-000000000020"));

        var run1 = CreateFinalizedPayrollRunForTopEarner(runId1, emp1, periodId, "EMP001", "Alice", 3000, "Engineering");
        var run2 = CreateFinalizedPayrollRunForTopEarner(runId2, emp2, periodId, "EMP002", "Bob", 2000, "Marketing");
        var run3 = CreateFinalizedPayrollRunForTopEarner(runId1, emp3, periodId, "EMP003", "Charlie", 1000, "Engineering");

        var handler = new GetTopEarnersHandler(new FakeRepository<PayrollRun>([run1, run2, run3]));

        var result = await handler.Handle(new GetTopEarnersQuery(runId1, 10), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.EmployeeName == "Alice");
        Assert.Contains(result, x => x.EmployeeName == "Charlie");
        Assert.DoesNotContain(result, x => x.EmployeeName == "Bob");
    }

    [Fact]
    public async Task TopEarners_UsesLatestRunWhenNoIdProvided()
    {
        var emp1 = new EmployeeId(Guid.Parse("30000000-0000-0000-0000-000000000010"));
        var emp2 = new EmployeeId(Guid.Parse("30000000-0000-0000-0000-000000000011"));

        var periodId = new PayrollPeriodId(Guid.Parse("40000000-0000-0000-0000-000000000020"));

        var earlierRun = CreateFinalizedPayrollRunForTopEarner(
            new PayrollRunId(Guid.Parse("10000000-0000-0000-0000-000000000001")),
            emp1, periodId, "EMP001", "Alice", 3000, "Engineering");
        SetFinalizedAt(earlierRun, new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc));

        var laterRun = CreateFinalizedPayrollRunForTopEarner(
            new PayrollRunId(Guid.Parse("20000000-0000-0000-0000-000000000002")),
            emp2, periodId, "EMP002", "Bob", 2000, "Marketing");
        SetFinalizedAt(laterRun, new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc));

        var handler = new GetTopEarnersHandler(new FakeRepository<PayrollRun>([earlierRun, laterRun]));

        var result = await handler.Handle(new GetTopEarnersQuery(null, 10), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Bob", result.Single().EmployeeName);
    }

    [Fact]
    public async Task Dashboard_AggregatesAllSections()
    {
        var workforceResponse = new WorkforceOverviewResponse(5, 4, 1, 3, 5);
        var payrollResponse = new PayrollAnalyticsResponse(10000, 2500, 4000, 1000, 4);
        var attendanceResponse = new AttendanceAnalyticsResponse(85.5m, 20, 1, 2);
        var overtimeResponse = new OvertimeAnalyticsResponse(10, 2.5m, 3, 1);

        var sender = new FakeSender(workforceResponse, payrollResponse, attendanceResponse, overtimeResponse);
        var handler = new GetAnalyticsDashboardHandler(sender);

        var result = await handler.Handle(new GetAnalyticsDashboardQuery(), CancellationToken.None);

        Assert.NotNull(result.Workforce);
        Assert.Equal(5, result.Workforce.TotalEmployees);

        Assert.NotNull(result.Payroll);
        Assert.Equal(10000, result.Payroll.TotalPayrollCost);

        Assert.NotNull(result.Attendance);
        Assert.Equal(85.5m, result.Attendance.AverageAttendanceRate);

        Assert.NotNull(result.Overtime);
        Assert.Equal(10, result.Overtime.TotalOvertimeHours);
    }

    private static Employee CreateEmployee(string id, DepartmentId deptId, string status = "active")
    {
        return new Employee
        {
            Id = new EmployeeId(Guid.Parse(id)),
            EmployeeCode = id[..8],
            FullName = "Employee " + id[..8],
            JoinDate = new DateOnly(2025, 1, 1),
            EmploymentStatusCode = status,
            EmploymentTypeCode = "full_time",
            GradeCode = "G1",
            PrimaryDepartmentId = deptId,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    private static Employee CreateEmployee(string id, string deptId, string status = "active")
    {
        return CreateEmployee(id, new DepartmentId(Guid.Parse(deptId)), status);
    }

    private static Department CreateDepartment(DepartmentId id, string name)
    {
        return new Department
        {
            Id = id,
            Code = id.Value.ToString("N")[..8],
            Name = name,
            IsActive = true
        };
    }

    private static Department CreateDepartment(string id, string name)
    {
        return CreateDepartment(new DepartmentId(Guid.Parse(id)), name);
    }

    private static Position CreatePosition(PositionId id)
    {
        return new Position
        {
            Id = id,
            Code = id.Value.ToString("N")[..8],
            Name = "Position " + id.Value.ToString("N")[..8],
            IsActive = true
        };
    }

    private static PayrollRun CreateFinalizedPayrollRun(string id, string employeeId, string periodId, decimal netAmount, string deptId = null, string deptName = null)
    {
        var run = new PayrollRun
        {
            Id = new PayrollRunId(Guid.Parse(id)),
            PayrollPeriodId = new PayrollPeriodId(Guid.Parse(periodId)),
            EmployeeId = new EmployeeId(Guid.Parse(employeeId)),
            EmployeeCode = "EMP001",
            EmployeeName = "Employee",
            PayScheduleType = "Monthly",
            NetAmount = netAmount,
            CurrencyCode = "VND",
            BaseSalary = netAmount,
            TotalAllowanceAmount = 0,
            GrossAmount = netAmount,
            TotalDeductionAmount = 0,
            StandardWorkingDays = 22,
            PaidWorkingDays = 22,
            UnpaidLeaveDays = 0,
            DailyRate = netAmount / 22,
            BasePayAmount = netAmount,
            CalculatedAt = DateTime.UtcNow,
            CalculatedBy = "test"
        };

        if (deptId != null)
        {
            run.DepartmentIdSnapshot = new DepartmentId(Guid.Parse(deptId));
            run.DepartmentNameSnapshot = deptName;
        }

        typeof(PayrollRun).GetProperty("Status")!.SetValue(run, PayrollRunStatus.Finalized);
        return run;
    }

    private static PayrollRun CreateFinalizedPayrollRunForTopEarner(PayrollRunId id, EmployeeId employeeId, PayrollPeriodId periodId, string empCode, string empName, decimal netAmount, string deptName)
    {
        var run = new PayrollRun
        {
            Id = id,
            PayrollPeriodId = periodId,
            EmployeeId = employeeId,
            EmployeeCode = empCode,
            EmployeeName = empName,
            PayScheduleType = "Monthly",
            NetAmount = netAmount,
            CurrencyCode = "VND",
            BaseSalary = netAmount,
            TotalAllowanceAmount = 0,
            GrossAmount = netAmount,
            TotalDeductionAmount = 0,
            StandardWorkingDays = 22,
            PaidWorkingDays = 22,
            UnpaidLeaveDays = 0,
            DailyRate = netAmount / 22,
            BasePayAmount = netAmount,
            DepartmentNameSnapshot = deptName,
            CalculatedAt = DateTime.UtcNow,
            CalculatedBy = "test"
        };

        typeof(PayrollRun).GetProperty("Status")!.SetValue(run, PayrollRunStatus.Finalized);
        typeof(PayrollRun).GetProperty("FinalizedAt")!.SetValue(run, DateTime.UtcNow);
        return run;
    }

    private static void SetFinalizedAt(PayrollRun run, DateTime finalizedAt)
    {
        typeof(PayrollRun).GetProperty("FinalizedAt")!.SetValue(run, finalizedAt);
    }

    private static AttendanceSummary CreateAttendanceSummary(string id, decimal workedDays, decimal leaveDays, decimal absentDays, decimal holidayDays, decimal paidWorkingDays, decimal paidLeaveDays, decimal unpaidLeaveDays)
    {
        return new AttendanceSummary
        {
            Id = new AttendanceSummaryId(Guid.Parse(id)),
            AttendancePeriodId = new AttendancePeriodId(Guid.NewGuid()),
            EmployeeId = new EmployeeId(Guid.NewGuid()),
            WorkedDays = workedDays,
            WorkedHours = workedDays * 8,
            LeaveDays = leaveDays,
            AbsentDays = absentDays,
            HolidayDays = holidayDays,
            PaidWorkingDays = paidWorkingDays,
            PaidLeaveDays = paidLeaveDays,
            UnpaidLeaveDays = unpaidLeaveDays,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "test"
        };
    }

    private static OvertimeRequest CreateOvertimeRequest(string id, string status, int hours)
    {
        var now = DateTime.UtcNow;
        var startTime = new TimeOnly(8, 0);
        var endTime = new TimeOnly(8, 0).AddHours(hours);

        var request = (OvertimeRequest)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(OvertimeRequest));

        typeof(Entity<OvertimeRequestId>).GetProperty("Id")!.SetValue(request, new OvertimeRequestId(Guid.Parse(id)));
        typeof(OvertimeRequest).GetProperty("EmployeeId")!.SetValue(request, new EmployeeId(Guid.NewGuid()));
        typeof(OvertimeRequest).GetProperty("OvertimeDate")!.SetValue(request, DateOnly.FromDateTime(now));
        typeof(OvertimeRequest).GetProperty("StartTime")!.SetValue(request, startTime);
        typeof(OvertimeRequest).GetProperty("EndTime")!.SetValue(request, endTime);
        typeof(OvertimeRequest).GetProperty("Reason")!.SetValue(request, "Test overtime");
        typeof(OvertimeRequest).GetProperty("Status")!.SetValue(request, status);
        typeof(OvertimeRequest).GetProperty("CreatedAt")!.SetValue(request, now);
        typeof(OvertimeRequest).GetProperty("UpdatedAt")!.SetValue(request, now);

        return request;
    }

    private sealed class FakeSender : ISender
    {
        private readonly WorkforceOverviewResponse _workforce;
        private readonly PayrollAnalyticsResponse _payroll;
        private readonly AttendanceAnalyticsResponse _attendance;
        private readonly OvertimeAnalyticsResponse _overtime;

        public FakeSender(
            WorkforceOverviewResponse workforce,
            PayrollAnalyticsResponse payroll,
            AttendanceAnalyticsResponse attendance,
            OvertimeAnalyticsResponse overtime)
        {
            _workforce = workforce;
            _payroll = payroll;
            _attendance = attendance;
            _overtime = overtime;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            where TResponse : notnull
        {
            return request switch
            {
                GetWorkforceOverviewQuery => Task.FromResult((TResponse)(object)_workforce),
                GetPayrollAnalyticsQuery => Task.FromResult((TResponse)(object)_payroll),
                GetAttendanceAnalyticsQuery => Task.FromResult((TResponse)(object)_attendance),
                GetOvertimeAnalyticsQuery => Task.FromResult((TResponse)(object)_overtime),
                _ => throw new NotSupportedException($"Unsupported request type: {request.GetType()}")
            };
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest => throw new NotSupportedException();

        public Task<object> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeRepository<T>(IEnumerable<T> initialItems) : ISqlRepository<T>
        where T : class
    {
        private readonly List<T> _items = initialItems.ToList();

        public IQueryable<T> GetQueryable(Expression<Func<T, bool>> conditionExpression = null)
        {
            var queryable = _items.AsQueryable();
            if (conditionExpression is not null)
                queryable = queryable.Where(conditionExpression);
            return new TestAsyncEnumerable<T>(queryable);
        }

        public IQueryable<T> GetQueryableFromRawQuery(string sql, params object[] parameters) => GetQueryable();

        public Task<T> GetFirstByConditionAsync(
            Expression<Func<T, bool>> conditionExpression = null,
            Func<IQueryable<T>, IQueryable<T>> specialAction = null,
            CancellationToken token = default)
        {
            IQueryable<T> queryable = GetQueryable(conditionExpression);
            if (specialAction is not null)
                queryable = specialAction(queryable);
            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<bool> ExistByConditionAsync(Expression<Func<T, bool>> conditionExpression = null, CancellationToken token = default)
        {
            return Task.FromResult(GetQueryable(conditionExpression).Any());
        }

        public Task<List<T>> GetManyByConditionAsync(
            Expression<Func<T, bool>> conditionExpression = null,
            Func<IQueryable<T>, IQueryable<T>> specialAction = null,
            CancellationToken token = default)
        {
            IQueryable<T> queryable = GetQueryable(conditionExpression);
            if (specialAction is not null)
                queryable = specialAction(queryable);
            return Task.FromResult(queryable.ToList());
        }

        public Task<Pagination<T>> GetManyByConditionWithPaginationAsync(Expression<Func<T, bool>> conditionExpression = null, Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default) => throw new NotSupportedException();
        public Task<long> CountByConditionAsync(Expression<Func<T, bool>> conditionExpression = null, Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default) => Task.FromResult((long)GetQueryable(conditionExpression).Count());

        public Task<OneOf<T, Exception>> CreateOneAsync(T item, CancellationToken token = default)
        {
            _items.Add(item);
            return Task.FromResult<OneOf<T, Exception>>(item);
        }

        public Task<OneOf<None, Exception>> CreateManyAsync(List<T> items, CancellationToken token = default)
        {
            _items.AddRange(items);
            return Task.FromResult<OneOf<None, Exception>>(None.Value);
        }

        public Task<OneOf<None, Exception>> RemoveOneAsync(OneOf<T, Expression<Func<T, bool>>> itemOrFilter, CancellationToken token = default) => throw new NotSupportedException();
        public Task<OneOf<None, Exception>> RemoveManyAsync(OneOf<List<T>, Expression<Func<T, bool>>> itemsOrFilter, CancellationToken token = default) => throw new NotSupportedException();
    }

    private sealed class TestAsyncQueryProvider<TElement>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public IQueryable<TElement1> CreateQuery<TElement1>(Expression expression) => new TestAsyncEnumerable<TElement1>(expression);
        public object Execute(Expression expression) => inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments().FirstOrDefault();
            if (expectedResultType is null)
                return Execute<TResult>(expression);

            var executeMethod = typeof(IQueryProvider)
                .GetMethods()
                .Single(x => x.Name == nameof(IQueryProvider.Execute) && x.IsGenericMethod)
                .MakeGenericMethod(expectedResultType);
            var result = executeMethod.Invoke(inner, [expression]);
            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, [result])!;
        }
    }

    private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression) : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;
        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    }
}
