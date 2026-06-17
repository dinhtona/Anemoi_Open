using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Domain.Models;
using Anemoi.BuildingBlock.Application.Resources;
using Microsoft.EntityFrameworkCore.Query;
using Anemoi.Centralize.Api.Controllers.Notification;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Commands.NotificationCommands.MarkAllAsRead;
using Anemoi.Contract.Notification.Commands.NotificationCommands.MarkAsRead;
using Anemoi.Contract.Notification.Commands.NotificationSettingsCommands.UpdateNotificationSettings;
using Anemoi.Contract.Notification.Constants;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Contract.Notification.Queries.NotificationQueries.GetNotifications;
using Anemoi.Contract.Notification.Queries.NotificationQueries.GetUnreadNotificationCount;
using Anemoi.Contract.Notification.Queries.NotificationSettingsQueries.GetNotificationSettings;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.CreateNotification;
using Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.MarkAllAsRead;
using Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.MarkAsRead;
using Anemoi.Notification.Application.Cqrs.Commands.NotificationSettingsCommands.UpdateNotificationSettings;
using Anemoi.Notification.Application.Cqrs.Queries.NotificationQueries.GetNotifications;
using Anemoi.Notification.Application.Cqrs.Queries.NotificationQueries.GetUnreadNotificationCount;
using Anemoi.Notification.Application.Cqrs.Queries.NotificationSettingsQueries.GetNotificationSettings;
using Anemoi.Notification.Application.Mappings;
using Anemoi.Notification.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using Serilog;
using Xunit;
using MassTransit;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Anemoi.Centralize.Api.Hubs;
using Anemoi.Centralize.Api.Consumers;
using Anemoi.Contract.Notification.Events;
using NSubstitute;

namespace Anemoi.BuildingBlock.Test.NotificationTests;

public class NotificationTests
{
    private static readonly NotificationMapper Mapper = new();
    private static readonly ILogger Logger = new LoggerConfiguration().CreateLogger();

    [Fact]
    public void NotificationController_ShouldNotHaveCreateNotificationAction()
    {
        var controllerType = typeof(NotificationController);
        var methods = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        var createMethod = methods.FirstOrDefault(m => m.Name.Contains("Create", StringComparison.OrdinalIgnoreCase));

        Assert.Null(createMethod);
    }

