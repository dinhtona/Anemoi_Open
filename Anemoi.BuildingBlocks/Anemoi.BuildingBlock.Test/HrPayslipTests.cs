using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using MediatR;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Domain.Models;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipsForPayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.PublishPayslip;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.CancelPayslip;
using Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDetail;
using Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipsByPayrollRun;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore.Query;
using OneOf;
using Xunit;
using MassTransit;
using NSubstitute;

namespace Anemoi.BuildingBlock.Test;

public sealed class HrPayslipTests
{
    private static PayrollRun CreatePayrollRun(PayrollRunId id, PayrollRunStatus status = PayrollRunStatus.Calculated)
    {
        var run = new PayrollRun
        {
            Id = id,
            PayrollPeriodId = new PayrollPeriodId(Guid.NewGuid()),
            EmployeeId = new EmployeeId(Guid.NewGuid()),
            EmployeeCode = "EMP-TEST",
            EmployeeName = "Test Employee",
            BaseSalary = 3000,
            CurrencyCode = "USD",
            PayScheduleType = "Monthly",
            StandardWorkingDays = 22,
            PaidWorkingDays = 20,
            UnpaidLeaveDays = 2,
            DailyRate = 136.3636m,
            BasePayAmount = 2727.27m,
            TotalAllowanceAmount = 100,
            TotalDeductionAmount = 50,
            GrossAmount = 2827.27m,
            NetAmount = 2777.27m,
            CalculatedAt = DateTime.UtcNow,
            CalculatedBy = "system"
        };

        if (status != PayrollRunStatus.Calculated)
        {
            // Use domain methods to transition to target status
            if (status == PayrollRunStatus.SubmittedForApproval)
            {
                run.SubmitForApproval("test", DateTime.UtcNow);
            }
            else if (status == PayrollRunStatus.Approved)
            {
                run.SubmitForApproval("test", DateTime.UtcNow);
                run.Approve("test", DateTime.UtcNow);
            }
            else if (status == PayrollRunStatus.Rejected)
            {
                run.SubmitForApproval("test", DateTime.UtcNow);
                run.Reject("test", DateTime.UtcNow, "Rejected reason");
            }
            else if (status == PayrollRunStatus.Finalized)
            {
                run.SubmitForApproval("test", DateTime.UtcNow);
                run.Approve("test", DateTime.UtcNow);
                run.FinalizeRun("test", DateTime.UtcNow);
            }
            else if (status == PayrollRunStatus.Cancelled)
            {
                run.SubmitForApproval("test", DateTime.UtcNow);
                run.Cancel("test", DateTime.UtcNow, "Cancelled reason");
            }
        }

        return run;
    }

