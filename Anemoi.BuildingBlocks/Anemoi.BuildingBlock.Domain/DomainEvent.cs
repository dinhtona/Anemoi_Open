using System;
using Anemoi.BuildingBlock.Domain.Abstractions;

namespace Anemoi.BuildingBlock.Domain;

public record DomainEvent : IDomainEvent
{
    public Guid AggregateId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string EventType { get; set; }
    public string AssemblyType { get; set; }

    protected DomainEvent()
    {
        // Same sequential-GUID semantics as IdGenerator.NextGuid() (which is
        // MassTransit.NewId.NextGuid()). BuildingBlock.Domain cannot reference
        // Anemoi.BuildingBlock.Application.Helpers.IdGenerator without inverting
        // the Clean Architecture dependency direction, so NewId is used directly.
        AggregateId = MassTransit.NewId.NextGuid();
        OccurredAt = DateTime.UtcNow;
        EventType = GetType().Name;
        AssemblyType = $"{GetType().FullName},{GetType().Assembly.GetName().Name}";
    }
}