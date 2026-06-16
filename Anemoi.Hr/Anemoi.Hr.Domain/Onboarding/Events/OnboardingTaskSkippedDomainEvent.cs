using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Onboarding.Events;

public sealed record OnboardingTaskSkippedDomainEvent(
    OnboardingTaskId TaskId,
    OnboardingInstanceId InstanceId,
    string SkippedBy,
    DateTime SkippedAt) : DomainEvent;
