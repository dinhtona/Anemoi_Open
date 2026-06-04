namespace Anemoi.Hr.Application.Events;

public sealed record EmployeeCreatedIntegrationEvent(string EmployeeId, string EmployeeCode, string WorkEmail);
