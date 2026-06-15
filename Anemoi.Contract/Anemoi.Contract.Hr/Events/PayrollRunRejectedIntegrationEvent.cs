#nullable enable

namespace Anemoi.Contract.Hr.Events;

public sealed record PayrollRunRejectedIntegrationEvent(string PayrollRunId, string RejectedBy);
