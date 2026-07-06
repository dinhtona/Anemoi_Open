using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Domain.Models;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.ActivateTaxRuleSet;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CalculateTax;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxBracket;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxDeductionRule;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxRuleSet;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.DeactivateTaxRuleSet;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.DeleteTaxBracket;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxBracket;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxDeductionRule;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxRuleSet;
using Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxCalculationSnapshotDetail;
using Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxCalculationSnapshots;
using Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxRuleSetDetail;
using Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxRuleSets;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Taxation;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore.Query;
using OneOf;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class HrTaxTests
{
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Should_Create_TaxRuleSet_Successfully()
    {
        // Arrange
        var ruleSets = new List<TaxRuleSet>();
        var repo = new FakeRepository<TaxRuleSet>(ruleSets);
        var handler = new CreateTaxRuleSetHandler(repo, _unitOfWork);

        var command = new CreateTaxRuleSetCommand(
            CountryCode: "VN",
            TaxType: "PIT",
            Name: "VN PIT 2026",
            EffectiveFrom: new DateOnly(2026, 1, 1),
            EffectiveTo: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0);
        Assert.Single(ruleSets);
        Assert.Equal("Draft", ruleSets[0].Status);
        Assert.Equal("VN", ruleSets[0].CountryCode);
        Assert.Equal("PIT", ruleSets[0].TaxType);
        Assert.Equal(1, ruleSets[0].Version);
    }

    [Fact]
    public async Task Should_Fail_To_Create_TaxRuleSet_When_Overlapping()
    {
        // Arrange
        var existingRuleSet = new TaxRuleSet
        {
            Id = new TaxRuleSetId(Guid.NewGuid()),
            CountryCode = "VN",
            TaxType = "PIT",
            Name = "Existing PIT",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            Status = "Active",
            Version = 1
        };

        var ruleSets = new List<TaxRuleSet> { existingRuleSet };
        var repo = new FakeRepository<TaxRuleSet>(ruleSets);
        var handler = new CreateTaxRuleSetHandler(repo, _unitOfWork);

        var overlappingCommand = new CreateTaxRuleSetCommand(
            CountryCode: "VN",
            TaxType: "PIT",
            Name: "Overlapping PIT",
            EffectiveFrom: new DateOnly(2026, 6, 1),
            EffectiveTo: null
        );

        // Act
        var result = await handler.Handle(overlappingCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1);
        Assert.Equal("HR_TAX_RULE_SET_OVERLAPPING_PERIOD", result.AsT1.Code);
        Assert.Single(ruleSets); // No new rule set added
    }

    [Fact]
    public async Task Should_Create_TaxBracket_Successfully()
    {
        // Arrange
        var ruleSetId = new TaxRuleSetId(Guid.NewGuid());
        var ruleSet = new TaxRuleSet
        {
            Id = ruleSetId,
            CountryCode = "VN",
            TaxType = "PIT",
            Status = "Draft"
        };

        var ruleSets = new List<TaxRuleSet> { ruleSet };
        var brackets = new List<TaxBracket>();

        var ruleSetRepo = new FakeRepository<TaxRuleSet>(ruleSets);
        var bracketRepo = new FakeRepository<TaxBracket>(brackets);
        var handler = new CreateTaxBracketHandler(ruleSetRepo, bracketRepo, _unitOfWork);

        var command = new CreateTaxBracketCommand(
            TaxRuleSetId: ruleSetId.Value.ToString(),
            FromAmount: 0,
            ToAmount: 5000000,
            Rate: 0.05m,
            QuickDeductionAmount: 0,
            SortOrder: 1
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0);
        Assert.Single(brackets);
        Assert.Equal(0, brackets[0].FromAmount);
        Assert.Equal(5000000, brackets[0].ToAmount);
        Assert.Equal(0.05m, brackets[0].Rate);
    }

    [Fact]
    public async Task Should_Fail_To_Create_Bracket_When_Overlapping()
    {
        // Arrange
        var ruleSetId = new TaxRuleSetId(Guid.NewGuid());
        var ruleSet = new TaxRuleSet
        {
            Id = ruleSetId,
            CountryCode = "VN",
            TaxType = "PIT",
            Status = "Draft"
        };

        var bracket1 = new TaxBracket
        {
            Id = new TaxBracketId(Guid.NewGuid()),
            TaxRuleSetId = ruleSetId,
            FromAmount = 0,
            ToAmount = 5000000,
            Rate = 0.05m,
            SortOrder = 1
        };

        var ruleSets = new List<TaxRuleSet> { ruleSet };
        var brackets = new List<TaxBracket> { bracket1 };

        var ruleSetRepo = new FakeRepository<TaxRuleSet>(ruleSets);
        var bracketRepo = new FakeRepository<TaxBracket>(brackets);
        var handler = new CreateTaxBracketHandler(ruleSetRepo, bracketRepo, _unitOfWork);

        var overlappingCommand = new CreateTaxBracketCommand(
            TaxRuleSetId: ruleSetId.Value.ToString(),
            FromAmount: 3000000, // Overlaps [0, 5M]
            ToAmount: 10000000,
            Rate: 0.10m,
            QuickDeductionAmount: 250000,
            SortOrder: 2
        );

        // Act
        var result = await handler.Handle(overlappingCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1);
        Assert.Equal("HR_TAX_BRACKET_OVERLAPS", result.AsT1.Code);
        Assert.Single(brackets);
    }

    [Fact]
    public async Task Should_Calculate_Progressive_Tax_Successfully()
    {
        // Arrange
        var ruleSetId = new TaxRuleSetId(Guid.NewGuid());
        var ruleSet = new TaxRuleSet
        {
            Id = ruleSetId,
            CountryCode = "VN",
            TaxType = "PIT",
            Name = "VN PIT 2026",
            Status = "Active",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = null,
            Version = 1
        };

        // Standard VN PIT progressive brackets
        var bracket1 = new TaxBracket { TaxRuleSetId = ruleSetId, FromAmount = 0, ToAmount = 5000000, Rate = 0.05m, SortOrder = 1 };
        var bracket2 = new TaxBracket { TaxRuleSetId = ruleSetId, FromAmount = 5000000, ToAmount = 10000000, Rate = 0.10m, SortOrder = 2 };
        var bracket3 = new TaxBracket { TaxRuleSetId = ruleSetId, FromAmount = 10000000, ToAmount = 18000000, Rate = 0.15m, SortOrder = 3 };

        // Standard VN PIT deductions
        var deduction1 = new TaxDeductionRule { TaxRuleSetId = ruleSetId, DeductionType = "PersonalDeduction", Amount = 11000000, IsActive = true };
        var deduction2 = new TaxDeductionRule { TaxRuleSetId = ruleSetId, DeductionType = "DependentDeduction", Amount = 4400000, IsActive = true };

        var ruleSets = new List<TaxRuleSet> { ruleSet };
        var brackets = new List<TaxBracket> { bracket1, bracket2, bracket3 };
        var deductions = new List<TaxDeductionRule> { deduction1, deduction2 };
        var snapshots = new List<TaxCalculationSnapshot>();

        var ruleSetRepo = new FakeRepository<TaxRuleSet>(ruleSets);
        var bracketRepo = new FakeRepository<TaxBracket>(brackets);
        var deductionRepo = new FakeRepository<TaxDeductionRule>(deductions);
        var snapshotRepo = new FakeRepository<TaxCalculationSnapshot>(snapshots);

        var handler = new CalculateTaxHandler(ruleSetRepo, bracketRepo, deductionRepo, snapshotRepo, _unitOfWork);

        // Standalone PIT calculation command
        // Gross Income: 30,000,000. Dependents: 2
        // Total Deductions = Personal (11M) + 2 * Dependent (4.4M) = 11M + 8.8M = 19.8M
        // Taxable Income = 30M - 19.8M = 10.2M
        // Progressive calculation for 10.2M:
        // - Bracket 1: 5M @ 5% = 250k
        // - Bracket 2: 5M @ 10% = 500k
        // - Bracket 3: 0.2M @ 15% = 30k
        // Total Tax = 250k + 500k + 30k = 780k
        var command = new CalculateTaxCommand(
            EmployeeId: Guid.NewGuid().ToString(),
            CountryCode: "VN",
            TaxType: "PIT",
            GrossIncome: 30000000,
            TaxableIncome: 0, // Should be computed dynamically
            Currency: "VND",
            PeriodStart: new DateOnly(2026, 5, 1),
            PeriodEnd: new DateOnly(2026, 5, 31),
            DeductionInputs: new Dictionary<string, decimal> { { "Dependents", 2 } },
            SourceModule: "Simulator"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.Equal(780000, response.TotalTaxAmount);
        Assert.Equal(10200000, response.TaxableIncome);
        Assert.Single(snapshots);
        Assert.Equal(780000, snapshots[0].TotalTaxAmount);
    }

    [Fact]
    public async Task Should_Create_Immutable_Snapshot()
    {
        // Arrange
        var ruleSetId = new TaxRuleSetId(Guid.NewGuid());
        var ruleSet = new TaxRuleSet
        {
            Id = ruleSetId,
            CountryCode = "VN",
            TaxType = "PIT",
            Name = "VN PIT 2026",
            Status = "Active",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = null,
            Version = 1
        };

        var bracket1 = new TaxBracket { TaxRuleSetId = ruleSetId, FromAmount = 0, ToAmount = 10000000, Rate = 0.10m, SortOrder = 1 };
        
        var ruleSets = new List<TaxRuleSet> { ruleSet };
        var brackets = new List<TaxBracket> { bracket1 };
        var deductions = new List<TaxDeductionRule>();
        var snapshots = new List<TaxCalculationSnapshot>();

        var ruleSetRepo = new FakeRepository<TaxRuleSet>(ruleSets);
        var bracketRepo = new FakeRepository<TaxBracket>(brackets);
        var deductionRepo = new FakeRepository<TaxDeductionRule>(deductions);
        var snapshotRepo = new FakeRepository<TaxCalculationSnapshot>(snapshots);

        var handler = new CalculateTaxHandler(ruleSetRepo, bracketRepo, deductionRepo, snapshotRepo, _unitOfWork);

        var command = new CalculateTaxCommand(
            EmployeeId: Guid.NewGuid().ToString(),
            CountryCode: "VN",
            TaxType: "PIT",
            GrossIncome: 15000000,
            TaxableIncome: 10000000,
            Currency: "VND",
            PeriodStart: new DateOnly(2026, 5, 1),
            PeriodEnd: new DateOnly(2026, 5, 31),
            DeductionInputs: new Dictionary<string, decimal>(),
            SourceModule: "Simulator"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0);
        Assert.Single(snapshots);
        var snapshot = snapshots[0];
        Assert.Equal(1000000, snapshot.TotalTaxAmount); // 10M @ 10% = 1M

        // Mutate the original bracket's rate to 50%
        bracket1.Rate = 0.50m;

        // Verify that the snapshot data is unchanged and doesn't point to live database updates
        Assert.Equal(1000000, snapshot.TotalTaxAmount);
        // And the serialised bracket snapshot records the original 10% rate
        Assert.Contains("\"Rate\":0.10", snapshot.BracketSnapshotJson);
        Assert.DoesNotContain("\"Rate\":0.50", snapshot.BracketSnapshotJson);
        Assert.Contains("\"Name\":\"VN PIT 2026\"", snapshot.RuleSetSnapshotJson);
        Assert.Contains("\"Version\":1", snapshot.RuleSetSnapshotJson);
    }

    [Fact]
    public async Task Should_Fail_To_Calculate_When_Negative_Income()
    {
        // Arrange
        var ruleSets = new List<TaxRuleSet>();
        var repo = new FakeRepository<TaxRuleSet>(ruleSets);
        var handler = new CalculateTaxHandler(repo, null, null, null, _unitOfWork);

        var command = new CalculateTaxCommand(
            EmployeeId: Guid.NewGuid().ToString(),
            CountryCode: "VN",
            TaxType: "PIT",
            GrossIncome: -100, // Invalid negative income
            TaxableIncome: 0,
            Currency: "VND",
            PeriodStart: new DateOnly(2026, 5, 1),
            PeriodEnd: new DateOnly(2026, 5, 31),
            DeductionInputs: new Dictionary<string, decimal>(),
            SourceModule: "Simulator"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1);
        Assert.Equal("HR_TAX_CALCULATION_NEGATIVE_INCOME", result.AsT1.Code);
    }

    [Fact]
    public void CalculateTaxValidator_Should_Reject_Invalid_Period_And_Required_Fields()
    {
        var validator = new CalculateTaxValidator();
        var command = new CalculateTaxCommand(
            EmployeeId: Guid.NewGuid().ToString(),
            CountryCode: "",
            TaxType: "",
            GrossIncome: 1000,
            TaxableIncome: 1000,
            Currency: "",
            PeriodStart: new DateOnly(2026, 6, 30),
            PeriodEnd: new DateOnly(2026, 6, 1),
            DeductionInputs: new Dictionary<string, decimal>(),
            SourceModule: ""
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CalculateTaxCommand.CountryCode));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CalculateTaxCommand.TaxType));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CalculateTaxCommand.Currency));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CalculateTaxCommand.SourceModule));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CalculateTaxCommand.PeriodStart));
    }

    [Fact]
    public async Task Should_Fail_To_Calculate_When_No_Active_Rule_Set_Exists()
    {
        var ruleSetId = new TaxRuleSetId(Guid.NewGuid());
        var ruleSets = new List<TaxRuleSet>
        {
            new()
            {
                Id = ruleSetId,
                CountryCode = "VN",
                TaxType = "PIT",
                Status = "Draft",
                EffectiveFrom = new DateOnly(2026, 1, 1),
                Name = "Draft PIT",
                Version = 1
            }
        };
        var handler = new CalculateTaxHandler(
            new FakeRepository<TaxRuleSet>(ruleSets),
            new FakeRepository<TaxBracket>([]),
            new FakeRepository<TaxDeductionRule>([]),
            new FakeRepository<TaxCalculationSnapshot>([]),
            _unitOfWork);

        var command = new CalculateTaxCommand(
            EmployeeId: Guid.NewGuid().ToString(),
            CountryCode: "VN",
            TaxType: "PIT",
            GrossIncome: 1000,
            TaxableIncome: 1000,
            Currency: "VND",
            PeriodStart: new DateOnly(2026, 6, 1),
            PeriodEnd: new DateOnly(2026, 6, 30),
            DeductionInputs: new Dictionary<string, decimal>(),
            SourceModule: "Simulator"
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.Equal("HR_TAX_RULE_SET_NOT_FOUND", result.AsT1.Code);
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
