using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeIdentityLinkLogId(Guid Value) : StronglyTypedId<Guid>(Value);
