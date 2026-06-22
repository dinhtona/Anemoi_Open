using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Transfers.Events;

public sealed record TransferApprovedDomainEvent(
    EmployeeTransferId TransferId,
    EmployeeId EmployeeId,
    string Actor,
    string? CorrelationId = null) : DomainEvent;
