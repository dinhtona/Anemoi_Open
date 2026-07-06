using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record OnboardingTaskTemplateId(Guid Value) : StronglyTypedId<Guid>(Value);
