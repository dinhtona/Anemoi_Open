namespace Anemoi.Contract.Hr.Events;

public sealed record RecruitmentRequestApprovedIntegrationEvent(
    string RecruitmentRequestId,
    string RequestNumber,
    string ApprovedBy);
