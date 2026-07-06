using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeSeparationId(Guid Value) : StronglyTypedId<Guid>(Value);
