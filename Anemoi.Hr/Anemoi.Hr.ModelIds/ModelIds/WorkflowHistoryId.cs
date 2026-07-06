using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record WorkflowHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
