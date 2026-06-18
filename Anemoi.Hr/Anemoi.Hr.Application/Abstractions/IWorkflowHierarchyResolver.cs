using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Abstractions;

public sealed record ResolvedApproverStep(
    int StepOrder, string ApproverType, string? ApproverValue);

public interface IWorkflowHierarchyResolver
{
    Task<IReadOnlyList<ResolvedApproverStep>> ResolveHierarchyAsync(
        EmployeeId requesterEmployeeId, CancellationToken ct);
}
