namespace Anemoi.Contract.Hr.Events;

public sealed record RecruitmentOpeningCreatedIntegrationEvent(
    string RecruitmentOpeningId,
    string RecruitmentRequestId,
    string Code,
    int PlannedHeadcount);
