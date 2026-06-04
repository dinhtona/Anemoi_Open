namespace Anemoi.Hr.Application.Events;

public sealed record LeaveRequestSubmittedIntegrationEvent(string LeaveRequestId, string EmployeeId, string LeavePolicyId);
