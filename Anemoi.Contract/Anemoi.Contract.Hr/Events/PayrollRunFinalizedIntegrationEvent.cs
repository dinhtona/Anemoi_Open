#nullable enable

namespace Anemoi.Contract.Hr.Events;

public sealed record PayrollRunFinalizedIntegrationEvent(string PayrollRunId, string FinalizedBy);
