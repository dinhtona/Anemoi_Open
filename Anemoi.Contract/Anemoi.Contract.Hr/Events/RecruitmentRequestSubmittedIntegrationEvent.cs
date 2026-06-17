namespace Anemoi.Contract.Hr.Events;

public sealed record RecruitmentRequestSubmittedIntegrationEvent(
    string RecruitmentRequestId,
    string RequestNumber,
    string RequestedByUser,
    string ApproverUserId,
    string DepartmentId,
    string PositionId,
    int Headcount);
