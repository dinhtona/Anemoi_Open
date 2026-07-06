namespace Anemoi.Hr.Application.Abstractions;

public interface IWorkflowTargetStatusUpdater
{
    bool CanHandle(string entityType);
    Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct);
    Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct);
}