    [Fact]
    public void Validator_ShouldPassForValidPlaintextCommand()
    {
        var validator = new CreateNotificationCommandValidator();
        var command = new CreateNotificationCommand(
            UserId: Guid.NewGuid().ToString(),
            Title: "Valid Plaintext Title",
            Content: "Valid Plaintext Content",
            Category: "System"
        );

        var result = validator.Validate(command);
        Assert.True(result.IsValid, string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [Fact]
    public void Validator_ShouldPassForLocalizationOnlyCommand()
    {
        var validator = new CreateNotificationCommandValidator();
        var command = new CreateNotificationCommand(
            UserId: Guid.NewGuid().ToString(),
            Category: "Workspace",
            TitleLocalizationKey: "title_key",
            TitleLocalizationArgs: new List<string> { "arg1" },
            ContentLocalizationKey: "content_key",
            ContentLocalizationArgs: new List<string> { "arg2" }
        );

        var result = validator.Validate(command);
        Assert.True(result.IsValid, string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [Fact]
    public void Validator_ShouldFailIfBothPlaintextAndLocalizationMissing()
    {
        var validator = new CreateNotificationCommandValidator();
        var command = new CreateNotificationCommand(
            UserId: Guid.NewGuid().ToString(),
            Category: "System"
        );

        var result = validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
        Assert.Contains(result.Errors, e => e.PropertyName == "Content");
    }

    [Fact]
    public void Validator_ShouldFailForInvalidCategory()
    {
        var validator = new CreateNotificationCommandValidator();
        var command = new CreateNotificationCommand(
            UserId: Guid.NewGuid().ToString(),
            Title: "Valid Title",
            Content: "Valid Content",
            Category: "InvalidCategoryValue"
        );

        var result = validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Category");
    }

    [Fact]
    public void Validator_ShouldBeCaseInsensitiveForAllowedCategories()
    {
        var validator = new CreateNotificationCommandValidator();
        var command = new CreateNotificationCommand(
            UserId: Guid.NewGuid().ToString(),
            Title: "Test Title",
            Content: "Test Content",
            Category: "system" // lowercase
        );

        var result = validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreateNotificationHandler_ShouldDeduplicateAndNotInsert()
    {
        var targetUserId = Guid.NewGuid();
        var dupKey = "unique-dedup-key";
        var existingNotification = new NotificationHistory
        {
            Id = new NotificationHistoryId(Guid.NewGuid()),
            UserId = new UserId(targetUserId),
            Title = "Original T",
            Content = "Original C",
            Category = "System",
            DeduplicationKey = dupKey,
            CreatedTime = DateTime.UtcNow
        };

        var notificationsList = new List<NotificationHistory> { existingNotification };
        var repo = new FakeRepository<NotificationHistory>(notificationsList);
        var subRepo = new FakeRepository<NotificationSubscription>([]);
        var preferenceRepo = new FakeRepository<NotificationPreference>([]);
        var unitOfWork = new FakeUnitOfWork();
        var mockPublisher = new MockPublishEndpoint();

        var handler = new CreateNotificationHandler(repo, subRepo, preferenceRepo, unitOfWork, mockPublisher, Mapper, Logger);

        var command = new CreateNotificationCommand(
            UserId: targetUserId.ToString(),
            Title: "Duplicate Title Attempt",
            Content: "Duplicate Content Attempt",
            Category: "System",
            DeduplicationKey: dupKey
        );

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.True(response.IsT0);
        var result = response.AsT0;
        Assert.Equal(existingNotification.Id.Value.ToString(), result.Id);
        Assert.Equal("Original T", result.Title);
        // Ensure no new records were created or saved
        Assert.Single(notificationsList);
        Assert.False(unitOfWork.SavedChanges);
        Assert.Empty(mockPublisher.PublishedEvents);
    }

    [Fact]
    public async Task CreateNotificationHandler_ShouldRecoverOnConcurrentException()
    {
        var targetUserId = new UserId(Guid.NewGuid());
        var dupKey = "concurrent-dedup-key";

        var notificationsList = new List<NotificationHistory>();
        var subRepo = new FakeRepository<NotificationSubscription>([]);
        var preferenceRepo = new FakeRepository<NotificationPreference>([]);
        
        // Setup repository that returns the notification once it's created concurrently
        var repo = new FakeRepositoryWithConcurrentFallback<NotificationHistory>(notificationsList, dupKey, targetUserId);
        var unitOfWork = new FakeUnitOfWorkWithException(); // Throws on SaveChanges
        var mockPublisher = new MockPublishEndpoint();

        var handler = new CreateNotificationHandler(repo, subRepo, preferenceRepo, unitOfWork, mockPublisher, Mapper, Logger);

        var command = new CreateNotificationCommand(
            UserId: targetUserId.ToString(),
            Title: "Attempt Title",
            Content: "Attempt Content",
            Category: "System",
            DeduplicationKey: dupKey
        );

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.True(response.IsT0);
        var result = response.AsT0;
        Assert.Equal("Concurrent Title", result.Title);
        Assert.Equal(dupKey, result.DeduplicationKey);
    }

    [Fact]
    public async Task GetNotificationsHandler_ShouldFilterByUserId()
    {
        var targetUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var notifications = new List<NotificationHistory>
        {
            new() { Id = new NotificationHistoryId(Guid.NewGuid()), UserId = new UserId(targetUserId), Title = "T1", Content = "C1", Category = "System", CreatedTime = DateTime.UtcNow },
            new() { Id = new NotificationHistoryId(Guid.NewGuid()), UserId = new UserId(targetUserId), Title = "T2", Content = "C2", Category = "Workspace", CreatedTime = DateTime.UtcNow },
            new() { Id = new NotificationHistoryId(Guid.NewGuid()), UserId = new UserId(otherUserId), Title = "T3", Content = "C3", Category = "System", CreatedTime = DateTime.UtcNow }
        };

        var repo = new FakeRepository<NotificationHistory>(notifications);
        var handler = new GetNotificationsHandler(repo, Mapper, Logger);

        var query = new GetNotificationsQuery(targetUserId.ToString())
        {
            PageIndex = 1,
            PageSize = 10
        };

        var response = await handler.Handle(query, CancellationToken.None);
        Assert.Equal(2, response.TotalRecord);
        Assert.All(response.Items, item => Assert.Equal(targetUserId.ToString(), item.UserId));
    }

    [Fact]
    public async Task GetUnreadNotificationCountHandler_ShouldCountOnlyUnreadForUser()
    {
        var targetUserId = Guid.NewGuid();

        var notifications = new List<NotificationHistory>
        {
            new() { Id = new NotificationHistoryId(Guid.NewGuid()), UserId = new UserId(targetUserId), CreatedTime = DateTime.UtcNow },
            new() { Id = new NotificationHistoryId(Guid.NewGuid()), UserId = new UserId(targetUserId), CreatedTime = DateTime.UtcNow },
            new() { Id = new NotificationHistoryId(Guid.NewGuid()), UserId = new UserId(Guid.NewGuid()), CreatedTime = DateTime.UtcNow }
        };
        notifications[1].MarkAsRead();

        var repo = new FakeRepository<NotificationHistory>(notifications);
        var handler = new GetUnreadNotificationCountHandler(repo, Logger);

        var query = new GetUnreadNotificationCountQuery(targetUserId.ToString());
        var response = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(1, response.Count);
    }

    [Fact]
    public async Task MarkNotificationAsReadHandler_ShouldUpdateDatabase()
    {
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var notification = new NotificationHistory
        {
            Id = new NotificationHistoryId(notificationId),
            UserId = new UserId(userId),
            CreatedTime = DateTime.UtcNow
        };

        var repo = new FakeRepository<NotificationHistory>(new List<NotificationHistory> { notification });
        var unitOfWork = new FakeUnitOfWork();
        var handler = new MarkNotificationAsReadHandler(repo, unitOfWork, Logger);

        var command = new MarkNotificationAsReadCommand(new NotificationHistoryId(notificationId), userId.ToString());
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0);
        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadTime);
        Assert.True(unitOfWork.SavedChanges);
    }

    [Fact]
    public async Task UpdateNotificationSettingsHandler_ShouldSaveSubscriptionSettings()
    {
        var userId = Guid.NewGuid();
        var repo = new FakeRepository<NotificationSubscription>([]);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UpdateNotificationSettingsHandler(repo, unitOfWork, Logger);

        var settings = new List<NotificationSettingResponse>
        {
            new() { Category = "System", IsEnabled = false },
            new() { Category = "Workspace", IsEnabled = true }
        };

        var command = new UpdateNotificationSettingsCommand(userId.ToString(), settings);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0);
        Assert.True(unitOfWork.SavedChanges);
        
        var systemSubscription = await repo.GetFirstByConditionAsync(x => x.UserId == new UserId(userId) && x.Category == "System");
        Assert.NotNull(systemSubscription);
        Assert.False(systemSubscription.IsEnabled);

        var workspaceSubscription = await repo.GetFirstByConditionAsync(x => x.UserId == new UserId(userId) && x.Category == "Workspace");
        Assert.NotNull(workspaceSubscription);
        Assert.True(workspaceSubscription.IsEnabled);
    }

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

    private sealed class FakeUnitOfWorkWithException : IUnitOfWork
    {
        public Task<OneOf<None, Exception>> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Simulate unique constraint violation exception from DB
            return Task.FromResult<OneOf<None, Exception>>(new Exception("Unique constraint violation"));
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

    private sealed class FakeRepositoryWithConcurrentFallback<T>(List<T> initialItems, string dupKey, UserId targetUserId)
        : FakeRepository<T>(initialItems) where T : class
    {
        public override Task<T> GetFirstByConditionAsync(
            Expression<Func<T, bool>> conditionExpression = null,
            Func<IQueryable<T>, IQueryable<T>> specialAction = null,
            CancellationToken token = default)
        {
            // If the query filter checks for the DeduplicationKey, simulate finding the concurrently created record.
            var filterStr = conditionExpression?.ToString();
            if (filterStr != null && filterStr.Contains("DeduplicationKey"))
            {
                var concurrentRecord = new NotificationHistory
                {
                    Id = new NotificationHistoryId(Guid.NewGuid()),
                    UserId = targetUserId,
                    Title = "Concurrent Title",
                    Content = "Concurrent Content",
                    Category = "System",
                    DeduplicationKey = dupKey,
                    CreatedTime = DateTime.UtcNow
                } as T;
                return Task.FromResult(concurrentRecord);
            }
            return base.GetFirstByConditionAsync(conditionExpression, specialAction, token);
        }
    }

    private sealed class MockPublishEndpoint : IPublishEndpoint
    {
        public List<object> PublishedEvents { get; } = [];

        public ConnectHandle ConnectPublishObserver(IPublishObserver observer) => throw new NotSupportedException();

        public Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            PublishedEvents.Add(message);
            return Task.CompletedTask;
        }

        public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class => throw new NotSupportedException();
        public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class => throw new NotSupportedException();
        public Task Publish(object message, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Publish<T>(object values, CancellationToken cancellationToken = default) where T : class => throw new NotSupportedException();
        public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class => throw new NotSupportedException();
        public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class => throw new NotSupportedException();
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

    [Fact]
    public void DataChangeOccurredValidator_ShouldValidateTagsAndProperties()
    {
        var validator = new DataChangeOccurredIntegrationEventValidator();

        // 1. Valid Event
        var validEvent = new DataChangeOccurredIntegrationEvent
        {
            Resource = "Employee",
            Action = "Create",
            EntityId = "123",
            Sensitivity = "Low",
            QueryTags = new List<string> { "tag1", "tag2" }
        };
        var res1 = validator.Validate(validEvent);
        Assert.True(res1.IsValid, string.Join(", ", res1.Errors.Select(e => e.ErrorMessage)));

        // 2. Invalid Action & Sensitivity
        var invalidEvent1 = validEvent with { Action = "InvalidAction", Sensitivity = "SuperSensitive" };
        var res2 = validator.Validate(invalidEvent1);
        Assert.False(res2.IsValid);
        Assert.Contains(res2.Errors, e => e.PropertyName == "Action");
        Assert.Contains(res2.Errors, e => e.PropertyName == "Sensitivity");

        // 3. Exceeded query tags limit
        var largeTags = Enumerable.Range(1, 21).Select(i => $"tag{i}").ToList();
        var invalidEvent2 = validEvent with { QueryTags = largeTags };
        var res3 = validator.Validate(invalidEvent2);
        Assert.False(res3.IsValid);
        Assert.Contains(res3.Errors, e => e.PropertyName == "QueryTags");

        // 4. Exceeded tag character length limit
        var longTag = new string('a', 101);
        var invalidEvent3 = validEvent with { QueryTags = new List<string> { longTag } };
        var res4 = validator.Validate(invalidEvent3);
        Assert.False(res4.IsValid);
        Assert.Contains(res4.Errors, e => e.PropertyName == "QueryTags[0]");
    }

    [Fact]
    public async Task DataChangeOccurredConsumer_ShouldBroadcastGlobally_WhenWorkspaceIdIsNullAndSensitivityIsLow()
    {
        var hubContext = Substitute.For<IHubContext<NotificationHub>>();
        var hubClients = Substitute.For<IHubClients>();
        var clientProxy = Substitute.For<IClientProxy>();

        hubContext.Clients.Returns(hubClients);
        hubClients.All.Returns(clientProxy);

        var consumer = new DataChangeOccurredIntegrationEventConsumer(hubContext, Logger);

        var @event = new DataChangeOccurredIntegrationEvent
        {
            Resource = "Employee",
            Action = "Create",
            EntityId = "123",
            WorkspaceId = null,
            Sensitivity = "Low",
            QueryTags = new List<string> { "tag1" }
        };

        var mockConsumeContext = Substitute.For<ConsumeContext<DataChangeOccurredIntegrationEvent>>();
        mockConsumeContext.Message.Returns(@event);

        await consumer.Consume(mockConsumeContext);

        await clientProxy.Received(1).SendCoreAsync(
            NotificationConstants.SignalRMethods.ReceiveDataChange,
            Arg.Is<object[]>(args => args.Length == 1 && Equals(args[0], @event)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DataChangeOccurredConsumer_ShouldSkipBroadcast_WhenWorkspaceIdIsNullAndSensitivityIsNotLow()
    {
        var hubContext = Substitute.For<IHubContext<NotificationHub>>();
        var hubClients = Substitute.For<IHubClients>();
        hubContext.Clients.Returns(hubClients);

        var consumer = new DataChangeOccurredIntegrationEventConsumer(hubContext, Logger);

        var @event = new DataChangeOccurredIntegrationEvent
        {
            Resource = "Employee",
            Action = "Update",
            EntityId = "123",
            WorkspaceId = null,
            Sensitivity = "High",
            QueryTags = new List<string> { "tag1" }
        };

        var mockConsumeContext = Substitute.For<ConsumeContext<DataChangeOccurredIntegrationEvent>>();
        mockConsumeContext.Message.Returns(@event);

        await consumer.Consume(mockConsumeContext);

        var temp = hubClients.DidNotReceive().All;
    }

    [Fact]
    public async Task DataChangeOccurredConsumer_ShouldBroadcastToWorkspaceGroup_WhenWorkspaceIdIsProvided()
    {
        var hubContext = Substitute.For<IHubContext<NotificationHub>>();
        var hubClients = Substitute.For<IHubClients>();
        var groupProxy = Substitute.For<IClientProxy>();

        hubContext.Clients.Returns(hubClients);
        hubClients.Group("Workspace-my-workspace-id").Returns(groupProxy);

        var consumer = new DataChangeOccurredIntegrationEventConsumer(hubContext, Logger);

        var @event = new DataChangeOccurredIntegrationEvent
        {
            Resource = "Employee",
            Action = "Delete",
            EntityId = "123",
            WorkspaceId = "my-workspace-id",
            Sensitivity = "High",
            QueryTags = new List<string> { "tag1" }
        };

        var mockConsumeContext = Substitute.For<ConsumeContext<DataChangeOccurredIntegrationEvent>>();
        mockConsumeContext.Message.Returns(@event);

        await consumer.Consume(mockConsumeContext);

        var temp = hubClients.DidNotReceive().All;
        await groupProxy.Received(1).SendCoreAsync(
            NotificationConstants.SignalRMethods.ReceiveDataChange,
            Arg.Is<object[]>(args => args.Length == 1 && Equals(args[0], @event)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Hub_JoinWorkspace_ShouldAllowAuthorizedUsers()
    {
        var hub = new NotificationHub(new MockLogger<NotificationHub>(), new MockConnectedUsersRegistry(), new MockLocalizer());
        
        var workspaceId = "tenant-123";
        var claims = new List<Claim> { new("workspaceId", workspaceId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(Guid.NewGuid().ToString());
        context.UserIdentifier.Returns("user-1");
        context.User.Returns(principal);

        var groupManager = Substitute.For<IGroupManager>();

        hub.Context = context;
        hub.Groups = groupManager;

        await hub.JoinWorkspace(workspaceId);

        await groupManager.Received(1).AddToGroupAsync(context.ConnectionId, $"Workspace-{workspaceId}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Hub_JoinWorkspace_ShouldAllowAdministrators()
    {
        var hub = new NotificationHub(new MockLogger<NotificationHub>(), new MockConnectedUsersRegistry(), new MockLocalizer());

        var workspaceId = "tenant-123";
        var claims = new List<Claim> { new(ClaimTypes.Role, "Administrator") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(Guid.NewGuid().ToString());
        context.UserIdentifier.Returns("user-admin");
        context.User.Returns(principal);

        var groupManager = Substitute.For<IGroupManager>();

        hub.Context = context;
        hub.Groups = groupManager;

        await hub.JoinWorkspace(workspaceId);

        await groupManager.Received(1).AddToGroupAsync(context.ConnectionId, $"Workspace-{workspaceId}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Hub_JoinWorkspace_ShouldRejectUnauthorizedUsers()
    {
        var hub = new NotificationHub(new MockLogger<NotificationHub>(), new MockConnectedUsersRegistry(), new MockLocalizer());

        var workspaceId = "tenant-123";
        var claims = new List<Claim> { new("workspaceId", "different-tenant") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(Guid.NewGuid().ToString());
        context.UserIdentifier.Returns("user-1");
        context.User.Returns(principal);

        var groupManager = Substitute.For<IGroupManager>();

        hub.Context = context;
        hub.Groups = groupManager;

        await Assert.ThrowsAsync<HubException>(() => hub.JoinWorkspace(workspaceId));
        await groupManager.DidNotReceive().AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private sealed class MockConnectedUsersRegistry : Anemoi.Centralize.Application.Abstractions.IConnectedUsersRegistry
    {
        public List<string> Users { get; } = new();
        public void AddUser(string userId) => Users.Add(userId);
        public void RemoveUser(string userId) => Users.Remove(userId);
        public IReadOnlyCollection<string> GetActiveUserIds() => Users.AsReadOnly();
    }

    private sealed class MockLocalizer : Microsoft.Extensions.Localization.IStringLocalizer<SharedResource>
    {
        public Microsoft.Extensions.Localization.LocalizedString this[string name] => new(name, name);
        public Microsoft.Extensions.Localization.LocalizedString this[string name, params object[] arguments] => new(name, name);
        public IEnumerable<Microsoft.Extensions.Localization.LocalizedString> GetAllStrings(bool includeParentCultures) => Array.Empty<Microsoft.Extensions.Localization.LocalizedString>();
    }

    private sealed class MockLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
    }
}
