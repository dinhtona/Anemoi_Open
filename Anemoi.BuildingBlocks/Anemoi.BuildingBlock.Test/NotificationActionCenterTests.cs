using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Domain.Models;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.Centralize.Api.Controllers.Notification;
using Anemoi.Contract.Notification.Commands.NotificationCommands.ArchiveAllReadNotifications;
using Anemoi.Contract.Notification.Commands.NotificationCommands.ArchiveNotification;
using Anemoi.Contract.Notification.Commands.NotificationCommands.ArchiveSelectedNotifications;
using Anemoi.Contract.Notification.Commands.NotificationCommands.ExecuteNotificationAction;
using Anemoi.Contract.Notification.Commands.NotificationCommands.UnarchiveNotification;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.ArchiveAllReadNotifications;
using Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.ArchiveNotification;
using Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.ArchiveSelectedNotifications;
using Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.ExecuteNotificationAction;
using Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.UnarchiveNotification;
using Anemoi.Notification.Application.Mappings;
using Anemoi.Notification.Application.Services;
using Anemoi.Notification.Domain.Models;
using Microsoft.EntityFrameworkCore.Query;
using OneOf;
using Serilog;
using Xunit;

namespace Anemoi.BuildingBlock.Test.NotificationActionCenterTests;

public class NotificationActionCenterTests
{
    private static readonly NotificationMapper Mapper = new();
    private static readonly ILogger Logger = new LoggerConfiguration().CreateLogger();

    // ---- Helper fakes ----

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public bool SavedChanges { get; private set; }

