using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Employees.Events;

public sealed record EmployeeCreatedDomainEvent(
    EmployeeId EmployeeId,
    string EmployeeCode,
    string FullName,
    string Actor,
    string? CorrelationId = null) : DomainEvent;
