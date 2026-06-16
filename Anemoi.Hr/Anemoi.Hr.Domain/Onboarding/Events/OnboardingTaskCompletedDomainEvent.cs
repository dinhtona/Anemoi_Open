using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Onboarding.Events;

public sealed record OnboardingTaskCompletedDomainEvent(
    OnboardingTaskId TaskId,
    OnboardingInstanceId InstanceId,
    string CompletedBy,
    DateTime CompletedAt) : DomainEvent;
