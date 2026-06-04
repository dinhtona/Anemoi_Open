using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record LeaveAccrualRunId(Guid Value) : StronglyTypedId<Guid>(Value);
