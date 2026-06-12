using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Domain.Models;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.ActivateWorkingCalendarRule;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.CreateWorkingCalendarRule;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.UpdateWorkingCalendarRule;
using Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CalendarStatus.GetCalendarStatus;
using Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.MonthlyCalendar.GetMonthlyCalendar;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.CalendarManagement;
using Anemoi.Hr.Infrastructure.Persistence;
using Anemoi.Hr.Infrastructure.Services;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using OneOf;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class HrCalendarManagementTests
{
    private static readonly CalendarManagementMapper Mapper = new();

    // ==========================================
    // 1. WorkingCalendarEngine Domain Logic Tests
    // ==========================================

    [Fact]
    public async Task WorkingCalendarEngine_DefaultWeeklyPattern_EvaluatesCorrectly()
    {
        var dbContext = CreateInMemoryDbContext();
        var engine = new WorkingCalendarEngine(dbContext);

        // Monday-Friday work pattern rule
        var rule = WorkingCalendarRule.Create(
            "Default Rule",
            "Mon-Fri work schedule",
            new DateOnly(2026, 1, 1),
            null,
            true, true, true, true, true, false, false
        );
        dbContext.Set<WorkingCalendarRule>().Add(rule);
        await dbContext.SaveChangesAsync();

        // 2026-06-08 is Monday (Working)
        var monStatus = await engine.GetCalendarStatus(new DateOnly(2026, 6, 8));
        Assert.Equal(CalendarStatus.WorkingDay, monStatus);

        // 2026-06-14 is Sunday (Holiday)
        var sunStatus = await engine.GetCalendarStatus(new DateOnly(2026, 6, 14));
        Assert.Equal(CalendarStatus.Holiday, sunStatus);
    }

    [Fact]
    public async Task WorkingCalendarEngine_PublicHoliday_OverridesWeeklyPattern()
    {
        var dbContext = CreateInMemoryDbContext();
        var engine = new WorkingCalendarEngine(dbContext);

        var rule = WorkingCalendarRule.Create("Default", "Mon-Fri", new DateOnly(2026, 1, 1), null, true, true, true, true, true, false, false);
        dbContext.Set<WorkingCalendarRule>().Add(rule);

        // 2026-06-08 is Monday (Normal working, but set public holiday)
        var date = new DateOnly(2026, 6, 8);
        var pubHoliday = PublicHoliday.Create(date, "New Year in June", "Description", "VN", false);
        dbContext.Set<PublicHoliday>().Add(pubHoliday);
        await dbContext.SaveChangesAsync();

        var status = await engine.GetCalendarStatus(date);
        Assert.Equal(CalendarStatus.Holiday, status);
    }

    [Fact]
    public async Task WorkingCalendarEngine_CompanyHoliday_OverridesPublicHolidayAndWeeklyPattern()
    {
        var dbContext = CreateInMemoryDbContext();
        var engine = new WorkingCalendarEngine(dbContext);

        var rule = WorkingCalendarRule.Create("Default", "Mon-Fri", new DateOnly(2026, 1, 1), null, true, true, true, true, true, false, false);
        dbContext.Set<WorkingCalendarRule>().Add(rule);

        var date = new DateOnly(2026, 6, 8); // Monday
        var pubHoliday = PublicHoliday.Create(date, "Public Holiday", "Desc", "VN", false);
        var compHoliday = CompanyHoliday.Create(date, "Company Anniversary", "Desc", false);

        dbContext.Set<PublicHoliday>().Add(pubHoliday);
        dbContext.Set<CompanyHoliday>().Add(compHoliday);
        await dbContext.SaveChangesAsync();

        var status = await engine.GetCalendarStatus(date);
        Assert.Equal(CalendarStatus.Holiday, status);
    }

    [Fact]
    public async Task WorkingCalendarEngine_CalendarException_OverridesHolidaysAndWeeklyPattern()
    {
        var dbContext = CreateInMemoryDbContext();
        var engine = new WorkingCalendarEngine(dbContext);

        var rule = WorkingCalendarRule.Create("Default", "Mon-Fri", new DateOnly(2026, 1, 1), null, true, true, true, true, true, false, false);
        dbContext.Set<WorkingCalendarRule>().Add(rule);

        var date = new DateOnly(2026, 6, 8); // Monday
        var pubHoliday = PublicHoliday.Create(date, "Public Holiday", "Desc", "VN", false);
        var compHoliday = CompanyHoliday.Create(date, "Company Anniversary", "Desc", false);

        // Create exception that makes it a working day override
        var exception = CalendarException.Create(date, CalendarStatus.WorkingDay, "Important makeup working day", null);

        dbContext.Set<PublicHoliday>().Add(pubHoliday);
        dbContext.Set<CompanyHoliday>().Add(compHoliday);
        dbContext.Set<CalendarException>().Add(exception);
        await dbContext.SaveChangesAsync();

        var status = await engine.GetCalendarStatus(date);
        Assert.Equal(CalendarStatus.WorkingDay, status);
    }

    [Fact]
    public async Task WorkingCalendarEngine_RecurringPublicHoliday_AppliesToOtherYears()
    {
        var dbContext = CreateInMemoryDbContext();
        var engine = new WorkingCalendarEngine(dbContext);

        var rule = WorkingCalendarRule.Create("Default", "Mon-Fri", new DateOnly(2020, 1, 1), null, true, true, true, true, true, false, false);
        dbContext.Set<WorkingCalendarRule>().Add(rule);

        // Register recurring holiday on Jan 1st 2020
        var pubHoliday = PublicHoliday.Create(new DateOnly(2020, 1, 1), "New Year", "Desc", "VN", true);
        dbContext.Set<PublicHoliday>().Add(pubHoliday);
        await dbContext.SaveChangesAsync();

        // Check on Jan 1st 2026 (should be Holiday)
        var status = await engine.GetCalendarStatus(new DateOnly(2026, 1, 1));
        Assert.Equal(CalendarStatus.Holiday, status);
    }

    [Fact]
    public async Task WorkingCalendarEngine_RecurringCompanyHoliday_AppliesToOtherYears()
    {
        var dbContext = CreateInMemoryDbContext();
        var engine = new WorkingCalendarEngine(dbContext);

        var rule = WorkingCalendarRule.Create("Default", "Mon-Fri", new DateOnly(2020, 1, 1), null, true, true, true, true, true, false, false);
        dbContext.Set<WorkingCalendarRule>().Add(rule);

        // Register recurring company holiday on June 1st 2020
        var compHoliday = CompanyHoliday.Create(new DateOnly(2020, 6, 1), "Foundation Day", "Desc", true);
        dbContext.Set<CompanyHoliday>().Add(compHoliday);
        await dbContext.SaveChangesAsync();

        // Check on June 1st 2026 (should be Holiday)
        var status = await engine.GetCalendarStatus(new DateOnly(2026, 6, 1));
        Assert.Equal(CalendarStatus.Holiday, status);
    }

    // ==========================================
    // 2. Query Handlers Name Resolution Tests
    // ==========================================

    [Fact]
    public async Task GetCalendarStatusQuery_ResolvesRecurringHolidayAndCompanyHolidayNames()
    {
        var dbContext = CreateInMemoryDbContext();
        var engine = new WorkingCalendarEngine(dbContext);

        var pubHoliday = PublicHoliday.Create(new DateOnly(2020, 1, 1), "Recurring New Year", "Desc", "VN", true);
        dbContext.Set<PublicHoliday>().Add(pubHoliday);
        await dbContext.SaveChangesAsync();

        var publicHolidayRepo = new FakeRepository<PublicHoliday>(dbContext.Set<PublicHoliday>().ToList());
        var companyHolidayRepo = new FakeRepository<CompanyHoliday>(dbContext.Set<CompanyHoliday>().ToList());
        var exceptionRepo = new FakeRepository<CalendarException>(dbContext.Set<CalendarException>().ToList());

        var handler = new GetCalendarStatusHandler(publicHolidayRepo, companyHolidayRepo, exceptionRepo, engine);

        // Query Jan 1st 2026 (should return Recurring New Year name)
        var response = await handler.Handle(new GetCalendarStatusQuery(new DateOnly(2026, 1, 1)), CancellationToken.None);
        Assert.Equal("Recurring New Year", response.HolidayName);
    }

    [Fact]
    public async Task GetMonthlyCalendarQuery_ResolvesNamesForRecurringHolidays()
    {
        var dbContext = CreateInMemoryDbContext();
        var engine = new WorkingCalendarEngine(dbContext);

        // Mon-Fri schedule
        var rule = WorkingCalendarRule.Create("Default", "Mon-Fri", new DateOnly(2020, 1, 1), null, true, true, true, true, true, false, false);
        var pubHoliday = PublicHoliday.Create(new DateOnly(2020, 1, 1), "New Year", "Desc", "VN", true);
        var compHoliday = CompanyHoliday.Create(new DateOnly(2020, 1, 15), "Comp Holiday", "Desc", true);

        dbContext.Set<WorkingCalendarRule>().Add(rule);
        dbContext.Set<PublicHoliday>().Add(pubHoliday);
        dbContext.Set<CompanyHoliday>().Add(compHoliday);
        await dbContext.SaveChangesAsync();

        var publicHolidayRepo = new FakeRepository<PublicHoliday>(dbContext.Set<PublicHoliday>().ToList());
        var companyHolidayRepo = new FakeRepository<CompanyHoliday>(dbContext.Set<CompanyHoliday>().ToList());
        var exceptionRepo = new FakeRepository<CalendarException>(dbContext.Set<CalendarException>().ToList());

        var handler = new GetMonthlyCalendarHandler(publicHolidayRepo, companyHolidayRepo, exceptionRepo, engine);

        var response = await handler.Handle(new GetMonthlyCalendarQuery(2026, 1), CancellationToken.None);

        var day1 = response.Days.First(d => d.Date.Day == 1);
        Assert.Equal(CalendarStatus.Holiday, day1.Status);
        Assert.Equal("New Year", day1.HolidayName);

        var day15 = response.Days.First(d => d.Date.Day == 15);
        Assert.Equal(CalendarStatus.Holiday, day15.Status);
        Assert.Equal("Comp Holiday", day15.HolidayName);
    }

    // ==========================================
    // 3. WorkingCalendarRule Overlap Validation
    // ==========================================

    [Fact]
    public async Task CreateWorkingCalendarRule_OverlappingDateRanges_ReturnsOverlapError()
    {
        // Existing active rule: 2026-06-01 to 2026-06-30
        var existing = WorkingCalendarRule.Create("Existing", null, new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30), true, false, false, false, false, false, false);
        var ruleRepo = new FakeRepository<WorkingCalendarRule>([existing]);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateWorkingCalendarRuleHandler(ruleRepo, unitOfWork, Mapper);

        // Proposed rule overlaps: 2026-06-15 to 2026-07-15
        var command = new CreateWorkingCalendarRuleCommand(
            "New Overlapping Rule",
            null,
            new DateOnly(2026, 6, 15),
            new DateOnly(2026, 7, 15),
            true, true, false, false, false, false, false
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.WorkingCalendarRuleOverlap, result.AsT1.Code);
    }

    [Fact]
    public async Task UpdateWorkingCalendarRule_OverlappingDateRanges_ReturnsOverlapError()
    {
        var ruleId1 = new WorkingCalendarRuleId(Guid.NewGuid());
        var ruleId2 = new WorkingCalendarRuleId(Guid.NewGuid());

        var rule1 = WorkingCalendarRule.Create("Rule1", null, new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30), true, false, false, false, false, false, false);
        typeof(WorkingCalendarRule).GetProperty(nameof(WorkingCalendarRule.Id))?.SetValue(rule1, ruleId1);

        var rule2 = WorkingCalendarRule.Create("Rule2", null, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31), true, false, false, false, false, false, false);
        typeof(WorkingCalendarRule).GetProperty(nameof(WorkingCalendarRule.Id))?.SetValue(rule2, ruleId2);

        var ruleRepo = new FakeRepository<WorkingCalendarRule>([rule1, rule2]);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdateWorkingCalendarRuleHandler(ruleRepo, unitOfWork);

        // Update Rule2 to overlap with Rule1 (move start to 2026-06-20)
        var command = new UpdateWorkingCalendarRuleCommand(
            ruleId2,
            "Rule2 Updated",
            null,
            new DateOnly(2026, 6, 20),
            new DateOnly(2026, 7, 31),
            true, false, false, false, false, false, false
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.WorkingCalendarRuleOverlap, result.AsT1.Code);
    }

    [Fact]
    public async Task ActivateWorkingCalendarRule_OverlappingDateRanges_ReturnsOverlapError()
    {
        var activeRule = WorkingCalendarRule.Create("ActiveRule", null, new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30), true, false, false, false, false, false, false);

        var inactiveRuleId = new WorkingCalendarRuleId(Guid.NewGuid());
        var inactiveRule = WorkingCalendarRule.Create("InactiveRule", null, new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 20), true, false, false, false, false, false, false);
        typeof(WorkingCalendarRule).GetProperty(nameof(WorkingCalendarRule.Id))?.SetValue(inactiveRule, inactiveRuleId);
        inactiveRule.Deactivate();

        var ruleRepo = new FakeRepository<WorkingCalendarRule>([activeRule, inactiveRule]);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ActivateWorkingCalendarRuleHandler(ruleRepo, unitOfWork);

        var result = await handler.Handle(new ActivateWorkingCalendarRuleCommand(inactiveRuleId), CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.WorkingCalendarRuleOverlap, result.AsT1.Code);
    }

    // ==========================================
    // Database Helpers and Fake Implementations
    // ==========================================

    private static HrDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<HrDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new HrDbContext(options);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<OneOf<None, Exception>> SaveChangesAsync(CancellationToken token = default)
        {
            return Task.FromResult<OneOf<None, Exception>>(None.Value);
        }
    }

    private sealed class FakeRepository<T>(List<T> initialItems) : ISqlRepository<T>
        where T : class
    {
        private readonly List<T> _items = initialItems;

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

        public Task<OneOf<None, Exception>> RemoveOneAsync(OneOf<T, Expression<Func<T, bool>>> itemOrFilter, CancellationToken token = default)
        {
            if (itemOrFilter.IsT0)
            {
                _items.Remove(itemOrFilter.AsT0);
            }
            return Task.FromResult<OneOf<None, Exception>>(None.Value);
        }

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
