using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using OneOf;

namespace Anemoi.Hr.Application.Abstractions;

public interface IWorkflowRoleResolver
{
    Task<OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>> ResolveAsync(
        string workflowRole, CancellationToken ct);
}
