using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record OnboardingPlanTemplateId(Guid Value) : StronglyTypedId<Guid>(Value);
