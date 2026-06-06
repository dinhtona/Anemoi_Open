using System.Linq.Expressions;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Domain.Models;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.AssignEmployeeAllowance;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ChangeSalary;
using Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetCompensationDashboard;
using Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetCompensationSnapshot;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Infrastructure.Persistence;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using OneOf;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class HrCompensationStabilizationTests
{
    [Fact]
    public void AssignEmployeeAllowanceValidator_Rejects_EffectiveToBeforeEffectiveFrom()
    {
        var validator = new AssignEmployeeAllowanceValidator();
        var command = new AssignEmployeeAllowanceCommand(
            new EmployeeId(Guid.NewGuid()),
            new AllowanceTypeId(Guid.NewGuid()),
            100,
            "USD",
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 4, 30),
            true);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "HR_ALLOWANCE_INVALID_DATE_RANGE");
    }

    [Fact]
    public void AssignEmployeeAllowanceValidator_Allows_OneDayAllowance()
    {
        var validator = new AssignEmployeeAllowanceValidator();
        var command = new AssignEmployeeAllowanceCommand(
            new EmployeeId(Guid.NewGuid()),
            new AllowanceTypeId(Guid.NewGuid()),
            100,
            "USD",
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 1),
            true);

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void HrDbContext_Has_CompensationUniqueIndexes()
    {
        var options = new DbContextOptionsBuilder<HrDbContext>()
            .UseNpgsql("Host=localhost;Database=anemoi_hr_metadata_only")
            .Options;

        using var dbContext = new HrDbContext(options);
        var salaryIndexes = dbContext.Model.FindEntityType(typeof(EmployeeSalary))!.GetIndexes().ToList();
        var allowanceIndexes = dbContext.Model.FindEntityType(typeof(EmployeeAllowance))!.GetIndexes().ToList();

        Assert.Contains(salaryIndexes, x =>
            x.IsUnique &&
            string.Join(",", x.Properties.Select(p => p.Name)) == "EmployeeId,EffectiveFrom");

        Assert.Contains(salaryIndexes, x =>
            x.IsUnique &&
            string.Join(",", x.Properties.Select(p => p.Name)) == "EmployeeId" &&
            x.GetFilter() == "\"EffectiveTo\" IS NULL");

        Assert.Contains(allowanceIndexes, x =>
            x.IsUnique &&
            string.Join(",", x.Properties.Select(p => p.Name)) == "EmployeeId,AllowanceTypeId,Currency,EffectiveFrom");

        Assert.Contains(allowanceIndexes, x =>
            x.IsUnique &&
            string.Join(",", x.Properties.Select(p => p.Name)) == "EmployeeId,AllowanceTypeId,Currency" &&
            x.GetFilter() == "\"EffectiveTo\" IS NULL");
    }

    [Fact]
    public async Task GetCompensationSnapshot_Groups_Totals_ByCurrency()
    {
        var employeeId = new EmployeeId(Guid.NewGuid());
        var allowanceTypeId = new AllowanceTypeId(Guid.NewGuid());
        var handler = new GetCompensationSnapshotHandler(
            new FakeRepository<EmployeeSalary>([
                Salary(employeeId, 1000, SalaryType.Monthly, "USD", new DateOnly(2026, 1, 1))
            ]),
            new FakeRepository<EmployeeAllowance>([
                Allowance(employeeId, allowanceTypeId, 200, "USD", new DateOnly(2026, 3, 1)),
                Allowance(employeeId, allowanceTypeId, 300000, "VND", new DateOnly(2026, 3, 1))
            ]),
            new CompensationMapper());

        var result = await handler.Handle(
            new GetCompensationSnapshotQuery(employeeId, new DateOnly(2026, 6, 1)),
            CancellationToken.None);

        Assert.Equal(1200, result.TotalsByCurrency.Single(x => x.Currency == "USD").TotalAmount);
        Assert.Equal(300000, result.TotalsByCurrency.Single(x => x.Currency == "VND").TotalAmount);
    }

    [Fact]
    public async Task GetCompensationDashboard_Groups_Projection_ByCurrency_And_SeparatesAllowanceCost()
    {
        var usdEmployeeId = new EmployeeId(Guid.NewGuid());
        var vndEmployeeId = new EmployeeId(Guid.NewGuid());
        var allowanceTypeId = new AllowanceTypeId(Guid.NewGuid());
        var allowanceType = new AllowanceType { Id = allowanceTypeId, Code = "MEAL", Name = "Meal", IsActive = true };

        var handler = new GetCompensationDashboardHandler(
            new FakeRepository<Employee>([
                Employee(usdEmployeeId),
                Employee(vndEmployeeId)
            ]),
            new FakeRepository<EmployeeSalary>([
                Salary(usdEmployeeId, 1000, SalaryType.Monthly, "USD", new DateOnly(2026, 1, 1), "G1"),
                Salary(vndEmployeeId, 200000, SalaryType.Monthly, "VND", new DateOnly(2026, 1, 1), "G2")
            ]),
            new FakeRepository<EmployeeAllowance>([
                Allowance(usdEmployeeId, allowanceTypeId, 300000, "VND", new DateOnly(2026, 1, 1), allowanceType)
            ]),
            new FakeRepository<SalaryValidationBypassLog>([]),
            new HrSettings { BusinessTimeZone = "Asia/Ho_Chi_Minh" },
            new CompensationMapper());

        var result = await handler.Handle(new GetCompensationDashboardQuery(), CancellationToken.None);

        Assert.Equal(1000, result.PayrollProjectionByCurrency.Single(x => x.Currency == "USD").Total);
        Assert.Equal(500000, result.PayrollProjectionByCurrency.Single(x => x.Currency == "VND").Total);
        Assert.Equal(1000, result.CostByGrade["G1"]);
        Assert.Equal(300000, result.AllowanceCostByType.Single(x => x.AllowanceTypeCode == "MEAL").Total);
    }

    [Fact]
    public async Task ChangeSalary_DuplicateInsertRace_Maps_ToSalaryConcurrencyConflict()
    {
        var employeeId = new EmployeeId(Guid.NewGuid());
        var handler = new ChangeSalaryHandler(
            new FakeRepository<Employee>([Employee(employeeId)]),
            new FakeRepository<SalaryGrade>([]),
            new FakeRepository<EmployeeSalary>([]),
            new FakeRepository<SalaryValidationBypassLog>([]),
            new NoopMediator(),
            new HrSettings { BusinessTimeZone = "Asia/Ho_Chi_Minh" },
            new FailingUnitOfWork());

        var result = await handler.Handle(
            new ChangeSalaryCommand(
                employeeId,
                1000,
                SalaryType.Monthly,
                "USD",
                new DateOnly(2026, 1, 1),
                CompensationChangeReason.Hired,
                true),
            CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.Equal("HR_SALARY_CONCURRENCY_CONFLICT", result.AsT1.Code);
    }

    [Fact]
    public async Task AssignEmployeeAllowance_DuplicateInsertRace_Maps_ToAllowanceConcurrencyConflict()
    {
        var employeeId = new EmployeeId(Guid.NewGuid());
        var allowanceTypeId = new AllowanceTypeId(Guid.NewGuid());
        var handler = new AssignEmployeeAllowanceHandler(
            new FakeRepository<Employee>([Employee(employeeId)]),
            new FakeRepository<AllowanceType>([new AllowanceType { Id = allowanceTypeId, Code = "MEAL", Name = "Meal", IsActive = true }]),
            new FakeRepository<EmployeeAllowance>([]),
            new HrSettings { BusinessTimeZone = "Asia/Ho_Chi_Minh" },
            new FailingUnitOfWork());

        var result = await handler.Handle(
            new AssignEmployeeAllowanceCommand(
                employeeId,
                allowanceTypeId,
                100,
                "USD",
                new DateOnly(2026, 1, 1),
                null,
                true),
            CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.Equal("HR_ALLOWANCE_CONCURRENCY_CONFLICT", result.AsT1.Code);
    }

    private static Employee Employee(EmployeeId id)
    {
        return new Employee
        {
            Id = id,
            EmployeeCode = id.Value.ToString("N")[..8],
            FullName = "Test Employee",
            JoinDate = new DateOnly(2025, 1, 1),
            EmploymentStatusCode = "active",
            EmploymentTypeCode = "full_time",
            GradeCode = "G1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static EmployeeSalary Salary(
        EmployeeId employeeId,
        decimal amount,
        SalaryType salaryType,
        string currency,
        DateOnly effectiveFrom,
        string gradeCode = "G1")
    {
        return new EmployeeSalary
        {
            Id = new EmployeeSalaryId(Guid.NewGuid()),
            EmployeeId = employeeId,
            BaseSalary = amount,
            SalaryType = salaryType,
            Currency = currency,
            EffectiveFrom = effectiveFrom,
            GradeCodeSnapshot = gradeCode,
            Reason = CompensationChangeReason.Hired,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
    }

    private static EmployeeAllowance Allowance(
        EmployeeId employeeId,
        AllowanceTypeId allowanceTypeId,
        decimal amount,
        string currency,
        DateOnly effectiveFrom,
        AllowanceType allowanceType = null)
    {
        return new EmployeeAllowance
        {
            Id = new EmployeeAllowanceId(Guid.NewGuid()),
            EmployeeId = employeeId,
            AllowanceTypeId = allowanceTypeId,
            AllowanceType = allowanceType,
            Amount = amount,
            Currency = currency,
            EffectiveFrom = effectiveFrom,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "test"
        };
    }

    private sealed class FailingUnitOfWork : IUnitOfWork
    {
        public Task<OneOf<None, Exception>> SaveChangesAsync(CancellationToken token = default)
        {
            return Task.FromResult<OneOf<None, Exception>>(
                new DbUpdateException("duplicate", new FakePostgresException()));
        }
    }

    private sealed class FakePostgresException : Exception
    {
        public string SqlState => "23505";
    }

    private sealed class NoopMediator : IMediator
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            where TResponse : notnull => throw new NotSupportedException();
        public Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResponse> => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest => throw new NotSupportedException();
        public Task<object> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
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
