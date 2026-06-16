using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record CandidateApplicationId(Guid Value) : StronglyTypedId<Guid>(Value);
