using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Probation.Events;

public sealed record ProbationStatusChangedDomainEvent(
    ProbationRecordId RecordId,
    EmployeeId EmployeeId,
    string FromStatus,
    string ToStatus,
    string Actor,
    string? CorrelationId = null) : DomainEvent;
