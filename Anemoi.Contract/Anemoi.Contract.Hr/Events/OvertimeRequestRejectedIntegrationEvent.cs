#nullable enable

namespace Anemoi.Contract.Hr.Events;

public sealed record OvertimeRequestRejectedIntegrationEvent(string OvertimeRequestId, string EmployeeId);
