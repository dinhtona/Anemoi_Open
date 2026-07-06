using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record HiringDecisionId(Guid Value) : StronglyTypedId<Guid>(Value);
