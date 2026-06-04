using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record LeavePolicyId(Guid Value) : StronglyTypedId<Guid>(Value);
