namespace Anemoi.Hr.Application.Events;

public sealed record LeaveRequestApprovedIntegrationEvent(string LeaveRequestId, string EmployeeId, string LeavePolicyId);
