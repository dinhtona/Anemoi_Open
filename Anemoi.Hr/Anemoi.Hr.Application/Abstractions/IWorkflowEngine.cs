using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Abstractions;

public interface IWorkflowEngine
{
    Task<OneOf<WorkflowInstance, ErrorDetailResponse>> StartAsync(
        string entityType, Guid entityId,
        EmployeeId requesterEmployeeId, UserId requesterUserId,
        UserId startedBy, CancellationToken ct);

    Task<OneOf<WorkflowInstance, ErrorDetailResponse>> ApproveAsync(
        WorkflowInstanceId workflowInstanceId, UserId performedBy,
        string? comment, CancellationToken ct);

    Task<OneOf<WorkflowInstance, ErrorDetailResponse>> RejectAsync(
        WorkflowInstanceId workflowInstanceId, UserId performedBy,
        string? comment, CancellationToken ct);

    Task<OneOf<WorkflowInstance, ErrorDetailResponse>> CancelAsync(
        WorkflowInstanceId workflowInstanceId, UserId performedBy,
        CancellationToken ct);

    Task<IReadOnlyList<UserId>> GetCurrentApproversAsync(
        WorkflowInstanceId workflowInstanceId, CancellationToken ct);
}
