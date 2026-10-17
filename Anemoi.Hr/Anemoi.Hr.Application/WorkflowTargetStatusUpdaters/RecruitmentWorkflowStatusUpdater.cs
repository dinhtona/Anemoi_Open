using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class RecruitmentWorkflowStatusUpdater(
    ISqlRepository<RecruitmentRequest> recruitmentRepository,
    IUnitOfWork unitOfWork)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.RecruitmentRequest;

    public async Task<OneOf<bool, ErrorDetailResponse>> MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new RecruitmentRequestId(Guid.Parse(entityId));
        var request = await recruitmentRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (request is null) return true;
        request.Approve(performedBy, DateTime.UtcNow);
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    public async Task<OneOf<bool, ErrorDetailResponse>> MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new RecruitmentRequestId(Guid.Parse(entityId));
        var request = await recruitmentRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (request is null) return true;
        request.Reject(performedBy, DateTime.UtcNow, reason);
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
