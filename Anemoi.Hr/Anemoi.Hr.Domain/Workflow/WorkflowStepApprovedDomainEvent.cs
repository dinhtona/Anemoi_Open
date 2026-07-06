using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed record WorkflowStepApprovedDomainEvent(
    WorkflowInstanceId WorkflowInstanceId,
    string EntityType,
    string EntityId,
    string PerformedBy,
    int ApprovedStepSequence,
    int? NextStepSequence) : DomainEvent;
