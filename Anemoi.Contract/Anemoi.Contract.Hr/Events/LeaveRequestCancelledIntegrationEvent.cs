namespace Anemoi.Contract.Hr.Events;

public sealed record LeaveRequestCancelledIntegrationEvent(string LeaveRequestId, string EmployeeId, string LeavePolicyId);
