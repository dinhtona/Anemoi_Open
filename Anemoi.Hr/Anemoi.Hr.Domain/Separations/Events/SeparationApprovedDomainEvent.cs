using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Separations.Events;

public sealed record SeparationApprovedDomainEvent(
    EmployeeSeparationId SeparationId,
    EmployeeId EmployeeId,
    string Actor,
    string? CorrelationId = null) : DomainEvent;
