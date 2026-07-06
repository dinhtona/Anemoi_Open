using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record JobPostingId(Guid Value) : StronglyTypedId<Guid>(Value);
