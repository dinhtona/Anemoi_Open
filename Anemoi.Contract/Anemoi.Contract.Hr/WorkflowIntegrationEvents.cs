#nullable enable
using MassTransit;

namespace Anemoi.Contract.Hr;

[EntityName("workflow-started")]
public sealed record WorkflowStartedIntegrationEvent(
    Guid WorkflowInstanceId,
    string EntityType,
    string EntityId,
    string StartedBy,
    string RequesterUserId,
    string RequesterEmployeeId,
    DateTime StartedAt);

[EntityName("workflow-approved")]
public sealed record WorkflowApprovedIntegrationEvent(
    Guid WorkflowInstanceId,
    string EntityType,
    string EntityId,
    string PerformedBy,
    string? Comment,
    DateTime ApprovedAt,
    bool IsCompleted);

[EntityName("workflow-rejected")]
public sealed record WorkflowRejectedIntegrationEvent(
    Guid WorkflowInstanceId,
    string EntityType,
    string EntityId,
    string PerformedBy,
    string? Comment,
    DateTime RejectedAt,
    bool IsCompleted);
