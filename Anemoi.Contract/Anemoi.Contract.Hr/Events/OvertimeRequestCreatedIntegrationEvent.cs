#nullable enable

namespace Anemoi.Contract.Hr.Events;

public sealed record OvertimeRequestCreatedIntegrationEvent(string OvertimeRequestId, string EmployeeId, string? ManagerEmployeeId);
