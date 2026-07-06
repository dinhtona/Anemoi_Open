using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record PositionId(Guid Value) : StronglyTypedId<Guid>(Value);
