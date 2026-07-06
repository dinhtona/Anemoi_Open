using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record InterviewScheduleId(Guid Value) : StronglyTypedId<Guid>(Value);
