#nullable enable

namespace Anemoi.Contract.Hr.Events;

public sealed record PayslipPublishedIntegrationEvent(string PayslipId, string EmployeeId);
