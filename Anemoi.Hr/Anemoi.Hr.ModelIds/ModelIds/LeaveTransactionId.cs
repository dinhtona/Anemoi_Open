using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record LeaveTransactionId(Guid Value) : StronglyTypedId<Guid>(Value);
