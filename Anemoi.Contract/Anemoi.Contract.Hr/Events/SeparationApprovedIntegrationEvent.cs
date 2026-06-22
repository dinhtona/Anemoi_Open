namespace Anemoi.Contract.Hr.Events;

public sealed record SeparationApprovedIntegrationEvent(
    Guid SeparationId,
    Guid EmployeeId,
    string SeparationType,
    DateOnly LastWorkingDate);
