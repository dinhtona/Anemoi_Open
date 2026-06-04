using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record LeaveRequestId(Guid Value) : StronglyTypedId<Guid>(Value);