    private static PayrollPeriod CreatePayrollPeriod(PayrollPeriodId id, string periodCode = "202605")
    {
        return new PayrollPeriod
        {
            Id = id,
            PeriodCode = periodCode,
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            StandardWorkingDays = 22,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
    }

    private static Payslip CreatePayslip(PayslipId id, PayrollRunId runId, EmployeeId empId, PayslipStatus status = PayslipStatus.Generated)
    {
        var payslip = new Payslip
        {
            Id = id,
            PayrollRunId = runId,
            EmployeeId = empId,
            PeriodCode = "202605",
            EmployeeCode = "EMP-TEST",
            EmployeeName = "Test Employee",
            BaseSalarySnapshot = 3000,
            DailyRateSnapshot = 136.3636m,
            PaidWorkingDays = 20,
            PaidLeaveDays = 0,
            UnpaidLeaveDays = 2,
            BasePayAmount = 2727.27m,
            AllowanceTotal = 100,
            DeductionTotal = 50,
            GrossPay = 2827.27m,
            NetPay = 2777.27m,
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = "system"
        };

        if (status == PayslipStatus.Published)
        {
            payslip.Publish("test", DateTime.UtcNow);
        }
        else if (status == PayslipStatus.Cancelled)
        {
            payslip.Cancel("test", DateTime.UtcNow);
        }

        return payslip;
    }

    [Fact]
    public async Task GeneratePayslips_ShouldSucceed_WhenPayrollRunIsFinalized_AndNoPayslipsExist()
    {
        // Arrange
        var runId = new PayrollRunId(Guid.NewGuid());
        var run = CreatePayrollRun(runId, PayrollRunStatus.Finalized);
        var period = CreatePayrollPeriod(run.PayrollPeriodId, "202605");

        var payrollRunRepo = new FakeRepository<PayrollRun>([run]);
        var periodRepo = new FakeRepository<PayrollPeriod>([period]);
        var payslipRepo = new FakeRepository<Payslip>([]);
        var uow = new FakeUnitOfWork();
        var mapper = new PayslipMapper();

        var handler = new GeneratePayslipsForPayrollRunHandler(payrollRunRepo, periodRepo, payslipRepo, uow, mapper);

        // Act
        var result = await handler.Handle(
            new GeneratePayslipsForPayrollRunCommand(runId, "test-user"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT0);
        var list = result.AsT0;
        Assert.Single(list);
        var response = list.First();
        Assert.Equal(runId.Value.ToString(), response.PayrollRunId);
        Assert.Equal(run.EmployeeId.Value.ToString(), response.EmployeeId);
        Assert.Equal("202605", response.PeriodCode);
        Assert.Equal(PayslipStatus.Generated.ToString(), response.Status);
    }

    [Theory]
    [InlineData(PayrollRunStatus.Calculated)]
    [InlineData(PayrollRunStatus.SubmittedForApproval)]
    [InlineData(PayrollRunStatus.Approved)]
    [InlineData(PayrollRunStatus.Rejected)]
    [InlineData(PayrollRunStatus.Cancelled)]
    public async Task GeneratePayslips_ShouldFail_WhenPayrollRunIsNotFinalized(PayrollRunStatus status)
    {
        // Arrange
        var runId = new PayrollRunId(Guid.NewGuid());
        var run = CreatePayrollRun(runId, status);
        var period = CreatePayrollPeriod(run.PayrollPeriodId, "202605");

        var payrollRunRepo = new FakeRepository<PayrollRun>([run]);
        var periodRepo = new FakeRepository<PayrollPeriod>([period]);
        var payslipRepo = new FakeRepository<Payslip>([]);
        var uow = new FakeUnitOfWork();
        var mapper = new PayslipMapper();

        var handler = new GeneratePayslipsForPayrollRunHandler(payrollRunRepo, periodRepo, payslipRepo, uow, mapper);

        // Act
        var result = await handler.Handle(
            new GeneratePayslipsForPayrollRunCommand(runId, "test-user"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.PayrollRunNotFinalized, result.AsT1.Code);
    }

    [Fact]
    public async Task GeneratePayslips_ShouldBeIdempotent_AndSkipExisting_WhenSomePayslipsAlreadyExist()
    {
        // Arrange
        var runId = new PayrollRunId(Guid.NewGuid());
        var run = CreatePayrollRun(runId, PayrollRunStatus.Finalized);
        var period = CreatePayrollPeriod(run.PayrollPeriodId, "202605");
        var existingPayslipId = new PayslipId(Guid.NewGuid());
        var existingPayslip = CreatePayslip(existingPayslipId, runId, run.EmployeeId, PayslipStatus.Generated);

        var payrollRunRepo = new FakeRepository<PayrollRun>([run]);
        var periodRepo = new FakeRepository<PayrollPeriod>([period]);
        var payslipRepo = new FakeRepository<Payslip>([existingPayslip]);
        var uow = new FakeUnitOfWork();
        var mapper = new PayslipMapper();

        var handler = new GeneratePayslipsForPayrollRunHandler(payrollRunRepo, periodRepo, payslipRepo, uow, mapper);

        // Act
        var result = await handler.Handle(
            new GeneratePayslipsForPayrollRunCommand(runId, "test-user"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT0);
        var list = result.AsT0;
        Assert.Single(list);
        var response = list.First();
        Assert.Equal(existingPayslipId.Value.ToString(), response.Id);
        Assert.Equal(PayslipStatus.Generated.ToString(), response.Status);

        // Verify no new payslips were added to repo
        var count = await payslipRepo.CountByConditionAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task PublishPayslip_ShouldSucceed_WhenStatusIsGenerated()
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var runId = new PayrollRunId(Guid.NewGuid());
        var empId = new EmployeeId(Guid.NewGuid());
        var payslip = CreatePayslip(payslipId, runId, empId, PayslipStatus.Generated);

        var payslipRepo = new FakeRepository<Payslip>([payslip]);
        var uow = new FakeUnitOfWork();
        var mapper = new PayslipMapper();

        var handler = new PublishPayslipHandler(payslipRepo, uow, Substitute.For<IPublishEndpoint>(), mapper);

        // Act
        var result = await handler.Handle(
            new PublishPayslipCommand(payslipId, "publisher-user"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT0);
        Assert.Equal(PayslipStatus.Published.ToString(), result.AsT0.Status);
        Assert.Equal("publisher-user", result.AsT0.PublishedBy);
        Assert.NotNull(result.AsT0.PublishedAt);
    }

    [Theory]
    [InlineData(PayslipStatus.Published)]
    [InlineData(PayslipStatus.Cancelled)]
    public async Task PublishPayslip_ShouldFail_WhenStatusIsCancelledOrPublished(PayslipStatus initialStatus)
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var runId = new PayrollRunId(Guid.NewGuid());
        var empId = new EmployeeId(Guid.NewGuid());
        var payslip = CreatePayslip(payslipId, runId, empId, initialStatus);

        var payslipRepo = new FakeRepository<Payslip>([payslip]);
        var uow = new FakeUnitOfWork();
        var mapper = new PayslipMapper();

        var handler = new PublishPayslipHandler(payslipRepo, uow, Substitute.For<IPublishEndpoint>(), mapper);

        // Act
        var result = await handler.Handle(
            new PublishPayslipCommand(payslipId, "publisher-user"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.PayslipInvalidStatus, result.AsT1.Code);
    }

    [Theory]
    [InlineData(PayslipStatus.Generated)]
    [InlineData(PayslipStatus.Published)]
    public async Task CancelPayslip_ShouldSucceed_WhenStatusIsGeneratedOrPublished(PayslipStatus initialStatus)
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var runId = new PayrollRunId(Guid.NewGuid());
        var empId = new EmployeeId(Guid.NewGuid());
        var payslip = CreatePayslip(payslipId, runId, empId, initialStatus);

        var payslipRepo = new FakeRepository<Payslip>([payslip]);
        var uow = new FakeUnitOfWork();
        var mapper = new PayslipMapper();

        var handler = new CancelPayslipHandler(payslipRepo, uow, Substitute.For<IPublishEndpoint>(), mapper);

        // Act
        var result = await handler.Handle(
            new CancelPayslipCommand(payslipId, "canceller-user"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT0);
        Assert.Equal(PayslipStatus.Cancelled.ToString(), result.AsT0.Status);
        Assert.Equal("canceller-user", result.AsT0.CancelledBy);
        Assert.NotNull(result.AsT0.CancelledAt);
    }

    [Fact]
    public async Task CancelPayslip_ShouldFail_WhenStatusIsCancelled()
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var runId = new PayrollRunId(Guid.NewGuid());
        var empId = new EmployeeId(Guid.NewGuid());
        var payslip = CreatePayslip(payslipId, runId, empId, PayslipStatus.Cancelled);

        var payslipRepo = new FakeRepository<Payslip>([payslip]);
        var uow = new FakeUnitOfWork();
        var mapper = new PayslipMapper();

        var handler = new CancelPayslipHandler(payslipRepo, uow, Substitute.For<IPublishEndpoint>(), mapper);

        // Act
        var result = await handler.Handle(
            new CancelPayslipCommand(payslipId, "canceller-user"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.PayslipInvalidStatus, result.AsT1.Code);
    }

    [Fact]
    public async Task GetPayslipDetail_ShouldReturnPayslip_WhenExists()
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var runId = new PayrollRunId(Guid.NewGuid());
        var empId = new EmployeeId(Guid.NewGuid());
        var payslip = CreatePayslip(payslipId, runId, empId, PayslipStatus.Generated);

        var payslipRepo = new FakeRepository<Payslip>([payslip]);
        var mapper = new PayslipMapper();

        var handler = new GetPayslipDetailHandler(payslipRepo, mapper);

        // Act
        var result = await handler.Handle(
            new GetPayslipDetailQuery(payslipId),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT0);
        Assert.Equal(payslipId.Value.ToString(), result.AsT0.Id);
    }

    [Fact]
    public async Task GetPayslipDetail_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var payslipRepo = new FakeRepository<Payslip>([]);
        var mapper = new PayslipMapper();

        var handler = new GetPayslipDetailHandler(payslipRepo, mapper);

        // Act
        var result = await handler.Handle(
            new GetPayslipDetailQuery(payslipId),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT1);
        Assert.Equal("HR_PAYSLIP_NOT_FOUND", result.AsT1.Code);
    }

    [Fact]
    public async Task GetPayslipsByPayrollRun_ShouldReturnPayslips()
    {
        // Arrange
        var runId = new PayrollRunId(Guid.NewGuid());
        var payslip1 = CreatePayslip(new PayslipId(Guid.NewGuid()), runId, new EmployeeId(Guid.NewGuid()), PayslipStatus.Generated);
        var payslip2 = CreatePayslip(new PayslipId(Guid.NewGuid()), runId, new EmployeeId(Guid.NewGuid()), PayslipStatus.Published);
        var otherRunPayslip = CreatePayslip(new PayslipId(Guid.NewGuid()), new PayrollRunId(Guid.NewGuid()), new EmployeeId(Guid.NewGuid()), PayslipStatus.Generated);

        var payslipRepo = new FakeRepository<Payslip>([payslip1, payslip2, otherRunPayslip]);
        var mapper = new PayslipMapper();

        var handler = new GetPayslipsByPayrollRunHandler(payslipRepo, mapper);

        // Act
        var result = await handler.Handle(
            new GetPayslipsByPayrollRunQuery(runId),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT0);
        var list = result.AsT0;
        Assert.Equal(2, list.Count);
        Assert.Contains(list, x => x.Id == payslip1.Id.Value.ToString());
        Assert.Contains(list, x => x.Id == payslip2.Id.Value.ToString());
        Assert.DoesNotContain(list, x => x.Id == otherRunPayslip.Id.Value.ToString());
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

    [Fact]
    public async Task GeneratePayslipPdf_ShouldSucceed_AndSaveToStorageAndDb()
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var runId = new PayrollRunId(Guid.NewGuid());
        var empId = new EmployeeId(Guid.NewGuid());
        var payslip = CreatePayslip(payslipId, runId, empId, PayslipStatus.Generated);

        var payslipRepo = new FakeRepository<Payslip>([payslip]);
        var docRepo = new FakeRepository<PayslipDocument>([]);
        var uow = new FakeUnitOfWork();
        var mapper = new PayslipDocumentMapper();
        var pdfRenderer = new FakePdfRenderer();
        var docStorage = new FakeDocumentStorage();

        var handler = new Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdf.GeneratePayslipPdfHandler(
            payslipRepo, docRepo, uow, pdfRenderer, docStorage, mapper);

        // Act
        var result = await handler.Handle(
            new Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdf.GeneratePayslipPdfCommand(
                payslipId.Value, "test-user", false),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.Equal(payslipId.Value, response.PayslipId);
        Assert.Equal(1, response.Version);
        Assert.True(response.IsActive);

        // Verify file stored
        Assert.True(docStorage.Files.ContainsKey(response.StoragePath));
        // Verify doc saved to repo
        Assert.Equal(1, await docRepo.CountByConditionAsync());
    }

    [Fact]
    public async Task GeneratePayslipPdf_WithForceRegenerate_ShouldDeactivatePreviousAndCreateNewVersion()
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var runId = new PayrollRunId(Guid.NewGuid());
        var empId = new EmployeeId(Guid.NewGuid());
        var payslip = CreatePayslip(payslipId, runId, empId, PayslipStatus.Generated);

        var existingDocId = new PayslipDocumentId(Guid.NewGuid());
        var existingDoc = PayslipDocument.Create(new PayslipDocumentId(Guid.NewGuid()), 
            payslipId, "payslip_v1.pdf", "application/pdf", $"{payslipId.Value}/v1.pdf", 100, "hash1", "test-user", DateTime.UtcNow, 1);

        var payslipRepo = new FakeRepository<Payslip>([payslip]);
        var docRepo = new FakeRepository<PayslipDocument>([existingDoc]);
        var uow = new FakeUnitOfWork();
        var mapper = new PayslipDocumentMapper();
        var pdfRenderer = new FakePdfRenderer();
        var docStorage = new FakeDocumentStorage();

        var handler = new Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdf.GeneratePayslipPdfHandler(
            payslipRepo, docRepo, uow, pdfRenderer, docStorage, mapper);

        // Act
        var result = await handler.Handle(
            new Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdf.GeneratePayslipPdfCommand(
                payslipId.Value, "test-user", true),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.Equal(2, response.Version);
        Assert.True(response.IsActive);

        // Verify existing document was deactivated
        Assert.False(existingDoc.IsActive);
        Assert.Equal(2, await docRepo.CountByConditionAsync());
    }

    [Fact]
    public async Task SendPayslipEmail_ShouldSucceed_AndWritePendingThenSentAuditLogs()
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var runId = new PayrollRunId(Guid.NewGuid());
        var empId = new EmployeeId(Guid.NewGuid());
        var payslip = CreatePayslip(payslipId, runId, empId, PayslipStatus.Published);

        var docId = new PayslipDocumentId(Guid.NewGuid());
        var doc = PayslipDocument.Create(new PayslipDocumentId(Guid.NewGuid()), 
            payslipId, "payslip_v1.pdf", "application/pdf", $"{payslipId.Value}/v1.pdf", 100, "hash1", "test-user", DateTime.UtcNow, 1);

        var payslipRepo = new FakeRepository<Payslip>([payslip]);
        var docRepo = new FakeRepository<PayslipDocument>([doc]);
        var deliveryRepo = new FakeRepository<PayslipEmailDelivery>([]);
        var employeeRepo = new FakeRepository<Anemoi.Hr.Domain.Employees.Employee>([]);
        var uow = new FakeUnitOfWork();
        var mapper = new PayslipDocumentMapper();
        var emailSender = new FakeEmailSender();
        var docStorage = new FakeDocumentStorage();
        docStorage.Files[doc.StoragePath] = [1, 2, 3];

        var handler = new Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmail.SendPayslipEmailHandler(
            payslipRepo, docRepo, deliveryRepo, employeeRepo, uow, emailSender, docStorage, mapper);

        // Act
        var result = await handler.Handle(
            new Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmail.SendPayslipEmailCommand(
                payslipId.Value, "test-user", "test@example.com"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.Equal("test@example.com", response.ToEmail);
        Assert.Equal(PayslipEmailDeliveryStatus.Sent.ToString(), response.Status);

        // Verify audit log exists
        Assert.Equal(1, await deliveryRepo.CountByConditionAsync());
        // Verify email sent
        Assert.Single(emailSender.SentEmails);
    }

    private sealed class FakePdfRenderer : IPayslipPdfRenderer
    {
        public Task<byte[]> RenderAsync(Payslip payslip, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new byte[] { 1, 2, 3 });
        }
    }

    private sealed class FakeDocumentStorage : IPayslipDocumentStorage
    {
        public Dictionary<string, byte[]> Files { get; } = new();

        public Task SaveAsync(string fileKey, Stream content, CancellationToken cancellationToken = default)
        {
            using var ms = new MemoryStream();
            content.CopyTo(ms);
            Files[fileKey] = ms.ToArray();
            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync(string fileKey, CancellationToken cancellationToken = default)
        {
            if (!Files.TryGetValue(fileKey, out var bytes))
                throw new FileNotFoundException();
            return Task.FromResult<Stream>(new MemoryStream(bytes));
        }

        public Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
        {
            Files.Remove(fileKey);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string fileKey, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Files.ContainsKey(fileKey));
        }
    }

    private sealed class FakeEmailSender : IPayslipEmailSender
    {
        public List<(string To, string Subject, string Body, string FileName, byte[] FileBytes)> SentEmails { get; } = new();

        public Task SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string body,
            string attachmentFileName,
            byte[] attachmentBytes,
            CancellationToken cancellationToken = default)
        {
            SentEmails.Add((toEmail, subject, body, attachmentFileName, attachmentBytes));
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task GetPayslipDocumentDownload_ShouldFail_WhenPayslipDoesNotExist()
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var docId = new PayslipDocumentId(Guid.NewGuid());
        var payslipRepo = new FakeRepository<Payslip>([]);
        var docRepo = new FakeRepository<PayslipDocument>([]);
        var storage = new FakeDocumentStorage();
        var handler = new Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload.GetPayslipDocumentDownloadHandler(
            payslipRepo, docRepo, storage);

        // Act
        var result = await handler.Handle(
            new Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload.GetPayslipDocumentDownloadQuery(
                payslipId.Value, docId.Value),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.PayslipNotFound, result.AsT1.Code);
    }

    [Fact]
    public async Task GetPayslipDocumentDownload_ShouldFail_WhenActiveDocumentDoesNotExist()
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var docId = new PayslipDocumentId(Guid.NewGuid());
        var runId = new PayrollRunId(Guid.NewGuid());
        var empId = new EmployeeId(Guid.NewGuid());
        var payslip = CreatePayslip(payslipId, runId, empId, PayslipStatus.Generated);
        
        var payslipRepo = new FakeRepository<Payslip>([payslip]);
        var docRepo = new FakeRepository<PayslipDocument>([]);
        var storage = new FakeDocumentStorage();
        var handler = new Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload.GetPayslipDocumentDownloadHandler(
            payslipRepo, docRepo, storage);

        // Act
        var result = await handler.Handle(
            new Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload.GetPayslipDocumentDownloadQuery(
                payslipId.Value, docId.Value),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.PayslipDocumentNotFound, result.AsT1.Code);
    }

    [Fact]
    public async Task GetPayslipDocumentDownload_ShouldFail_WhenPhysicalFileIsMissing()
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var runId = new PayrollRunId(Guid.NewGuid());
        var empId = new EmployeeId(Guid.NewGuid());
        var payslip = CreatePayslip(payslipId, runId, empId, PayslipStatus.Generated);
        
        var doc = PayslipDocument.Create(new PayslipDocumentId(Guid.NewGuid()), 
            payslipId, "payslip.pdf", "application/pdf", $"{payslipId.Value}/v1.pdf", 100, "hash", "test", DateTime.UtcNow, 1);

        var payslipRepo = new FakeRepository<Payslip>([payslip]);
        var docRepo = new FakeRepository<PayslipDocument>([doc]);
        var storage = new FakeDocumentStorage(); // No files saved in storage
        var handler = new Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload.GetPayslipDocumentDownloadHandler(
            payslipRepo, docRepo, storage);

        // Act
        var result = await handler.Handle(
            new Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload.GetPayslipDocumentDownloadQuery(
                payslipId.Value, doc.Id.Value),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.PayslipDocumentNotFound, result.AsT1.Code);
    }

    [Fact]
    public async Task GetPayslipDocumentDownload_ShouldSucceed_WhenEverythingIsValid()
    {
        // Arrange
        var payslipId = new PayslipId(Guid.NewGuid());
        var runId = new PayrollRunId(Guid.NewGuid());
        var empId = new EmployeeId(Guid.NewGuid());
        var payslip = CreatePayslip(payslipId, runId, empId, PayslipStatus.Generated);
        
        var doc = PayslipDocument.Create(new PayslipDocumentId(Guid.NewGuid()), 
            payslipId, "payslip.pdf", "application/pdf", $"{payslipId.Value}/v1.pdf", 100, "hash", "test", DateTime.UtcNow, 1);

        var payslipRepo = new FakeRepository<Payslip>([payslip]);
        var docRepo = new FakeRepository<PayslipDocument>([doc]);
        var storage = new FakeDocumentStorage();
        storage.Files[doc.StoragePath] = [1, 2, 3];
        
        var handler = new Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload.GetPayslipDocumentDownloadHandler(
            payslipRepo, docRepo, storage);

        // Act
        var result = await handler.Handle(
            new Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload.GetPayslipDocumentDownloadQuery(
                payslipId.Value, doc.Id.Value),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.Equal("payslip.pdf", response.FileName);
        Assert.Equal("application/pdf", response.ContentType);
        Assert.NotNull(response.Stream);
    }

    [Fact]
    public async Task GeneratePayslipPdfsForPayrollRun_ShouldReturnPartialFailure_WhenSomePayslipsFail()
    {
        // Arrange
        var runId = new PayrollRunId(Guid.NewGuid());
        var run = CreatePayrollRun(runId, PayrollRunStatus.Finalized);

        var payslip1 = CreatePayslip(new PayslipId(Guid.NewGuid()), runId, new EmployeeId(Guid.NewGuid()), PayslipStatus.Generated);
        var payslip2 = CreatePayslip(new PayslipId(Guid.NewGuid()), runId, new EmployeeId(Guid.NewGuid()), PayslipStatus.Generated);

        var payrollRunRepo = new FakeRepository<PayrollRun>([run]);
        var payslipRepo = new FakeRepository<Payslip>([payslip1, payslip2]);
        var docRepo = new FakeRepository<PayslipDocument>([]);

        // Mediator callback: succeed for payslip1, fail for payslip2
        var mediator = new FakeMediator(req =>
        {
            if (req is Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdf.GeneratePayslipPdfCommand cmd)
            {
                if (cmd.PayslipId == payslip1.Id.Value)
                {
                    return OneOf<PayslipDocumentResponse, ErrorDetailResponse>.FromT0(new PayslipDocumentResponse
                    {
                        Id = Guid.NewGuid(),
                        PayslipId = payslip1.Id.Value,
                        FileName = "payslip1.pdf",
                        StoragePath = "path1",
                        Version = 1,
                        IsActive = true
                    });
                }
                else
                {
                    return OneOf<PayslipDocumentResponse, ErrorDetailResponse>.FromT1(
                        new ErrorDetailResponse { Code = "MOCK_ERROR", Messages = new[] { "Generation failed" } });
                }
            }
            throw new InvalidOperationException();
        });

        var handler = new Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdfsForPayrollRun.GeneratePayslipPdfsForPayrollRunHandler(
            payrollRunRepo, payslipRepo, docRepo, mediator);

        // Act
        var result = await handler.Handle(
            new Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdfsForPayrollRun.GeneratePayslipPdfsForPayrollRunCommand(
                runId.Value, "test", false),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.Equal(2, response.Total);
        Assert.Equal(1, response.Succeeded);
        Assert.Equal(1, response.Failed);
        Assert.Equal(0, response.Skipped);

        var item1 = response.Items.First(x => x.PayslipId == payslip1.Id.Value);
        Assert.Equal("Succeeded", item1.Status);

        var item2 = response.Items.First(x => x.PayslipId == payslip2.Id.Value);
        Assert.Equal("Failed", item2.Status);
        Assert.Equal("MOCK_ERROR", item2.ErrorCode);
    }

    [Fact]
    public async Task SendPayslipEmailsForPayrollRun_ShouldReturnPartialFailure_WhenSomeEmailsFail()
    {
        // Arrange
        var runId = new PayrollRunId(Guid.NewGuid());
        var run = CreatePayrollRun(runId, PayrollRunStatus.Finalized);

        var payslip1 = CreatePayslip(new PayslipId(Guid.NewGuid()), runId, new EmployeeId(Guid.NewGuid()), PayslipStatus.Published);
        var payslip2 = CreatePayslip(new PayslipId(Guid.NewGuid()), runId, new EmployeeId(Guid.NewGuid()), PayslipStatus.Published);
        var payslip3 = CreatePayslip(new PayslipId(Guid.NewGuid()), runId, new EmployeeId(Guid.NewGuid()), PayslipStatus.Generated);

        var doc1 = PayslipDocument.Create(new PayslipDocumentId(Guid.NewGuid()), 
            payslip1.Id, "payslip1.pdf", "application/pdf", "path1", 100, "h1", "test", DateTime.UtcNow, 1);
        var doc2 = PayslipDocument.Create(new PayslipDocumentId(Guid.NewGuid()), 
            payslip2.Id, "payslip2.pdf", "application/pdf", "path2", 100, "h2", "test", DateTime.UtcNow, 1);

        var payrollRunRepo = new FakeRepository<PayrollRun>([run]);
        var payslipRepo = new FakeRepository<Payslip>([payslip1, payslip2, payslip3]);
        var docRepo = new FakeRepository<PayslipDocument>([doc1, doc2]);

        // Mediator callback
        var mediator = new FakeMediator(req =>
        {
            if (req is Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmail.SendPayslipEmailCommand cmd)
            {
                if (cmd.PayslipId == payslip1.Id.Value)
                {
                    return OneOf<PayslipEmailDeliveryResponse, ErrorDetailResponse>.FromT0(new PayslipEmailDeliveryResponse
                    {
                        Id = Guid.NewGuid(),
                        PayslipId = payslip1.Id.Value,
                        PayslipDocumentId = doc1.Id.Value,
                        ToEmail = "emp1@test.com",
                        Status = "Sent"
                    });
                }
                else
                {
                    return OneOf<PayslipEmailDeliveryResponse, ErrorDetailResponse>.FromT1(
                        new ErrorDetailResponse { Code = "SMTP_ERROR", Messages = new[] { "Connection timeout" } });
                }
            }
            throw new InvalidOperationException();
        });

        var handler = new Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmailsForPayrollRun.SendPayslipEmailsForPayrollRunHandler(
            payrollRunRepo, payslipRepo, docRepo, mediator);

        // Act
        var result = await handler.Handle(
            new Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmailsForPayrollRun.SendPayslipEmailsForPayrollRunCommand(
                runId.Value, "test"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.Equal(3, response.Total);
        Assert.Equal(1, response.Succeeded);
        Assert.Equal(1, response.Failed);
        Assert.Equal(1, response.Skipped);

        var item1 = response.Items.First(x => x.PayslipId == payslip1.Id.Value);
        Assert.Equal("Succeeded", item1.Status);

        var item2 = response.Items.First(x => x.PayslipId == payslip2.Id.Value);
        Assert.Equal("Failed", item2.Status);
        Assert.Equal("SMTP_ERROR", item2.ErrorCode);

        var item3 = response.Items.First(x => x.PayslipId == payslip3.Id.Value);
        Assert.Equal("Skipped", item3.Status);
    }

    private sealed class FakeMediator(Func<object, object> sendCallback) : IMediator
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var res = sendCallback(request);
            return Task.FromResult((TResponse)res);
        }

        public Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResponse>
        {
            var res = sendCallback(request);
            return Task.FromResult((TResponse)res);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            sendCallback(request);
            return Task.CompletedTask;
        }

        public Task<object> Send(object request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(sendCallback(request));
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
