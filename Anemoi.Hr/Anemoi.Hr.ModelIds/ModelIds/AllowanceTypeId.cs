using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record AllowanceTypeId(Guid Value) : StronglyTypedId<Guid>(Value);
