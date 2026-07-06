using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Onboarding.Events;

public sealed record OnboardingTaskAssignedDomainEvent(
    OnboardingTaskId TaskId,
    OnboardingInstanceId InstanceId,
    string AssignedUserId,
    string AssignedBy,
    DateTime DueDate) : DomainEvent;
