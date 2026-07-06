using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record LeaveTypeId(Guid Value) : StronglyTypedId<Guid>(Value);
