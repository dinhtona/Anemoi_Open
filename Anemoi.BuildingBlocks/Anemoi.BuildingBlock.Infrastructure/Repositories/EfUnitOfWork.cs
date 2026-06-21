using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Domain;
using Anemoi.BuildingBlock.Domain.Abstractions;

namespace Anemoi.BuildingBlock.Infrastructure.Repositories;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IMediator _mediator;

    protected EfUnitOfWork(DbContext dbContext, ILogger logger, IMediator mediator)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<OneOf<None, Exception>> SaveChangesAsync(CancellationToken token = default)
    {
        _logger.Information("Save context async!");
        try
        {
            await _dbContext.SaveChangesAsync(token);
            await DispatchDomainEventsAsync(token);
            return None.Value;
        }
        catch (Exception e)
        {
            _logger.Error("Error while save changes using dbContext: {Error}", e.Message);
            return e;
        }
    }

    private async Task DispatchDomainEventsAsync(CancellationToken token)
    {
        var entries = _dbContext.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .Where(e => e != null && IsEntityWithDomainEvents(e.GetType()))
            .ToList();

        if (entries.Count == 0) return;

        var eventsToPublish = new List<IDomainEvent>();
        foreach (var entity in entries)
        {
            var domainEventsProp = entity.GetType()
                .GetProperty("DomainEvents", BindingFlags.Public | BindingFlags.Instance);
            var clearMethod = entity.GetType()
                .GetMethod("ClearEvents", BindingFlags.Public | BindingFlags.Instance);
            if (domainEventsProp == null || clearMethod == null) continue;

            var events = domainEventsProp.GetValue(entity) as IReadOnlyCollection<IDomainEvent>;
            if (events == null || events.Count == 0) continue;

            eventsToPublish.AddRange(events);
            clearMethod.Invoke(entity, null);
        }

        foreach (var domainEvent in eventsToPublish)
        {
            await _mediator.Publish(domainEvent, token);
        }
    }

    private static bool IsEntityWithDomainEvents(Type type)
    {
        var current = type;
        while (current != null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Entity<>))
                return true;
            current = current.BaseType;
        }
        return false;
    }
}