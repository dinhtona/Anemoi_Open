using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record InterviewFeedbackId(Guid Value) : StronglyTypedId<Guid>(Value);
