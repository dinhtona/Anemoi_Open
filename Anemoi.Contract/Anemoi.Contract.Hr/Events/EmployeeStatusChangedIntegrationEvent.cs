namespace Anemoi.Contract.Hr.Events;

public sealed record EmployeeStatusChangedIntegrationEvent(
    Guid EmployeeId,
    string FromStatus,
    string ToStatus);
