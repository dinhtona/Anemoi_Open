using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Separations.Events;

public sealed record SeparationSubmittedDomainEvent(
    EmployeeSeparationId SeparationId,
    EmployeeId EmployeeId,
    string Actor,
    string? CorrelationId = null) : DomainEvent;
