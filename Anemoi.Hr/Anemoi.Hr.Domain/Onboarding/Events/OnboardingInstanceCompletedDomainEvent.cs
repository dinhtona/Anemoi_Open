using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Onboarding.Events;

public sealed record OnboardingInstanceCompletedDomainEvent(
    OnboardingInstanceId InstanceId,
    EmployeeId EmployeeId,
    string CompletedBy,
    string CompletionType) : DomainEvent;
