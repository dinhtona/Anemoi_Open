using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeAssetId(Guid Value) : StronglyTypedId<Guid>(Value);
