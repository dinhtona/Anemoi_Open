using Anemoi.BuildingBlock.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Abstractions;

public interface IWorkflowTargetStatusUpdater
{
    bool CanHandle(string entityType);
    Task<OneOf<bool, ErrorDetailResponse>> MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct);
    Task<OneOf<bool, ErrorDetailResponse>> MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct);
}
