using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using OneOf;

namespace Anemoi.Hr.Application.Abstractions;

public interface IApprovalResolver
{
    Task<OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>> ResolveApproversAsync(
        string approverType, string? approverValue,
        ApprovalRoutingContext context, CancellationToken ct);
}
