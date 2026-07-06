namespace Anemoi.Contract.Hr.Events;

public sealed record LeaveRequestRejectedIntegrationEvent(string LeaveRequestId, string EmployeeId, string LeavePolicyId);
