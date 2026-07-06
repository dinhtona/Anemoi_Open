#nullable enable

namespace Anemoi.Contract.Hr.Events;

public sealed record PayslipCancelledIntegrationEvent(string PayslipId, string EmployeeId);
