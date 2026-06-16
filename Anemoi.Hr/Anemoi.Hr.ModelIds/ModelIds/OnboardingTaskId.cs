using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record OnboardingTaskId(Guid Value) : StronglyTypedId<Guid>(Value);
