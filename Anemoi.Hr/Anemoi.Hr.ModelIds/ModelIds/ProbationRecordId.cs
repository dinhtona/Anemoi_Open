using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record ProbationRecordId(Guid Value) : StronglyTypedId<Guid>(Value);
