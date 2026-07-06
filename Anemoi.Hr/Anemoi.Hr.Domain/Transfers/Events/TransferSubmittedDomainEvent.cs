using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Transfers.Events;

public sealed record TransferSubmittedDomainEvent(
    EmployeeTransferId TransferId,
    EmployeeId EmployeeId,
    string Actor,
    string? CorrelationId = null) : DomainEvent;
