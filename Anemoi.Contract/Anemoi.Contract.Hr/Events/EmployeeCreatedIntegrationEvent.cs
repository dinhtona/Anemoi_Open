namespace Anemoi.Contract.Hr.Events;

public sealed record EmployeeCreatedIntegrationEvent(
    Guid EmployeeId,
    string EmployeeCode,
    string FullName,
    string WorkEmail);
