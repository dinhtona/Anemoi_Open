#nullable enable

namespace Anemoi.Contract.Hr.Events;

public sealed record PayrollRunApprovedIntegrationEvent(string PayrollRunId, string ApprovedBy);
