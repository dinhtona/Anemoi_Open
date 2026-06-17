using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed record WorkflowInstanceRejectedDomainEvent(
    WorkflowInstanceId WorkflowInstanceId,
    string EntityType,
    string EntityId,
    string PerformedBy,
    string? Comment) : DomainEvent;
