#nullable enable

namespace Anemoi.Contract.Hr.Events;

public sealed record OvertimeRequestCancelledIntegrationEvent(string OvertimeRequestId, string EmployeeId);
