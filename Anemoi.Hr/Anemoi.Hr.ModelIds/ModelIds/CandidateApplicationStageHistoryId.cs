using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record CandidateApplicationStageHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
