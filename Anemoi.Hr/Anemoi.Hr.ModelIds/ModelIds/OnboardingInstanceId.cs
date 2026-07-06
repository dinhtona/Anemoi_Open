using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record OnboardingInstanceId(Guid Value) : StronglyTypedId<Guid>(Value);
