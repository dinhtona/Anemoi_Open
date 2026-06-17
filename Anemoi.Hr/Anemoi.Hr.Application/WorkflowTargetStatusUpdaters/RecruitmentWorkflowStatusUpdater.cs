using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class RecruitmentWorkflowStatusUpdater(
    ISqlRepository<RecruitmentRequest> recruitmentRepository)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.RecruitmentRequest;

    public async Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new RecruitmentRequestId(Guid.Parse(entityId));
        var request = await recruitmentRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (request is null) return;
        request.Approve(performedBy, DateTime.UtcNow);
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new RecruitmentRequestId(Guid.Parse(entityId));
        var request = await recruitmentRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (request is null) return;
        request.Reject(performedBy, DateTime.UtcNow, reason);
    }
}
