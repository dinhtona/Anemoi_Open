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
using Anemoi.Contract.Hr.Events;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Constants;
using Anemoi.Contract.Notification.Events;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Application.Consumers;
using Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.CreateNotification;
using Anemoi.Notification.Application.Mappings;
using Anemoi.Notification.Application.Services;
using Anemoi.Notification.Domain.Models;
using MassTransit;
using MediatR;
using NSubstitute;
using OneOf;
using Serilog;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class NotificationFailureBehaviorTests
{
    private static readonly NotificationMapper Mapper = new();
    private static readonly ILogger Logger = new LoggerConfiguration().CreateLogger();

    /// <summary>
    /// Verifies that if CreateNotificationHandler fails to save the notification to the database,
    /// an error is returned and no NotificationCreatedIntegrationEvent is published.
    /// This simulates the "consumer fails before commit" scenario.
    /// </summary>
    [Fact]
    public async Task CreateNotification_ShouldReturnError_WhenSaveChangesFails()
    {
        var userId = Guid.NewGuid();
        var subRepo = new FakeRepository<NotificationSubscription>([]);
        var repo = new FakeRepository<NotificationHistory>([]);
        var preferenceRepo = new FakeRepository<NotificationPreference>([]);
        var unitOfWork = new FakeUnitOfWorkThatFails();
        var mockPublisher = new MockPublishEndpoint();

        var handler = new CreateNotificationHandler(repo, subRepo, preferenceRepo, unitOfWork, mockPublisher, Mapper, Logger);

        var command = new CreateNotificationCommand(
            UserId: userId.ToString(),
            Title: "Test Title",
            Content: "Test Content",
            Category: "System"
        );

        var response = await handler.Handle(command, CancellationToken.None);

        // Assert: handler returns error (not a successful notification response)
        Assert.True(response.IsT1, $"Expected error response but got success. IsT0={response.IsT0}");

        // Assert: no NotificationCreatedIntegrationEvent was published
        Assert.Empty(mockPublisher.PublishedEvents);
    }

    /// <summary>
    /// Verifies that when the notification IS successfully created, 
    /// a NotificationCreatedIntegrationEvent is published through the bus outbox.
    /// This simulates the successful path where DataChangeOccurred is not lost.
    /// </summary>
    [Fact]
    public async Task CreateNotification_ShouldPublishIntegrationEvent_AfterSuccessfulSave()
    {
        var userId = Guid.NewGuid();
        var notificationsList = new List<NotificationHistory>();
        var subRepo = new FakeRepository<NotificationSubscription>([]);
        var repo = new FakeRepository<NotificationHistory>(notificationsList);
        var preferenceRepo = new FakeRepository<NotificationPreference>([]);
        var unitOfWork = new FakeSuccessfulUnitOfWork();
        var mockPublisher = new MockPublishEndpoint();

        var handler = new CreateNotificationHandler(repo, subRepo, preferenceRepo, unitOfWork, mockPublisher, Mapper, Logger);

        var command = new CreateNotificationCommand(
            UserId: userId.ToString(),
            Title: "Test Title",
            Content: "Test Content",
            Category: "System",
            DeduplicationKey: "success-test-key"
        );

        var response = await handler.Handle(command, CancellationToken.None);

        // Assert: handler returns success
        Assert.True(response.IsT0);

        // Assert: one notification history was persisted
        Assert.Single(notificationsList);

        // Assert: NotificationCreatedIntegrationEvent was published
        Assert.Single(mockPublisher.PublishedEvents);
        Assert.IsType<NotificationCreatedIntegrationEvent>(mockPublisher.PublishedEvents[0]);
    }

    /// <summary>
    /// Verifies that DataChangeOccurredIntegrationEvent is emitted by the consumer
    /// even when the recipient resolution fails (notification is skipped).
    /// This ensures the data-change signal is never lost.
    /// </summary>
    [Fact]
    public async Task DataChangeOccurred_ShouldAlwaysBeEmitted_EvenWhenNotificationIsSkipped()
    {
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var mediator = Substitute.For<IMediator>();
        var recipientResolver = Substitute.For<INotificationRecipientResolver>();
        var logger = new LoggerConfiguration().CreateLogger();

        var payslipId = Guid.NewGuid().ToString();
        var employeeId = Guid.NewGuid().ToString();

        // Recipient resolution fails (returns null) - notification will be skipped
        recipientResolver.ResolveUserIdByEmployeeId(employeeId, Arg.Any<CancellationToken>())
            .Returns((string)null);

        var consumer = new PayslipPublishedConsumer(recipientResolver, mediator, publishEndpoint, logger);
        var context = Substitute.For<ConsumeContext<PayslipPublishedIntegrationEvent>>();
        context.Message.Returns(new PayslipPublishedIntegrationEvent(payslipId, employeeId));
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        // DataChangeOccurred must still be emitted even though notification was skipped
        await publishEndpoint.Received(1).Publish(Arg.Is<DataChangeOccurredIntegrationEvent>(e =>
            e.Resource == "hr.payslip" && e.EntityId == payslipId
        ), Arg.Any<CancellationToken>());

        // Notification must NOT be created
        await mediator.DidNotReceive().Send(Arg.Any<CreateNotificationCommand>(), Arg.Any<CancellationToken>());
    }

    private sealed class FakeUnitOfWorkThatFails : IUnitOfWork
    {
        public Task<OneOf<None, Exception>> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<OneOf<None, Exception>>(new Exception("Database save failed"));

        public Task<OneOf<None, Exception>> BeginTransactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<OneOf<None, Exception>> CommitTransactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FakeSuccessfulUnitOfWork : IUnitOfWork
    {
        public Task<OneOf<None, Exception>> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<OneOf<None, Exception>>(None.Value);

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
            return queryable;
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
            => Task.FromResult(GetQueryable(conditionExpression).Any());

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
                _items.Remove(itemOrFilter.AsT0);
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
}
