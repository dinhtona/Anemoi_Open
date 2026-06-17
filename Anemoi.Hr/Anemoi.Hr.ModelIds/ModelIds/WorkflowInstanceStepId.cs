using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record WorkflowInstanceStepId(Guid Value) : StronglyTypedId<Guid>(Value);
