using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Transfers.Events;

public sealed record TransferRejectedDomainEvent(
    EmployeeTransferId TransferId,
    EmployeeId EmployeeId,
    string Reason,
    string Actor,
    string? CorrelationId = null) : DomainEvent;
