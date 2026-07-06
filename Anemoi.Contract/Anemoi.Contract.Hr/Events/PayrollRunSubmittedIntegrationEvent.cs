#nullable enable

namespace Anemoi.Contract.Hr.Events;

public sealed record PayrollRunSubmittedIntegrationEvent(string PayrollRunId, string SubmittedBy);
