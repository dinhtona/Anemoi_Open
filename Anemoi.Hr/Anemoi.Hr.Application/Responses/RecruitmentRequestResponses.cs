using System;

namespace Anemoi.Hr.Application.Responses;

public sealed record RecruitmentRequestResponse(
    string Id,
    string RequestNumber,
    string DepartmentId,
    string DepartmentName,
    string PositionId,
    string PositionName,
    int RequestedHeadcount,
    string Reason,
    string PriorityCode,
    string RequestedBy,
    DateTime RequestedAt,
    string Status,
    string ApprovedBy,
    DateTime? ApprovedAt,
    string RejectedBy,
    DateTime? RejectedAt,
    string Comment,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record RecruitmentRequestHistoryResponse(
    string Id,
    string RecruitmentRequestId,
    string ActionCode,
    string OldStatus,
    string NewStatus,
    string Comment,
    string PerformedBy,
    DateTime PerformedAt);

public sealed record RecruitmentOpeningResponse(
    string Id,
    string RecruitmentRequestId,
    string Code,
    int PlannedHeadcount,
    int FilledHeadcount,
    int RemainingHeadcount,
    string Status,
    DateTime OpenedAt);

public sealed record RecruitmentDashboardWidgetsResponse(
    int OpenRequests,
    int ApprovedRequests,
    int RejectedRequests,
    int PendingApprovals,
    int OpenPositions,
    int Vacancies,
    int ActiveCandidates,
    int InPipeline,
    int Hired,
    double ConversionRate);
