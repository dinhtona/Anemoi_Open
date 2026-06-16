using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record CandidateId(Guid Value) : StronglyTypedId<Guid>(Value);
