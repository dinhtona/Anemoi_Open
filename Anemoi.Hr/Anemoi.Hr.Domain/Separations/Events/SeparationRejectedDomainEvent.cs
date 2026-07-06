using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Separations.Events;

public sealed record SeparationRejectedDomainEvent(
    EmployeeSeparationId SeparationId,
    EmployeeId EmployeeId,
    string Reason,
    string Actor,
    string? CorrelationId = null) : DomainEvent;
