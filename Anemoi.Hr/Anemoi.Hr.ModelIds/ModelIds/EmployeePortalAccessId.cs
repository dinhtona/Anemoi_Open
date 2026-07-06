using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeePortalAccessId(Guid Value) : StronglyTypedId<Guid>(Value);
