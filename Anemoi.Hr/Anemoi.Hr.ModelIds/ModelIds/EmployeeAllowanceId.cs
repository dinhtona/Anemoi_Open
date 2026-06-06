using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeAllowanceId(Guid Value) : StronglyTypedId<Guid>(Value);
