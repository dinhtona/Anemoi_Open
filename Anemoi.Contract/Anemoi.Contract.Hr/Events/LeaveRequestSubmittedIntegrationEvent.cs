#nullable enable

namespace Anemoi.Contract.Hr.Events;

public sealed record LeaveRequestSubmittedIntegrationEvent(string LeaveRequestId, string EmployeeId, string LeavePolicyId, string? ApproverEmployeeId);
