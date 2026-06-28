using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class RecruitmentWorkflowStatusUpdater(
    ISqlRepository<RecruitmentRequest> recruitmentRepository,
    IUnitOfWork unitOfWork)
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
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new RecruitmentRequestId(Guid.Parse(entityId));
        var request = await recruitmentRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (request is null) return;
        request.Reject(performedBy, DateTime.UtcNow, reason);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
