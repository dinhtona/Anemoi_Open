#nullable enable

namespace Anemoi.Contract.Hr.Events;

public sealed record OvertimeRequestApprovedIntegrationEvent(string OvertimeRequestId, string EmployeeId);
