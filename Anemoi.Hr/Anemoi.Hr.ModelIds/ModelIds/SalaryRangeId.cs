using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record SalaryRangeId(Guid Value) : StronglyTypedId<Guid>(Value);
