using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Employees.Events;

public sealed record EmployeeStatusChangedDomainEvent(
    EmployeeId EmployeeId,
    string FromStatus,
    string ToStatus,
    string Actor,
    string? CorrelationId = null) : DomainEvent;
