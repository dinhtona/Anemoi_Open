using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeId(Guid Value) : StronglyTypedId<Guid>(Value);
