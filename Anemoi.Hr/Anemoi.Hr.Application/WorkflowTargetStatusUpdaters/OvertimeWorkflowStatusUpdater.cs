using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Overtime;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class OvertimeWorkflowStatusUpdater(
    ISqlRepository<OvertimeRequest> overtimeRepository,
    IUnitOfWork unitOfWork)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.OvertimeRequest;

    public async Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new OvertimeRequestId(Guid.Parse(entityId));
        var overtime = await overtimeRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (overtime is null) return;
        overtime.Approve(performedBy);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new OvertimeRequestId(Guid.Parse(entityId));
        var overtime = await overtimeRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (overtime is null) return;
        overtime.Reject(performedBy, reason);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
