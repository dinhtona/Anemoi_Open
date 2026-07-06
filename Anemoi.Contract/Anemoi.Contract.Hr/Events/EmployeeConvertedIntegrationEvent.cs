namespace Anemoi.Contract.Hr.Events;

public sealed record EmployeeConvertedIntegrationEvent(
    string CandidateId,
    string EmployeeId,
    string RecruitmentOpeningId,
    string ConvertedBy);
