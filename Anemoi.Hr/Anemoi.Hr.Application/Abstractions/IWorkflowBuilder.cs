using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Abstractions;

public sealed record WorkflowBuildError(string ErrorCode);

public sealed record WorkflowBuildResult(
    IReadOnlyList<WorkflowInstanceStep> Steps,
    WorkflowDefinitionId? DefinitionId,
    string? DefinitionName,
    int? DefinitionVersion);

public interface IWorkflowBuilder
{
    Task<OneOf<WorkflowBuildResult, WorkflowBuildError>> BuildAsync(
        string entityType, EmployeeId requesterEmployeeId, string startedBy, CancellationToken ct);
}
