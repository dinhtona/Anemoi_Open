namespace Anemoi.Hr.Application.Responses;

public sealed record WorkflowDefinitionResponse(
    string Id, string Code, string Name, string? Description,
    string WorkflowTypeCode, string TargetEntityType, int Version, bool IsActive,
    IReadOnlyCollection<WorkflowDefinitionStepResponse> Steps,
    DateTime CreatedAt, DateTime UpdatedAt);

public sealed record WorkflowDefinitionStepResponse(
    string Id, int Sequence,
    string ApproverType, string? ApproverValue, bool IsRequired);

public sealed record WorkflowInstanceResponse(
    string Id, string? WorkflowDefinitionId, string? WorkflowDefinitionName,
    int? WorkflowDefinitionVersion,
    string EntityType, string EntityId, int CurrentStep,
    string Status, string StartedBy, string RequesterEmployeeId, string RequesterUserId,
    string? RequesterName, string? CurrentApproverName,
    DateTime StartedAt, DateTime? CompletedAt,
    IReadOnlyCollection<WorkflowInstanceStepResponse> Steps,
    IReadOnlyCollection<WorkflowHistoryResponse> Histories);

public sealed record WorkflowInstanceStepResponse(
    string Id, int Sequence,
    string ApproverTypeSnapshot, string? ApproverValueSnapshot,
    string? ApproverUserId, string Status,
    DateTime? ApprovedAt, DateTime? RejectedAt, string? Comment);

public sealed record WorkflowHistoryResponse(
    string Id, string WorkflowInstanceId,
    string Action, string PerformedBy, string? Comment, DateTime PerformedAt);
