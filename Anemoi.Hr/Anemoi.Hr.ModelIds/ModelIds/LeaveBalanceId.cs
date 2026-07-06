using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record LeaveBalanceId(Guid Value) : StronglyTypedId<Guid>(Value);
