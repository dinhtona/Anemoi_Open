using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record WorkflowDefinitionId(Guid Value) : StronglyTypedId<Guid>(Value);
