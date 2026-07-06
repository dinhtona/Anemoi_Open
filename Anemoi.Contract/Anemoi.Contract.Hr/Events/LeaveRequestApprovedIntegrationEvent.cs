namespace Anemoi.Contract.Hr.Events;

public sealed record LeaveRequestApprovedIntegrationEvent(string LeaveRequestId, string EmployeeId, string LeavePolicyId);
