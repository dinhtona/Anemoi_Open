namespace Anemoi.Contract.Hr.Events;

public sealed record RecruitmentRequestRejectedIntegrationEvent(
    string RecruitmentRequestId,
    string RequestNumber,
    string RejectedBy,
    string Reason);
