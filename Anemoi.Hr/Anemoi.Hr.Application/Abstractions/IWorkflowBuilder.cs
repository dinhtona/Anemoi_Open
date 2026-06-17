using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Abstractions;

public sealed record WorkflowBuildError(string ErrorCode);

public interface IWorkflowBuilder
{
    Task<OneOf<IReadOnlyList<WorkflowInstanceStep>, WorkflowBuildError>> BuildAsync(
        string entityType, EmployeeId requesterEmployeeId, string startedBy, CancellationToken ct);
}