        public Task<OneOf<None, Exception>> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SavedChanges = true;
            return Task.FromResult<OneOf<None, Exception>>(None.Value);
        }

        public Task<OneOf<None, Exception>> BeginTransactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<OneOf<None, Exception>> CommitTransactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private class FakeRepository<T>(List<T> initialItems) : ISqlRepository<T>
        where T : class
    {
        protected readonly List<T> _items = initialItems;

        public IQueryable<T> GetQueryable(Expression<Func<T, bool>> conditionExpression = null)
        {
            var queryable = _items.AsQueryable();
            if (conditionExpression is not null)
                queryable = queryable.Where(conditionExpression);
            return new TestAsyncEnumerable<T>(queryable);
        }

        public IQueryable<T> GetQueryableFromRawQuery(string sql, params object[] parameters) => GetQueryable();

        public virtual Task<T> GetFirstByConditionAsync(
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
            else
            {
                var match = _items.AsQueryable().Where(itemOrFilter.AsT1).ToList();
                foreach (var m in match) _items.Remove(m);
            }
            return Task.FromResult<OneOf<None, Exception>>(None.Value);
        }

        public Task<OneOf<None, Exception>> RemoveManyAsync(OneOf<List<T>, Expression<Func<T, bool>>> itemsOrFilter, CancellationToken token = default) => throw new NotSupportedException();
        public Task<OneOf<T, Exception>> UpdateOneAsync(T item, CancellationToken token = default) => Task.FromResult<OneOf<T, Exception>>(item);
        public Task<OneOf<None, Exception>> UpdateManyAsync(List<T> items, CancellationToken token = default) => throw new NotSupportedException();
    }

    private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

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

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(inner.MoveNext());
        }
    }

    private sealed class TestAsyncQueryProvider<TEntity>(IQueryProvider newInner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public object Execute(Expression expression) => newInner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => newInner.Execute<TResult>(expression);
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = newInner.Execute(expression);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, [executionResult]);
        }
    }

    // Mock executor that succeeds
    private sealed class MockSuccessExecutor : INotificationActionExecutor
    {
        public string ActionCode => "TestAction";

        public Task<OneOf<ExecuteActionResult, ErrorDetailResponse>> ExecuteAsync(
            NotificationHistory notification, NotificationAction action, CancellationToken cancellationToken)
        {
            var result = new ExecuteActionResult(action.ActionType, "/test-url", true, "Executed successfully");
            return Task.FromResult<OneOf<ExecuteActionResult, ErrorDetailResponse>>(result);
        }
    }

    // Mock executor that fails
    private sealed class MockFailingExecutor : INotificationActionExecutor
    {
        public string ActionCode => "FailingAction";

        public Task<OneOf<ExecuteActionResult, ErrorDetailResponse>> ExecuteAsync(
            NotificationHistory notification, NotificationAction action, CancellationToken cancellationToken)
        {
            var error = new ErrorDetailResponse { Code = "EXEC_ERR", Messages = ["Execution failed"] };
            return Task.FromResult<OneOf<ExecuteActionResult, ErrorDetailResponse>>(error);
        }
    }

    private static NotificationActionResolverWrapper CreateResolver(params INotificationActionExecutor[] executors)
        => new(executors);

    private sealed class NotificationActionResolverWrapper : INotificationActionExecutorResolver
    {
        private readonly List<INotificationActionExecutor> _executors;

        public NotificationActionResolverWrapper(IEnumerable<INotificationActionExecutor> executors)
        {
            _executors = executors.ToList();
        }

        public INotificationActionExecutor? Resolve(string actionCode)
            => _executors.FirstOrDefault(e => e.ActionCode == actionCode);
    }

    // ---- Tests ----

    private static NotificationHistory CreateTestNotification(Guid userId, Guid? notificationId = null)
    {
        return new NotificationHistory
        {
            Id = new NotificationHistoryId(notificationId ?? Guid.NewGuid()),
            UserId = userId,
            Title = "Test Notification",
            Content = "Test Content",
            Category = "System",
            CreatedTime = DateTime.UtcNow,
            AggregateType = "LeaveRequest",
            AggregateId = Guid.NewGuid().ToString(),
            WorkflowType = "Approval",
            WorkflowState = "Pending"
        };
    }

    private static NotificationAction CreateTestAction(NotificationHistoryId notificationId, string actionCode = "TestAction", string actionType = "Navigate")
    {
        return new NotificationAction
        {
            Id = new NotificationActionId(Guid.NewGuid()),
            NotificationId = notificationId,
            ActionCode = actionCode,
            ActionLabel = "Test Action",
            ActionUrl = "/test",
            ActionType = actionType,
            RequiresConfirmation = false,
            SortOrder = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task ArchiveNotificationHandler_ShouldArchiveNotification()
    {
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        var repo = new FakeRepository<NotificationHistory>([notification]);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ArchiveNotificationHandler(repo, unitOfWork, Logger);

        var command = new ArchiveNotificationCommand(notification.Id, userId.ToString());
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0);
        Assert.True(notification.IsArchived);
        Assert.NotNull(notification.ArchivedAt);
        Assert.Equal(userId, notification.ArchivedBy);
        Assert.True(unitOfWork.SavedChanges);
    }

    [Fact]
    public async Task ArchiveNotificationHandler_ShouldFailForNonExistentNotification()
    {
        var userId = Guid.NewGuid();
        var repo = new FakeRepository<NotificationHistory>([]);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ArchiveNotificationHandler(repo, unitOfWork, Logger);

        var command = new ArchiveNotificationCommand(new NotificationHistoryId(Guid.NewGuid()), userId.ToString());
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task ArchiveNotificationHandler_ShouldFailForAlreadyArchived()
    {
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        notification.Archive(userId);
        var repo = new FakeRepository<NotificationHistory>([notification]);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ArchiveNotificationHandler(repo, unitOfWork, Logger);

        var command = new ArchiveNotificationCommand(notification.Id, userId.ToString());
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task UnarchiveNotificationHandler_ShouldUnarchiveNotification()
    {
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        notification.Archive(userId);
        var repo = new FakeRepository<NotificationHistory>([notification]);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UnarchiveNotificationHandler(repo, unitOfWork, Logger);

        var command = new UnarchiveNotificationCommand(notification.Id, userId.ToString());
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0);
        Assert.False(notification.IsArchived);
        Assert.Null(notification.ArchivedAt);
        Assert.Null(notification.ArchivedBy);
        Assert.True(unitOfWork.SavedChanges);
    }

    [Fact]
    public async Task UnarchiveNotificationHandler_ShouldFailForNonArchived()
    {
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        var repo = new FakeRepository<NotificationHistory>([notification]);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UnarchiveNotificationHandler(repo, unitOfWork, Logger);

        var command = new UnarchiveNotificationCommand(notification.Id, userId.ToString());
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task ExecuteNotificationActionHandler_ShouldExecuteSuccessfully()
    {
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        var action = CreateTestAction(notification.Id);
        notification.Actions = [action];

        var notifRepo = new FakeRepository<NotificationHistory>([notification]);
        var actionRepo = new FakeRepository<NotificationAction>([action]);
        var auditRepo = new FakeRepository<NotificationActionAudit>([]);
        var unitOfWork = new FakeUnitOfWork();
        var resolver = CreateResolver(new MockSuccessExecutor());
        var handler = new ExecuteNotificationActionHandler(notifRepo, actionRepo, auditRepo, resolver, unitOfWork, Logger);

        var command = new ExecuteNotificationActionCommand(
            notification.Id, action.Id, userId.ToString(), "127.0.0.1", "TestAgent");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.True(response.Success);
        Assert.Equal("/test-url", response.TargetUrl);
        Assert.Equal("Navigate", response.ActionType);
        Assert.True(unitOfWork.SavedChanges);
    }

    [Fact]
    public async Task ExecuteNotificationActionHandler_ShouldCreateAuditRecord()
    {
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        var action = CreateTestAction(notification.Id);
        notification.Actions = [action];

        var notifRepo = new FakeRepository<NotificationHistory>([notification]);
        var actionRepo = new FakeRepository<NotificationAction>([action]);
        var auditItems = new List<NotificationActionAudit>();
        var auditRepo = new FakeRepository<NotificationActionAudit>(auditItems);
        var unitOfWork = new FakeUnitOfWork();
        var resolver = CreateResolver(new MockSuccessExecutor());
        var handler = new ExecuteNotificationActionHandler(notifRepo, actionRepo, auditRepo, resolver, unitOfWork, Logger);

        var command = new ExecuteNotificationActionCommand(
            notification.Id, action.Id, userId.ToString(), "192.168.1.1", "Mozilla/5.0");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0);
        Assert.Single(auditItems);
        var audit = auditItems[0];
        Assert.Equal(notification.Id.Value, audit.NotificationId.Value);
        Assert.Equal(action.Id.Value, audit.ActionId.Value);
        Assert.Equal(userId, audit.ExecutedBy);
        Assert.True(audit.Success);
        Assert.Equal("192.168.1.1", audit.ClientIp);
        Assert.Equal("Mozilla/5.0", audit.UserAgent);
    }

    [Fact]
    public async Task ExecuteNotificationActionHandler_ShouldFailForNonExistentNotification()
    {
        var userId = Guid.NewGuid();
        var notifRepo = new FakeRepository<NotificationHistory>([]);
        var actionRepo = new FakeRepository<NotificationAction>([]);
        var auditRepo = new FakeRepository<NotificationActionAudit>([]);
        var unitOfWork = new FakeUnitOfWork();
        var resolver = CreateResolver(new MockSuccessExecutor());
        var handler = new ExecuteNotificationActionHandler(notifRepo, actionRepo, auditRepo, resolver, unitOfWork, Logger);

        var command = new ExecuteNotificationActionCommand(
            new NotificationHistoryId(Guid.NewGuid()), new NotificationActionId(Guid.NewGuid()),
            userId.ToString(), "127.0.0.1", "TestAgent");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task ExecuteNotificationActionHandler_ShouldFailForOwnershipMismatch()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var notification = CreateTestNotification(ownerId);
        var action = CreateTestAction(notification.Id);
        notification.Actions = [action];

        var notifRepo = new FakeRepository<NotificationHistory>([notification]);
        var actionRepo = new FakeRepository<NotificationAction>([action]);
        var auditRepo = new FakeRepository<NotificationActionAudit>([]);
        var unitOfWork = new FakeUnitOfWork();
        var resolver = CreateResolver(new MockSuccessExecutor());
        var handler = new ExecuteNotificationActionHandler(notifRepo, actionRepo, auditRepo, resolver, unitOfWork, Logger);

        var command = new ExecuteNotificationActionCommand(
            notification.Id, action.Id, otherUserId.ToString(), "127.0.0.1", "TestAgent");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task ExecuteNotificationActionHandler_ShouldFailForHiddenNotification()
    {
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        notification.Hide();
        var action = CreateTestAction(notification.Id);
        notification.Actions = [action];

        var notifRepo = new FakeRepository<NotificationHistory>([notification]);
        var actionRepo = new FakeRepository<NotificationAction>([action]);
        var auditRepo = new FakeRepository<NotificationActionAudit>([]);
        var unitOfWork = new FakeUnitOfWork();
        var resolver = CreateResolver(new MockSuccessExecutor());
        var handler = new ExecuteNotificationActionHandler(notifRepo, actionRepo, auditRepo, resolver, unitOfWork, Logger);

        var command = new ExecuteNotificationActionCommand(
            notification.Id, action.Id, userId.ToString(), "127.0.0.1", "TestAgent");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task ExecuteNotificationActionHandler_ShouldFailForArchivedNotification()
    {
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        notification.Archive(userId);
        var action = CreateTestAction(notification.Id);
        notification.Actions = [action];

        var notifRepo = new FakeRepository<NotificationHistory>([notification]);
        var actionRepo = new FakeRepository<NotificationAction>([action]);
        var auditRepo = new FakeRepository<NotificationActionAudit>([]);
        var unitOfWork = new FakeUnitOfWork();
        var resolver = CreateResolver(new MockSuccessExecutor());
        var handler = new ExecuteNotificationActionHandler(notifRepo, actionRepo, auditRepo, resolver, unitOfWork, Logger);

        var command = new ExecuteNotificationActionCommand(
            notification.Id, action.Id, userId.ToString(), "127.0.0.1", "TestAgent");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task ExecuteNotificationActionHandler_ShouldFailForMissingAction()
    {
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        var notifRepo = new FakeRepository<NotificationHistory>([notification]);
        var actionRepo = new FakeRepository<NotificationAction>([]);
        var auditRepo = new FakeRepository<NotificationActionAudit>([]);
        var unitOfWork = new FakeUnitOfWork();
        var resolver = CreateResolver(new MockSuccessExecutor());
        var handler = new ExecuteNotificationActionHandler(notifRepo, actionRepo, auditRepo, resolver, unitOfWork, Logger);

        var command = new ExecuteNotificationActionCommand(
            notification.Id, new NotificationActionId(Guid.NewGuid()), userId.ToString(), "127.0.0.1", "TestAgent");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task ExecuteNotificationActionHandler_ShouldFailForMissingExecutor()
    {
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        var action = CreateTestAction(notification.Id, "NonExistentExecutor");
        notification.Actions = [action];

        var notifRepo = new FakeRepository<NotificationHistory>([notification]);
        var actionRepo = new FakeRepository<NotificationAction>([action]);
        var auditRepo = new FakeRepository<NotificationActionAudit>([]);
        var unitOfWork = new FakeUnitOfWork();
        var resolver = CreateResolver(); // No executors registered
        var handler = new ExecuteNotificationActionHandler(notifRepo, actionRepo, auditRepo, resolver, unitOfWork, Logger);

        var command = new ExecuteNotificationActionCommand(
            notification.Id, action.Id, userId.ToString(), "127.0.0.1", "TestAgent");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task ExecuteNotificationActionHandler_ShouldCreateAuditOnFailure()
    {
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        var action = CreateTestAction(notification.Id, "FailingAction");
        notification.Actions = [action];

        var notifRepo = new FakeRepository<NotificationHistory>([notification]);
        var actionRepo = new FakeRepository<NotificationAction>([action]);
        var auditItems = new List<NotificationActionAudit>();
        var auditRepo = new FakeRepository<NotificationActionAudit>(auditItems);
        var unitOfWork = new FakeUnitOfWork();
        var resolver = CreateResolver(new MockFailingExecutor());
        var handler = new ExecuteNotificationActionHandler(notifRepo, actionRepo, auditRepo, resolver, unitOfWork, Logger);

        var command = new ExecuteNotificationActionCommand(
            notification.Id, action.Id, userId.ToString(), "10.0.0.1", "TestAgent");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.Single(auditItems);
        var audit = auditItems[0];
        Assert.False(audit.Success);
        Assert.Equal(userId, audit.ExecutedBy);
    }

    [Fact]
    public void NotificationResponse_ShouldContainActionsList()
    {
        var response = new NotificationResponse
        {
            Id = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            Title = "Test",
            Content = "Test",
            Category = "System",
            CreatedTime = DateTime.UtcNow,
            Actions =
            [
                new NotificationActionResponse
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Approve",
                    ActionType = "Command",
                    RequiresConfirmation = true
                },
                new NotificationActionResponse
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "View Details",
                    ActionType = "Navigate",
                    RequiresConfirmation = false
                }
            ]
        };

        Assert.Equal(2, response.Actions.Count);
        Assert.Contains(response.Actions, a => a.Label == "Approve" && a.ActionType == "Command");
        Assert.Contains(response.Actions, a => a.Label == "View Details" && a.ActionType == "Navigate");
    }

    [Fact]
    public void Mapper_ShouldMapActionsToResponse()
    {
        var userId = Guid.NewGuid();
        var history = CreateTestNotification(userId);
        var action = CreateTestAction(history.Id);
        history.Actions = [action];

        var response = Mapper.ToNotificationResponse(history);

        Assert.NotNull(response.Actions);
        Assert.Single(response.Actions);
        Assert.Equal(action.ActionLabel, response.Actions[0].Label);
        Assert.Equal(action.ActionType, response.Actions[0].ActionType);
        Assert.Equal(action.RequiresConfirmation, response.Actions[0].RequiresConfirmation);
    }

    [Fact]
    public void Mapper_ShouldMapWorkflowMetadata()
    {
        var userId = Guid.NewGuid();
        var history = new NotificationHistory
        {
            Id = new NotificationHistoryId(Guid.NewGuid()),
            UserId = userId,
            Title = "Test",
            Content = "Test",
            Category = "Leave",
            CreatedTime = DateTime.UtcNow,
            AggregateType = "LeaveRequest",
            AggregateId = Guid.NewGuid().ToString(),
            WorkflowType = "Approval",
            WorkflowState = "Pending"
        };

        var response = Mapper.ToNotificationResponse(history);

        Assert.Equal("LeaveRequest", response.AggregateType);
        Assert.Equal(history.AggregateId, response.AggregateId);
        Assert.Equal("Approval", response.WorkflowType);
        Assert.Equal("Pending", response.WorkflowState);
    }

    [Fact]
    public async Task DefaultNotificationActionExecutor_ShouldReturnNavigateResult()
    {
        var notification = CreateTestNotification(Guid.NewGuid());
        var action = CreateTestAction(notification.Id, "*", "Navigate");
        var executor = new DefaultNotificationActionExecutor();

        var result = await executor.ExecuteAsync(notification, action, CancellationToken.None);

        Assert.True(result.IsT0);
        var executeResult = result.AsT0;
        Assert.True(executeResult.Success);
        Assert.Equal("/test", executeResult.TargetUrl);
        Assert.Equal("Navigate", executeResult.ActionType);
    }

    [Fact]
    public async Task DefaultNotificationActionExecutor_ShouldReturnNullTargetForCommand()
    {
        var notification = CreateTestNotification(Guid.NewGuid());
        var action = CreateTestAction(notification.Id, "*", "Command");
        var executor = new DefaultNotificationActionExecutor();

        var result = await executor.ExecuteAsync(notification, action, CancellationToken.None);

        Assert.True(result.IsT0);
        var executeResult = result.AsT0;
        Assert.Null(executeResult.TargetUrl);
    }

    [Fact]
    public async Task LeaveApprovalNotificationExecutor_ShouldReturnLeaveRequestUrl()
    {
        var notification = CreateTestNotification(Guid.NewGuid());
        notification.AggregateId = "leave-request-123";
        var action = CreateTestAction(notification.Id, "LeaveApproval", "Navigate");
        var executor = new LeaveApprovalNotificationExecutor();

        var result = await executor.ExecuteAsync(notification, action, CancellationToken.None);

        Assert.True(result.IsT0);
        var executeResult = result.AsT0;
        Assert.True(executeResult.Success);
        Assert.Equal("/leave-requests/leave-request-123", executeResult.TargetUrl);
    }

    [Fact]
    public void NotificationActionExecutorResolver_ShouldReturnCorrectExecutor()
    {
        var successExecutor = new MockSuccessExecutor();
        var failingExecutor = new MockFailingExecutor();
        var resolver = CreateResolver(successExecutor, failingExecutor);

        var resolved = resolver.Resolve("TestAction");
        Assert.NotNull(resolved);
        Assert.IsType<MockSuccessExecutor>(resolved);

        var resolvedFailing = resolver.Resolve("FailingAction");
        Assert.NotNull(resolvedFailing);
        Assert.IsType<MockFailingExecutor>(resolvedFailing);

        var notFound = resolver.Resolve("NonExistent");
        Assert.Null(notFound);
    }

    [Fact]
    public void NotificationHistory_Archive_ShouldSetProperties()
    {
        var notification = CreateTestNotification(Guid.NewGuid());
        var userId = Guid.NewGuid();

        notification.Archive(userId);

        Assert.True(notification.IsArchived);
        Assert.NotNull(notification.ArchivedAt);
        Assert.Equal(userId, notification.ArchivedBy);
    }

    [Fact]
    public void NotificationHistory_Unarchive_ShouldClearProperties()
    {
        var notification = CreateTestNotification(Guid.NewGuid());
        notification.Archive(Guid.NewGuid());

        notification.Unarchive();

        Assert.False(notification.IsArchived);
        Assert.Null(notification.ArchivedAt);
        Assert.Null(notification.ArchivedBy);
    }

    [Fact]
    public void NotificationHistory_Hide_ShouldSetHiddenProperties()
    {
        var notification = CreateTestNotification(Guid.NewGuid());

        notification.Hide();

        Assert.True(notification.IsHidden);
        Assert.NotNull(notification.HiddenAt);
    }

    [Fact]
    public void Permissions_ShouldContainNewNotificationPermissions()
    {
        Assert.Contains(Permissions.NotificationActionExecute, Permissions.All);
        Assert.Contains(Permissions.NotificationArchive, Permissions.All);
    }

    [Fact]
    public async Task ExecuteNotificationActionHandler_ShouldFailForNonExistentAction()
    {
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        var notifRepo = new FakeRepository<NotificationHistory>([notification]);

        // Action repo is empty - the action doesn't exist
        var actionRepo = new FakeRepository<NotificationAction>([]);
        var auditRepo = new FakeRepository<NotificationActionAudit>([]);
        var unitOfWork = new FakeUnitOfWork();
        var resolver = CreateResolver(new MockSuccessExecutor());
        var handler = new ExecuteNotificationActionHandler(notifRepo, actionRepo, auditRepo, resolver, unitOfWork, Logger);

        var command = new ExecuteNotificationActionCommand(
            notification.Id, new NotificationActionId(Guid.NewGuid()), userId.ToString(), "127.0.0.1", "TestAgent");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
        // Verify audit was still created even though action wasn't found
        var auditCheck = await auditRepo.GetFirstByConditionAsync(x => x.NotificationId == notification.Id);
        Assert.Null(auditCheck); // No audit because we failed before trying to execute
    }

    [Fact]
    public async Task Controller_ShouldNotExposeCreateAction()
    {
        var controllerType = typeof(NotificationController);
        var methods = controllerType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly);
        var createMethod = methods.FirstOrDefault(m => m.Name.Contains("Create", StringComparison.OrdinalIgnoreCase));
        Assert.Null(createMethod);
    }
}
