namespace Anemoi.Hr.Application.Abstractions;

public sealed record WorkflowSummaryResponse(
    string? CurrentApproverName,
    string? CurrentStepName,
    string? WorkflowStatus);

public interface IWorkflowQueryService
{
    Task<Dictionary<Guid, WorkflowSummaryResponse>> GetWorkflowSummariesAsync(
        string entityType,
        IReadOnlyCollection<Guid> entityIds,
        CancellationToken ct);
}
