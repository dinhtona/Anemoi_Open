using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Onboarding.Events;

public sealed record OnboardingInstanceCancelledDomainEvent(
    OnboardingInstanceId InstanceId,
    EmployeeId EmployeeId,
    string CancelledBy,
    DateTime CancelledAt) : DomainEvent;
